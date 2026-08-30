using System;
using System.Collections.Generic;
using System.Linq;
using NQVolatility.Core;
using Xunit;

namespace NQVolatility.Tests
{
    /// <summary>Required cases 32-33, 37-38 plus lifecycle and quirk regressions.</summary>
    public class SessionEngineTests
    {
        private const double Atr = 300.0; // f = 1.0 -> up 45 / down 48

        private static SessionEngine RunDays(int days, int lookback = PineDefaults.LookbackPeriod,
            double? atr = Atr, DateTime? start = null)
        {
            var engine = new SessionEngine(Build.Shipped(), lookback);
            var bars = Build.HourlyBars(
                Tz.LosAngeles,
                start ?? new DateTime(2024, 1, 8),
                days,
                Build.AsiaRange);

            foreach (var b in bars) engine.OnBar(b, atr);
            return engine;
        }

        [Fact]
        public void Session_AccumulatesOnlyInRangeBars_AndLocksAfterTwentyOne()
        {
            var engine = RunDays(1);
            var s = Assert.Single(engine.Sessions);

            Assert.True(s.ProjectionsLocked);
            Assert.True(s.SessionComplete);

            // Only 17:00-21:00 bars are folded in; the extreme out-of-window bars
            // (H 99999 / L 1) must never appear.
            Assert.Equal(20100, s.FinalHigh!.Value, 9);
            Assert.Equal(19900, s.FinalLow!.Value, 9);
            Assert.Equal(5, s.AccumulatedBars); // 17,18,19,20,21

            // Lock fires on the first bar at/after 21:01 -- here the 22:00 bar.
            Assert.Equal(22, new PineTime(Tz.LosAngeles).Local(s.LockTime!.Value).Hour);
        }

        [Fact]
        public void Session_StartAndEnd_AreSeventeenAndTwentyOneLocal()
        {
            var engine = RunDays(1);
            var s = engine.Sessions[0];
            var pt = new PineTime(Tz.LosAngeles);
            Assert.Equal(17, pt.Local(s.StartTime).Hour);
            Assert.Equal(21, pt.Local(s.EndTime).Hour);
        }

        [Fact]
        public void FirstSession_LocksBaseDistances()
        {
            var engine = RunDays(1);
            var s = engine.Sessions[0];
            Assert.Equal(45.0, s.LockedUpDistance!.Value, 9);
            Assert.Equal(48.0, s.LockedDownDistance!.Value, 9);
            Assert.Equal(1.0, s.ScaleFactor!.Value, 9);
            Assert.Equal(48.0 / 45.0, s.SkewRatio!.Value, 9);
            Assert.Equal(Atr, s.AtrValue!.Value, 9);
        }

        [Fact]
        public void PreviousDistances_CarryForwardBetweenSessions()
        {
            var engine = RunDays(3);
            Assert.Equal(45.0, engine.PrevUpDistance!.Value, 9);
            Assert.Equal(48.0, engine.PrevDownDistance!.Value, 9);
            Assert.All(engine.Sessions, s => Assert.Equal(45.0, s.LockedUpDistance!.Value, 9));
        }

        // ---- 32. Historical lookback cleanup -------------------------------
        [Fact]
        public void Retention_NeverExceedsLookbackPeriod()
        {
            var engine = RunDays(days: 7, lookback: 3);
            Assert.Equal(3, engine.Sessions.Count);
            Assert.Equal(4, engine.EvictedSessions.Count);

            // Retained sessions are the most recent ones, in order.
            var starts = engine.Sessions.Select(s => s.StartTime).ToList();
            Assert.Equal(starts.OrderBy(x => x).ToList(), starts);
            Assert.True(engine.EvictedSessions.Last().StartTime < engine.Sessions.First().StartTime);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public void Retention_HonoursEveryAllowedLookback(int lookback)
        {
            var engine = RunDays(days: 12, lookback: lookback);
            Assert.Equal(lookback, engine.Sessions.Count);
        }

        [Fact]
        public void Retention_RejectsOutOfRangeLookback()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SessionEngine(Build.Shipped(), 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SessionEngine(Build.Shipped(), 11));
        }

        // ---- 33. Deterministic recalculation -------------------------------
        [Fact]
        public void Recalculation_IsDeterministic()
        {
            var a = RunDays(5);
            var b = RunDays(5);

            Assert.Equal(a.Sessions.Count, b.Sessions.Count);
            for (var i = 0; i < a.Sessions.Count; i++)
            {
                Assert.Equal(a.Sessions[i].StartTime, b.Sessions[i].StartTime);
                Assert.Equal(a.Sessions[i].FinalHigh, b.Sessions[i].FinalHigh);
                Assert.Equal(a.Sessions[i].FinalLow, b.Sessions[i].FinalLow);
                Assert.Equal(a.Sessions[i].LockedUpDistance, b.Sessions[i].LockedUpDistance);
                Assert.Equal(a.Sessions[i].LockedDownDistance, b.Sessions[i].LockedDownDistance);
                Assert.Equal(a.Sessions[i].DrawTo, b.Sessions[i].DrawTo);
            }
            Assert.Equal(a.PrevUpDistance, b.PrevUpDistance);
        }

