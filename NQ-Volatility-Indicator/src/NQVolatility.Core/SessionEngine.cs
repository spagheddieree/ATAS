using System;
using System.Collections.Generic;

namespace NQVolatility.Core
{
    /// <summary>What the information table displays (Pine 928-977).</summary>
    public readonly struct InfoTableData
    {
        public InfoTableData(double? atr, double? up, double? down)
        {
            Atr = atr; UpProjection = up; DownProjection = down;
        }

        public double? Atr { get; }
        public double? UpProjection { get; }
        public double? DownProjection { get; }

        /// <summary>Pine <c>str.tostring(v, "#.##")</c> with "N/A" for na.</summary>
        public static string Format(double? v)
            => v.HasValue ? v.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) : "N/A";
    }

    /// <summary>
    /// Counters proving the dead Pine branches never execute. Tests assert these
    /// stay at zero on the shipped configuration (PINE-PARITY-SPEC 15, 19).
    /// </summary>
    public sealed class DeadPathCounters
    {
        public int UpdateZonesAndLinesFromNewSession { get; internal set; }
        public int UpdateZonesAndLinesFromExtension { get; internal set; }
        public int CrossMidnightStartAdjustment { get; internal set; }
        public int SentinelLeak { get; internal set; }

        public int Total => UpdateZonesAndLinesFromNewSession
                          + UpdateZonesAndLinesFromExtension
                          + CrossMidnightStartAdjustment
                          + SentinelLeak;
    }

    /// <summary>
    /// The deterministic bar-by-bar state machine, reproducing the Pine main
    /// logic (765-924) in source order. Platform independent: no ATAS types.
    /// </summary>
    public sealed class SessionEngine
    {
        private readonly SessionWindow _window;
        private readonly int _lookbackPeriod;
        private readonly List<RangeSession> _sessions = new List<RangeSession>();
        private readonly List<RangeSession> _evicted = new List<RangeSession>();

        private RangeSession? _current;
        private double? _prevUpDistance;   // Pine var, line 215
        private double? _prevDownDistance; // Pine var, line 216

        public SessionEngine(SessionWindow window, int lookbackPeriod = PineDefaults.LookbackPeriod)
        {
            if (lookbackPeriod < 1 || lookbackPeriod > 10)
                throw new ArgumentOutOfRangeException(nameof(lookbackPeriod), "Pine minval=1 maxval=10 (line 143)");
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _lookbackPeriod = lookbackPeriod;
        }

        /// <summary>
        /// Seed the carry-forward distances (Pine vars prevUpDistance / prevDownDistance,
        /// lines 215-216) before the first bar. Used by parity fixtures to start
        /// mid-history; must be called before <see cref="OnBar"/>.
        /// </summary>
        public void SeedPreviousDistances(double? up, double? down)
        {
            if (_sessions.Count > 0)
                throw new InvalidOperationException("Seed the previous distances before processing any bar.");
            _prevUpDistance = up;
            _prevDownDistance = down;
        }

        public IReadOnlyList<RangeSession> Sessions => _sessions;

        /// <summary>Sessions removed by the retention cap, in eviction order.</summary>
        public IReadOnlyList<RangeSession> EvictedSessions => _evicted;

        public RangeSession? CurrentSession => _current;
        public double? PrevUpDistance => _prevUpDistance;
        public double? PrevDownDistance => _prevDownDistance;
        public DeadPathCounters DeadPaths { get; } = new DeadPathCounters();

        /// <summary>
        /// Process one chart bar.
        /// </summary>
        /// <param name="bar">The bar; its OpenTimeUtc is what every wall-clock test uses.</param>
        /// <param name="yesterdayAtr">Pine <c>yesterdayATR</c> == <c>atrDaily[1]</c> for this bar.</param>
        public void OnBar(Ohlc bar, double? yesterdayAtr)
        {
            var t = bar.OpenTimeUtc;

            // Pine 768-770
            var inRangeNow = _window.InRangeNow(t);
            var atOrAfterLock = _window.IsAtOrAfterLockTime(t);
            var newSessionCondition = inRangeNow && (_current is null || _current.SessionComplete);

            // ---- Pine 772-792: create a new session -------------------------
            if (newSessionCondition)
            {
                var newStartTime = _window.SessionStartForBar(t);

                // Pine 778-782: finalise the previous session to the new start.
                // UNREACHABLE (Q-02): sessionComplete implies zonesFinalizedToNextSession.
                if (_sessions.Count > 0)
                {
                    var prev = _sessions[_sessions.Count - 1];
                    if (prev.SessionComplete && !prev.ZonesFinalizedToNextSession)
                        DeadPaths.UpdateZonesAndLinesFromNewSession++;
                }

                var endTime = _window.SessionEnd(newStartTime);
                _current = new RangeSession(newStartTime, endTime);
                _sessions.Add(_current);
                CleanupOldSessions();
            }

            // ---- Pine 795-811: accumulate ------------------------------------
            if (_current is not null && !_current.ProjectionsLocked)
            {
                var sessionEndTime = _window.SessionEnd(_current.StartTime);
                if (t <= sessionEndTime && inRangeNow)
                    _current.Accumulate(bar);
            }

            // ---- Pine 815-863: lock ------------------------------------------
            if (_current is not null && !_current.ProjectionsLocked && atOrAfterLock && !inRangeNow)
            {
                if (_current.AccumulatedBars == 0)
                    DeadPaths.SentinelLeak++; // Q-10: finalHigh=0 / finalLow=999999 would leak

                var projection = AutoProjection.Calculate(yesterdayAtr, _prevUpDistance, _prevDownDistance);

                var sessionEndTime = _window.SessionEnd(_current.StartTime);
                var nextSessionStart = _window.NextSessionStart(sessionEndTime);

                _current.Lock(
                    lockTime: t,
                    projection: projection,
                    atr: yesterdayAtr,
                    drawFrom: _current.EndTime,
                    drawTo: nextSessionStart);

                // Pine 832-833: carry forward for the next session's clamp.
                _prevUpDistance = projection.UpDistance;
                _prevDownDistance = projection.DownDistance;
            }

            // ---- Pine 867-873: extend completed zones to the live bar ---------
            // UNREACHABLE (Q-02).
            if (_sessions.Count > 0 && !inRangeNow)
            {
                for (var i = _sessions.Count - 1; i >= 0; i--)
                {
                    var s = _sessions[i];
                    if (s.SessionComplete && !s.ZonesFinalizedToNextSession)
                    {
                        DeadPaths.UpdateZonesAndLinesFromExtension++;
                        break;
                    }
                }
            }

            // Pine 877-924 (zone-label re-centring) is idempotent given that the
            // boxes never move, so it has no effect on state. It is a pure
            // rendering concern and lives in the adapter.
        }

        /// <summary>Pine <c>cleanupOldSessions()</c> (760-763).</summary>
        private void CleanupOldSessions()
        {
            while (_sessions.Count > _lookbackPeriod)
            {
                var oldest = _sessions[0];
                _sessions.RemoveAt(0);
                _evicted.Add(oldest);
            }
        }

        /// <summary>
        /// Pine 934-941: newest-to-oldest scan for the first session with locked
        /// projections.
        /// <para>
        /// NOTE Q-11: Pine's display variables are <c>var</c> and are never reset,
        /// so once populated the table can never revert to N/A. Reproduced by
        /// caching the last non-empty result.
        /// </para>
        /// </summary>
        public InfoTableData GetInfoTableData()
        {
            for (var i = _sessions.Count - 1; i >= 0; i--)
            {
                var s = _sessions[i];
                if (s.ProjectionsLocked)
                {
                    _lastTable = new InfoTableData(s.AtrValue, s.LockedUpDistance, s.LockedDownDistance);
                    return _lastTable;
                }
            }
            return _lastTable;
        }

        private InfoTableData _lastTable = new InfoTableData(null, null, null);
    }
}
