using System;
using NQVolatility.Core;
using Xunit;

namespace NQVolatility.Tests
{
    /// <summary>Required cases 26-31.</summary>
    public class SessionTimingTests
    {
        // ---- 26. The 17:00-21:00 session window ----------------------------
        [Fact]
        public void AsiaWindow_IsSeventeenToTwentyOne_Inclusive()
        {
            var w = Build.Shipped();
            Assert.Equal(17, w.Times.StartHour);
            Assert.Equal(21, w.Times.EndHour);
            Assert.False(w.Times.CrossesMidnight);

            DateTime At(int h, int mi = 0) => Build.Utc(Tz.LosAngeles, 2024, 1, 8, h, mi);

            Assert.False(w.IsInCustomRange(At(16, 59)));
            Assert.True(w.IsInCustomRange(At(17, 0)));
            Assert.True(w.IsInCustomRange(At(19, 30)));
            Assert.True(w.IsInCustomRange(At(21, 0)));   // inclusive upper bound
            Assert.False(w.IsInCustomRange(At(21, 1)));
        }

        [Fact]
        public void LockWindow_IsTwentyOneHundredToMidnightOnly()
        {
            var w = Build.Shipped();
            DateTime At(int h, int mi = 0) => Build.Utc(Tz.LosAngeles, 2024, 1, 8, h, mi);

            Assert.False(w.IsAtOrAfterLockTime(At(20, 59)));
            Assert.True(w.IsAtOrAfterLockTime(At(21, 0)));
            Assert.True(w.IsAtOrAfterLockTime(At(23, 59)));
            // Same-day minute-of-day test: after midnight it is false again.
            Assert.False(w.IsAtOrAfterLockTime(At(0, 30)));
        }

        [Fact]
        public void SessionEnd_IsFourHoursAfterStart()
        {
            var w = Build.Shipped();
            var start = Build.Utc(Tz.LosAngeles, 2024, 1, 8, 17);
            Assert.Equal(start.AddHours(4), w.SessionEnd(start));
        }

        // ---- 27. Generic cross-midnight session ----------------------------
        [Fact]
        public void CrossMidnightWindow_WrapsCorrectly()
        {
            var w = Build.Custom(new SessionTimes(22, 0, 2, 0));
            Assert.True(w.Times.CrossesMidnight);

            DateTime At(int d, int h) => Build.Utc(Tz.LosAngeles, 2024, 1, d, h);

            Assert.False(w.IsInCustomRange(At(8, 21)));
            Assert.True(w.IsInCustomRange(At(8, 22)));
            Assert.True(w.IsInCustomRange(At(8, 23)));
            Assert.True(w.IsInCustomRange(At(9, 0)));
            Assert.True(w.IsInCustomRange(At(9, 2)));
            Assert.False(w.IsInCustomRange(At(9, 3)));

            // getSessionEnd adds 1440 when the delta is non-positive.
            var start = At(8, 22);
            Assert.Equal(start.AddHours(4), w.SessionEnd(start));
        }

        // ---- 28. Named timezone behaviour ----------------------------------
        [Fact]
        public void NamedTimezone_ChangesWhichInstantsAreInRange()
        {
            var la = Build.Shipped(Tz.LosAngeles);
            var utc = Build.Shipped(Tz.Utc);

            // 2024-01-08 17:00 America/Los_Angeles == 2024-01-09 01:00 UTC.
            var instant = Build.Utc(Tz.LosAngeles, 2024, 1, 8, 17);
            Assert.Equal(new DateTime(2024, 1, 9, 1, 0, 0, DateTimeKind.Utc), instant);

            Assert.True(la.IsInCustomRange(instant));   // 17:00 local
            Assert.False(utc.IsInCustomRange(instant)); // 01:00 UTC
        }

        [Fact]
        public void NamedTimezone_IsResolvedByIanaId_NotFixedOffset()
        {
            var pt = new PineTime(Tz.LosAngeles);
            var winter = pt.Timestamp(2024, 1, 8, 17, 0);
            var summer = pt.Timestamp(2024, 7, 8, 17, 0);

            // PST = UTC-8 -> 01:00Z next day; PDT = UTC-7 -> 00:00Z next day.
            Assert.Equal(1, winter.Hour);
            Assert.Equal(0, summer.Hour);
        }

