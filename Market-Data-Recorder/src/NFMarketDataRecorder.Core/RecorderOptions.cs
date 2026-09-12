using System;

namespace NFMarketDataRecorder.Core
{
    /// <summary>Configuration for one capture run.</summary>
    public sealed class RecorderOptions
    {
        /// <summary>Directory that will hold events.jsonl, faults.jsonl and manifest.json.</summary>
        public string OutputDirectory;

        /// <summary>Instrument label recorded in the manifest. Free text; not parsed.</summary>
        public string Instrument = "";

        /// <summary>
        /// Operator-supplied label for the run, e.g. "1x" or "accel". Recorded in
        /// the manifest and used by the comparison tool for reporting only; it never
        /// affects capture behaviour.
        /// </summary>
        public string RunLabel = "";

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
            if (DrainTimeout < TimeSpan.Zero)
                throw new ArgumentException("DrainTimeout cannot be negative.");
        }
    }
}
