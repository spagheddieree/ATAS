using System;
using System.Collections.Generic;
using System.IO;

using NFMarketReplayRecorder.Core;
using NFMarketReplayRecorder.Harness;
using Xunit;

namespace NFMarketReplayRecorder.Tests
{
    /// <summary>Required area: deterministic comparisons.</summary>
    public class DeterministicComparisonTests
    {
        // ---------------------------------------------------------- canonicalize

        [Fact]
        public void Canonicalize_removes_only_recorder_seq_and_recv_ts()
        {
            string line = "{\"recorder_seq\":5,\"kind\":\"trade\",\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"," +
                          "\"recv_ts\":\"2026-09-11T00:00:00.0000000Z\",\"price\":1,\"volume\":2,\"aggressor\":\"buy\"}";

            Assert.Equal(
                "{\"kind\":\"trade\",\"src_ts\":\"2026-03-10T14:30:00.0000000Z\"," +
                "\"price\":1,\"volume\":2,\"aggressor\":\"buy\"}",
                StreamComparer.Canonicalize(line));
        }

        [Fact]
        public void Two_lines_differing_only_in_recorder_seq_and_recv_ts_are_canonically_equal()
        {
            var a = RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 20000.25m, 3m, Aggressor.Buy);
            var b = RawEvent.Trade(987654, Sample.Epoch, Sample.Epoch.AddDays(30), 20000.25m, 3m, Aggressor.Buy);

            Assert.Equal(
                StreamComparer.Canonicalize(EventSerializer.ToLine(a)),
                StreamComparer.Canonicalize(EventSerializer.ToLine(b)));
        }

