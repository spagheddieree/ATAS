namespace NQVolatility.Core
{
    /// <summary>
    /// Every constant baked into the Pine source, with the line it comes from.
    /// Values here are Phase 1 parity constants and must not be "improved".
    /// </summary>
    public static class PineDefaults
    {
        // --- Session (Pine 10, 13, 17-20) -----------------------------------
        public const string RangePreset = "Asia";      // line 13, hard-fixed
        public const string AsiaSession = "1700-2100"; // line 10
        public const string LondonSession = "0000-0600"; // line 14, unreachable
        public const string NyAmSession = "0630-0900";   // line 15, unreachable
        public const string NyPmSession = "0900-1200";   // line 16, unreachable
        public const int FallbackStartHour = 17;   // line 17
        public const int FallbackStartMinute = 0;  // line 18
        public const int FallbackEndHour = 21;     // line 19
        public const int FallbackEndMinute = 0;    // line 20

        // --- Lock time (Pine 42-43) ------------------------------------------
        public const int LockHour = 21;
        public const int LockMinute = 0;

        // --- Projection (Pine 27-34) -----------------------------------------
        public const string ProjectionMode = "Auto"; // line 27, hard-fixed
        public const double BaselineVol = 300.0;     // line 28
        public const double BaseUpRef = 45.0;        // line 29
        public const double BaseDownRef = 48.0;      // line 30
        public const double ScaleClampLow = 0.80;    // line 32
        public const double ScaleClampHigh = 1.40;   // line 33
        public const double MaxDailyChange = 0.40;   // line 34

        // --- Exposed input defaults ------------------------------------------
        public const string TimeZoneId = "America/Los_Angeles"; // line 7
        public const int AtrLength = 20;                        // line 24
        public const int LookbackPeriod = 3;                    // line 143

        // --- Label offset (Pine 422) -----------------------------------------
        public const int LabelOffsetMs = 5 * 60 * 1000;

        // --- DEAD constants, preserved for documentation only -----------------
        // Pine 31: skewConst = 1.067  -- never read (D-02)
        public const double DeadSkewConst = 1.067;
        // Pine 46: roundTo = 0.25     -- never read (D-01); NO ROUNDING OCCURS
        public const double DeadRoundTo = 0.25;
        // Pine 37-38: manual mode distances -- dead branch (D-03)
        public const double DeadUpPointDistance = 45.0;
        public const double DeadDownPointDistance = 48.0;
    }

    /// <summary>Fibonacci projection multipliers (Pine 126-139).</summary>
    public static class FibMultipliers
    {
        // Stored negative in Pine, consumed through math.abs().
        public const double Fib200 = -2.0;
        public const double Fib233 = -2.33;
        public const double Fib250 = -2.5;
        public const double Fib300 = -3.0;
        public const double Fib400 = -4.0;
        public const double Fib450 = -4.5;
        public const double Fib600 = -6.0;
        public const double Fib650 = -6.5;

        /// <summary>
        /// QUIRK Q-07: named "800" but the value is 7.78 (Pine line 138).
        /// The value is what executes.
        /// </summary>
        public const double Fib800 = -7.78;
        public const double Fib850 = -8.5;

        public static readonly double[] All =
            { Fib200, Fib233, Fib250, Fib300, Fib400, Fib450, Fib600, Fib650, Fib800, Fib850 };
    }
}
