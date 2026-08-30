using NQVolatility.Core;
using Xunit;

namespace NQVolatility.Tests
{
    /// <summary>Required cases 20-25, 39-40.</summary>
    public class RangeAndZoneTests
    {
        // ---- 21. High / Low ------------------------------------------------
        [Fact]
        public void RangeLevels_HighAndLow()
        {
            var r = new RangeLevels(finalHigh: 20100, finalLow: 19900);
            Assert.Equal(20100, r.High, 9);
            Assert.Equal(19900, r.Low, 9);
            Assert.Equal(200, r.RangeSize, 9);
        }

        // ---- 22. 25% level -------------------------------------------------
        [Fact]
        public void RangeLevels_TwentyFivePercent()
        {
            var r = new RangeLevels(20100, 19900);
            Assert.Equal(19950, r.Q1Price, 9); // low + size*0.25
        }

        // ---- 23. EQ --------------------------------------------------------
        [Fact]
        public void RangeLevels_Equilibrium()
        {
            var r = new RangeLevels(20100, 19900);
            Assert.Equal(20000, r.EqPrice, 9);
        }

        // ---- 24. 75% level -------------------------------------------------
        [Fact]
        public void RangeLevels_SeventyFivePercent()
        {
            var r = new RangeLevels(20100, 19900);
            Assert.Equal(20050, r.Q3Price, 9);
        }

        // ---- 25. Q1/Q3 Pine label quirk (Q-03) -----------------------------
        [Fact]
        public void Quirk_Q1Q3_LabelsAreTransposed_InPineCompatibleMode()
        {
            // The 25% PRICE carries the text "Q3", and the 75% PRICE carries "Q1".
            Assert.Equal("Q3", RangeLevelLabelMap.LabelForQ1Price(RangeLabelMode.PineCompatible));
            Assert.Equal("Q1", RangeLevelLabelMap.LabelForQ3Price(RangeLabelMode.PineCompatible));
        }

        [Fact]
        public void CorrectedMode_UnswapsLabels_WithoutTouchingPrices()
        {
            Assert.Equal("Q1", RangeLevelLabelMap.LabelForQ1Price(RangeLabelMode.Corrected));
            Assert.Equal("Q3", RangeLevelLabelMap.LabelForQ3Price(RangeLabelMode.Corrected));

            // Prices are identical in both modes -- the quirk is purely presentational.
            var r = new RangeLevels(20100, 19900);
            Assert.Equal(19950, r.Q1Price, 9);
            Assert.Equal(20050, r.Q3Price, 9);
        }

        // ---- 20 / 39. Upward zones must be COMPLETELY above finalHigh -------
        [Fact]
        public void ZoneEligibility_UpwardRequiresBothBoundariesAboveHigh()
        {
            // low=100 high=200, up=45: Up200=190 (inside), Up250=212.5 (outside).
            var partly = new ProjectionLevels(200, 100, upDistance: 45, downDistance: 45);
            Assert.False(ZoneEligibility.Evaluate(partly, 200, 100).ShowUpLevel1);

            // up=60: Up200=220, Up250=250 -- both clear.
            var clear = new ProjectionLevels(200, 100, upDistance: 60, downDistance: 60);
            Assert.True(ZoneEligibility.Evaluate(clear, 200, 100).ShowUpLevel1);
        }

        // ---- 40. Downward zones must be COMPLETELY below finalLow ----------
        [Fact]
        public void ZoneEligibility_DownwardRequiresBothBoundariesBelowLow()
        {
            // down=45: Down200=110 (inside), Down250=87.5 (outside).
            var partly = new ProjectionLevels(200, 100, upDistance: 45, downDistance: 45);
            Assert.False(ZoneEligibility.Evaluate(partly, 200, 100).ShowDownLevel1);

            var clear = new ProjectionLevels(200, 100, upDistance: 60, downDistance: 60);
            Assert.True(ZoneEligibility.Evaluate(clear, 200, 100).ShowDownLevel1);
        }

        [Fact]
        public void ZoneEligibility_ComparisonsAreStrict()
        {
            // up=50 puts Up200 exactly ON finalHigh (100 + 50*2 = 200). Strict > fails.
            var onBoundary = new ProjectionLevels(200, 100, upDistance: 50, downDistance: 50);
            var v = ZoneEligibility.Evaluate(onBoundary, 200, 100);
            Assert.False(v.ShowUpLevel1);
            Assert.False(v.ShowDownLevel1); // Down200 = 200 - 100 = 100, exactly on finalLow
        }

        [Fact]
        public void ZoneEligibility_AvrMinusLineIsTestedIndependently()
        {
            // up=35: Up200=170, Up250=187.5, Up300=205. Level 1 is hidden but AVR- shows.
            var p = new ProjectionLevels(200, 100, upDistance: 35, downDistance: 35);
            var v = ZoneEligibility.Evaluate(p, 200, 100);
            Assert.False(v.ShowUpLevel1);
            Assert.True(v.ShowUpLine300);
        }

        [Fact]
        public void ZoneEligibility_OuterZonesCanShowWhileInnerOnesAreSuppressed()
        {
            // up=45: Up200=190 hidden; Up400=280, Up600=370, Up800=450.1 all clear.
            var p = new ProjectionLevels(200, 100, upDistance: 45, downDistance: 45);
            var v = ZoneEligibility.Evaluate(p, 200, 100);
            Assert.False(v.ShowUpLevel1);
            Assert.True(v.ShowUpAvr);
            Assert.True(v.ShowUpAvrPlus);
            Assert.True(v.ShowUpMax);
        }
    }
}
