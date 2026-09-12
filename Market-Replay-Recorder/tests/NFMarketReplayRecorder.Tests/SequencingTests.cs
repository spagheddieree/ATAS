using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NFMarketReplayRecorder.Core;
using Xunit;

namespace NFMarketReplayRecorder.Tests
{
    /// <summary>Required area: sequencing, including snapshot scheduling on source time.</summary>
    public class SequencingTests
    {
        // ------------------------------------------------------------- scheduler

        [Fact]
        public void Scheduler_emits_nothing_on_the_anchoring_event()
        {
            var s = new SnapshotScheduler(TimeSpan.FromSeconds(1), 10);
            var due = s.Advance(Sample.Epoch.AddMilliseconds(250), out _);
            Assert.Empty(due);
        }

        [Fact]
        public void Scheduler_anchors_to_the_interval_grid_not_to_the_first_event()
        {
            // Two runs whose first event differs by microseconds must still agree on
            // every subsequent boundary, or no capture could ever be compared.
            var a = new SnapshotScheduler(TimeSpan.FromSeconds(1), 10);
            var b = new SnapshotScheduler(TimeSpan.FromSeconds(1), 10);

            a.Advance(Sample.Epoch.AddTicks(1), out _);
            b.Advance(Sample.Epoch.AddTicks(9999), out _);

            Assert.Equal(a.NextDue, b.NextDue);
            Assert.Equal(Sample.Epoch.AddSeconds(1), a.NextDue);
        }

        [Fact]
        public void Scheduler_emits_one_boundary_per_elapsed_interval_in_ascending_order()
        {
            var s = new SnapshotScheduler(TimeSpan.FromSeconds(1), 10);
            s.Advance(Sample.Epoch, out _);

            var due = s.Advance(Sample.Epoch.AddMilliseconds(3500), out bool truncated);

            Assert.False(truncated);
            Assert.Equal(
                new[] { Sample.Epoch.AddSeconds(1), Sample.Epoch.AddSeconds(2), Sample.Epoch.AddSeconds(3) },
                due);
        }

        [Fact]
        public void Scheduler_boundaries_are_exact_multiples_of_the_interval()
        {
            var s = new SnapshotScheduler(TimeSpan.FromMilliseconds(250), 100);
            s.Advance(Sample.Epoch, out _);
            var due = s.Advance(Sample.Epoch.AddMilliseconds(1000), out _);

            foreach (var t in due)
                Assert.Equal(0, (t - Sample.Epoch).Ticks % TimeSpan.FromMilliseconds(250).Ticks);
        }

        [Fact]
        public void Scheduler_truncates_a_large_gap_and_realigns_to_the_grid()
        {
            var s = new SnapshotScheduler(TimeSpan.FromSeconds(1), 3);
            s.Advance(Sample.Epoch, out _);

            var due = s.Advance(Sample.Epoch.AddSeconds(100), out bool truncated);

            Assert.True(truncated);
            Assert.Equal(3, due.Count);

            // After truncating, the next boundary must still sit on the global grid
            // and be strictly ahead of the observed time.
            Assert.True(s.NextDue > Sample.Epoch.AddSeconds(100));
            Assert.Equal(0, (s.NextDue - Sample.Epoch).Ticks % TimeSpan.FromSeconds(1).Ticks);
        }

        [Fact]
        public void Scheduler_is_a_pure_function_of_source_time_so_speed_cannot_change_it()
        {
            // The core claim of the whole tool: identical source timestamps in,
            // identical snapshot boundaries out, no matter how fast they arrive.
            var fast = new SnapshotScheduler(TimeSpan.FromSeconds(1), 100);
            var slow = new SnapshotScheduler(TimeSpan.FromSeconds(1), 100);

            var stamps = new List<DateTime>();
            for (int i = 0; i < 500; i++) stamps.Add(Sample.Epoch.AddMilliseconds(i * 37));

            var fastDue = new List<DateTime>();
            var slowDue = new List<DateTime>();

            foreach (var t in stamps) fastDue.AddRange(fast.Advance(t, out _));
            foreach (var t in stamps) { slowDue.AddRange(slow.Advance(t, out _)); Thread.Sleep(0); }

            Assert.Equal(fastDue, slowDue);
        }

        // -------------------------------------------------------------- recorder