        [Fact]
        public void Extracting_source_timestamp_and_kind_works_on_every_event_kind()
        {
            var book = Sample.Book();
            foreach (var e in new[]
            {
                RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 1m, 1m, Aggressor.Buy),
                RawEvent.Depth(2, Sample.Epoch, Sample.Epoch, Side.Bid, 1m, 1m),
                RawEvent.Snapshot(3, Sample.Epoch, Sample.Epoch, book.Bids, book.Asks, 2),
            })
            {
                string line = EventSerializer.ToLine(e);
                Assert.Equal("2026-03-10T14:30:00.0000000Z", StreamComparer.ExtractSourceTs(line));
                Assert.Equal(e.Kind, StreamComparer.ExtractKind(line));
            }
        }

        // -------------------------------------------------------------- comparer

        [Fact]
        public void Identical_streams_compare_identical()
        {
            var a = Canon(Lines());
            var b = Canon(Lines());

            var r = StreamComparer.Compare(a, b);

            Assert.True(r.StrictMatch);
            Assert.True(r.TimestampBucketMatch);
            Assert.Equal("IDENTICAL", r.Verdict);
            Assert.Equal(-1, r.FirstDivergenceIndex);
            Assert.Equal(0, r.TotalDifferingLines);
        }

        [Fact]
        public void A_changed_value_is_reported_at_its_exact_index()
        {
            var a = Canon(Lines());
            var b = Canon(Lines());
            b[2] = b[2].Replace("\"volume\":3", "\"volume\":4");

            var r = StreamComparer.Compare(a, b);

            Assert.False(r.StrictMatch);
            Assert.False(r.TimestampBucketMatch);
            Assert.Equal("DIVERGENT", r.Verdict);
            Assert.Equal(2, r.FirstDivergenceIndex);
        }

        [Fact]
        public void A_dropped_event_is_reported_as_divergent()
        {
            var a = Canon(Lines());
            var b = Canon(Lines());
            b.RemoveAt(1);

            var r = StreamComparer.Compare(a, b);

            Assert.False(r.StrictMatch);
            Assert.False(r.TimestampBucketMatch);
            Assert.Equal(a.Count - 1, r.CountB);
        }

        [Fact]
        public void A_truncated_stream_is_flagged_as_a_prefix_not_a_content_difference()
        {
            // Distinguishing "stopped early" from "recorded different things" matters:
            // the first is a shutdown problem, the second is a feed-fidelity problem.
            var a = Canon(Lines());
            var b = Canon(Lines());
            b.RemoveRange(b.Count - 2, 2);

            var r = StreamComparer.Compare(a, b);

            Assert.False(r.StrictMatch);
            Assert.True(r.ShorterIsPrefix);
            Assert.Equal(-1, r.FirstDivergenceIndex);
        }

        [Fact]
        public void Reordering_within_one_source_timestamp_is_complete_but_unordered()
        {
            // The verdict that matters most for the objective: nothing lost, but the
            // order inside an instant changed. Usable for some research, fatal for
            // research that depends on intra-timestamp sequencing, so it must not be
            // collapsed into either a pass or a failure.
            var a = new List<string>
            {
                Canon(RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 1m, 1m, Aggressor.Buy)),
                Canon(RawEvent.Trade(2, Sample.Epoch, Sample.Epoch, 2m, 1m, Aggressor.Sell)),
            };
            var b = new List<string> { a[1], a[0] };

            var r = StreamComparer.Compare(a, b);

            Assert.False(r.StrictMatch);
            Assert.True(r.TimestampBucketMatch);
            Assert.Equal("COMPLETE_BUT_UNORDERED", r.Verdict);
        }

        [Fact]
        public void Reordering_across_source_timestamps_is_divergent()
        {
            var a = new List<string>
            {
                Canon(RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 1m, 1m, Aggressor.Buy)),
                Canon(RawEvent.Trade(2, Sample.Epoch.AddSeconds(1), Sample.Epoch, 2m, 1m, Aggressor.Sell)),
            };
            var b = new List<string> { a[1], a[0] };

            var r = StreamComparer.Compare(a, b);

            Assert.False(r.StrictMatch);
            // Both timestamps still hold the same events, so the bucket test passes
            // while the strict test fails; the report distinguishes the two.
            Assert.True(r.TimestampBucketMatch);
            Assert.Equal(0, r.FirstDivergenceIndex);
        }

        [Fact]
        public void A_duplicated_event_breaks_the_bucket_test()
        {
            var a = new List<string> { Canon(RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 1m, 1m, Aggressor.Buy)) };
            var b = new List<string> { a[0], a[0] };

            var r = StreamComparer.Compare(a, b);

            Assert.False(r.StrictMatch);
            Assert.False(r.TimestampBucketMatch);
            Assert.NotEmpty(r.BucketMismatchSamples);
        }

        [Fact]
        public void Per_kind_counts_are_reported_for_both_sides()
        {
            var a = Canon(Lines());
            var r = StreamComparer.Compare(a, a);

            Assert.Equal(2, r.KindCountsA[EventKind.Trade]);
            Assert.Equal(2, r.KindCountsA[EventKind.Depth]);
            Assert.Equal(1, r.KindCountsA[EventKind.Snapshot]);
        }

        [Fact]
        public void The_report_names_the_verdict_and_the_first_divergence()
        {
            var a = Canon(Lines());
            var b = Canon(Lines());
            b[0] = b[0].Replace("\"price\":1", "\"price\":9");

            string text = StreamComparer.Report(StreamComparer.Compare(a, b), "A", "B");

            Assert.StartsWith("VERDICT: DIVERGENT", text);
            Assert.Contains("first divergence at canonical line index 0", text);
            Assert.Contains("\"price\":9", text);
        }

        // ---------------------------------------------- end-to-end determinism

        [Fact]
        public void The_synthetic_script_is_identical_for_the_same_seed()
        {
            var a = SyntheticFeed.Build(1234, 1, 50, 10);
            var b = SyntheticFeed.Build(1234, 1, 50, 10);

            Assert.Equal(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.Equal(a[i].OffsetTicks, b[i].OffsetTicks);
                Assert.Equal(a[i].IsTrade, b[i].IsTrade);
                Assert.Equal(a[i].Price, b[i].Price);
                Assert.Equal(a[i].Volume, b[i].Volume);
                Assert.Equal(a[i].Side, b[i].Side);
                Assert.Equal(a[i].Aggressor, b[i].Aggressor);
            }
        }

        [Fact]
        public void The_synthetic_script_differs_for_a_different_seed()
        {
            var a = SyntheticFeed.Build(1, 1, 50, 10);
            var b = SyntheticFeed.Build(2, 1, 50, 10);
            Assert.NotEqual(Render(a), Render(b));
        }

        [Fact]
        public void The_same_script_recorded_twice_yields_byte_identical_captures()
        {
            // The property the whole 1x-versus-accelerated experiment rests on: given
            // the same source events, the recorder's output is a pure function of
            // them. Without this, any difference seen in a real ATAS run could not be
            // attributed to ATAS rather than to this tool.
            using var d1 = new TempDir();
            using var d2 = new TempDir();

            var script = SyntheticFeed.Build(777, 1, 120, 12);

            string h1 = RecordScript(script, d1.Path, "first");
            string h2 = RecordScript(script, d2.Path, "second");

            // The RAW files must differ, because each carries recv_ts, a genuine
            // wall clock. That is the point of excluding it: if the raw bytes
            // matched, recv_ts would not be a real wall clock and the canonical
            // projection would be removing nothing.
            Assert.NotEqual(h1, h2);

            var r = StreamComparer.Compare(
                StreamComparer.LoadCanonical(Path.Combine(d1.Path, "events.jsonl")),
                StreamComparer.LoadCanonical(Path.Combine(d2.Path, "events.jsonl")));

            Assert.True(r.StrictMatch);
            Assert.True(r.CountA > 1000);
        }

        [Fact]
        public void Snapshot_content_depends_only_on_preceding_source_events()
        {
            using var d1 = new TempDir();
            using var d2 = new TempDir();

            var script = SyntheticFeed.Build(555, 1, 200, 15);

            RecordScript(script, d1.Path, "a");
            RecordScript(script, d2.Path, "b");

            var s1 = SnapshotLines(Path.Combine(d1.Path, "events.jsonl"));
            var s2 = SnapshotLines(Path.Combine(d2.Path, "events.jsonl"));

            Assert.NotEmpty(s1);
            Assert.Equal(s1, s2);
        }

        // ------------------------------------------------------------- helpers

        private static List<string> SnapshotLines(string path)
        {
            var list = new List<string>();
            foreach (var l in StreamComparer.LoadCanonical(path))
                if (StreamComparer.ExtractKind(l) == EventKind.Snapshot) list.Add(l);
            return list;
        }

        /// <summary>Records a script through the real recorder and returns the file hash.</summary>
        private static string RecordScript(List<ScriptEvent> script, string outDir, string label)
        {
            var book = new SyntheticBook();
            var options = new RecorderOptions
            {
                OutputDirectory = outDir,
                RawInstrument = "SYNTHETIC|NQU6@CME",
                RunLabel = label,
                SnapshotInterval = TimeSpan.FromMilliseconds(500),
                SnapshotDepthLimit = 10,
                QueueCapacity = 1 << 18,
                DrainTimeout = TimeSpan.FromSeconds(60),
            };

            using var rec = new EventRecorder(options, book);

            foreach (var e in script)
            {
                DateTime src = Sample.Epoch.AddTicks(e.OffsetTicks);
                if (e.IsTrade)
                {
                    rec.OnTrade(src, e.Price, e.Volume, e.Aggressor);
                }
                else
                {
                    rec.OnDepthChange(src, e.Side, e.Price, e.Volume);
                    book.Apply(e.Side, e.Price, e.Volume);
                }
            }

            var m = rec.Complete();
            Assert.True(m.CaptureComplete);
            return m.EventsSha256;
        }

        private static string Render(List<ScriptEvent> s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var e in s) sb.Append(e.OffsetTicks).Append(e.IsTrade).Append(e.Price).Append(e.Volume).Append('|');
            return sb.ToString();
        }

        private static string Canon(RawEvent e) => StreamComparer.Canonicalize(EventSerializer.ToLine(e));

        private static List<string> Canon(IEnumerable<RawEvent> events)
        {
            var list = new List<string>();
            foreach (var e in events) list.Add(Canon(e));
            return list;
        }

        private static List<RawEvent> Lines()
        {
            var book = Sample.Book();
            return new List<RawEvent>
            {
                RawEvent.Trade(1, Sample.Epoch, Sample.Epoch, 1m, 1m, Aggressor.Buy),
                RawEvent.Depth(2, Sample.Epoch.AddMilliseconds(1), Sample.Epoch, Side.Bid, 2m, 2m),
                RawEvent.Trade(3, Sample.Epoch.AddMilliseconds(2), Sample.Epoch, 3m, 3m, Aggressor.Sell),
                RawEvent.Snapshot(4, Sample.Epoch.AddSeconds(1), Sample.Epoch, book.Bids, book.Asks, 2),
                RawEvent.Depth(5, Sample.Epoch.AddSeconds(2), Sample.Epoch, Side.Ask, 5m, 0m),
            };
        }
    }
}
