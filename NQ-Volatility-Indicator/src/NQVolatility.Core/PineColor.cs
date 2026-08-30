namespace NQVolatility.Core
{
    /// <summary>Straight RGBA colour, platform independent.</summary>
    public readonly struct PineColor
    {
        public PineColor(byte r, byte g, byte b, byte a)
        {
            R = r; G = g; B = b; A = a;
        }

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        /// <summary>
        /// Pine <c>color.new(rgb, transparency)</c>. Transparency is 0 = opaque,
        /// 100 = invisible -- the INVERSE of what the inputs are labelled (quirk Q-05).
        /// </summary>
        public static PineColor FromRgbTransparency(byte r, byte g, byte b, int transparency)
        {
            if (transparency < 0) transparency = 0;
            if (transparency > 100) transparency = 100;
            var a = (byte)System.Math.Round(255.0 * (100 - transparency) / 100.0);
            return new PineColor(r, g, b, a);
        }

        public static PineColor FromHex(string hex, int transparency)
        {
            var h = hex.TrimStart('#');
            var r = byte.Parse(h.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(h.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(h.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return FromRgbTransparency(r, g, b, transparency);
        }

        /// <summary>Pine <c>color.new(existing, t)</c> -- REPLACES the transparency.</summary>
        public PineColor WithTransparency(int transparency)
            => FromRgbTransparency(R, G, B, transparency);

        /// <summary>Transparency (0-100) implied by the current alpha.</summary>
        public int Transparency => (int)System.Math.Round(100.0 - A * 100.0 / 255.0);

        public override string ToString() => $"#{R:X2}{G:X2}{B:X2} a={A}";
    }

    /// <summary>
    /// A Pine colour input paired with its separate "Opacity (%)" int input.
    /// <para>
    /// Encodes quirk Q-06: shapes re-wrap the colour through
    /// <c>color.new(input, opacityInput)</c>, discarding the input's own alpha,
    /// while LABELS use the raw input colour including its alpha.
    /// </para>
    /// </summary>
    public readonly struct ColorSetting
    {
        public ColorSetting(PineColor raw, int opacityInput)
        {
            Raw = raw; OpacityInput = opacityInput;
        }

        public static ColorSetting FromHex(string hex, int defaultTransparency, int opacityInput)
            => new ColorSetting(PineColor.FromHex(hex, defaultTransparency), opacityInput);

        public PineColor Raw { get; }
        public int OpacityInput { get; }

        /// <summary>Colour actually used for boxes and lines.</summary>
        public PineColor ForShape => Raw.WithTransparency(OpacityInput);

        /// <summary>Colour actually used for label text (raw, alpha preserved).</summary>
        public PineColor ForLabel => Raw;
    }

    public enum PineLabelSize { Tiny, Small, Normal, Large, Huge }

    public enum PineLineStyle { Solid, Dotted, Dashed, ArrowLeft, ArrowRight, ArrowBoth }

    public static class PineLabelSizes
    {
        /// <summary>
        /// Approximate pixel ladder for Pine's size constants. TradingView's exact
        /// font metrics are internal, so text extents WILL differ slightly
        /// (PINE-PARITY-SPEC 25.3).
        /// </summary>
        public static int ToPixels(PineLabelSize size)
        {
            switch (size)
            {
                case PineLabelSize.Tiny: return 9;
                case PineLabelSize.Small: return 11;
                case PineLabelSize.Normal: return 13;
                case PineLabelSize.Large: return 16;
                case PineLabelSize.Huge: return 20;
                default: return 13;
            }
        }

        public static PineLabelSize Parse(string s)
        {
            switch (s)
            {
                case "tiny": case "Tiny": return PineLabelSize.Tiny;
                case "small": case "Small": return PineLabelSize.Small;
                case "large": case "Large": return PineLabelSize.Large;
                case "huge": case "Huge": return PineLabelSize.Huge;
                default: return PineLabelSize.Normal;
            }
        }

        public static PineLineStyle ParseLineStyle(string s)
        {
            switch (s)
            {
                case "Dotted": return PineLineStyle.Dotted;
                case "Dashed": return PineLineStyle.Dashed;
                case "Arrow Left": return PineLineStyle.ArrowLeft;
                case "Arrow Right": return PineLineStyle.ArrowRight;
                case "Arrow Both": return PineLineStyle.ArrowBoth;
                default: return PineLineStyle.Solid;
            }
        }
    }
}
