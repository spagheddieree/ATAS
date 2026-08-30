using System;

namespace NQVolatility.Core
{
    /// <summary>Parsed session window: start and end wall-clock times.</summary>
    public readonly struct SessionTimes
    {
        public SessionTimes(int startHour, int startMinute, int endHour, int endMinute)
        {
            StartHour = startHour; StartMinute = startMinute;
            EndHour = endHour; EndMinute = endMinute;
        }

        public int StartHour { get; }
        public int StartMinute { get; }
        public int EndHour { get; }
        public int EndMinute { get; }

        public int StartMinutes => StartHour * 60 + StartMinute;
        public int EndMinutes => EndHour * 60 + EndMinute;

        /// <summary>True when the window wraps past local midnight.</summary>
        public bool CrossesMidnight => StartMinutes > EndMinutes;

        /// <summary>
        /// Pine <c>parseSessionString</c> (285-298). Falls back to the internal
        /// 17:00-21:00 constants when the string is not "HHMM-HHMM".
        /// </summary>
        public static SessionTimes Parse(string sessionStr)
        {
            var fallback = new SessionTimes(
                PineDefaults.FallbackStartHour, PineDefaults.FallbackStartMinute,
                PineDefaults.FallbackEndHour, PineDefaults.FallbackEndMinute);

            if (string.IsNullOrEmpty(sessionStr)) return fallback;
            var parts = sessionStr.Split('-');
            if (parts.Length != 2) return fallback;
            if (parts[0].Length < 4 || parts[1].Length < 4) return fallback;

            if (!int.TryParse(parts[0].Substring(0, 2), out var sh)) return fallback;
            if (!int.TryParse(parts[0].Substring(2, 2), out var sm)) return fallback;
            if (!int.TryParse(parts[1].Substring(0, 2), out var eh)) return fallback;
            if (!int.TryParse(parts[1].Substring(2, 2), out var em)) return fallback;

            return new SessionTimes(sh, sm, eh, em);
        }

        /// <summary>Pine <c>getSessionTimes()</c> (301-311) for a preset name.</summary>
        public static SessionTimes ForPreset(string preset)
        {
            switch (preset)
            {
                case "Asia": return Parse(PineDefaults.AsiaSession);
                case "London": return Parse(PineDefaults.LondonSession);
                case "NY AM": return Parse(PineDefaults.NyAmSession);
                case "NY PM": return Parse(PineDefaults.NyPmSession);
                default:
                    return new SessionTimes(
                        PineDefaults.FallbackStartHour, PineDefaults.FallbackStartMinute,
                        PineDefaults.FallbackEndHour, PineDefaults.FallbackEndMinute);
            }
        }

        /// <summary>The shipped configuration: rangePreset is hard-fixed to "Asia".</summary>
        public static SessionTimes Shipped => ForPreset(PineDefaults.RangePreset);
    }

    /// <summary>
    /// Session-window predicates and timestamp arithmetic, one-for-one with the
    /// Pine helper functions.
    /// </summary>
    public sealed class SessionWindow
    {
        private readonly PineTime _time;
        private readonly SessionTimes _times;
        private readonly int _lockMinutes;

        public SessionWindow(PineTime time, SessionTimes times,
            int lockHour = PineDefaults.LockHour, int lockMinute = PineDefaults.LockMinute)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _times = times;
            _lockMinutes = lockHour * 60 + lockMinute;
        }

        public SessionTimes Times => _times;
        public PineTime Time => _time;

        /// <summary>
        /// Pine <c>isInCustomRange()</c> (365-375). Bounds are INCLUSIVE at
        /// minute granularity, so 21:00:59 is still in range.
        /// </summary>
        public bool IsInCustomRange(DateTime barOpenUtc)
        {
            var cur = _time.MinuteOfDay(barOpenUtc);
            return _times.CrossesMidnight
                ? cur >= _times.StartMinutes || cur <= _times.EndMinutes
                : cur >= _times.StartMinutes && cur <= _times.EndMinutes;
        }

        /// <summary>
        /// Pine <c>isMarketActive()</c> (377-379).
        /// QUIRK Q-04: <c>dayofweek &gt;= 1 and &lt;= 7</c> is a tautology --
        /// this returns true for every day including weekends. Reproduced verbatim.
        /// </summary>
        public bool IsMarketActive(DateTime barOpenUtc)
        {
            var dow = _time.DayOfWeek(barOpenUtc);
            return dow >= 1 && dow <= 7;
        }

        /// <summary>Pine line 768: <c>inRangeNow</c>.</summary>
        public bool InRangeNow(DateTime barOpenUtc)
            => IsInCustomRange(barOpenUtc) && IsMarketActive(barOpenUtc);

        /// <summary>
        /// Pine <c>isAtOrAfterLockTime()</c> (266-271). Same-day minute-of-day test,
        /// so it is true only for 21:00-23:59 local.
        /// </summary>
        public bool IsAtOrAfterLockTime(DateTime barOpenUtc)
            => _time.MinuteOfDay(barOpenUtc) >= _lockMinutes;

        /// <summary>
        /// Pine session start for the bar's local calendar date (775):
        /// <c>timestamp(tz, year, month, dayofmonth, startHour, startMinute)</c>.
        /// </summary>
        public DateTime SessionStartForBar(DateTime barOpenUtc)
        {
            var (y, m, d) = _time.Date(barOpenUtc);
            var start = _time.Timestamp(y, m, d, _times.StartHour, _times.StartMinute);

            // Pine 787-788: dead for all shipped presets (17 > 21 is false),
            // implemented for structural fidelity.
            if (_times.StartHour > _times.EndHour && _time.Hour(barOpenUtc) < _times.StartHour)
                start = start.AddDays(-1);

            return start;
        }

        /// <summary>
        /// Pine <c>getSessionEnd</c> (338-343).
        /// NOTE Q-12: raw millisecond arithmetic, NOT timezone aware. Harmless for
        /// the 17:00-21:00 Asia preset; would misbehave across a DST transition
        /// that falls inside the window.
        /// </summary>
        public DateTime SessionEnd(DateTime sessionStartUtc)
        {
            var delta = _times.EndMinutes - _times.StartMinutes;
            if (delta <= 0) delta += 1440;
            return sessionStartUtc.AddMinutes(delta);
        }

        /// <summary>
        /// Pine <c>getNextSessionStartTime</c> (314-336). Weekend skipping applies
        /// here and ONLY here -- never to session creation (see Q-04).
        /// </summary>
        public DateTime NextSessionStart(DateTime currentEndUtc)
        {
            var (endY, endM, endD) = _time.Date(currentEndUtc);
            var endHourTz = _time.Hour(currentEndUtc);

            // Pine 322-323: dead for shipped presets.
            if (_times.StartHour > _times.EndHour && endHourTz < _times.StartHour)
                return _time.Timestamp(endY, endM, endD, _times.StartHour, _times.StartMinute);

            var nextDay = currentEndUtc.AddDays(1);
            var dow = _time.DayOfWeek(nextDay);
            if (dow == PineTime.Saturday) nextDay = nextDay.AddDays(2);
            else if (dow == PineTime.Sunday) nextDay = nextDay.AddDays(1);

            var (ny, nm, nd) = _time.Date(nextDay);
            return _time.Timestamp(ny, nm, nd, _times.StartHour, _times.StartMinute);
        }
    }
}
