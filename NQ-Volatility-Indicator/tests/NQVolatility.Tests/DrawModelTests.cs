using System;
using System.Linq;
using NQVolatility.Core;
using Xunit;

namespace NQVolatility.Tests
{
    public class DrawModelTests
    {
        private static SessionEngine Run(double? atr = 300.0)
        {
            var engine = new SessionEngine(Build.Shipped());
            foreach (var b in Build.HourlyBars(Tz.LosAngeles, new DateTime(2024, 1, 8), 1, Build.AsiaRange))
                engine.OnBar(b, atr);
            return engine;
        }

        // ---- Quirk Q-05: "Opacity" is really transparency -------------------
        [Fact]
        public void OpacityInput_IsAppliedAsTransparency()
        {
            // Opacity 0 -> fully OPAQUE.
            Assert.Equal(255, ColorSetting.FromHex("e5ff00", 50, 0).ForShape.A);
            // Opacity 100 -> fully INVISIBLE.
            Assert.Equal(0, ColorSetting.FromHex("e5ff00", 50, 100).ForShape.A);
            // Opacity 85 -> 15% opaque.
            Assert.Equal(38, ColorSetting.FromHex("ffffff", 85, 85).ForShape.A);
        }

        // ---- Quirk Q-06: input alpha discarded for shapes, kept for labels ---
        [Fact]
        public void ShapeDiscardsInputAlpha_ButLabelKeepsIt()
        {
            var c = ColorSetting.FromHex("e5ff00", 50, 0); // default H/L: 50 transp, opacity 0

            Assert.Equal(255, c.ForShape.A);  // line is fully opaque
            Assert.Equal(128, c.ForLabel.A);  // label text is 50% transparent

            // RGB survives both paths.
            Assert.Equal(0xE5, c.ForShape.R);
            Assert.Equal(0xE5, c.ForLabel.R);
        }

        [Fact]
        public void DefaultSettings_MatchThePineDefaults()
        {
            var cfg = new IndicatorSettings();
            Assert.Equal("America/Los_Angeles", cfg.TimeZoneId);
            Assert.Equal(20, cfg.AtrLength);
            Assert.Equal(3, cfg.LookbackPeriod);
            Assert.Equal(PineLineStyle.Dotted, cfg.Line300Style);
            Assert.Equal(1, cfg.Line300Width);
            Assert.Equal(PineLabelSize.Small, cfg.RangeLevelLabelSize);
            Assert.Equal(PineLabelSize.Normal, cfg.TableTextSize);
            Assert.Equal("Top Right", cfg.TablePosition);
            Assert.Equal(RangeLabelMode.PineCompatible, cfg.LabelMode);
        }

        // ---- Quirk Q-09: inverted boxes are normalised ----------------------
        [Fact]
        public void Boxes_AreNormalisedSoY1IsAlwaysTheLowerPrice()
        {
            var s = Run().Sessions[0];
            var prims = SessionDrawModel.Build(s, new IndicatorSettings());
            Assert.All(prims.Where(p => p.Kind == PrimitiveKind.Box),
                p => Assert.True(p.Y1 <= p.Y2, $"{p.Id}: Y1={p.Y1} > Y2={p.Y2}"));
        }

        // ---- Quirk Q-03 reaches the draw model ------------------------------
        [Fact]
        public void QuarterLabels_AreTransposedInTheDrawModel()
        {
            var s = Run().Sessions[0];
            var prims = SessionDrawModel.Build(s, new IndicatorSettings());

            var q1 = prims.Single(p => p.Id == "q1Label"); // drawn at the 25% price
            var q3 = prims.Single(p => p.Id == "q3Label"); // drawn at the 75% price

            Assert.Equal(19950, q1.Y1, 9);
            Assert.Equal("Q3", q1.Text);
            Assert.Equal(20050, q3.Y1, 9);
            Assert.Equal("Q1", q3.Text);
        }

        [Fact]
        public void Labels_SitFiveMinutesRightOfTheLineEnd()
        {
            var s = Run().Sessions[0];
            var prims = SessionDrawModel.Build(s, new IndicatorSettings());
            var high = prims.Single(p => p.Id == "highLabel");
            Assert.Equal(s.DrawTo!.Value.AddMinutes(5), high.From);
        }

        [Fact]
        public void SuppressedZones_ProduceNeitherBoxNorLabel()
        {
            var s = Run().Sessions[0];
            var v = s.Visibility!.Value;
            var prims = SessionDrawModel.Build(s, new IndicatorSettings());

            // Baseline fixture: Level 1, AVR- and AVR are suppressed both ways.
            Assert.False(v.ShowUpLevel1);
            Assert.False(v.ShowUpAvr);
            Assert.DoesNotContain(prims, p => p.Id == "upZone1");
            Assert.DoesNotContain(prims, p => p.Id == "upZone1Inner");
            Assert.DoesNotContain(prims, p => p.Id == "upZone1Label");
            Assert.DoesNotContain(prims, p => p.Id == "upZone2");
            Assert.DoesNotContain(prims, p => p.Id == "upLine300");

            // AVR+ and MAX clear the range and are drawn with their labels.
            Assert.Contains(prims, p => p.Id == "upZone3");
            Assert.Contains(prims, p => p.Id == "upZone3Label");
            Assert.Contains(prims, p => p.Id == "downZone4");
        }

