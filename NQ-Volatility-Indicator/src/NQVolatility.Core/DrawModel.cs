using System;
using System.Collections.Generic;

namespace NQVolatility.Core
{
    public enum PrimitiveKind { Box, Line, Label }

    /// <summary>Anchor for a label, mirroring Pine's label styles used here.</summary>
    public enum LabelAnchor
    {
        /// <summary>Pine <c>label.style_label_left</c> -- text sits to the right of x.</summary>
        Left,
        /// <summary>Pine <c>label.style_text_outline</c> -- text centred on x.</summary>
        Center
    }

    /// <summary>
    /// One thing to draw, in chart coordinates (UTC time on x, price on y).
    /// Deliberately platform independent: the ATAS adapter translates these to
    /// RenderRectangle / RenderLine / RenderString and nothing else.
    /// </summary>
    public readonly struct DrawPrimitive
    {
        private DrawPrimitive(PrimitiveKind kind, string id, DateTime from, DateTime to,
            double y1, double y2, PineColor color, PineLineStyle style, int width,
            string? text, PineLabelSize size, LabelAnchor anchor)
        {
            Kind = kind; Id = id; From = from; To = to;
            Y1 = y1; Y2 = y2; Color = color; Style = style; Width = width;
            Text = text; Size = size; Anchor = anchor;
        }

        public PrimitiveKind Kind { get; }

        /// <summary>Stable identity, e.g. "upZone2" -- lets the adapter diff instead of rebuild.</summary>
        public string Id { get; }

        public DateTime From { get; }
        public DateTime To { get; }

        /// <summary>Box: the LOWER price. Line/Label: the price.</summary>
        public double Y1 { get; }

        /// <summary>Box: the UPPER price. Unused otherwise.</summary>
        public double Y2 { get; }

        public PineColor Color { get; }
        public PineLineStyle Style { get; }
        public int Width { get; }
        public string? Text { get; }
        public PineLabelSize Size { get; }
        public LabelAnchor Anchor { get; }

        /// <summary>
        /// Box primitive. Pine declares several zone boxes with top &lt; bottom
        /// (quirk Q-09); this normalises so Y1 &lt;= Y2 and the adapter never has to.
        /// </summary>
        public static DrawPrimitive Box(string id, DateTime from, DateTime to, double a, double b, PineColor color)
            => new DrawPrimitive(PrimitiveKind.Box, id, from, to,
                Math.Min(a, b), Math.Max(a, b), color, PineLineStyle.Solid, 0, null, PineLabelSize.Normal, LabelAnchor.Left);

        public static DrawPrimitive Line(string id, DateTime from, DateTime to, double y,
            PineColor color, PineLineStyle style, int width)
            => new DrawPrimitive(PrimitiveKind.Line, id, from, to, y, y, color, style, width, null, PineLabelSize.Normal, LabelAnchor.Left);

        public static DrawPrimitive Label(string id, DateTime at, double y, string text,
            PineColor color, PineLabelSize size, LabelAnchor anchor)
            => new DrawPrimitive(PrimitiveKind.Label, id, at, at, y, y, color, PineLineStyle.Solid, 0, text, size, anchor);
    }

    /// <summary>
    /// Turns a locked <see cref="RangeSession"/> plus settings into the exact set of
    /// primitives Pine would have created. All the visual quirks live here, so they
    /// are unit-testable without ATAS.
    /// </summary>
    public static class SessionDrawModel
    {
        /// <summary>Pine 422: labels sit 5 minutes to the right of the line end.</summary>
        public static readonly TimeSpan LabelOffset = TimeSpan.FromMilliseconds(PineDefaults.LabelOffsetMs);

