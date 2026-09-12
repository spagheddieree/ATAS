using System;
using System.Collections.Generic;
using System.Text;

namespace NFMarketReplayRecorder.Core
{
    /// <summary>
    /// The run's self-report. Written last by <see cref="EventRecorder.Complete"/>,
    /// so its presence means the run shut down through the intended path.
    /// </summary>
    public sealed class RunManifest
    {
        public string SchemaVersion;
        public string RecorderVersion;

        /// <summary>Canonical run identity. What downstream joins on.</summary>
        public string RunId;

        /// <summary>RAW_SOURCE for the recorder. Declared, never assumed.</summary>
        public string SourceClassValue;

        /// <summary>LIVE / REPLAY / UNKNOWN, as declared by the operator.</summary>
        public string AcquisitionModeValue;

        public RawInstrumentIdentity RawInstrument = RawInstrumentIdentity.Undeclared;
        public CanonicalInstrumentIdentity CanonicalInstrument;

        public string SourceTimeBasis;
        public bool SourceTimeVerified;

        /// <summary>CLEAN / DEGRADED / CORRUPT.</summary>
        public string IntegrityStateValue = Core.IntegrityState.Clean;

        public string RunLabel;

        public DateTime StartedWallUtc;
        public DateTime EndedWallUtc;
        public DateTime FirstSourceUtc;
        public DateTime LastSourceUtc;

        public long SnapshotIntervalMs;
        public int SnapshotDepthLimit;
        public int QueueCapacity;
        public int QueueHighWater;

        public long TradesSeen;
        public long DepthChangesSeen;
        public long SnapshotsTaken;
        public long EventsWritten;
        public long EventsDropped;
        public long WriteFailures;

        public bool Drained;

        /// <summary>True only when nothing was dropped, nothing failed to write, and the queue drained.</summary>
        public bool CaptureComplete;

        public string EventsSha256;
        public string SidecarError;

        public List<FaultRecord> Faults = new List<FaultRecord>();

        /// <summary>Source-time span covered, which is the span the comparison is valid over.</summary>
        public TimeSpan SourceSpan
        {
            get
            {
                if (FirstSourceUtc == DateTime.MinValue || LastSourceUtc == DateTime.MinValue) return TimeSpan.Zero;
                return LastSourceUtc - FirstSourceUtc;
            }
        }

        public string ToJson()
        {
            var sb = new StringBuilder(1024);
            var j = new JsonLine(sb);
            var canon = CanonicalInstrument ?? CanonicalInstrumentIdentity.Derive(RawInstrument);

            j.Str("schema_version", SchemaVersion)
             .Str("recorder_version", RecorderVersion ?? "")
             .Str("run_id", RunId ?? "")
             .Str("source_class", SourceClassValue ?? "")
             .Str("acquisition_mode", AcquisitionModeValue ?? "")
             .Bool("acquisition_mode_declared", Core.AcquisitionMode.IsDeclared(AcquisitionModeValue))
             .Str("raw_instrument", RawInstrument.ToComposite())
             .Str("raw_provider", RawInstrument.Provider)
             .Str("raw_symbol", RawInstrument.Symbol)
             .Str("raw_exchange", RawInstrument.Exchange)
             .Str("canonical_root", canon.Root)
             .Str("canonical_size_class", canon.Size)
             .Str("canonical_contract", canon.Contract)
             .Str("canonical_series", canon.Series)
             .Str("canonical_partition_key", canon.PartitionKey)
             .Str("source_time_basis", SourceTimeBasis ?? "")
             .Bool("source_time_verified", SourceTimeVerified)
             .Str("integrity_state", IntegrityStateValue ?? "")
             .Bool("has_unverified_contract_fields", RecorderFieldRegister.HasUnverifiedFields())
             .Str("run_label", RunLabel ?? "")
             .Time("started_wall_utc", StartedWallUtc)
             .Time("ended_wall_utc", EndedWallUtc)
             .Time("first_src_ts", FirstSourceUtc)
             .Time("last_src_ts", LastSourceUtc)
             .Num("source_span_ms", (long)SourceSpan.TotalMilliseconds)
             .Num("wall_span_ms", (long)(EndedWallUtc - StartedWallUtc).TotalMilliseconds)
             .Num("snapshot_interval_ms", SnapshotIntervalMs)
             .Num("snapshot_depth_limit", (long)SnapshotDepthLimit)
             .Num("queue_capacity", (long)QueueCapacity)
             .Num("queue_high_water", (long)QueueHighWater)
             .Num("trades_seen", TradesSeen)
             .Num("depth_changes_seen", DepthChangesSeen)
             .Num("snapshots_taken", SnapshotsTaken)
             .Num("events_written", EventsWritten)
             .Num("events_dropped", EventsDropped)
             .Num("write_failures", WriteFailures)
             .Bool("drained", Drained)
             .Bool("capture_complete", CaptureComplete)
             .Str("events_sha256", EventsSha256 ?? "")
             .Str("sidecar_error", SidecarError ?? "")
             .Num("fault_kinds", (long)(Faults == null ? 0 : Faults.Count));
            j.End();
            return sb.ToString() + "\n";
        }
    }
}
