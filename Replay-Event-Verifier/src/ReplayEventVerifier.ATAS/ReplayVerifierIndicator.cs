using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

using ATAS.Indicators;

using ReplayEventVerifier.Core;

namespace ReplayEventVerifier.ATAS
{
    /// <summary>
    /// Captures every trade and every market-depth change delivered by the
    /// platform, plus a periodic snapshot of the platform's own order book, to
    /// plain files for offline comparison.
    /// </summary>
    /// <remarks>
    /// <para><b>This indicator does not trade and computes nothing.</b> It draws
    /// nothing, produces no signal, and holds no strategy state. Its only purpose
    /// is to answer one question: does ATAS Replay in ticks+DOM mode deliver the
    /// same event stream at an accelerated speed as it does at 1x?</para>
    ///
    /// <para><b>This file is the only part of the tool that touches the ATAS API,
    /// and the API surface it uses is UNVERIFIED.</b> No ATAS SDK, assemblies or
    /// documentation were reachable from the environment this was written in — see
    /// <c>docs/ATAS-API-VERIFICATION.md</c>, which lists every ATAS symbol used
    /// here, what is assumed about it, and how to correct it. The design keeps all
    /// behaviour in <c>ReplayEventVerifier.Core</c>, which is fully tested, so a
    /// wrong assumption here is a localised compile error and never a silent
    /// behavioural difference.</para>
    ///
    /// <para><b>Callback discipline.</b> Each handler does bounded work and
    /// returns: a sequence increment, a source-time comparison, a bounded enqueue.
    /// No file I/O, no allocation beyond one event object, no lock held across
    /// anything that can block. All writing happens on the recorder's own
    /// background thread.</para>
    /// </remarks>
    [DisplayName("Replay Event Verifier")]
    [Display(Name = "Replay Event Verifier",
             Description = "Records raw trades, depth changes and periodic DOM snapshots to disk. No trading logic.")]
    public class ReplayVerifierIndicator : Indicator, IDomSource
    {
        private EventRecorder _recorder;
        private readonly object _lifecycleGate = new object();

        // ------------------------------------------------------------- settings