        public static IReadOnlyList<DrawPrimitive> Build(RangeSession s, IndicatorSettings cfg)
        {
            var result = new List<DrawPrimitive>();
            if (!s.ProjectionsLocked || s.Levels is null || s.DrawFrom is null || s.DrawTo is null)
                return result;

            var from = s.DrawFrom.Value;
            var to = s.DrawTo.Value;
            var labelX = to + LabelOffset;
            var levels = s.Levels.Value;

            // ---- range box (Pine 846-849) -----------------------------------
            if (cfg.ShowRangeBox)
                result.Add(DrawPrimitive.Box("rangeBox", s.StartTime, s.EndTime,
                    levels.Low, levels.High, cfg.RangeBox.ForShape));

            // ---- range level lines + labels (Pine 417-453) -------------------
            if (cfg.ShowHighLowLines)
            {
                result.Add(DrawPrimitive.Line("highLine", from, to, levels.High, cfg.HighLowLine.ForShape, PineLineStyle.Solid, 1));
                result.Add(DrawPrimitive.Line("lowLine", from, to, levels.Low, cfg.HighLowLine.ForShape, PineLineStyle.Solid, 1));
                if (cfg.ShowRangeLevelLabels)
                {
                    result.Add(DrawPrimitive.Label("highLabel", labelX, levels.High,
                        RangeLevelLabelMap.HighLabel(cfg.LabelMode), cfg.HighLowLine.ForLabel, cfg.RangeLevelLabelSize, LabelAnchor.Left));
                    result.Add(DrawPrimitive.Label("lowLabel", labelX, levels.Low,
                        RangeLevelLabelMap.LowLabel(cfg.LabelMode), cfg.HighLowLine.ForLabel, cfg.RangeLevelLabelSize, LabelAnchor.Left));
                }
            }

            if (cfg.ShowQuarterLines)
            {
                result.Add(DrawPrimitive.Line("q1Line", from, to, levels.Q1Price, cfg.QuarterLine.ForShape, PineLineStyle.Dotted, 1));
                result.Add(DrawPrimitive.Line("q3Line", from, to, levels.Q3Price, cfg.QuarterLine.ForShape, PineLineStyle.Dotted, 1));
                if (cfg.ShowRangeLevelLabels)
                {
                    // Quirk Q-03: the 25% price carries "Q3" and the 75% price carries "Q1".
                    result.Add(DrawPrimitive.Label("q1Label", labelX, levels.Q1Price,
                        RangeLevelLabelMap.LabelForQ1Price(cfg.LabelMode), cfg.QuarterLine.ForLabel, cfg.RangeLevelLabelSize, LabelAnchor.Left));
                    result.Add(DrawPrimitive.Label("q3Label", labelX, levels.Q3Price,
                        RangeLevelLabelMap.LabelForQ3Price(cfg.LabelMode), cfg.QuarterLine.ForLabel, cfg.RangeLevelLabelSize, LabelAnchor.Left));
                }
            }

            if (cfg.ShowEqLine)
            {
                result.Add(DrawPrimitive.Line("eqLine", from, to, levels.EqPrice, cfg.EqLine.ForShape, PineLineStyle.Solid, 1));
                if (cfg.ShowRangeLevelLabels)
                    result.Add(DrawPrimitive.Label("eqLabel", labelX, levels.EqPrice,
                        RangeLevelLabelMap.EqLabel(cfg.LabelMode), cfg.EqLine.ForLabel, cfg.RangeLevelLabelSize, LabelAnchor.Left));
            }

            // ---- projection zones (Pine 456-590) -----------------------------
            if (s.Projections is null || s.Visibility is null) return result;
            var p = s.Projections.Value;
            var v = s.Visibility.Value;

            // Pine 514: zoneLabelPosition == "center".
            var centerX = new DateTime((from.Ticks + to.Ticks) / 2, DateTimeKind.Utc);

            void Zone(string id, bool enabled, bool visible, double a, double b,
                ColorSetting fill, string text, PineColor labelColor)
            {
                if (!enabled || !visible) return;
                result.Add(DrawPrimitive.Box(id, from, to, a, b, fill.ForShape));
                if (cfg.ShowZoneLabels)
                    result.Add(DrawPrimitive.Label(id + "Label", centerX, (a + b) / 2.0,
                        text, labelColor, cfg.ZoneLabelSize, LabelAnchor.Center));
            }

            // Level 1: outer box, then the inner shade drawn on top (Pine 509-511).
            if (cfg.ShowLevel1 && v.ShowUpLevel1)
            {
                result.Add(DrawPrimitive.Box("upZone1", from, to, p.Up200, p.Up250, cfg.Zone1.ForShape));
                result.Add(DrawPrimitive.Box("upZone1Inner", from, to, p.Up233, p.Up250, cfg.Zone1Inner.ForShape));
                if (cfg.ShowZoneLabels)
                    result.Add(DrawPrimitive.Label("upZone1Label", centerX, (p.Up200 + p.Up250) / 2.0,
                        "Level 1", cfg.Zone1LabelColor, cfg.ZoneLabelSize, LabelAnchor.Center));
            }
            Zone("upZone2", cfg.ShowAvr, v.ShowUpAvr, p.Up400, p.Up450, cfg.Zone2, "AVR", cfg.Zone2LabelColor);
            Zone("upZone3", cfg.ShowAvrPlus, v.ShowUpAvrPlus, p.Up600, p.Up650, cfg.Zone3, "AVR+", cfg.Zone3LabelColor);
            Zone("upZone4", cfg.ShowMax, v.ShowUpMax, p.Up800, p.Up850, cfg.Zone4, "MAX", cfg.Zone4LabelColor);

            if (cfg.ShowAvrMinus && v.ShowUpLine300)
            {
                result.Add(DrawPrimitive.Line("upLine300", from, to, p.Up300,
                    cfg.Line300.ForShape, cfg.Line300Style, cfg.Line300Width));
                if (cfg.ShowZoneLabels)
                    result.Add(DrawPrimitive.Label("upLine300Label", labelX, p.Up300, "AVR-",
                        cfg.Line300.ForLabel, cfg.ZoneLabelSize, LabelAnchor.Left));
            }

            if (cfg.ShowLevel1 && v.ShowDownLevel1)
            {
                result.Add(DrawPrimitive.Box("downZone1", from, to, p.Down250, p.Down200, cfg.Zone1.ForShape));
                result.Add(DrawPrimitive.Box("downZone1Inner", from, to, p.Down250, p.Down233, cfg.Zone1Inner.ForShape));
                if (cfg.ShowZoneLabels)
                    result.Add(DrawPrimitive.Label("downZone1Label", centerX, (p.Down200 + p.Down250) / 2.0,
                        "Level 1", cfg.Zone1LabelColor, cfg.ZoneLabelSize, LabelAnchor.Center));
            }
            Zone("downZone2", cfg.ShowAvr, v.ShowDownAvr, p.Down450, p.Down400, cfg.Zone2, "AVR", cfg.Zone2LabelColor);
            Zone("downZone3", cfg.ShowAvrPlus, v.ShowDownAvrPlus, p.Down650, p.Down600, cfg.Zone3, "AVR+", cfg.Zone3LabelColor);
            Zone("downZone4", cfg.ShowMax, v.ShowDownMax, p.Down850, p.Down800, cfg.Zone4, "MAX", cfg.Zone4LabelColor);

            if (cfg.ShowAvrMinus && v.ShowDownLine300)
            {
                result.Add(DrawPrimitive.Line("downLine300", from, to, p.Down300,
                    cfg.Line300.ForShape, cfg.Line300Style, cfg.Line300Width));
                if (cfg.ShowZoneLabels)
                    result.Add(DrawPrimitive.Label("downLine300Label", labelX, p.Down300, "AVR-",
                        cfg.Line300.ForLabel, cfg.ZoneLabelSize, LabelAnchor.Left));
            }

            return result;
        }
    }
}