        // ---- 29. DST transition --------------------------------------------
        [Fact]
        public void DstTransition_ShiftsTheSessionByOneHourInUtc()
        {
            var pt = new PineTime(Tz.LosAngeles);

            // US spring forward: 2024-03-10 02:00 local.
            var beforeDst = pt.Timestamp(2024, 3, 9, 17, 0);  // PST, UTC-8
            var afterDst = pt.Timestamp(2024, 3, 10, 17, 0);  // PDT, UTC-7

            Assert.Equal(new DateTime(2024, 3, 10, 1, 0, 0, DateTimeKind.Utc), beforeDst);
            Assert.Equal(new DateTime(2024, 3, 11, 0, 0, 0, DateTimeKind.Utc), afterDst);

            // 23 hours apart, not 24 -- proof the named zone is honoured.
            Assert.Equal(23, (afterDst - beforeDst).TotalHours, 9);
        }

        [Fact]
        public void DstTransition_LocalWallClockTestsStayStable()
        {
            var w = Build.Shipped();
            // On both sides of the transition, 17:00 local is in range and 21:01 is not.
            foreach (var (m, d) in new[] { (3, 9), (3, 10), (11, 2), (11, 3) })
            {
                Assert.True(w.IsInCustomRange(Build.Utc(Tz.LosAngeles, 2024, m, d, 17)));
                Assert.False(w.IsInCustomRange(Build.Utc(Tz.LosAngeles, 2024, m, d, 21, 1)));
            }
        }

        // ---- 30. Next-session calculation ----------------------------------
        [Theory]
        // 2024-01-04 is a Thursday, 01-05 Friday, 01-06 Saturday, 01-07 Sunday, 01-08 Monday.
        [InlineData(4, 5)]   // Thu -> Fri
        [InlineData(5, 8)]   // Fri -> Mon (weekend skipped)
        [InlineData(6, 8)]   // Sat -> Mon
        [InlineData(7, 8)]   // Sun -> Mon
        [InlineData(8, 9)]   // Mon -> Tue
        public void NextSessionStart_SkipsWeekends(int sessionDay, int expectedNextDay)
        {
            var w = Build.Shipped();
            var start = Build.Utc(Tz.LosAngeles, 2024, 1, sessionDay, 17);
            var end = w.SessionEnd(start);

            var next = w.NextSessionStart(end);
            var expected = Build.Utc(Tz.LosAngeles, 2024, 1, expectedNextDay, 17);
            Assert.Equal(expected, next);
        }

        [Fact]
        public void NextSessionStart_IsDstSafeAcrossSpringForward()
        {
            var w = Build.Shipped();
            // Session on Sat 2024-03-09 ends 21:00; next start must be Mon 2024-03-11 17:00 PDT.
            var end = w.SessionEnd(Build.Utc(Tz.LosAngeles, 2024, 3, 9, 17));
            var next = w.NextSessionStart(end);
            Assert.Equal(Build.Utc(Tz.LosAngeles, 2024, 3, 11, 17), next);
            Assert.Equal(17, new PineTime(Tz.LosAngeles).Local(next).Hour);
        }

        // ---- 31. Pine-compatible weekend behaviour (quirk Q-04) ------------
        [Fact]
        public void Quirk_IsMarketActive_IsATautology_WeekendsIncluded()
        {
            var w = Build.Shipped();
            // 2024-01-06 Saturday and 2024-01-07 Sunday, both inside 17:00-21:00.
            Assert.True(w.IsMarketActive(Build.Utc(Tz.LosAngeles, 2024, 1, 6, 18)));
            Assert.True(w.IsMarketActive(Build.Utc(Tz.LosAngeles, 2024, 1, 7, 18)));
            Assert.True(w.InRangeNow(Build.Utc(Tz.LosAngeles, 2024, 1, 6, 18)));

            // Every day of a full week passes.
            for (var d = 1; d <= 7; d++)
                Assert.True(w.IsMarketActive(Build.Utc(Tz.LosAngeles, 2024, 1, d, 18)));
        }
    }
}
