namespace NQVolatility.Core
{
    /// <summary>How range-level labels are named.</summary>
    public enum RangeLabelMode
    {
        /// <summary>
        /// Phase 1 default. Reproduces Pine's transposed Q1/Q3 label texts
        /// (Pine 443-444, quirk Q-03): the 25% line is labelled "Q3" and the
        /// 75% line is labelled "Q1".
        /// </summary>
        PineCompatible = 0,

        /// <summary>
        /// Phase 2 candidate P2-05. Defined but NOT reachable in Phase 1.
        /// </summary>
        Corrected = 1
    }

    /// <summary>
    /// Completed-range levels (Pine 417-453). Prices are named by VALUE
    /// (Q1 = 25%, Q3 = 75%); display text is a separate concern so the
    /// label quirk can be corrected later without touching the maths.
    /// </summary>
    public readonly struct RangeLevels
    {
        public RangeLevels(double finalHigh, double finalLow)
        {
            High = finalHigh;
            Low = finalLow;
            var size = finalHigh - finalLow;
            RangeSize = size;
            Q1Price = finalLow + size * 0.25;
            EqPrice = finalLow + size * 0.50;
            Q3Price = finalLow + size * 0.75;
        }

        public double High { get; }
        public double Low { get; }
        public double RangeSize { get; }

        /// <summary>25% of the range above the low.</summary>
        public double Q1Price { get; }

        /// <summary>50% of the range above the low.</summary>
        public double EqPrice { get; }

        /// <summary>75% of the range above the low.</summary>
        public double Q3Price { get; }
    }

    /// <summary>
    /// Maps a range level to its displayed text. Isolating this is what lets
    /// Phase 2 fix quirk Q-03 without rewriting any calculation.
    /// </summary>
    public static class RangeLevelLabelMap
    {
        public static string HighLabel(RangeLabelMode mode) => "H";
        public static string LowLabel(RangeLabelMode mode) => "L";
        public static string EqLabel(RangeLabelMode mode) => "EQ";

        /// <summary>Text drawn at the 25% price.</summary>
        public static string LabelForQ1Price(RangeLabelMode mode)
            => mode == RangeLabelMode.PineCompatible ? "Q3" : "Q1";

        /// <summary>Text drawn at the 75% price.</summary>
        public static string LabelForQ3Price(RangeLabelMode mode)
            => mode == RangeLabelMode.PineCompatible ? "Q1" : "Q3";
    }
}
