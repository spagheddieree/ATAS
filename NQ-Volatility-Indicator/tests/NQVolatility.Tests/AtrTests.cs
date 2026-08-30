using NQVolatility.Core;
using Xunit;

namespace NQVolatility.Tests
{
    /// <summary>Required cases 34-36 plus Wilder RMA correctness.</summary>
    public class AtrTests
    {
        // Three daily bars with hand-computed true ranges.
        //   d1: H10 L8  C9   -> TR = 10-8 = 2      (no previous close)
        //   d2: H12 L9  C11  -> TR = max(3, |12-9|, |9-9|)   = 3
        //   d3: H13 L11 C12  -> TR = max(2, |13-11|, |11-11|) = 2
        // ATR(2) after d2 = (2+3)/2 = 2.5 ; after d3 = (2.5*1 + 2)/2 = 2.25
        private static DailyAtrSeries Seeded() => new DailyAtrSeries(2);

        [Fact]
        public void WilderAtr_SeedsWithSmaThenSmooths()
        {
            var atr = new WilderAtr(2);
            Assert.Null(atr.Update(10, 8, 9));           // not seeded yet
            Assert.Equal(2.5, atr.Update(12, 9, 11)!.Value, 9);
            Assert.Equal(2.25, atr.Update(13, 11, 12)!.Value, 9);
        }

        [Fact]
        public void TrueRange_FirstBarHasNoPreviousClose()
        {
            Assert.Equal(2.0, WilderAtr.TrueRange(10, 8, null), 9);
            Assert.Equal(3.0, WilderAtr.TrueRange(12, 9, 9), 9);
        }

        // ---- 34. No lookahead ----------------------------------------------
        [Fact]
        public void NoLookahead_PublishedValueNeverIncludesTheOpenDailyBar()
        {
            var s = Seeded();
            s.OnDailyBarClosed(10, 8, 9);    // d1 closed
            s.AdvanceChartBar();             // a bar inside d2
            Assert.Null(s.Published);        // ATR(2) needs two closed dailies

            s.OnDailyBarClosed(12, 9, 11);   // d2 closed
            s.AdvanceChartBar();             // first bar of d3
            // Published reflects d1..d2 only -- d3 is still open.
            Assert.Equal(2.5, s.Published!.Value, 9);

            s.OnDailyBarClosed(13, 11, 12);  // d3 closed
            s.AdvanceChartBar();             // first bar of d4
            Assert.Equal(2.25, s.Published!.Value, 9);
        }

        // ---- 35. Yesterday-completed-ATR semantics -------------------------
        [Fact]
        public void YesterdayAtr_IsThePreviousChartBarsPublishedValue()
        {
            var s = Seeded();
            s.OnDailyBarClosed(10, 8, 9);
            s.OnDailyBarClosed(12, 9, 11);   // ATR now 2.5

            // First chart bar of the new daily period: the series has just stepped,
            // so [1] still holds the stale value. This is the rollover-bar case.
            var rollover = s.AdvanceChartBar();
            Assert.Null(rollover);
            Assert.Equal(2.5, s.Published!.Value, 9);

            // Every later bar in the same daily period sees the completed value.
            Assert.Equal(2.5, s.AdvanceChartBar()!.Value, 9);
            Assert.Equal(2.5, s.AdvanceChartBar()!.Value, 9);
        }

        [Fact]
        public void YesterdayAtr_LagsByExactlyOneChartBarAcrossRollover()
        {
            var s = Seeded();
            s.OnDailyBarClosed(10, 8, 9);
            s.OnDailyBarClosed(12, 9, 11);
            s.AdvanceChartBar();
            s.AdvanceChartBar();                     // settled at 2.5

            s.OnDailyBarClosed(13, 11, 12);          // ATR steps to 2.25
            var onRollover = s.AdvanceChartBar();
            Assert.Equal(2.5, onRollover!.Value, 9);  // [1] is still the old value
            Assert.Equal(2.25, s.Published!.Value, 9);

            var afterRollover = s.AdvanceChartBar();
            Assert.Equal(2.25, afterRollover!.Value, 9);
        }

        // ---- 36. Missing ATR behaviour (TV-OPEN-1, interpretation A) --------
        [Fact]
        public void MissingAtr_ProducesNaProjections_AndNoZones()
        {
            var r = AutoProjection.Calculate(atr: null, prevUp: null, prevDown: null);
            Assert.Null(r.ScaleFactor);
            Assert.Null(r.UpDistance);
            Assert.Null(r.DownDistance);
            Assert.Null(r.SkewRatio);
            Assert.False(r.IsAvailable);
        }

        [Fact]
        public void MissingAtr_WithPreviousValues_StillProducesNa()
        {
            // uUpAutoBase is na, so the clamp is na regardless of the bounds.
            var r = AutoProjection.Calculate(atr: null, prevUp: 45, prevDown: 48);
            Assert.Null(r.UpDistance);
            Assert.Null(r.DownDistance);
        }

        [Fact]
        public void InfoTable_FormatsNaAsNotAvailable()
        {
            Assert.Equal("N/A", InfoTableData.Format(null));
            Assert.Equal("45", InfoTableData.Format(45.0));
            Assert.Equal("49.5", InfoTableData.Format(49.5));
            Assert.Equal("2.25", InfoTableData.Format(2.25));
            Assert.Equal("1.07", InfoTableData.Format(1.06666));  // "#.##"
        }
    }
}