        // ---- 37. Locked high/low immutability ------------------------------
        [Fact]
        public void LockedSession_RejectsFurtherAccumulation()
        {
            var engine = RunDays(1);
            var s = engine.Sessions[0];
            var high = s.FinalHigh!.Value;

            var ex = Record.Exception(() =>
            {
                var m = typeof(RangeSession).GetMethod("Accumulate",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                try { m.Invoke(s, new object[] { new Ohlc(DateTime.UtcNow, 1, 999999, 0, 1) }); }
                catch (System.Reflection.TargetInvocationException tie) { throw tie.InnerException!; }
            });

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal(high, s.FinalHigh!.Value, 9);
        }

        // ---- 38. Locked projection immutability ----------------------------
        [Fact]
        public void LockedSession_RejectsRelocking()
        {
            var engine = RunDays(1);
            var s = engine.Sessions[0];
            var up = s.LockedUpDistance!.Value;

            var ex = Record.Exception(() =>
            {
                var m = typeof(RangeSession).GetMethod("Lock",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                try
                {
                    m.Invoke(s, new object[]
                    {
                        DateTime.UtcNow,
                        new AutoProjectionResult(999, 999, 9, 9),
                        999.0,
                        DateTime.UtcNow,
                        DateTime.UtcNow
                    });
                }
                catch (System.Reflection.TargetInvocationException tie) { throw tie.InnerException!; }
            });

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal(up, s.LockedUpDistance!.Value, 9);
        }

        // ---- Drawn extent is fixed at lock (quirk Q-02) --------------------
        [Fact]
        public void Quirk_DrawnExtentIsFixedFromSessionEndToNextSessionStart()
        {
            var engine = RunDays(2);
            var s = engine.Sessions[0];
            var w = Build.Shipped();

            Assert.Equal(s.EndTime, s.DrawFrom!.Value);
            Assert.Equal(w.NextSessionStart(w.SessionEnd(s.StartTime)), s.DrawTo!.Value);

            // Never widened toward the live bar.
            Assert.True(s.DrawTo!.Value < engine.Sessions[1].EndTime);
        }

        [Fact]
        public void Quirk_UpdateZonesAndLines_IsNeverReached()
        {
            var engine = RunDays(7);
            Assert.Equal(0, engine.DeadPaths.UpdateZonesAndLinesFromNewSession);
            Assert.Equal(0, engine.DeadPaths.UpdateZonesAndLinesFromExtension);
        }

        [Fact]
        public void Quirk_SentinelSeedsNeverLeak_OnTheShippedPath()
        {
            var engine = RunDays(7);
            Assert.Equal(0, engine.DeadPaths.SentinelLeak);
            Assert.All(engine.Sessions, s => Assert.True(s.AccumulatedBars > 0));
        }

        [Fact]
        public void Quirk_CrossMidnightStartAdjustment_IsNeverReached()
        {
            var engine = RunDays(7);
            Assert.Equal(0, engine.DeadPaths.CrossMidnightStartAdjustment);
            Assert.Equal(0, engine.DeadPaths.Total);
        }

        // ---- Q-01: the lock can never fire on a 4h chart -------------------
        [Fact]
        public void Quirk_FourHourChart_NeverLocks_AndWedgesTheIndicator()
        {
            var engine = new SessionEngine(Build.Shipped());
            var pt = new PineTime(Tz.LosAngeles);

            // CME-aligned 4h bars: 17:00, 21:00, 01:00, 05:00, 09:00, 13:00.
            var bars = new List<Ohlc>();
            for (var day = 0; day < 5; day++)
            {
                var date = new DateTime(2024, 1, 8).AddDays(day);
                foreach (var h in new[] { 1, 5, 9, 13, 17, 21 })
                {
                    var utc = pt.Timestamp(date.Year, date.Month, date.Day, h, 0);
                    bars.Add(new Ohlc(utc, 20000, 20050, 19950, 20000));
                }
            }
            bars.Sort((a, b) => a.OpenTimeUtc.CompareTo(b.OpenTimeUtc));
            foreach (var b in bars) engine.OnBar(b, Atr);

            // Exactly one session is ever created, and it never locks.
            var s = Assert.Single(engine.Sessions);
            Assert.False(s.ProjectionsLocked);
            Assert.False(s.SessionComplete);
            Assert.Null(s.FinalHigh);

            // The table therefore has nothing to show.
            var table = engine.GetInfoTableData();
            Assert.Null(table.Atr);
            Assert.Equal("N/A", InfoTableData.Format(table.UpProjection));
        }

        [Fact]
        public void InfoTable_ShowsMostRecentLockedSession()
        {
            var engine = RunDays(3);
            var table = engine.GetInfoTableData();
            var newest = engine.Sessions.Last(s => s.ProjectionsLocked);

            Assert.Equal(newest.AtrValue, table.Atr);
            Assert.Equal(newest.LockedUpDistance, table.UpProjection);
            Assert.Equal(newest.LockedDownDistance, table.DownProjection);
        }

        [Fact]
        public void MissingAtr_LocksSessionButDrawsNoZones()
        {
            var engine = RunDays(1, atr: null);
            var s = engine.Sessions[0];

            Assert.True(s.ProjectionsLocked);
            Assert.Null(s.LockedUpDistance);
            Assert.Null(s.LockedDownDistance);
            Assert.Null(s.Projections);   // Pine 457 guard fails -> no zones
            Assert.Null(s.Visibility);
            Assert.NotNull(s.Levels);     // range levels still drawn
        }

        [Fact]
        public void WeekendSessions_AreCreated_BecauseIsMarketActiveIsATautology()
        {
            // 2024-01-06 is a Saturday.
            var engine = RunDays(days: 2, start: new DateTime(2024, 1, 6));
            Assert.Equal(2, engine.Sessions.Count);
            var pt = new PineTime(Tz.LosAngeles);
            Assert.Equal(DayOfWeek.Saturday, pt.Local(engine.Sessions[0].StartTime).DayOfWeek);
            Assert.Equal(DayOfWeek.Sunday, pt.Local(engine.Sessions[1].StartTime).DayOfWeek);
        }
    }
}
