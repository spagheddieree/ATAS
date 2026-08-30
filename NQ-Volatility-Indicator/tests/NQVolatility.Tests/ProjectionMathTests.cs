using System;
using NQVolatility.Core;
using Xunit;

namespace NQVolatility.Tests
{
    /// <summary>Required cases 1-19 plus the no-rounding and fib-value regressions.</summary>
    public class ProjectionMathTests
    {
        private const double Tol = 1e-9;

        // ---- 1. ATR scale lower clamp -------------------------------------
        [Fact]
        public void AtrScale_LowerClamp()
        {
            // 200/300 = 0.6667 -> clamped up to 0.80
            var r = AutoProjection.Calculate(atr: 200, prevUp: null, prevDown: null);
            Assert.Equal(0.80, r.ScaleFactor!.Value, 9);
            Assert.Equal(45.0 * 0.80, r.UpDistance!.Value, 9);
            Assert.Equal(48.0 * 0.80, r.DownDistance!.Value, 9);
        }

        // ---- 2. ATR scale normal (no clamp) -------------------------------
        [Fact]
        public void AtrScale_NoClamp()
        {
            var r = AutoProjection.Calculate(atr: 330, prevUp: null, prevDown: null);
            Assert.Equal(1.1, r.ScaleFactor!.Value, 9);
            Assert.Equal(49.5, r.UpDistance!.Value, 9);
            Assert.Equal(52.8, r.DownDistance!.Value, 9);
        }

        // ---- 3. ATR scale upper clamp -------------------------------------
        [Fact]
        public void AtrScale_UpperClamp()
        {
            // 600/300 = 2.0 -> clamped down to 1.40
            var r = AutoProjection.Calculate(atr: 600, prevUp: null, prevDown: null);
            Assert.Equal(1.40, r.ScaleFactor!.Value, 9);
            Assert.Equal(63.0, r.UpDistance!.Value, 9);
            Assert.Equal(67.2, r.DownDistance!.Value, 9);
        }

        // ---- 4. First-session projection initialisation --------------------
        [Fact]
        public void FirstSession_UsesOwnBaseAsPrevious()
        {
            var r = AutoProjection.Calculate(atr: 300, prevUp: null, prevDown: null);
            // f = 1.0, so base = 45 / 48 and the day-over-day clamp must be a no-op.
            Assert.Equal(45.0, r.UpDistance!.Value, 9);
            Assert.Equal(48.0, r.DownDistance!.Value, 9);
            Assert.Equal(48.0 / 45.0, r.SkewRatio!.Value, 9);
        }

        // ---- 5. +40% daily clamp ------------------------------------------
        [Fact]
        public void DailyClamp_UpperBoundBinds()
        {
            // prevUp = 10 -> max 14; base 45 is clamped down to 14.
            var r = AutoProjection.Calculate(atr: 300, prevUp: 10, prevDown: 10);
            Assert.Equal(14.0, r.UpDistance!.Value, 9);
            Assert.Equal(14.0, r.DownDistance!.Value, 9);
        }

        // ---- 6. -40% daily clamp ------------------------------------------
        [Fact]
        public void DailyClamp_LowerBoundBinds()
        {
            // prevUp = 100 -> min 60; base 45 is clamped up to 60.
            var r = AutoProjection.Calculate(atr: 300, prevUp: 100, prevDown: 100);
            Assert.Equal(60.0, r.UpDistance!.Value, 9);
            Assert.Equal(60.0, r.DownDistance!.Value, 9);
        }

        // ---- 7. Independent up/down distances ------------------------------
        [Fact]
        public void UpAndDown_AreClampedIndependently()
        {
            var r = AutoProjection.Calculate(atr: 300, prevUp: 10, prevDown: 1000);
            Assert.Equal(14.0, r.UpDistance!.Value, 9);    // clamped down against prevUp
            Assert.Equal(600.0, r.DownDistance!.Value, 9); // clamped up against prevDown
            Assert.NotEqual(r.UpDistance!.Value, r.DownDistance!.Value);
        }

        // ---- 8-17. Every projection multiplier -----------------------------
        private static ProjectionLevels Sample()
            => new ProjectionLevels(finalHigh: 200, finalLow: 100, upDistance: 10, downDistance: 20);

        [Theory]
        [InlineData(-2.0, 120.0, 160.0)]
        [InlineData(-2.33, 123.3, 153.4)]
        [InlineData(-2.5, 125.0, 150.0)]
        [InlineData(-3.0, 130.0, 140.0)]
        [InlineData(-4.0, 140.0, 120.0)]
        [InlineData(-4.5, 145.0, 110.0)]
        [InlineData(-6.0, 160.0, 80.0)]
        [InlineData(-6.5, 165.0, 70.0)]
        [InlineData(-7.78, 177.8, 44.4)]
        [InlineData(-8.5, 185.0, 30.0)]
        public void EveryMultiplier_ProjectsCorrectly(double k, double expectedUp, double expectedDown)
        {
            var p = Sample();
            Assert.Equal(expectedUp, p.Up(k), 9);
            Assert.Equal(expectedDown, p.Down(k), 9);
        }

        // ---- 18. Upward anchor is finalLow ---------------------------------
        [Fact]
        public void UpwardProjections_AnchorOnFinalLow()
        {
            var p = new ProjectionLevels(finalHigh: 500, finalLow: 100, upDistance: 10, downDistance: 999);
            // Independent of finalHigh and of downDistance.
            Assert.Equal(100 + 10 * 2.0, p.Up200, Tol);
            var q = new ProjectionLevels(finalHigh: 9999, finalLow: 100, upDistance: 10, downDistance: 1);
            Assert.Equal(p.Up200, q.Up200, Tol);
        }

        // ---- 19. Downward anchor is finalHigh ------------------------------
        [Fact]
        public void DownwardProjections_AnchorOnFinalHigh()
        {
            var p = new ProjectionLevels(finalHigh: 500, finalLow: 100, upDistance: 999, downDistance: 20);
            Assert.Equal(500 - 20 * 2.0, p.Down200, Tol);
            var q = new ProjectionLevels(finalHigh: 500, finalLow: -9999, upDistance: 1, downDistance: 20);
            Assert.Equal(p.Down200, q.Down200, Tol);
        }

        // ---- Regression: fib800 really is 7.78 (quirk Q-07) ----------------
        [Fact]
        public void Fib800_IsSevenPointSevenEight_NotEight()
        {
            Assert.Equal(-7.78, FibMultipliers.Fib800, 9);
            var p = new ProjectionLevels(0, 0, 1, 1);
            Assert.Equal(7.78, p.Up800, 9);
        }

        // ---- Regression: no rounding is applied (dead code D-01) -----------
        [Fact]
        public void Projections_AreNotRounded()
        {
            // 0.07 is not a multiple of the dead roundTo = 0.25.
            var p = new ProjectionLevels(finalHigh: 100, finalLow: 0, upDistance: 0.035, downDistance: 0.035);
            Assert.Equal(0.07, p.Up200, 12);
            Assert.NotEqual(0.0, p.Up200 % PineDefaults.DeadRoundTo, 12);
        }

        // ---- Regression: skewConst is not used -----------------------------
        [Fact]
        public void SkewRatio_IsComputed_NotTheDeadConstant()
        {
            var r = AutoProjection.Calculate(atr: 300, prevUp: 10, prevDown: 1000);
            Assert.Equal(600.0 / 14.0, r.SkewRatio!.Value, 9);
            Assert.NotEqual(PineDefaults.DeadSkewConst, r.SkewRatio!.Value, 3);
        }
    }
}