        [Display(Name = "Output directory", GroupName = "Capture", Order = 10,
                 Description = "Directory for events.jsonl, faults.jsonl and manifest.json. A per-run subdirectory is created inside it.")]
        public string OutputDirectory { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ReplayEventVerifier");

        [Display(Name = "Run label", GroupName = "Capture", Order = 20,
                 Description = "Free-text label for this run, e.g. 1x or accel-10x. Recorded in the manifest; does not change behaviour.")]
        public string RunLabel { get; set; } = "unlabelled";

        [Display(Name = "Snapshot interval (ms, source time)", GroupName = "Capture", Order = 30,
                 Description = "DOM snapshot interval measured in the feed's own timestamps, never wall clock. Identical at every replay speed.")]
        [Range(1, 3600000)]
        public int SnapshotIntervalMs { get; set; } = 1000;

        [Display(Name = "Snapshot depth (levels per side)", GroupName = "Capture", Order = 40,
                 Description = "Levels per side to record per snapshot. 0 records everything the platform returns.")]
        [Range(0, 1000)]
        public int SnapshotDepthLimit { get; set; } = 20;

        [Display(Name = "Queue capacity (events)", GroupName = "Integrity", Order = 50,
                 Description = "Bounded queue size. On overflow events are DROPPED and counted, never delayed, so the feed thread is never blocked.")]
        [Range(1024, 8388608)]
        public int QueueCapacity { get; set; } = 262144;

        [Display(Name = "Drain timeout (s)", GroupName = "Integrity", Order = 60,
                 Description = "How long shutdown waits for the writer to finish before declaring the capture incomplete.")]
        [Range(1, 600)]
        public int DrainTimeoutSeconds { get; set; } = 30;

        // ------------------------------------------------------------ lifecycle

        public ReplayVerifierIndicator()
        {
            // This indicator renders nothing. Declaring that up front avoids the
            // platform allocating or invoking any drawing path on our behalf.
            EnableCustomDrawing = false;
            SubscribeToDrawingEvents(DrawingLayouts.None);

            // A verifier that only sees bar-close events would miss exactly what it
            // is here to measure, so it must run on every tick.
            DenyToChangePanel = true;
        }

        protected override void OnInitialize()
        {
            lock (_lifecycleGate)
            {
                StopRecorderLocked();

                string runDir = Path.Combine(
                    OutputDirectory,
                    string.Format("{0:yyyyMMdd-HHmmss}-{1}", DateTime.UtcNow, Sanitize(RunLabel)));

                var options = new RecorderOptions
                {
                    OutputDirectory = runDir,
                    Instrument = InstrumentInfoSafe(),
                    RunLabel = RunLabel ?? "",
                    SnapshotInterval = TimeSpan.FromMilliseconds(SnapshotIntervalMs),
                    SnapshotDepthLimit = SnapshotDepthLimit,
                    QueueCapacity = QueueCapacity,
                    DrainTimeout = TimeSpan.FromSeconds(DrainTimeoutSeconds),
                };

                _recorder = new EventRecorder(options, this);
            }
        }

        /// <summary>
        /// Required override. Deliberately empty: bar-level calculation is exactly
        /// the aggregated view this tool exists to bypass.
        /// </summary>
        protected override void OnCalculate(int bar, decimal value)
        {
        }

        protected override void OnDispose()
        {
            lock (_lifecycleGate)
            {
                StopRecorderLocked();
            }
            base.OnDispose();
        }

        private void StopRecorderLocked()
        {
            var r = _recorder;
            _recorder = null;
            if (r != null) r.Complete();
        }

        // --------------------------------------------------------- market data

        /// <summary>One individual trade from the platform.</summary>
        protected override void OnNewTrade(MarketDataArg trade)
        {
            var r = _recorder;
            if (r == null || trade == null) return;

            r.OnTrade(ToUtc(trade.Time), trade.Price, trade.Volume, MapAggressor(trade.Direction));
        }

        /// <summary>One individual market-depth change from the platform.</summary>
        protected override void MarketDepthChanged(MarketDataArg depth)
        {
            var r = _recorder;
            if (r == null || depth == null) return;

            string side = MapSide(depth.DataType);
            if (side == null) return; // not a book-side update; nothing raw to record

            r.OnDepthChange(ToUtc(depth.Time), side, depth.Price, depth.Volume);
        }

        // ------------------------------------------------------------ DOM source

        /// <summary>
        /// Reads the platform's own order book. Called by the recorder on a
        /// source-time interval boundary.
        /// </summary>
        /// <remarks>
        /// The book is read from the platform API rather than rebuilt from the
        /// depth-change stream on purpose. A locally reconstructed book is a
        /// function of the changes we received, so it can never contradict them and
        /// can never reveal a change that went missing. The platform's book can.
        /// </remarks>
        public DomBook GetBook(int depthLimit)
        {
            var bids = ReadSide(MarketDataType.Bid, depthLimit);
            var asks = ReadSide(MarketDataType.Ask, depthLimit);
            return new DomBook(bids, asks);
        }

        private DomLevel[] ReadSide(MarketDataType side, int depthLimit)
        {
            var rows = MarketDepthInfo.GetMarketDepth(side);
            if (rows == null) return new DomLevel[0];

            int n = 0;
            var buffer = new DomLevel[depthLimit > 0 ? depthLimit : 256];

            foreach (var row in rows)
            {
                if (row == null) continue;
                if (depthLimit > 0 && n >= depthLimit) break;
                if (n == buffer.Length) Array.Resize(ref buffer, buffer.Length * 2);
                buffer[n++] = new DomLevel(row.Price, row.Volume);
            }

            if (n == buffer.Length) return buffer;
            var exact = new DomLevel[n];
            Array.Copy(buffer, exact, n);
            return exact;
        }

        // ---------------------------------------------------------------- mapping

        /// <summary>
        /// Maps the platform's trade direction to a raw label. An unmapped value
        /// becomes <see cref="Aggressor.Unknown"/> rather than being inferred from
        /// price: inferring it would be a derived feature, and a wrong inference in
        /// a data-fidelity tool is worse than an honest gap.
        /// </summary>
        private static string MapAggressor(TradeDirection direction)
        {
            switch (direction)
            {
                case TradeDirection.Buy: return Aggressor.Buy;
                case TradeDirection.Sell: return Aggressor.Sell;
                default: return Aggressor.Unknown;
            }
        }

        private static string MapSide(MarketDataType type)
        {
            switch (type)
            {
                case MarketDataType.Bid: return Side.Bid;
                case MarketDataType.Ask: return Side.Ask;
                default: return null;
            }
        }

        /// <summary>
        /// Normalises a platform timestamp to UTC without ever substituting the
        /// wall clock. An <see cref="DateTimeKind.Unspecified"/> value is treated as
        /// already-UTC rather than local, because reinterpreting it in the machine's
        /// timezone would shift every timestamp by the local offset and make two
        /// captures taken on differently configured machines incomparable.
        /// </summary>
        private static DateTime ToUtc(DateTime t)
        {
            switch (t.Kind)
            {
                case DateTimeKind.Utc: return t;
                case DateTimeKind.Local: return t.ToUniversalTime();
                default: return DateTime.SpecifyKind(t, DateTimeKind.Utc);
            }
        }

        private string InstrumentInfoSafe()
        {
            try
            {
                var info = InstrumentInfo;
                return info == null ? "" : info.Instrument ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "run";
            var chars = s.ToCharArray();
            var invalid = Path.GetInvalidFileNameChars();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0 || chars[i] == ' ') chars[i] = '-';
            }
            return new string(chars);
        }
    }
}
