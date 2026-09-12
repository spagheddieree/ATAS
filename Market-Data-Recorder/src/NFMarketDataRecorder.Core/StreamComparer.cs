using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NFMarketDataRecorder.Core
{
    /// <summary>Outcome of comparing two captures.</summary>
    public sealed class ComparisonResult
    {
        public int CountA;
        public int CountB;

        /// <summary>
        /// True when the two captures are identical line for line in canonical
        /// form. This is the strong result: same events, same order.
        /// </summary>
        public bool StrictMatch;

        /// <summary>
        /// True when, for every source timestamp, both captures hold the same
        /// multiset of events — differing only in the order of events sharing one
        /// timestamp.
        /// </summary>
        /// <remarks>
        /// Strict false with bucket true is the interesting middle verdict: the
        /// replay delivered every event but not in a stable order within an
        /// instant. That is usable for most quantitative research and fatal for
        /// research that depends on intra-timestamp sequencing, so the two are
        /// reported separately rather than collapsed into one pass/fail.
        /// </remarks>
        public bool TimestampBucketMatch;

        /// <summary>Index of the first differing canonical line, or -1.</summary>
        public int FirstDivergenceIndex = -1;
        public string FirstDivergenceA;
        public string FirstDivergenceB;

        /// <summary>True when the shorter capture is an exact prefix of the longer one.</summary>
        public bool ShorterIsPrefix;

        public int TotalDifferingLines;

        public readonly Dictionary<string, int> KindCountsA = new Dictionary<string, int>(StringComparer.Ordinal);
        public readonly Dictionary<string, int> KindCountsB = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>Source timestamps present in one capture but not the other, capped for reporting.</summary>
        public readonly List<string> BucketMismatchSamples = new List<string>();

        public bool Equivalent { get { return StrictMatch; } }

        public string Verdict
        {
            get
            {
                if (StrictMatch) return "IDENTICAL";
                if (TimestampBucketMatch) return "COMPLETE_BUT_UNORDERED";
                return "DIVERGENT";
            }
        }
    }

    /// <summary>
    /// Deterministic comparison of two capture files.
    /// </summary>
    /// <remarks>
    /// Works on the canonical projection of each line — <c>seq</c> and
    /// <c>recv_ts</c> stripped — because those two fields are properties of the
    /// observer rather than the market and are expected to differ between a 1x and
    /// an accelerated run. Everything else must match exactly.
    /// </remarks>
    public static class StreamComparer
    {
        /// <summary>
        /// Strips <c>seq</c> and <c>recv_ts</c> from a written event line.
        /// </summary>
        /// <remarks>
        /// Both fields are written in fixed positions by
        /// <see cref="EventSerializer.Write"/> — <c>seq</c> first and <c>recv_ts</c>
        /// fourth — with values that cannot contain a comma or a brace. That lets
        /// this be a string operation instead of a JSON parse, which keeps the
        /// comparison free of any dependency and fast enough to run over multi
        /// million line captures.
        /// </remarks>
        public static string Canonicalize(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;

            string s = StripField(line, "\"seq\":");
            s = StripField(s, "\"recv_ts\":");
            return s;
        }

        private static string StripField(string line, string key)
        {
            int k = line.IndexOf(key, StringComparison.Ordinal);
            if (k < 0) return line;

            int valueEnd = line.IndexOf(',', k + key.Length);
            if (valueEnd < 0)
            {
                // Last field before the closing brace.
                valueEnd = line.LastIndexOf('}');
                if (valueEnd < 0) return line;
                // Also drop the comma that preceded this key, if any.
                int comma = line.LastIndexOf(',', k);
                if (comma >= 0) return line.Substring(0, comma) + line.Substring(valueEnd);
                return line.Substring(0, k) + line.Substring(valueEnd);
            }

            return line.Substring(0, k) + line.Substring(valueEnd + 1);
        }

        /// <summary>Reads the <c>src_ts</c> value out of a canonical or raw line.</summary>
        public static string ExtractSourceTs(string line)
        {
            const string key = "\"src_ts\":\"";
            int k = line.IndexOf(key, StringComparison.Ordinal);
            if (k < 0) return "";
            int start = k + key.Length;
            int end = line.IndexOf('"', start);
            return end < 0 ? "" : line.Substring(start, end - start);
        }

        public static string ExtractKind(string line)
        {
            const string key = "\"kind\":\"";
            int k = line.IndexOf(key, StringComparison.Ordinal);
            if (k < 0) return "";
            int start = k + key.Length;
            int end = line.IndexOf('"', start);
            return end < 0 ? "" : line.Substring(start, end - start);
        }

        public static List<string> LoadCanonical(string eventsJsonlPath)
        {
            var list = new List<string>();
            using (var r = new StreamReader(eventsJsonlPath, new UTF8Encoding(false)))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    if (line.Length == 0) continue;
                    list.Add(Canonicalize(line));
                }
            }
            return list;
        }

        public static ComparisonResult Compare(IList<string> canonicalA, IList<string> canonicalB)
        {
            var res = new ComparisonResult { CountA = canonicalA.Count, CountB = canonicalB.Count };

            foreach (var l in canonicalA) Bump(res.KindCountsA, ExtractKind(l));
            foreach (var l in canonicalB) Bump(res.KindCountsB, ExtractKind(l));

            int n = Math.Min(canonicalA.Count, canonicalB.Count);
            for (int i = 0; i < n; i++)
            {
                if (!string.Equals(canonicalA[i], canonicalB[i], StringComparison.Ordinal))
                {
                    res.TotalDifferingLines++;
                    if (res.FirstDivergenceIndex < 0)
                    {
                        res.FirstDivergenceIndex = i;
                        res.FirstDivergenceA = canonicalA[i];
                        res.FirstDivergenceB = canonicalB[i];
                    }
                }
            }

            res.TotalDifferingLines += Math.Abs(canonicalA.Count - canonicalB.Count);
            res.ShorterIsPrefix = res.FirstDivergenceIndex < 0 && canonicalA.Count != canonicalB.Count;
            res.StrictMatch = res.FirstDivergenceIndex < 0 && canonicalA.Count == canonicalB.Count;

            res.TimestampBucketMatch = res.StrictMatch || BucketsMatch(canonicalA, canonicalB, res);

            return res;
        }

        /// <summary>
        /// Groups both captures by source timestamp and compares the multiset of
        /// lines in each group, which tolerates reordering within one instant but
        /// nothing else.
        /// </summary>
        private static bool BucketsMatch(IList<string> a, IList<string> b, ComparisonResult res)
        {
            var ba = GroupByTs(a);
            var bb = GroupByTs(b);

            bool ok = true;
            const int maxSamples = 10;

            foreach (var kv in ba)
            {
                Dictionary<string, int> other;
                if (!bb.TryGetValue(kv.Key, out other) || !SameMultiset(kv.Value, other))
                {
                    ok = false;
                    if (res.BucketMismatchSamples.Count < maxSamples) res.BucketMismatchSamples.Add(kv.Key);
                }
            }

            foreach (var kv in bb)
            {
                if (!ba.ContainsKey(kv.Key))
                {
                    ok = false;
                    if (res.BucketMismatchSamples.Count < maxSamples) res.BucketMismatchSamples.Add(kv.Key);
                }
            }

            res.BucketMismatchSamples.Sort(StringComparer.Ordinal);
            return ok;
        }

        private static Dictionary<string, Dictionary<string, int>> GroupByTs(IList<string> lines)
        {
            var g = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
            foreach (var line in lines)
            {
                string ts = ExtractSourceTs(line);
                Dictionary<string, int> bucket;
                if (!g.TryGetValue(ts, out bucket))
                {
                    bucket = new Dictionary<string, int>(StringComparer.Ordinal);
                    g.Add(ts, bucket);
                }
                Bump(bucket, line);
            }
            return g;
        }

        private static bool SameMultiset(Dictionary<string, int> x, Dictionary<string, int> y)
        {
            if (x.Count != y.Count) return false;
            foreach (var kv in x)
            {
                int v;
                if (!y.TryGetValue(kv.Key, out v) || v != kv.Value) return false;
            }
            return true;
        }

        private static void Bump(Dictionary<string, int> d, string key)
        {
            int v;
            d[key] = d.TryGetValue(key, out v) ? v + 1 : 1;
        }

        /// <summary>Human-readable report; also the file written by the comparison tool.</summary>
        public static string Report(ComparisonResult r, string labelA, string labelB)
        {
            var sb = new StringBuilder(2048);
            var inv = CultureInfo.InvariantCulture;

            sb.Append("VERDICT: ").Append(r.Verdict).Append('\n');
            sb.Append("A = ").Append(labelA).Append('\n');
            sb.Append("B = ").Append(labelB).Append('\n');
            sb.Append('\n');
            sb.Append("events A            : ").Append(r.CountA.ToString(inv)).Append('\n');
            sb.Append("events B            : ").Append(r.CountB.ToString(inv)).Append('\n');
            sb.Append("differing lines     : ").Append(r.TotalDifferingLines.ToString(inv)).Append('\n');
            sb.Append("strict match        : ").Append(r.StrictMatch ? "yes" : "no").Append('\n');
            sb.Append("ts-bucket match     : ").Append(r.TimestampBucketMatch ? "yes" : "no").Append('\n');
            sb.Append("shorter is prefix   : ").Append(r.ShorterIsPrefix ? "yes" : "no").Append('\n');
            sb.Append('\n');

            sb.Append("per-kind counts (A | B):\n");
            var kinds = new List<string>();
            foreach (var k in r.KindCountsA.Keys) if (!kinds.Contains(k)) kinds.Add(k);
            foreach (var k in r.KindCountsB.Keys) if (!kinds.Contains(k)) kinds.Add(k);
            kinds.Sort(StringComparer.Ordinal);
            foreach (var k in kinds)
            {
                int ca, cb;
                r.KindCountsA.TryGetValue(k, out ca);
                r.KindCountsB.TryGetValue(k, out cb);
                sb.Append("  ").Append(k.PadRight(10))
                  .Append(ca.ToString(inv).PadLeft(10)).Append(" | ").Append(cb.ToString(inv).PadLeft(10))
                  .Append(ca == cb ? "" : "   <-- DIFFERS").Append('\n');
            }

            if (r.FirstDivergenceIndex >= 0)
            {
                sb.Append('\n');
                sb.Append("first divergence at canonical line index ")
                  .Append(r.FirstDivergenceIndex.ToString(inv)).Append(":\n");
                sb.Append("  A: ").Append(r.FirstDivergenceA).Append('\n');
                sb.Append("  B: ").Append(r.FirstDivergenceB).Append('\n');
            }

            if (r.BucketMismatchSamples.Count > 0)
            {
                sb.Append('\n');
                sb.Append("source timestamps whose event sets differ (first ")
                  .Append(r.BucketMismatchSamples.Count.ToString(inv)).Append("):\n");
                foreach (var ts in r.BucketMismatchSamples) sb.Append("  ").Append(ts).Append('\n');
            }

            sb.Append('\n');
            sb.Append("interpretation:\n");
            if (r.StrictMatch)
                sb.Append("  Replay speed did not change the event stream. Every event and its\n" +
                          "  order is reproduced exactly at both speeds.\n");
            else if (r.TimestampBucketMatch)
                sb.Append("  No event was lost or added, but events sharing a source timestamp were\n" +
                          "  delivered in a different order. Usable where intra-timestamp sequencing\n" +
                          "  does not matter; NOT usable where it does.\n");
            else
                sb.Append("  The captures hold different events. Replay speed changes what the feed\n" +
                          "  delivers, so an accelerated replay is not a faithful substitute.\n");

            return sb.ToString();
        }
    }
}
