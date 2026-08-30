using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using NQVolatility.Core;
using Xunit;

namespace NQVolatility.Tests
{
    /// <summary>
    /// Runs the platform-independent parity fixtures. Expected values in the JSON
    /// are hand-derived from PINE-PARITY-SPEC, so this is an independent check of
    /// the implementation rather than a snapshot of it.
    /// </summary>
    public class FixtureTests
    {
        private const double Tol = 1e-6;

        public static IEnumerable<object[]> FixtureFiles()
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "fixtures");
            foreach (var f in Directory.GetFiles(dir, "*.json").OrderBy(x => x))
                yield return new object[] { Path.GetFileName(f) };
        }

        [Fact]
        public void AllFixturesAreDiscovered()
        {
            Assert.Equal(5, FixtureFiles().Count());
        }

        [Theory]
        [MemberData(nameof(FixtureFiles))]
        public void Fixture_MatchesExpectedOutput(string fileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "fixtures", fileName);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            var input = root.GetProperty("input");
            var expected = root.GetProperty("expected");

            // ---- build the engine from the fixture input ---------------------
            var tz = TimeZoneInfo.FindSystemTimeZoneById(input.GetProperty("timeZone").GetString()!);
            var times = ParseTimes(input.GetProperty("sessionStart").GetString()!,
                                   input.GetProperty("sessionEnd").GetString()!);
            var window = new SessionWindow(new PineTime(tz), times);
            var engine = new SessionEngine(window, input.GetProperty("lookbackPeriod").GetInt32());
            engine.SeedPreviousDistances(
                Nullable(input.GetProperty("previousUpDistance")),
                Nullable(input.GetProperty("previousDownDistance")));

            var atr = Nullable(input.GetProperty("yesterdayAtr"));

            foreach (var b in input.GetProperty("bars").EnumerateArray())
            {
                var t = DateTimeOffset.Parse(b.GetProperty("t").GetString()!,
                    CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).UtcDateTime;
                engine.OnBar(new Ohlc(t,
                    b.GetProperty("o").GetDouble(),
                    b.GetProperty("h").GetDouble(),
                    b.GetProperty("l").GetDouble(),
                    b.GetProperty("c").GetDouble()), atr);
            }

            // ---- compare -----------------------------------------------------
            Assert.Equal(expected.GetProperty("sessionCount").GetInt32(), engine.Sessions.Count);

            var expectedSessions = expected.GetProperty("sessions").EnumerateArray().ToList();
            for (var i = 0; i < expectedSessions.Count; i++)
            {
                var e = expectedSessions[i];
                var s = engine.Sessions[i];
                var where = $"{fileName}[{i}]";

                Assert.Equal(Utc(e, "sessionStartUtc"), s.StartTime);
                Assert.Equal(Utc(e, "sessionEndUtc"), s.EndTime);
                Assert.Equal(Utc(e, "lockTimeUtc"), s.LockTime);
                Assert.Equal(e.GetProperty("projectionsLocked").GetBoolean(), s.ProjectionsLocked);
                Assert.Equal(e.GetProperty("sessionComplete").GetBoolean(), s.SessionComplete);
                Assert.Equal(e.GetProperty("accumulatedBars").GetInt32(), s.AccumulatedBars);
                Assert.Equal(Utc(e, "drawFromUtc"), s.DrawFrom);
                Assert.Equal(Utc(e, "drawToUtc"), s.DrawTo);

                Near(e, "finalHigh", s.FinalHigh, where);
                Near(e, "finalLow", s.FinalLow, where);
                Near(e, "atr", s.AtrValue, where);
                Near(e, "scaleFactor", s.ScaleFactor, where);
                Near(e, "lockedUpDistance", s.LockedUpDistance, where);
                Near(e, "lockedDownDistance", s.LockedDownDistance, where);
                Near(e, "skewRatio", s.SkewRatio, where);

                var levels = e.GetProperty("levels");
                Assert.NotNull(s.Levels);
                Near(levels, "high", s.Levels!.Value.High, where);
                Near(levels, "low", s.Levels!.Value.Low, where);
                Near(levels, "q1", s.Levels!.Value.Q1Price, where);
                Near(levels, "eq", s.Levels!.Value.EqPrice, where);
                Near(levels, "q3", s.Levels!.Value.Q3Price, where);

                var up = e.GetProperty("up");
                if (up.ValueKind == JsonValueKind.Null)
                {
                    Assert.Null(s.Projections);
                    Assert.Null(s.Visibility);
                    Assert.Equal(JsonValueKind.Null, e.GetProperty("down").ValueKind);
                    Assert.Equal(JsonValueKind.Null, e.GetProperty("visibility").ValueKind);
                    continue;
                }

                Assert.NotNull(s.Projections);
                var p = s.Projections!.Value;
                Near(up, "m200", p.Up200, where); Near(up, "m233", p.Up233, where);
                Near(up, "m250", p.Up250, where); Near(up, "m300", p.Up300, where);
                Near(up, "m400", p.Up400, where); Near(up, "m450", p.Up450, where);
                Near(up, "m600", p.Up600, where); Near(up, "m650", p.Up650, where);
                Near(up, "m778", p.Up800, where); Near(up, "m850", p.Up850, where);

                var down = e.GetProperty("down");
                Near(down, "m200", p.Down200, where); Near(down, "m233", p.Down233, where);
                Near(down, "m250", p.Down250, where); Near(down, "m300", p.Down300, where);
                Near(down, "m400", p.Down400, where); Near(down, "m450", p.Down450, where);
                Near(down, "m600", p.Down600, where); Near(down, "m650", p.Down650, where);
                Near(down, "m778", p.Down800, where); Near(down, "m850", p.Down850, where);

                var vis = e.GetProperty("visibility");
                Assert.NotNull(s.Visibility);
                var v = s.Visibility!.Value;
                Assert.Equal(vis.GetProperty("upLevel1").GetBoolean(), v.ShowUpLevel1);
                Assert.Equal(vis.GetProperty("upLine300").GetBoolean(), v.ShowUpLine300);
                Assert.Equal(vis.GetProperty("upAvr").GetBoolean(), v.ShowUpAvr);
                Assert.Equal(vis.GetProperty("upAvrPlus").GetBoolean(), v.ShowUpAvrPlus);
                Assert.Equal(vis.GetProperty("upMax").GetBoolean(), v.ShowUpMax);
                Assert.Equal(vis.GetProperty("downLevel1").GetBoolean(), v.ShowDownLevel1);
                Assert.Equal(vis.GetProperty("downLine300").GetBoolean(), v.ShowDownLine300);
                Assert.Equal(vis.GetProperty("downAvr").GetBoolean(), v.ShowDownAvr);
                Assert.Equal(vis.GetProperty("downAvrPlus").GetBoolean(), v.ShowDownAvrPlus);
                Assert.Equal(vis.GetProperty("downMax").GetBoolean(), v.ShowDownMax);
            }
        }

        private static SessionTimes ParseTimes(string start, string end)
        {
            var s = start.Split(':');
            var e = end.Split(':');
            return new SessionTimes(int.Parse(s[0]), int.Parse(s[1]), int.Parse(e[0]), int.Parse(e[1]));
        }

        private static double? Nullable(JsonElement el)
            => el.ValueKind == JsonValueKind.Null ? (double?)null : el.GetDouble();

        private static DateTime? Utc(JsonElement e, string name)
        {
            var el = e.GetProperty(name);
            if (el.ValueKind == JsonValueKind.Null) return null;
            return DateTime.SpecifyKind(
                DateTime.Parse(el.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                DateTimeKind.Utc);
        }

        private static void Near(JsonElement e, string name, double? actual, string where)
        {
            var el = e.GetProperty(name);
            if (el.ValueKind == JsonValueKind.Null)
            {
                Assert.True(actual is null, $"{where}.{name}: expected na, got {actual}");
                return;
            }
            Assert.True(actual.HasValue, $"{where}.{name}: expected {el.GetDouble()}, got na");
            Assert.True(Math.Abs(el.GetDouble() - actual.Value) < Tol,
                $"{where}.{name}: expected {el.GetDouble()}, got {actual.Value}");
        }
    }
}
