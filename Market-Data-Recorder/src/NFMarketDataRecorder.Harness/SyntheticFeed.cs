using System;
using System.Collections.Generic;

using NFMarketDataRecorder.Core;

namespace NFMarketDataRecorder.Harness
{
    /// <summary>One scripted market event.</summary>
    public struct ScriptEvent
    {
        public long OffsetTicks;     // source time offset from the script's start
        public bool IsTrade;
        public string Side;          // depth only
        public decimal Price;
        public decimal Volume;
        public string Aggressor;     // trade only
    }

    /// <summary>
    /// A deterministic, NQ-shaped event script plus the order book it implies.
    /// </summary>
    /// <remarks>
    /// This stands in for the ATAS Replay feed so that the recorder, the
    /// source-time snapshot scheduling and the whole comparison pipeline can be
    /// exercised end to end in an environment with no ATAS. It is NOT a claim
    /// about what ATAS Replay does — that question can only be answered on a
    /// machine running ATAS, per docs/GUI-REPLAY-RUNBOOK.md. What it does prove is
    /// that the recorder itself produces an identical capture at any speed, so
    /// that any difference observed in a real ATAS run is attributable to ATAS
    /// rather than to this tool.
    /// </remarks>
    public static class SyntheticFeed
    {
        public const decimal TickSize = 0.25m;

        /// <summary>
        /// Builds the script. A given (seed, minutes, eventsPerSecond) always
        /// yields byte-identical output, independent of speed, threading or host.
        /// </summary>
        public static List<ScriptEvent> Build(ulong seed, int minutes, int eventsPerSecond, int ladderDepth)
        {
            var rng = new DeterministicRng(seed);
            var script = new List<ScriptEvent>(minutes * 60 * eventsPerSecond);

            decimal mid = 20000.00m;
            long totalEvents = (long)minutes * 60L * eventsPerSecond;
            long spanTicks = TimeSpan.FromMinutes(minutes).Ticks;
            long stepTicks = spanTicks / Math.Max(1L, totalEvents);

            for (long i = 0; i < totalEvents; i++)
            {
                // Source timestamps advance on a fixed grid with jitter that never
                // makes them go backwards. Several events landing on one timestamp is
                // deliberate: that is the case where 1x and accelerated replays are
                // most likely to disagree on ordering.
                long offset = i * stepTicks + rng.Next((int)Math.Max(1, stepTicks));

                bool isTrade = rng.NextDouble() < 0.25;

                if (isTrade)
                {
                    // Random walk on the tick grid.
                    int drift = rng.Next(3) - 1;
                    mid += drift * TickSize;

                    double u = rng.NextDouble();
                    string agg = u < 0.47 ? Aggressor.Buy : (u < 0.94 ? Aggressor.Sell : Aggressor.Unknown);

                    script.Add(new ScriptEvent
                    {
                        OffsetTicks = offset,
                        IsTrade = true,
                        Price = mid,
                        Volume = 1 + rng.Next(25),
                        Aggressor = agg,
                    });
                }
                else
                {
                    bool bid = rng.NextDouble() < 0.5;
                    int level = rng.Next(ladderDepth);
                    decimal price = bid
                        ? mid - TickSize * (level + 1)
                        : mid + TickSize * (level + 1);

                    // Volume 0 means the level was removed. Exercising removal matters:
                    // it is the depth update most likely to be dropped by a feed, and
                    // the one whose loss a snapshot comparison will catch.
                    decimal vol = rng.NextDouble() < 0.08 ? 0m : 1 + rng.Next(400);

                    script.Add(new ScriptEvent
                    {
                        OffsetTicks = offset,
                        IsTrade = false,
                        Side = bid ? Side.Bid : Side.Ask,
                        Price = price,
                        Volume = vol,
                    });
                }
            }

            return script;
        }
    }

    /// <summary>
    /// The book the synthetic platform maintains, standing in for the depth API
    /// the ATAS adapter reads. The recorder reads it through
    /// <see cref="IDomSource"/> exactly as it reads the real one.
    /// </summary>
    public sealed class SyntheticBook : IDomSource
    {
        private readonly SortedDictionary<decimal, decimal> _bids =
            new SortedDictionary<decimal, decimal>(Comparer<decimal>.Create((a, b) => b.CompareTo(a))); // best (highest) first
        private readonly SortedDictionary<decimal, decimal> _asks =
            new SortedDictionary<decimal, decimal>(); // best (lowest) first

        private readonly object _gate = new object();

        public void Apply(string side, decimal price, decimal volume)
        {
            var book = side == Side.Bid ? _bids : _asks;
            lock (_gate)
            {
                if (volume <= 0m) book.Remove(price);
                else book[price] = volume;
            }
        }

        public DomBook GetBook(int depthLimit)
        {
            lock (_gate)
            {
                return new DomBook(Top(_bids, depthLimit), Top(_asks, depthLimit));
            }
        }

        private static DomLevel[] Top(SortedDictionary<decimal, decimal> book, int limit)
        {
            int n = limit > 0 ? Math.Min(limit, book.Count) : book.Count;
            var result = new DomLevel[n];
            int i = 0;
            foreach (var kv in book)
            {
                if (i == n) break;
                result[i++] = new DomLevel(kv.Key, kv.Value);
            }
            return result;
        }
    }
}
