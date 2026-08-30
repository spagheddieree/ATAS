using System;

namespace NQVolatility.Core
{
    /// <summary>
    /// Pine <c>type RangeSession</c> (152-207).
    /// <para>
    /// Mutable while accumulating. Once <see cref="ProjectionsLocked"/> is set,
    /// FinalHigh, FinalLow, LockedUpDistance and LockedDownDistance are frozen --
    /// any further write throws. This mirrors the Pine guarantee, which holds
    /// structurally there because every mutating block is gated on
    /// <c>not projectionsLocked</c> (PINE-PARITY-SPEC 5.4).
    /// </para>
    /// </summary>
    public sealed class RangeSession
    {
        /// <summary>Pine line 156: <c>float highPrice = 0.0</c>. Sentinel seed, quirk Q-10.</summary>
        public const double PineHighSeed = 0.0;

        /// <summary>Pine line 157: <c>float lowPrice = 999999.0</c>. Sentinel seed, quirk Q-10.</summary>
        public const double PineLowSeed = 999999.0;

        public RangeSession(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
            HighPrice = PineHighSeed;
            LowPrice = PineLowSeed;
        }

        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public DateTime? LockTime { get; private set; }

        /// <summary>Running high while accumulating (Pine <c>highPrice</c>).</summary>
        public double HighPrice { get; private set; }

        /// <summary>Running low while accumulating (Pine <c>lowPrice</c>).</summary>
        public double LowPrice { get; private set; }

        /// <summary>How many bars were folded in. Zero means the sentinels leak (Q-10).</summary>
        public int AccumulatedBars { get; private set; }

        public double? FinalHigh { get; private set; }
        public double? FinalLow { get; private set; }

        public bool SessionComplete { get; private set; }
        public bool ProjectionsLocked { get; private set; }
        public bool ZonesFinalizedToNextSession { get; private set; }

        public double? LockedUpDistance { get; private set; }
        public double? LockedDownDistance { get; private set; }
        public double? AtrValue { get; private set; }
        public double? ScaleFactor { get; private set; }
        public double? SkewRatio { get; private set; }

        /// <summary>
        /// Fixed drawn extent for lines/zones: [EndTime, NextSessionStart].
        /// Never widened -- Pine's updateZonesAndLines is unreachable (Q-02).
        /// </summary>
        public DateTime? DrawFrom { get; private set; }
        public DateTime? DrawTo { get; private set; }

        public RangeLevels? Levels { get; private set; }
        public ProjectionLevels? Projections { get; private set; }
        public ZoneVisibility? Visibility { get; private set; }

        /// <summary>Range box right edge while accumulating (tracks bar open time).</summary>
        public DateTime? BoxRight { get; private set; }

        /// <summary>Pine 801-802.</summary>
        internal void Accumulate(Ohlc bar)
        {
            if (ProjectionsLocked)
                throw new InvalidOperationException("Session is locked; running high/low is immutable.");

            HighPrice = Math.Max(HighPrice, bar.High);
            LowPrice = Math.Min(LowPrice, bar.Low);
            BoxRight = bar.OpenTimeUtc;
            AccumulatedBars++;
        }

        /// <summary>Pine 817-863. Freezes the session.</summary>
        internal void Lock(
            DateTime lockTime,
            AutoProjectionResult projection,
            double? atr,
            DateTime drawFrom,
            DateTime drawTo)
        {
            if (ProjectionsLocked)
                throw new InvalidOperationException("Session is already locked; projections are immutable.");

            FinalHigh = HighPrice;
            FinalLow = LowPrice;
            LockTime = lockTime;

            LockedUpDistance = projection.UpDistance;
            LockedDownDistance = projection.DownDistance;
            AtrValue = atr;
            ScaleFactor = projection.ScaleFactor;
            SkewRatio = projection.SkewRatio;

            ProjectionsLocked = true;

            // Pine 847-849: the box right edge snaps back to the nominal session end.
            BoxRight = EndTime;

            DrawFrom = drawFrom;
            DrawTo = drawTo;

            Levels = new RangeLevels(FinalHigh.Value, FinalLow.Value);

            // Pine 457: zones exist only when both distances are non-na.
            if (projection.IsAvailable)
            {
                var p = new ProjectionLevels(
                    FinalHigh.Value, FinalLow.Value,
                    projection.UpDistance!.Value, projection.DownDistance!.Value);
                Projections = p;
                Visibility = ZoneEligibility.Evaluate(p, FinalHigh.Value, FinalLow.Value);
            }

            SessionComplete = true;
            ZonesFinalizedToNextSession = true; // Pine 863 -- what makes Q-02 dead
        }
    }
}
