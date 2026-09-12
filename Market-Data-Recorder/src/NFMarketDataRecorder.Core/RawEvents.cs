using System;

namespace NFMarketDataRecorder.Core
{
    /// <summary>One price/volume pair in a depth snapshot.</summary>
    public struct DomLevel
    {
        public readonly decimal Price;
        public readonly decimal Volume;

        public DomLevel(decimal price, decimal volume)
        {
            Price = price;
            Volume = volume;
        }
    }

    /// <summary>
    /// A single captured record. This is deliberately one struct-like class with a
    /// discriminant rather than a class hierarchy: the queue is a hot, allocation
    /// sensitive path on a market-data callback thread, and a flat shape keeps the
    /// writer's serialization branch-free and its output field order fixed.
    /// </summary>
    public sealed class RawEvent
    {
        /// <summary>Capture-order sequence number. Total order of OBSERVATION, not of the exchange.</summary>
        public long Seq;

        /// <summary>One of <see cref="EventKind"/>.</summary>
        public string Kind;

        /// <summary>
        /// The platform's own timestamp for this event, in UTC. This is the only
        /// clock any scheduling or comparison decision is ever made on.
        /// </summary>
        public DateTime SourceUtc;

        /// <summary>
        /// Wall clock at the instant of capture, UTC. DIAGNOSTIC ONLY. It is written
        /// to disk but excluded from the canonical form, because it is exactly the
        /// field that must differ between a 1x and an accelerated replay.
        /// </summary>
        public DateTime ReceivedUtc;

        // --- trade / depth payload -------------------------------------------------
        public decimal Price;
        public decimal Volume;

        /// <summary>Trade only: one of <see cref="Aggressor"/>.</summary>
        public string AggressorSide;

        /// <summary>Depth only: one of <see cref="Side"/>.</summary>
        public string BookSide;

        // --- additional raw fields the platform actually carries -------------
        //
        // Measured on the real MarketDataArg. They are optional because their
        // presence is per-event, and a null is written as an ABSENT key rather
        // than a null value: the file then states what arrived, and never implies
        // a zero the feed did not send.

        /// <summary>Platform's OriginPrice. Semantics relative to Price are UNVERIFIED.</summary>
        public decimal? OriginPrice;

        /// <summary>Platform's OpenInterest as carried on the event.</summary>
        public decimal? OpenInterest;

        /// <summary>
        /// Exchange ORDER identifier. Not a sequence number and never used for ordering.
        /// </summary>
        public long? ExchangeOrderId;

        /// <summary>Exchange order identifier of the aggressing side, where supplied.</summary>
        public long? AggressorExchangeOrderId;

        // --- snapshot payload ------------------------------------------------------
        /// <summary>Snapshot only: bid levels, best first, as returned by the platform API.</summary>
        public DomLevel[] Bids;

        /// <summary>Snapshot only: ask levels, best first, as returned by the platform API.</summary>
        public DomLevel[] Asks;

        /// <summary>Snapshot only: the configured max levels per side that was applied.</summary>
        public int DepthLimit;

        public static RawEvent Trade(long seq, DateTime sourceUtc, DateTime receivedUtc,
                                     decimal price, decimal volume, string aggressor)
        {
            return new RawEvent
            {
                Seq = seq,
                Kind = EventKind.Trade,
                SourceUtc = sourceUtc,
                ReceivedUtc = receivedUtc,
                Price = price,
                Volume = volume,
                AggressorSide = aggressor ?? Aggressor.Unknown,
            };
        }

        /// <summary>
        /// A trade carrying the optional raw fields the platform supplied.
        /// </summary>
        /// <remarks>
        /// Each optional argument is written only when it has a value. A field the
        /// platform did not supply is absent from the line rather than written as
        /// zero or null, so the capture distinguishes "not sent" from "sent as
        /// zero" -- a distinction that cannot be recovered later.
        /// </remarks>
        public static RawEvent Trade(long seq, DateTime sourceUtc, DateTime receivedUtc,
                                     decimal price, decimal volume, string aggressor,
                                     decimal? originPrice, decimal? openInterest,
                                     long? exchangeOrderId, long? aggressorExchangeOrderId)
        {
            var e = Trade(seq, sourceUtc, receivedUtc, price, volume, aggressor);
            e.OriginPrice = originPrice;
            e.OpenInterest = openInterest;
            e.ExchangeOrderId = exchangeOrderId;
            e.AggressorExchangeOrderId = aggressorExchangeOrderId;
            return e;
        }

        public static RawEvent Depth(long seq, DateTime sourceUtc, DateTime receivedUtc,
                                     string side, decimal price, decimal volume)
        {
            return new RawEvent
            {
                Seq = seq,
                Kind = EventKind.Depth,
                SourceUtc = sourceUtc,
                ReceivedUtc = receivedUtc,
                BookSide = side,
                Price = price,
                Volume = volume,
            };
        }

        /// <summary>A depth change carrying the optional raw fields the platform supplied.</summary>
        public static RawEvent Depth(long seq, DateTime sourceUtc, DateTime receivedUtc,
                                     string side, decimal price, decimal volume,
                                     long? exchangeOrderId)
        {
            var e = Depth(seq, sourceUtc, receivedUtc, side, price, volume);
            e.ExchangeOrderId = exchangeOrderId;
            return e;
        }

        public static RawEvent Snapshot(long seq, DateTime sourceUtc, DateTime receivedUtc,
                                        DomLevel[] bids, DomLevel[] asks, int depthLimit)
        {
            return new RawEvent
            {
                Seq = seq,
                Kind = EventKind.Snapshot,
                SourceUtc = sourceUtc,
                ReceivedUtc = receivedUtc,
                Bids = bids ?? new DomLevel[0],
                Asks = asks ?? new DomLevel[0],
                DepthLimit = depthLimit,
            };
        }
    }

    /// <summary>
    /// An order book as read from the platform's depth API. The recorder never
    /// builds one of these from depth changes; doing so would make the snapshot a
    /// derived feature instead of an independent observation, and the whole point
    /// of taking snapshots is to check the change stream against the platform.
    /// </summary>
    public sealed class DomBook
    {
        public readonly DomLevel[] Bids;
        public readonly DomLevel[] Asks;

        public DomBook(DomLevel[] bids, DomLevel[] asks)
        {
            Bids = bids ?? new DomLevel[0];
            Asks = asks ?? new DomLevel[0];
        }

        public static readonly DomBook Empty = new DomBook(new DomLevel[0], new DomLevel[0]);
    }
}
