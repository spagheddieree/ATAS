using System;

using NFMarketDataRecorder.Core;
using Xunit;

namespace NFMarketDataRecorder.Tests
{
    /// <summary>Required area: timestamp handling.</summary>
    public class TimestampTests
    {
        [Fact]
        public void Times_render_as_fixed_width_utc_with_tick_resolution()
        {
            var t = new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc).AddTicks(1);
            Assert.Equal("2026-03-10T14:30:00.0000001Z", JsonLine.FormatTime(t));
        }

        [Fact]
        public void Time_text_sorts_in_value_order()
        {
            // Fixed width is not cosmetic: it lets a capture be checked for source
            // time monotonicity with a text sort and no parsing at all.
            var a = JsonLine.FormatTime(new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc).AddTicks(9));
            var b = JsonLine.FormatTime(new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc).AddTicks(10));
            Assert.True(string.CompareOrdinal(a, b) < 0);
        }

        [Fact]
        public void Local_times_are_converted_not_relabelled()
        {
            var utc = new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc);
            var local = utc.ToLocalTime();
            Assert.Equal(JsonLine.FormatTime(utc), JsonLine.FormatTime(local));
        }

        [Fact]
        public void Unspecified_kind_is_treated_as_utc_not_shifted_by_the_host_timezone()
        {
            // Reinterpreting an Unspecified stamp in the machine's local zone would
            // shift every timestamp in the capture by the local offset, and two
            // captures taken on differently configured machines would never compare.
            using var dir = new TempDir();
            var sink = new MemorySink();
            var unspecified = new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Unspecified);

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(unspecified, 1m, 1m, Aggressor.Buy);
            rec.Complete();

            Assert.Single(sink.Lines);
            Assert.Contains("\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"", sink.Lines[0]);
        }

        [Fact]
        public void Source_time_regression_is_recorded_but_never_repaired()
        {
            // Clamping an out-of-order timestamp would erase the exact defect this
            // tool exists to detect, so the stamp is preserved and a fault raised.
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch.AddSeconds(5), 1m, 1m, Aggressor.Buy);
            rec.OnTrade(Sample.Epoch.AddSeconds(2), 2m, 1m, Aggressor.Sell);
            rec.Complete();

            Assert.Equal(1, rec.Faults.CountOf(FaultCode.SourceTimeRegression));
            Assert.Contains("\"src_ts\":\"2026-03-10T14:30:02.0000000Z\"", sink.Lines[1]);
        }

        [Fact]
        public void An_event_with_no_source_time_is_refused_and_faulted()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(default, 1m, 1m, Aggressor.Buy);
            rec.Complete();

            Assert.Empty(sink.Lines);
            Assert.Equal(1, rec.Faults.CountOf(FaultCode.MissingSourceTime));
        }

        [Fact]
        public void Wall_clock_never_influences_a_source_timestamp()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.Complete();

            string line = sink.Lines[0];
            Assert.Contains("\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"", line);

            // recv_ts is present and is genuinely the wall clock, i.e. it is not the
            // source stamp copied across.
            string recv = Between(line, "\"recv_ts\":\"", "\"");
            Assert.NotEqual("2026-03-10T14:30:00.0000000Z", recv);
        }

        private static string Between(string s, string open, string close)
        {
            int a = s.IndexOf(open, StringComparison.Ordinal) + open.Length;
            int b = s.IndexOf(close, a, StringComparison.Ordinal);
            return s.Substring(a, b - a);
        }

        private static RecorderOptions Options(TempDir dir) => new RecorderOptions
        {
            OutputDirectory = dir.Path,
            SnapshotInterval = TimeSpan.FromHours(1),
            DrainTimeout = TimeSpan.FromSeconds(10),
        };
    }
}
