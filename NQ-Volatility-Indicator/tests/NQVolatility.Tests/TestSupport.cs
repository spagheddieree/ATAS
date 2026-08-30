using System;
using System.Collections.Generic;
using NQVolatility.Core;

namespace NQVolatility.Tests
{
    internal static class Tz
    {
        public static readonly TimeZoneInfo LosAngeles = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        public static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;
        public static readonly TimeZoneInfo London = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        public static readonly TimeZoneInfo Tokyo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
    }

    internal static class Build
    {
        public static SessionWindow Shipped(TimeZoneInfo? tz = null)
            => new SessionWindow(new PineTime(tz ?? Tz.LosAngeles), SessionTimes.Shipped);

        public static SessionWindow Custom(SessionTimes times, TimeZoneInfo? tz = null)
            => new SessionWindow(new PineTime(tz ?? Tz.LosAngeles), times);

        /// <summary>A UTC instant from a local wall-clock time in <paramref name="tz"/>.</summary>
        public static DateTime Utc(TimeZoneInfo tz, int y, int mo, int d, int h, int mi = 0)
            => new PineTime(tz).Timestamp(y, mo, d, h, mi);

        /// <summary>
        /// Hourly bars spanning [startLocalDate 00:00, endLocalDate 23:00] inclusive,
        /// in the given zone. OHLC comes from <paramref name="ohlc"/> keyed by local hour.
        /// </summary>
        public static List<Ohlc> HourlyBars(
            TimeZoneInfo tz, DateTime startLocalDate, int days,
            Func<DateTime, (double o, double h, double l, double c)> ohlc)
        {
            var pt = new PineTime(tz);
            var bars = new List<Ohlc>();
            for (var day = 0; day < days; day++)
            {
                var date = startLocalDate.AddDays(day);
                for (var h = 0; h < 24; h++)
                {
                    var utc = pt.Timestamp(date.Year, date.Month, date.Day, h, 0);
                    var local = pt.Local(utc);
                    var v = ohlc(local);
                    bars.Add(new Ohlc(utc, v.o, v.h, v.l, v.c));
                }
            }
            bars.Sort((a, b) => a.OpenTimeUtc.CompareTo(b.OpenTimeUtc));
            return bars;
        }

        /// <summary>Flat bars except inside 17:00-21:00, where a known range is produced.</summary>
        public static (double o, double h, double l, double c) AsiaRange(DateTime local)
        {
            if (local.Hour >= 17 && local.Hour <= 21)
            {
                // High 20100 at 18:00, low 19900 at 20:00.
                var h = local.Hour == 18 ? 20100.0 : 20000.0;
                var l = local.Hour == 20 ? 19900.0 : 20000.0;
                return (20000, h, l, 20000);
            }
            // Outside the window: deliberately extreme, to prove it is never folded in.
            return (30000, 99999, 1, 30000);
        }
    }
}
