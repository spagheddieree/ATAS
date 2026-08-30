namespace NQVolatility.Core
{
    /// <summary>Result of Pine <c>calculateAutoUValues</c> (237-263).</summary>
    public readonly struct AutoProjectionResult
    {
        public AutoProjectionResult(double? up, double? down, double? scaleFactor, double? skewRatio)
        {
            UpDistance = up; DownDistance = down; ScaleFactor = scaleFactor; SkewRatio = skewRatio;
        }

        public double? UpDistance { get; }
        public double? DownDistance { get; }
        public double? ScaleFactor { get; }
        public double? SkewRatio { get; }

        public bool IsAvailable => UpDistance.HasValue && DownDistance.HasValue;
    }

    /// <summary>
    /// Pine <c>calculateAutoUValues(atr, prevUp, prevDown)</c> (237-263).
    /// Up and down distances are constrained INDEPENDENTLY against their own
    /// previous locked value and are never collapsed into one symmetric distance.
    /// </summary>
    public static class AutoProjection
    {
        public static AutoProjectionResult Calculate(
            double? atr,
            double? prevUp,
            double? prevDown,
            double baselineVol = PineDefaults.BaselineVol,
            double baseUpRef = PineDefaults.BaseUpRef,
            double baseDownRef = PineDefaults.BaseDownRef,
            double scaleClampLow = PineDefaults.ScaleClampLow,
            double scaleClampHigh = PineDefaults.ScaleClampHigh,
            double maxDailyChange = PineDefaults.MaxDailyChange)
        {
            // fRaw = atr / baselineVol           (Pine 239)
            double? fRaw = atr.HasValue ? atr.Value / baselineVol : (double?)null;

            // f = clampValue(fRaw, low, high)    (Pine 241)
            double? f = PineMath.Clamp(fRaw, scaleClampLow, scaleClampHigh);

            // Base distances scaled by volatility (Pine 244-245)
            double? uUpAutoBase = f.HasValue ? baseUpRef * f.Value : (double?)null;
            double? uDownAutoBase = f.HasValue ? baseDownRef * f.Value : (double?)null;

            // First session: seed the previous value from today's base (Pine 248-249)
            double? localPrevUp = prevUp ?? uUpAutoBase;
            double? localPrevDown = prevDown ?? uDownAutoBase;

            // Day-over-day clamp, independently per direction (Pine 252-258)
            double? minUp = localPrevUp.HasValue ? localPrevUp.Value * (1 - maxDailyChange) : (double?)null;
            double? maxUp = localPrevUp.HasValue ? localPrevUp.Value * (1 + maxDailyChange) : (double?)null;
            double? uUpToday = PineMath.Clamp(uUpAutoBase, minUp, maxUp);

            double? minDown = localPrevDown.HasValue ? localPrevDown.Value * (1 - maxDailyChange) : (double?)null;
            double? maxDown = localPrevDown.HasValue ? localPrevDown.Value * (1 + maxDailyChange) : (double?)null;
            double? uDownToday = PineMath.Clamp(uDownAutoBase, minDown, maxDown);

            // Diagnostic only (Pine 261)
            double? skew = (uUpToday.HasValue && uDownToday.HasValue)
                ? uDownToday.Value / uUpToday.Value
                : (double?)null;

            return new AutoProjectionResult(uUpToday, uDownToday, f, skew);
        }
    }
}
