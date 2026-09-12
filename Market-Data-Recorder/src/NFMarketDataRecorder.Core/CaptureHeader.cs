using System;
using System.Text;

namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// The first line of every <c>events.jsonl</c>: the provenance that applies to
    /// every event in the file.
    /// </summary>
    /// <remarks>
    /// <para><b>Why a header rather than per-event fields.</b> The logical contract
    /// for a trade or depth row includes run identity, acquisition mode, source class
    /// and instrument identity. Physically repeating all of that on every line would
    /// multiply file size several times over for values that are constant for the
    /// whole run, on files that reach millions of rows.</para>
    /// <para>So the file carries them once, on line one, and the logical row is
    /// <c>header ⊗ event</c>. That keeps the file self-describing — a capture
    /// separated from its manifest still knows what it is — while keeping event lines
    /// lean. Dataset ingestion joins the two.</para>
    /// <para>The header is excluded from canonical stream comparison, because it
    /// carries <c>run_id</c> and wall-clock values that necessarily differ between two
    /// captures of the same market events.</para>
    /// </remarks>
    public sealed class CaptureHeader
    {
        public string SchemaVersion = Core.SchemaVersion.Current;
        public string RecorderVersion = Core.SchemaVersion.RecorderVersion;

        /// <summary>
        /// Canonical identity of this capture run. Machine-generated and unique; the
        /// thing downstream joins on. Distinct from <see cref="RunLabel"/>.
        /// </summary>
        public string RunId;

        /// <summary>Free-text operator label, e.g. "1x". Never used as identity.</summary>
        public string RunLabel = "";

        /// <summary>Always RAW_SOURCE for the recorder. Declared, not assumed.</summary>
        public string SourceClassValue = Core.SourceClass.RawSource;

        /// <summary>LIVE, REPLAY or UNKNOWN. Declared by the operator; never inferred.</summary>
        public string AcquisitionModeValue = Core.AcquisitionMode.Unknown;

        public RawInstrumentIdentity RawInstrument = RawInstrumentIdentity.Undeclared;
        public CanonicalInstrumentIdentity CanonicalInstrument;

        /// <summary>
        /// What <c>src_ts</c> on each event actually is, named explicitly rather than
        /// left for a consumer to assume.
        /// </summary>
        public string SourceTimeBasis = SourceTimeBases.PlatformEventTime;

        /// <summary>
        /// Whether the source timestamp's meaning has been verified against the real
        /// platform API. False means <c>src_ts</c> is believed to be the feed clock
        /// but has not been proven to be, which bounds what latency or ordering
        /// analysis may legitimately be done with it.
        /// </summary>
        public bool SourceTimeVerified;

        public DateTime StartedWallUtc;

        public void Validate()
        {
            if (string.IsNullOrEmpty(RunId))
                throw new InvalidOperationException("RunId is required: a capture without canonical run identity cannot be joined or deduplicated.");
            if (!Core.SourceClass.IsValid(SourceClassValue))
                throw new InvalidOperationException("Invalid source_class: " + SourceClassValue);
            if (!Core.AcquisitionMode.IsValid(AcquisitionModeValue))
                throw new InvalidOperationException("Invalid acquisition_mode: " + AcquisitionModeValue);
        }

        public string ToJson()
        {
            var sb = new StringBuilder(640);
            var j = new JsonLine(sb);
            j.Str("kind", EventKind.Header)
             .Str("schema_version", SchemaVersion)
             .Str("recorder_version", RecorderVersion)
             .Str("run_id", RunId)
             .Str("run_label", RunLabel ?? "")
             .Str("source_class", SourceClassValue)
             .Str("acquisition_mode", AcquisitionModeValue)
             .Str("raw_instrument", RawInstrument.ToComposite())
             .Str("raw_provider", RawInstrument.Provider)
             .Str("raw_symbol", RawInstrument.Symbol)
             .Str("raw_exchange", RawInstrument.Exchange);

            var canon = CanonicalInstrument ?? CanonicalInstrumentIdentity.Derive(RawInstrument);
            j.Str("canonical_root", canon.Root)
             .Str("canonical_size_class", canon.Size)
             .Str("canonical_contract", canon.Contract)
             .Str("canonical_series", canon.Series)
             .Str("canonical_partition_key", canon.PartitionKey)
             .Str("source_time_basis", SourceTimeBasis)
             .Bool("source_time_verified", SourceTimeVerified)
             .Time("started_wall_utc", StartedWallUtc);
            j.End();
            return sb.ToString();
        }
    }

    /// <summary>What the recorded source timestamp is understood to represent.</summary>
    public static class SourceTimeBases
    {
        /// <summary>The timestamp the trading platform attached to the event.</summary>
        public const string PlatformEventTime = "PLATFORM_EVENT_TIME";

        /// <summary>A source-time interval boundary computed by the recorder (snapshots).</summary>
        public const string RecorderScheduledBoundary = "RECORDER_SCHEDULED_BOUNDARY";

        /// <summary>No usable source timestamp was available.</summary>
        public const string None = "NONE";
    }
}