        [Fact]
        public void Sequence_numbers_start_at_one_and_never_repeat_or_skip()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir, TimeSpan.FromHours(1)), new StaticDom(), sink);
            for (int i = 0; i < 50; i++) rec.OnTrade(Sample.Epoch.AddMilliseconds(i), i, 1m, Aggressor.Buy);
            rec.Complete();

            var lines = sink.MarketLines;
            Assert.Equal(50, lines.Count);
            for (int i = 0; i < 50; i++)
                Assert.StartsWith("{\"recorder_seq\":" + (i + 1) + ",", lines[i]);
        }

        [Fact]
        public void A_snapshot_is_written_before_the_event_that_crossed_its_boundary()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();
            var dom = new StaticDom { Book = Sample.Book() };

            var rec = new EventRecorder(Options(dir, TimeSpan.FromSeconds(1)), dom, sink);
            rec.OnTrade(Sample.Epoch.AddMilliseconds(100), 1m, 1m, Aggressor.Buy);   // anchors
            rec.OnTrade(Sample.Epoch.AddMilliseconds(1500), 2m, 1m, Aggressor.Buy);  // crosses +1s
            rec.Complete();

            Assert.Equal(3, sink.MarketLines.Count);
            Assert.Contains("\"kind\":\"trade\"", sink.MarketLines[0]);
            Assert.Contains("\"kind\":\"snapshot\"", sink.MarketLines[1]);
            Assert.Contains("\"kind\":\"trade\"", sink.MarketLines[2]);

            // The snapshot carries the boundary instant, not the arrival instant.
            Assert.Contains("\"src_ts\":\"2026-03-10T14:30:01.0000000Z\"", sink.MarketLines[1]);
        }

        [Fact]
        public void Written_order_matches_sequence_order_for_a_single_producer()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir, TimeSpan.FromMilliseconds(100)), new StaticDom { Book = Sample.Book() }, sink);
            for (int i = 0; i < 500; i++) rec.OnDepthChange(Sample.Epoch.AddMilliseconds(i), Side.Bid, 100 + i, i);
            rec.Complete();

            long prev = 0;
            foreach (var line in sink.MarketLines)
            {
                long seq = Lines.SeqOf(line);
                Assert.True(seq > prev, "sequence went backwards in the written file");
                prev = seq;
            }
        }

        [Fact]
        public void Concurrent_producers_lose_nothing_and_produce_unique_sequence_numbers()
        {
            // Sequence numbers are a total order of OBSERVATION. Under concurrency
            // the interleaving is genuinely nondeterministic, so what must hold is
            // uniqueness and completeness, not a particular order.
            using var dir = new TempDir();
            var sink = new MemorySink();

            const int threads = 8, per = 2000;
            var rec = new EventRecorder(Options(dir, TimeSpan.FromHours(1)), new StaticDom(), sink);

            Parallel.For(0, threads, t =>
            {
                for (int i = 0; i < per; i++)
                    rec.OnTrade(Sample.Epoch.AddMilliseconds(i), t, 1m, Aggressor.Buy);
            });
            rec.Complete();

            Assert.Equal(threads * per, sink.MarketLines.Count);
            Assert.Equal(0, rec.Queue.Dropped);

            var seen = new HashSet<long>();
            foreach (var line in sink.MarketLines)
            {
                long seq = Lines.SeqOf(line);
                Assert.True(seen.Add(seq), "duplicate sequence number " + seq);
            }
            Assert.Equal(threads * per, seen.Count);
        }

        [Fact]
        public void Counters_separate_trades_depth_changes_and_snapshots()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir, TimeSpan.FromSeconds(1)), new StaticDom { Book = Sample.Book() }, sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.OnDepthChange(Sample.Epoch.AddMilliseconds(10), Side.Bid, 1m, 1m);
            rec.OnDepthChange(Sample.Epoch.AddMilliseconds(20), Side.Ask, 2m, 2m);
            rec.OnTrade(Sample.Epoch.AddSeconds(2), 3m, 1m, Aggressor.Sell);
            var m = rec.Complete();

            Assert.Equal(2, m.TradesSeen);
            Assert.Equal(2, m.DepthChangesSeen);
            Assert.Equal(2, m.SnapshotsTaken); // boundaries +1s and +2s
        }

        private static RecorderOptions Options(TempDir dir, TimeSpan interval) => new RecorderOptions
        {
            OutputDirectory = dir.Path,
            SnapshotInterval = interval,
            DrainTimeout = TimeSpan.FromSeconds(30),
        };
    }
}
