using System;

namespace NQVolatility.Core
{
    /// <summary>
    /// Projected prices for one completed session (Pine 467-489).
    /// Upward projections anchor on finalLow; downward on finalHigh.
    /// No rounding is applied -- roundPrice/roundTo are dead in the source (D-01).
    /// </summary>
    public readonly struct ProjectionLevels
    {
        private readonly double _finalHigh;
        private readonly double _finalLow;
        private readonly double _upDistance;
        private readonly double _downDistance;

        public ProjectionLevels(double finalHigh, double finalLow, double upDistance, double downDistance)
        {
            _finalHigh = finalHigh;
            _finalLow = finalLow;
            _upDistance = upDistance;
            _downDistance = downDistance;
        }

        /// <summary>Pine: <c>finalLow + upDistance * math.abs(k)</c>.</summary>
        public double Up(double multiplier) => _finalLow + _upDistance * Math.Abs(multiplier);

        /// <summary>Pine: <c>finalHigh - downDistance * math.abs(k)</c>.</summary>
        public double Down(double multiplier) => _finalHigh - _downDistance * Math.Abs(multiplier);

        public double Up200 => Up(FibMultipliers.Fib200);
        public double Up233 => Up(FibMultipliers.Fib233);
        public double Up250 => Up(FibMultipliers.Fib250);
        public double Up300 => Up(FibMultipliers.Fib300);
        public double Up400 => Up(FibMultipliers.Fib400);
        public double Up450 => Up(FibMultipliers.Fib450);
        public double Up600 => Up(FibMultipliers.Fib600);
        public double Up650 => Up(FibMultipliers.Fib650);
        public double Up800 => Up(FibMultipliers.Fib800);
        public double Up850 => Up(FibMultipliers.Fib850);

        public double Down200 => Down(FibMultipliers.Fib200);
        public double Down233 => Down(FibMultipliers.Fib233);
        public double Down250 => Down(FibMultipliers.Fib250);
        public double Down300 => Down(FibMultipliers.Fib300);
        public double Down400 => Down(FibMultipliers.Fib400);
        public double Down450 => Down(FibMultipliers.Fib450);
        public double Down600 => Down(FibMultipliers.Fib600);
        public double Down650 => Down(FibMultipliers.Fib650);
        public double Down800 => Down(FibMultipliers.Fib800);
        public double Down850 => Down(FibMultipliers.Fib850);
    }

    /// <summary>Per-structure visibility flags (Pine 494-504).</summary>
    public readonly struct ZoneVisibility
    {
        public ZoneVisibility(
            bool upLevel1, bool upLine300, bool upAvr, bool upAvrPlus, bool upMax,
            bool downLevel1, bool downLine300, bool downAvr, bool downAvrPlus, bool downMax)
        {
            ShowUpLevel1 = upLevel1; ShowUpLine300 = upLine300; ShowUpAvr = upAvr;
            ShowUpAvrPlus = upAvrPlus; ShowUpMax = upMax;
            ShowDownLevel1 = downLevel1; ShowDownLine300 = downLine300; ShowDownAvr = downAvr;
            ShowDownAvrPlus = downAvrPlus; ShowDownMax = downMax;
        }

        public bool ShowUpLevel1 { get; }
        public bool ShowUpLine300 { get; }
        public bool ShowUpAvr { get; }
        public bool ShowUpAvrPlus { get; }
        public bool ShowUpMax { get; }

        public bool ShowDownLevel1 { get; }
        public bool ShowDownLine300 { get; }
        public bool ShowDownAvr { get; }
        public bool ShowDownAvrPlus { get; }
        public bool ShowDownMax { get; }
    }

    /// <summary>
    /// Zone eligibility (Pine 494-504). A zone is drawn only when BOTH of its
    /// boundaries lie strictly outside the completed range.
    /// <para>
    /// Both terms of each conjunction are kept even though the wider boundary
    /// dominates for positive distances -- this preserves the source structure
    /// and stays correct if a distance is ever non-positive.
    /// </para>
    /// </summary>
    public static class ZoneEligibility
    {
        public static ZoneVisibility Evaluate(ProjectionLevels p, double finalHigh, double finalLow)
        {
            var upLevel1 = p.Up250 > finalHigh && p.Up200 > finalHigh;
            var upLine300 = p.Up300 > finalHigh;
            var upAvr = p.Up450 > finalHigh && p.Up400 > finalHigh;
            var upAvrPlus = p.Up650 > finalHigh && p.Up600 > finalHigh;
            var upMax = p.Up850 > finalHigh && p.Up800 > finalHigh;

            var downLevel1 = p.Down250 < finalLow && p.Down200 < finalLow;
            var downLine300 = p.Down300 < finalLow;
            var downAvr = p.Down450 < finalLow && p.Down400 < finalLow;
            var downAvrPlus = p.Down650 < finalLow && p.Down600 < finalLow;
            var downMax = p.Down850 < finalLow && p.Down800 < finalLow;

            return new ZoneVisibility(
                upLevel1, upLine300, upAvr, upAvrPlus, upMax,
                downLevel1, downLine300, downAvr, downAvrPlus, downMax);
        }
    }
}
