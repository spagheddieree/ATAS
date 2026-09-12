using System;
using System.Collections.Generic;
using System.Text;

namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// Integrity fault codes. A capture that hit any of these is not a faithful
    /// record of the feed, and the manifest must say so rather than let a silently
    /// lossy file be mistaken for a complete one.
    /// </summary>
    public static class FaultCode
    {
        /// <summary>The bounded queue was full; the event was dropped, not delayed.</summary>
        public const string QueueOverflow = "queue_overflow";

        /// <summary>The background writer threw while writing to disk.</summary>
        public const string WriteFailure = "write_failure";

        /// <summary>A source timestamp went backwards relative to the previous event.</summary>
        public const string SourceTimeRegression = "source_time_regression";

        /// <summary>An event carried no usable source timestamp.</summary>
        public const string MissingSourceTime = "missing_source_time";

        /// <summary>A source-time gap spanned more snapshot intervals than the catch-up cap allows.</summary>
        public const string SnapshotCatchUpTruncated = "snapshot_catchup_truncated";

        /// <summary>The platform depth API threw or returned nothing when a snapshot was due.</summary>
        public const string SnapshotSourceUnavailable = "snapshot_source_unavailable";

        /// <summary>An event arrived after shutdown began and was refused.</summary>
        public const string EnqueueAfterComplete = "enqueue_after_complete";

        /// <summary>The queue did not drain within the shutdown timeout.</summary>
        public const string DrainTimeout = "drain_timeout";
    }

    /// <summary>
    /// One integrity fault occurrence, aggregated by code. Faults are counted
    /// rather than written per-occurrence so that a pathological overflow storm
    /// cannot itself become the thing that overwhelms the writer.
    /// </summary>
    public sealed class FaultRecord
    {
        public string Code;
        public long Count;
        public DateTime FirstSourceUtc;
        public DateTime LastSourceUtc;
        public long FirstSeq;
        public long LastSeq;
        public string FirstDetail;

        public string ToJson()
        {
            var sb = new StringBuilder(256);
            var j = new JsonLine(sb);
            j.Str("code", Code)
             .Num("count", Count)
             .Time("first_src_ts", FirstSourceUtc)
             .Time("last_src_ts", LastSourceUtc)
             .Num("first_seq", FirstSeq)
             .Num("last_seq", LastSeq)
             .Str("first_detail", FirstDetail ?? "");
            j.End();
            return sb.ToString();
        }
    }

    /// <summary>
    /// Thread-safe fault tally. Every method here is callable from a market-data
    /// callback thread and takes a short uncontended lock; it never allocates per
    /// occurrence after the first of each code, and it never blocks on I/O.
    /// </summary>
    public sealed class FaultLog
    {
        private readonly object _gate = new object();
        private readonly Dictionary<string, FaultRecord> _byCode = new Dictionary<string, FaultRecord>(StringComparer.Ordinal);

        /// <summary>
        /// Invoked for every fault occurrence, so integrity state can react without
        /// this class needing to know what integrity state is.
        /// </summary>
        /// <remarks>
        /// Called outside the lock: the handler must not call back into this log, and
        /// must be cheap, because faults are raised from market-data callback threads.
        /// </remarks>
        public Action<string> OnFault;

        public void Record(string code, DateTime sourceUtc, long seq, string detail = null)
        {
            lock (_gate)
            {
                FaultRecord r;
                if (!_byCode.TryGetValue(code, out r))
                {
                    r = new FaultRecord
                    {
                        Code = code,
                        FirstSourceUtc = sourceUtc,
                        FirstSeq = seq,
                        FirstDetail = detail,
                    };
                    _byCode.Add(code, r);
                }
                r.Count++;
                r.LastSourceUtc = sourceUtc;
                r.LastSeq = seq;
            }

            var handler = OnFault;
            if (handler != null) handler(code);
        }

        public long CountOf(string code)
        {
            lock (_gate)
            {
                FaultRecord r;
                return _byCode.TryGetValue(code, out r) ? r.Count : 0L;
            }
        }

        public long TotalCount
        {
            get
            {
                lock (_gate)
                {
                    long t = 0;
                    foreach (var kv in _byCode) t += kv.Value.Count;
                    return t;
                }
            }
        }

        public bool Any
        {
            get { lock (_gate) { return _byCode.Count > 0; } }
        }

        /// <summary>Snapshot of all faults, ordered by code so output is deterministic.</summary>
        public List<FaultRecord> Snapshot()
        {
            lock (_gate)
            {
                var list = new List<FaultRecord>(_byCode.Count);
                foreach (var kv in _byCode) list.Add(kv.Value);
                list.Sort((a, b) => string.CompareOrdinal(a.Code, b.Code));
                return list;
            }
        }
    }
}
