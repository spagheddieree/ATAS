using System;

namespace NQVolatility.Core
{
    /// <summary>
    /// Pine arithmetic helpers. Nullable double models Pine's <c>na</c>.
    /// </summary>
    public static class PineMath
    {
        /// <summary>
        /// Pine <c>clampValue(value, minVal, maxVal) =&gt; math.max(minVal, math.min(maxVal, value))</c>
        /// (Pine 233-234).
        /// <para>
        /// na propagation: Phase 1 adopts interpretation (A) from PINE-PARITY-SPEC 6.4 --
        /// any na operand yields na. This is TV-OPEN-1 and is unverified against TradingView.
        /// </para>
        /// </summary>
        public static double? Clamp(double? value, double? minVal, double? maxVal)
        {
            if (value is null || minVal is null || maxVal is null) return null;
            return Math.Max(minVal.Value, Math.Min(maxVal.Value, value.Value));
        }

        /// <summary>Non-nullable overload for the common case.</summary>
        public static double Clamp(double value, double minVal, double maxVal)
            => Math.Max(minVal, Math.Min(maxVal, value));
    }
}
