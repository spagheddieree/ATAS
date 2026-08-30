using System;

namespace NQVolatility.Core
{
    /// <summary>
    /// Named-timezone wall-clock helpers matching Pine's <c>hour()</c>, <c>minute()</c>,
    /// <c>dayofweek()</c>, <c>year/month/dayofmonth()</c> and <c>timestamp()</c>.
    /// Fixed UTC offsets are deliberately NOT used (PINE-PARITY-SPEC 3).
    /// </summary>
    public sealed class PineTime
    {
        private readonly TimeZoneInfo _tz;

        public PineTime(TimeZoneInfo tz)
        {
            _tz = tz ?? throw new ArgumentNullException(nameof(tz));
        }

        public TimeZoneInfo Zone => _tz;

        /// <summary>Wall-clock local time for a UTC instant.</summary>
        public DateTime Local(DateTime utc)
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _tz);

        public int Hour(DateTime utc) => Local(utc).Hour;
        public int Minute(DateTime utc) => Local(utc).Minute;

        /// <summary>Minutes since local midnight -- Pine's <c>hour*60 + minute</c>.</summary>
        public int MinuteOfDay(DateTime utc)
        {
            var l = Local(utc);
            return l.Hour * 60 + l.Minute;
        }

        /// <summary>Pine <c>dayofweek</c>: Sunday = 1 ... Saturday = 7.</summary>
        public int DayOfWeek(DateTime utc) => (int)Local(utc).DayOfWeek + 1;

        public const int Sunday = 1;
        public const int Saturday = 7;

        /// <summary>
        /// Pine <c>timestamp(tz, year, month, day, hour, minute)</c> -&gt; UTC instant.
        /// <para>
        /// DST edge handling (not reachable for the shipped 17:00-21:00 Asia preset,
        /// where the US transition at 02:00 lies outside the window):
        /// an invalid local time is shifted forward by the transition delta;
        /// an ambiguous local time resolves to the earlier (pre-transition) instant.
        /// </para>
        /// </summary>
        public DateTime Timestamp(int year, int month, int day, int hour, int minute)
        {
            var local = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified)
                .AddHours(hour).AddMinutes(minute);

            if (_tz.IsInvalidTime(local))
            {
                // Spring-forward gap: advance until the local time exists.
                var probe = local;
                for (var i = 0; i < 24 && _tz.IsInvalidTime(probe); i++)
                    probe = probe.AddMinutes(15);
                local = probe;
            }

            if (_tz.IsAmbiguousTime(local))
            {
                // Fall-back overlap: take the earlier (still-DST) offset.
                var offsets = _tz.GetAmbiguousTimeOffsets(local);
                var chosen = offsets[0];
                foreach (var o in offsets) if (o > chosen) chosen = o;
                return DateTime.SpecifyKind(local - chosen, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTimeToUtc(local, _tz);
        }

        /// <summary>Local calendar date components of a UTC instant.</summary>
        public (int Year, int Month, int Day) Date(DateTime utc)
        {
            var l = Local(utc);
            return (l.Year, l.Month, l.Day);
        }
    }
}
