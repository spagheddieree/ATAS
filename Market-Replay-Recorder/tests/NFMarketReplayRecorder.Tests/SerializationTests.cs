using System;
using System.Globalization;
using System.Text;

using NFMarketReplayRecorder.Core;
using Xunit;

namespace NFMarketReplayRecorder.Tests
{
    /// <summary>Required area: serialization.</summary>
    public class SerializationTests
    {
        [Fact]
        public void Trade_line_has_exact_expected_shape()
        {
            var e = RawEvent.Trade(7, Sample.Epoch, Sample.Epoch.AddSeconds(1), 20000.25m, 3m, Aggressor.Buy);

            Assert.Equal(
                "{\"recorder_seq\":7,\"kind\":\"trade\"," +
                "\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"," +
                "\"recv_ts\":\"2026-03-10T14:30:01.0000000Z\"," +
                "\"price\":20000.25,\"volume\":3,\"aggressor\":\"buy\"}",
                EventSerializer.ToLine(e));
        }

        [Fact]
        public void Depth_line_has_exact_expected_shape()
        {
            var e = RawEvent.Depth(8, Sample.Epoch, Sample.Epoch, Side.Ask, 20001m, 0m);

            Assert.Equal(
                "{\"recorder_seq\":8,\"kind\":\"depth\"," +
                "\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"," +
                "\"recv_ts\":\"2026-03-10T14:30:00.0000000Z\"," +
                "\"side\":\"ask\",\"price\":20001,\"volume\":0}",
                EventSerializer.ToLine(e));
        }

        [Fact]
        public void Snapshot_line_renders_both_ladders()
        {
            var b = Sample.Book();
            var e = RawEvent.Snapshot(9, Sample.Epoch, Sample.Epoch, b.Bids, b.Asks, 2);

            Assert.Equal(
                "{\"recorder_seq\":9,\"kind\":\"snapshot\"," +
                "\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"," +
                "\"recv_ts\":\"2026-03-10T14:30:00.0000000Z\"," +
                "\"depth_limit\":2," +
                "\"bids\":[[19999.75,10],[19999.5,20]]," +
                "\"asks\":[[20000,15],[20000.25,25]]}",
                EventSerializer.ToLine(e));
        }

        [Fact]
        public void Empty_ladders_render_as_empty_arrays()
        {
            var e = RawEvent.Snapshot(1, Sample.Epoch, Sample.Epoch, new DomLevel[0], new DomLevel[0], 0);
            Assert.Contains("\"bids\":[],\"asks\":[]", EventSerializer.ToLine(e));
        }

        [Theory]
        // Trailing fractional zeros must be stripped so the same value at two
        // different decimal scales cannot produce two different lines.
        [InlineData("1.50", "1.5")]
        [InlineData("1.500000", "1.5")]
        [InlineData("20000.00", "20000")]
        [InlineData("0.0", "0")]
        [InlineData("-0.00", "0")]
        [InlineData("-1.250", "-1.25")]
        [InlineData("0.0000000001", "0.0000000001")]
        public void Decimal_formatting_is_canonical(string input, string expected)
        {
            decimal d = decimal.Parse(input, CultureInfo.InvariantCulture);
            Assert.Equal(expected, JsonLine.FormatDecimal(d));
        }

        [Fact]
        public void Decimal_scale_difference_cannot_change_a_line()
        {
            var a = RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 20000.00m, 1.0m, Aggressor.Buy);
            var b = RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 20000m, 1m, Aggressor.Buy);
            Assert.Equal(EventSerializer.ToLine(a), EventSerializer.ToLine(b));
        }

        [Fact]
        public void Strings_escape_the_json_specials()
        {
            var sb = new StringBuilder();
            JsonLine.WriteString(sb, "a\"b\\c\nd\tef");
            Assert.Equal("\"a\\\"b\\\\c\\nd\\tef\"", sb.ToString());
        }

        [Fact]
        public void Strings_escape_other_control_characters_as_unicode()
        {
            var sb = new StringBuilder();
            JsonLine.WriteString(sb, "x" + (char)0x01 + (char)0x7F + "y");
            Assert.Equal("\"x\\u0001\\u007fy\"", sb.ToString());
        }

        [Fact]
        public void Null_aggressor_serializes_as_unknown_never_as_null()
        {
            var e = RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 1m, 1m, null);
            Assert.Contains("\"aggressor\":\"unknown\"", EventSerializer.ToLine(e));
        }

        [Fact]
        public void Canonical_form_drops_seq_and_recv_ts_and_nothing_else()
        {
            var e = RawEvent.Trade(42, Sample.Epoch, Sample.Epoch.AddHours(5), 20000.25m, 3m, Aggressor.Sell);

            string canon = EventSerializer.ToCanonical(e);

            Assert.DoesNotContain("\"recorder_seq\"", canon);
            Assert.DoesNotContain("\"recv_ts\"", canon);
            Assert.Contains("\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"", canon);
            Assert.Contains("\"price\":20000.25", canon);
            Assert.Contains("\"aggressor\":\"sell\"", canon);
        }

        [Fact]
        public void Canonicalizing_a_written_line_equals_the_canonical_writer()
        {
            // The comparer strips fields textually for speed; that shortcut must agree
            // with the structured writer on every event kind, or two captures could be
            // judged equal on a projection that is not the one documented.
            var book = Sample.Book();
            var events = new[]
            {
                RawEvent.Trade(1, Sample.Epoch, Sample.Epoch.AddHours(2), 20000.25m, 3m, Aggressor.Buy),
                RawEvent.Depth(2, Sample.Epoch, Sample.Epoch.AddHours(2), Side.Bid, 19999.5m, 0m),
                RawEvent.Snapshot(3, Sample.Epoch, Sample.Epoch.AddHours(2), book.Bids, book.Asks, 2),
            };

            foreach (var e in events)
            {
                Assert.Equal(
                    EventSerializer.ToCanonical(e),
                    StreamComparer.Canonicalize(EventSerializer.ToLine(e)));
            }
        }

        [Fact]
        public void Raw_schema_carries_no_derived_field()
        {
            // The brief forbids delta, imbalance and ratios. This asserts the ban
            // structurally rather than trusting review: any such field added to the
            // raw schema fails here.
            var book = Sample.Book();
            string[] banned = { "delta", "imbalance", "ratio", "cvd", "vwap", "poc", "average", "cumulative", "total", "net" };

            foreach (var e in new[]
            {
                RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 1m, 1m, Aggressor.Buy),
                RawEvent.Depth(2, Sample.Epoch, Sample.Epoch, Side.Bid, 1m, 1m),
                RawEvent.Snapshot(3, Sample.Epoch, Sample.Epoch, book.Bids, book.Asks, 2),
            })
            {
                string line = EventSerializer.ToLine(e);
                foreach (var word in banned)
                    Assert.DoesNotContain("\"" + word, line, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
