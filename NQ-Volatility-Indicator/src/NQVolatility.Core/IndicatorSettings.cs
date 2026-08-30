namespace NQVolatility.Core
{
    /// <summary>
    /// The 44 exposed Pine inputs, with their exact defaults. Property names follow
    /// the Pine identifiers so the mapping stays auditable.
    /// </summary>
    public sealed class IndicatorSettings
    {
        // Time Range Settings
        public string TimeZoneId { get; set; } = PineDefaults.TimeZoneId;

        // Projection Mode Configuration
        public int AtrLength { get; set; } = PineDefaults.AtrLength;

        // Range Box Settings
        public bool ShowRangeBox { get; set; } = true;
        public ColorSetting RangeBox { get; set; } = ColorSetting.FromHex("ffffff", 85, 85);

        // Range Level Lines
        public bool ShowHighLowLines { get; set; } = true;
        public bool ShowQuarterLines { get; set; } = true;
        public bool ShowEqLine { get; set; } = true;
        public ColorSetting HighLowLine { get; set; } = ColorSetting.FromHex("e5ff00", 50, 0);
        public ColorSetting QuarterLine { get; set; } = ColorSetting.FromHex("ff0000", 50, 0);
        public ColorSetting EqLine { get; set; } = ColorSetting.FromHex("ff0000", 50, 0);
        public bool ShowRangeLevelLabels { get; set; } = true;
        public PineLabelSize RangeLevelLabelSize { get; set; } = PineLabelSize.Small;

        // Fibonacci Zone Settings
        public bool ShowLevel1 { get; set; } = true;
        public ColorSetting Zone1 { get; set; } = ColorSetting.FromHex("ffffff", 80, 80);
        public ColorSetting Zone1Inner { get; set; } = ColorSetting.FromHex("f23645", 80, 80);
        public bool ShowAvr { get; set; } = true;
        public ColorSetting Zone2 { get; set; } = ColorSetting.FromHex("ff7b00", 80, 80);
        public bool ShowAvrPlus { get; set; } = true;
        public ColorSetting Zone3 { get; set; } = ColorSetting.FromHex("b90000", 80, 80);
        public bool ShowMax { get; set; } = true;
        public ColorSetting Zone4 { get; set; } = ColorSetting.FromHex("7a0000", 80, 80);
        public bool ShowAvrMinus { get; set; } = true;
        public ColorSetting Line300 { get; set; } = ColorSetting.FromHex("ffffff", 50, 0);
        public int Line300Width { get; set; } = 1;
        public PineLineStyle Line300Style { get; set; } = PineLineStyle.Dotted;

        // Zone Label Settings
        public bool ShowZoneLabels { get; set; } = true;
        public PineColor Zone1LabelColor { get; set; } = new PineColor(255, 255, 255, 255);
        public PineColor Zone2LabelColor { get; set; } = new PineColor(255, 255, 255, 255);
        public PineColor Zone3LabelColor { get; set; } = new PineColor(255, 255, 255, 255);
        public PineColor Zone4LabelColor { get; set; } = new PineColor(255, 255, 255, 255);

        // Display Settings
        public int LookbackPeriod { get; set; } = PineDefaults.LookbackPeriod;

        // Information Table
        public bool ShowTable { get; set; } = true;
        public string TablePosition { get; set; } = "Top Right";
        public PineLabelSize TableTextSize { get; set; } = PineLabelSize.Normal;

        // Internal (not exposed) -- Phase 1 keeps these fixed.
        public RangeLabelMode LabelMode { get; } = RangeLabelMode.PineCompatible;
        public PineLabelSize ZoneLabelSize { get; } = PineLabelSize.Small;
    }
}
