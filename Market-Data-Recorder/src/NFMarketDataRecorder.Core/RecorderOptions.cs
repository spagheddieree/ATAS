using System;

namespace NFMarketDataRecorder.Core
{
    /// <summary>Configuration for one capture run.</summary>
    public sealed class RecorderOptions
    {
        /// <summary>Directory that will hold events.jsonl, faults.jsonl and manifest.json.</summary>
        public string OutputDirectory;

        /// <summary>
        /// Canonical run identity. Left empty, the recorder generates one. This is
        /// what downstream joins and deduplicates on — never <see cref="RunLabel"/>.
        /// </summary>
        public string RunId = "";

        /// <summary>
        /// Operator-supplied label for the run, e.g. "1x" or "accel". Reporting only;
        /// it is not identity and never affects capture behaviour.
        /// </summary>
        public string RunLabel = "";

        /// <summary>
        /// The instrument exactly as the platform names it, e.g. "dxFeed|NQU6@CME".
        /// Stored verbatim; canonical identity is derived from it, never instead of it.
        /// </summary>
        public string RawInstrument = "";

        /// <summary>
        /// LIVE or REPLAY, declared by the operator.
        /// </summary>
        /// <remarks>
        /// Deliberately defaults to UNKNOWN rather than LIVE. Inferring the mode from
        /// timestamps, symbol, account or date is banned — during a historical replay
        /// the receive clock is present-day while the event clock is historical, so
        /// every one of those signals is ambiguous or misleading. An undeclared run is
        /// reported as undeclared.
        /// </remarks>
        public string AcquisitionMode = Core.AcquisitionMode.Unknown;

        /// <summary>
        /// Whether the meaning of the platform's event timestamp has been verified
        /// against the real API. Governs how far downstream timing analysis may go.
        /// </summary>
        public bool SourceTimeVerified = false;

        /// <summary>DOM snapshot interval, measured in SOURCE time.</summary>
        public TimeSpan SnapshotInterval = TimeSpan.FromSeconds(1);

        /// <summary>Max snapshots emitted for a single observed source-time gap.</summary>
        public int SnapshotMaxCatchUp = 10;

        /// <summary>Levels per side to record in a snapshot. 0 means whatever the platform returns.</summary>
        public int SnapshotDepthLimit = 20;

        /// <summary>Bounded queue capacity, in events.</summary>
        public int QueueCapacity = 1 << 18; // 262144

        /// <summary>Lines between writer flushes.</summary>
        public int FlushEveryLines = 2000;

        /// <summary>How long shutdown waits for the queue to drain.</summary>
        public TimeSpan DrainTimeout = TimeSpan.FromSeconds(30);

        public void Validate()
        {
            if (string.IsNullOrEmpty(OutputDirectory))
                throw new ArgumentException("OutputDirectory is required.");
            if (SnapshotInterval <= TimeSpan.Zero)
                throw new ArgumentException("SnapshotInterval must be positive.");
            if (SnapshotMaxCatchUp < 1)
                throw new ArgumentException("SnapshotMaxCatchUp must be at least 1.");
            if (QueueCapacity < 1)
                throw new ArgumentException("QueueCapacity must be at least 1.");
            if (SnapshotDepthLimit < 0)
                throw new ArgumentException("SnapshotDepthLimit cannot be negative.");
            if (!Core.AcquisitionMode.IsValid(AcquisitionMode))
                throw new ArgumentException("AcquisitionMode must be LIVE, REPLAY or UNKNOWN; got '" + AcquisitionMode + "'.");
            if (DrainTimeout < TimeSpan.Zero)
                throw new ArgumentException("DrainTimeout cannot be negative.");
        }
    }
}
