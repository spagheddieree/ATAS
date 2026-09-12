using System;
using System.Collections.Generic;

namespace ReplayEventVerifier.Core
{
    /// <summary>
    /// Tracks the latest source timestamp seen and reports regressions.
    /// </summary>
    /// <remarks>
    /// Deliberately does NOT repair out-of-order timestamps. A replay that emits
    /// events out of source order is exactly the defect this whole tool exists to
    /// detect; clamping the value would erase the finding and produce a file that
    /// looks clean. The event keeps its original stamp and a
    /// <see cref="FaultCode.SourceTimeRegression"/> is recorded.
    /// </remarks>
    public sealed class SourceClock
    {
        private DateTime _high = DateTime.MinValue;
        private bool _started;

        public bool Started { get { return _started; } }

        /// <summary>Highest source timestamp observed so far.</summary>
        public DateTime High { get { return _high; } }

        /// <summary>First source timestamp observed.</summary>
        public DateTime First { get; private set; }

        /// <summary>
        /// Observes a source timestamp. Returns true when it regressed (was
        /// strictly earlier than the previous high).
        /// </summary>
        public bool Observe(DateTime sourceUtc)
        {
            if (!_started)
            {
                _started = true;
                First = sourceUtc;
                _high = sourceUtc;
                return false;
            }

            if (sourceUtc < _high) return true;
            _high = sourceUtc;
            return false;
        }
    }

    /// <summary>
    /// Decides when a DOM snapshot is due, using source time only.
    /// </summary>
    /// <remarks>
    /// This is the single most important class for the objective. Snapshot
    /// boundaries are computed from the feed's own timestamps, anchored to the
    /// first event floored to the interval, so the boundary instants are a pure
    /// function of the event stream. A 1x replay and a 10x replay therefore
    /// schedule snapshots at <em>identical source instants</em>, and the two
    /// captures can be compared line for line. Scheduling on wall clock would make
    /// the accelerated run take roughly a tenth as many snapshots at unrelated
    /// points, and no meaningful comparison would be possible at all.
    /// </remarks>
    public sealed class SnapshotScheduler
    {
        private readonly long _intervalTicks;
        private readonly int _maxCatchUp;
        private long _nextDueTicks;
        private bool _anchored;

        /// <param name="interval">Snapshot interval in source time. Must be positive.</param>
        /// <param name="maxCatchUp">
        /// Upper bound on snapshots emitted for one observed gap. A replay that
        /// jumps over a session break would otherwise emit thousands of identical
        /// snapshots; the excess is dropped and reported as a fault instead.
        /// </param>
        public SnapshotScheduler(TimeSpan interval, int maxCatchUp)
        {
            if (interval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("interval", "Snapshot interval must be positive.");
            if (maxCatchUp < 1)
                throw new ArgumentOutOfRangeException("maxCatchUp", "Catch-up cap must be at least 1.");

            _intervalTicks = interval.Ticks;
            _maxCatchUp = maxCatchUp;
        }

        /// <summary>Source instant of the next snapshot, valid once anchored.</summary>
        public DateTime NextDue { get { return new DateTime(_nextDueTicks, DateTimeKind.Utc); } }

        public bool Anchored { get { return _anchored; } }

        /// <summary>
        /// Advances the scheduler to <paramref name="sourceUtc"/> and returns every
        /// snapshot boundary that has now been reached, in ascending order.
        /// </summary>
        /// <param name="truncated">
        /// Set when the catch-up cap suppressed further boundaries for this call.
        /// </param>
        public List<DateTime> Advance(DateTime sourceUtc, out bool truncated)
        {
            truncated = false;
            var due = new List<DateTime>();

            long t = sourceUtc.Ticks;

            if (!_anchored)
            {
                // Anchor on an interval boundary rather than on the first event, so
                // that two runs whose first event differs by a few microseconds still
                // agree on every subsequent boundary.
                long floored = t - (t % _intervalTicks);
                _nextDueTicks = floored + _intervalTicks;
                _anchored = true;
                return due;
            }

            while (t >= _nextDueTicks)
            {
                if (due.Count == _maxCatchUp)
                {
                    // Skip ahead to the first boundary at or after the current time so
                    // the scheduler stays aligned to the global grid after truncating.
                    long behind = t - _nextDueTicks;
                    _nextDueTicks += ((behind / _intervalTicks) + 1) * _intervalTicks;
                    truncated = true;
                    break;
                }

                due.Add(new DateTime(_nextDueTicks, DateTimeKind.Utc));
                _nextDueTicks += _intervalTicks;
            }

            return due;
        }
    }
}