        [Fact]
        public void ZoneLabels_AreCentredOnTheDrawnExtent()
        {
            var s = Run().Sessions[0];
            var prims = SessionDrawModel.Build(s, new IndicatorSettings());
            var label = prims.Single(p => p.Id == "upZone3Label");
            var expected = new DateTime((s.DrawFrom!.Value.Ticks + s.DrawTo!.Value.Ticks) / 2, DateTimeKind.Utc);
            Assert.Equal(expected, label.From);
            Assert.Equal(LabelAnchor.Center, label.Anchor);

            // y is the midpoint of the OUTER boundaries.
            var p = s.Projections!.Value;
            Assert.Equal((p.Up600 + p.Up650) / 2.0, label.Y1, 9);
        }

        [Fact]
        public void VisibilityToggles_RemoveTheirPrimitives()
        {
            var s = Run().Sessions[0];

            var all = SessionDrawModel.Build(s, new IndicatorSettings());
            Assert.Contains(all, p => p.Id == "rangeBox");
            Assert.Contains(all, p => p.Id == "eqLine");

            var off = SessionDrawModel.Build(s, new IndicatorSettings
            {
                ShowRangeBox = false,
                ShowEqLine = false,
                ShowQuarterLines = false,
                ShowHighLowLines = false,
                ShowZoneLabels = false
            });
            Assert.DoesNotContain(off, p => p.Id == "rangeBox");
            Assert.DoesNotContain(off, p => p.Id == "eqLine");
            Assert.DoesNotContain(off, p => p.Id == "q1Label");
            Assert.DoesNotContain(off, p => p.Id == "upZone3Label");
            Assert.Contains(off, p => p.Id == "upZone3"); // the box itself remains
        }

        [Fact]
        public void UnlockedSession_DrawsNothing()
        {
            var engine = new SessionEngine(Build.Shipped());
            var pt = new PineTime(Tz.LosAngeles);
            engine.OnBar(new Ohlc(pt.Timestamp(2024, 1, 8, 18, 0), 20000, 20050, 19950, 20000), 300);

            var s = engine.CurrentSession!;
            Assert.False(s.ProjectionsLocked);
            Assert.Empty(SessionDrawModel.Build(s, new IndicatorSettings()));
        }

        [Fact]
        public void MissingAtr_DrawsRangeLevelsButNoZones()
        {
            var s = Run(atr: null).Sessions[0];
            var prims = SessionDrawModel.Build(s, new IndicatorSettings());

            Assert.Contains(prims, p => p.Id == "highLine");
            Assert.Contains(prims, p => p.Id == "eqLine");
            Assert.DoesNotContain(prims, p => p.Id.StartsWith("upZone"));
            Assert.DoesNotContain(prims, p => p.Id.StartsWith("downZone"));
            Assert.DoesNotContain(prims, p => p.Id == "upLine300");
        }

        [Fact]
        public void RangeBoxSpansSessionStartToSessionEnd_NotTheDrawnExtent()
        {
            var s = Run().Sessions[0];
            var box = SessionDrawModel.Build(s, new IndicatorSettings()).Single(p => p.Id == "rangeBox");
            Assert.Equal(s.StartTime, box.From);
            Assert.Equal(s.EndTime, box.To);  // Pine 847 snaps it back to the nominal end
            Assert.Equal(19900, box.Y1, 9);
            Assert.Equal(20100, box.Y2, 9);
        }

        [Fact]
        public void LineStyleParsing_CoversEveryPineOption()
        {
            Assert.Equal(PineLineStyle.Solid, PineLabelSizes.ParseLineStyle("Solid"));
            Assert.Equal(PineLineStyle.Dotted, PineLabelSizes.ParseLineStyle("Dotted"));
            Assert.Equal(PineLineStyle.Dashed, PineLabelSizes.ParseLineStyle("Dashed"));
            Assert.Equal(PineLineStyle.ArrowLeft, PineLabelSizes.ParseLineStyle("Arrow Left"));
            Assert.Equal(PineLineStyle.ArrowRight, PineLabelSizes.ParseLineStyle("Arrow Right"));
            Assert.Equal(PineLineStyle.ArrowBoth, PineLabelSizes.ParseLineStyle("Arrow Both"));
            Assert.Equal(PineLineStyle.Solid, PineLabelSizes.ParseLineStyle("nonsense"));
        }

        [Fact]
        public void LabelSizeParsing_CoversEveryPineOption()
        {
            Assert.Equal(PineLabelSize.Tiny, PineLabelSizes.Parse("tiny"));
            Assert.Equal(PineLabelSize.Small, PineLabelSizes.Parse("Small"));
            Assert.Equal(PineLabelSize.Normal, PineLabelSizes.Parse("Normal"));
            Assert.Equal(PineLabelSize.Large, PineLabelSizes.Parse("large"));
            Assert.Equal(PineLabelSize.Huge, PineLabelSizes.Parse("Huge"));
        }
    }
}
