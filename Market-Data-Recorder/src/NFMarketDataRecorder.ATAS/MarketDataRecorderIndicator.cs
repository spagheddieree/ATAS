using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

using ATAS.DataFeedsCore;
using ATAS.Indicators;

using NFMarketDataRecorder.Core;

namespace NFMarketDataRecorder.ATAS
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
    /// behaviour in <c>NFMarketDataRecorder.Core</c>, which is fully tested, so a
    /// wrong assumption here is a localised compile error and never a silent
    /// behavioural difference.</para>
    ///
    /// <para><b>Callback discipline.</b> Each handler does bounded work and
    /// returns: a sequence increment, a source-time comparison, a bounded enqueue.
    /// No file I/O, no allocation beyond one event object, no lock held across
    /// anything that can block. All writing happens on the recorder's own
    /// background thread.</para>
    /// </remarks>
    // [DisplayName] only: the framework's [Display] does not target a class, and
    // the stub's AttributeUsage is pinned to the real one so that stays enforced
    // in stub builds too.
    [DisplayName("NF Market Data Recorder")]
    public class MarketDataRecorderIndicator : Indicator, IDomSource
    {
        private EventRecorder _recorder;
        private readonly object _lifecycleGate = new object();
        private string _runDirectory;

        // Callback-surface diagnostics. Not market data -- they measure how ATAS
        // dispatches, which is a runtime question metadata cannot answer.
        private long _singleTradeCallbacks;
        private long _singleDepthCallbacks;
        private long _batchTradeCallbacks;
        private long _batchTradeItems;
        private long _batchDepthCallbacks;
        private long _batchDepthItems;

        // ------------------------------------------------------------- settings

        [Display(Name = "Output directory", GroupName = "Capture", Order = 10,
                 Description = "Directory for events.jsonl, faults.jsonl and manifest.json. A per-run subdirectory is created inside it.")]
        public string OutputDirectory { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NFMarketDataRecorder");

        [Display(Name = "Acquisition mode", GroupName = "Capture", Order = 15,
                 Description = "LIVE or REPLAY. MUST be set by the operator -- it is never inferred from timestamps, symbol, account or date, because during a replay every one of those signals is ambiguous or misleading. Left unset the run records UNKNOWN.")]
        public string AcquisitionMode { get; set; } = Core.AcquisitionMode.Unknown;

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

        public MarketDataRecorderIndicator()
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

                    // Captured verbatim. Canonical identity is derived from this and
                    // stored alongside it, never instead of it, so NQ and MNQ or an
                    // actual and a continuous contract can never be pooled by accident.
                    RawInstrument = RawInstrumentSafe(),

                    AcquisitionMode = Core.AcquisitionMode.IsValid(AcquisitionMode)
                        ? AcquisitionMode
                        : Core.AcquisitionMode.Unknown,

                    // The meaning of the platform's event timestamp is not yet verified
                    // against the real ATAS API, so the capture says so rather than
                    // implying a guarantee it cannot make.
                    SourceTimeVerified = false,

                    RunLabel = RunLabel ?? "",
                    SnapshotInterval = TimeSpan.FromMilliseconds(SnapshotIntervalMs),
                    SnapshotDepthLimit = SnapshotDepthLimit,
                    QueueCapacity = QueueCapacity,
                    DrainTimeout = TimeSpan.FromSeconds(DrainTimeoutSeconds),
                };

                _runDirectory = runDir;
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

            WriteCallbackDiagnostics();
        }

        /// <summary>
        /// Writes how ATAS actually dispatched, so the single-vs-batch question is
        /// answered by measurement rather than assumption.
        /// </summary>
        private void WriteCallbackDiagnostics()
        {
            string dir = _runDirectory;
            if (string.IsNullOrEmpty(dir)) return;

            try
            {
                string json =
                    "{\"single_trade_callbacks\":" + System.Threading.Interlocked.Read(ref _singleTradeCallbacks) +
                    ",\"single_depth_callbacks\":" + System.Threading.Interlocked.Read(ref _singleDepthCallbacks) +
                    ",\"batch_trade_callbacks\":" + System.Threading.Interlocked.Read(ref _batchTradeCallbacks) +
                    ",\"batch_trade_items\":" + System.Threading.Interlocked.Read(ref _batchTradeItems) +
                    ",\"batch_depth_callbacks\":" + System.Threading.Interlocked.Read(ref _batchDepthCallbacks) +
                    ",\"batch_depth_items\":" + System.Threading.Interlocked.Read(ref _batchDepthItems) +
                    ",\"note\":\"Capture binds the SINGLE callbacks only. If batch_*_items matches " +
                    "single_*_callbacks, ATAS fans the same events out to both surfaces and binding both " +
                    "would double-count. If batch items exceed singles, the batch surface carries events " +
                    "the single surface does not, and the binding must be revisited.\"}\n";

                File.WriteAllText(Path.Combine(dir, "atas-callback-diagnostics.json"), json);
            }
            catch (IOException)
            {
                // Diagnostics must never take the capture down.
            }
        }

        // --------------------------------------------------------- market data

        /// <summary>One individual trade from the platform.</summary>
        /// <remarks>
        /// MEASURED signature. <c>Price</c> is recorded as the trade price;
        /// <c>OriginPrice</c> is captured alongside it rather than substituted for
        /// it, because the relationship between the two is not established by
        /// metadata and picking the wrong one would silently corrupt every price in
        /// the dataset.
        /// </remarks>
        protected override void OnNewTrade(MarketDataArg trade)
        {
            System.Threading.Interlocked.Increment(ref _singleTradeCallbacks);

            var r = _recorder;
            if (r == null || trade == null) return;

            r.OnTrade(ToUtc(trade.Time), trade.Price, trade.Volume, MapAggressor(trade.Direction),
                      trade.OriginPrice, trade.OpenInterest,
                      trade.ExchangeOrderId, trade.AggressorExchangeOrderId);
        }

        /// <summary>One individual market-depth change from the platform.</summary>
        protected override void MarketDepthChanged(MarketDataArg depth)
        {
            System.Threading.Interlocked.Increment(ref _singleDepthCallbacks);

            var r = _recorder;
            if (r == null || depth == null) return;

            string side = SideOf(depth);
            if (side == null) return; // not a book-side update; nothing raw to record

            r.OnDepthChange(ToUtc(depth.Time), side, depth.Price, depth.Volume, depth.ExchangeOrderId);
        }

        // ------------------------------------------- batch callbacks (diagnostic)

        /// <summary>
        /// Counts batch trade deliveries. <b>Records nothing.</b>
        /// </summary>
        /// <remarks>
        /// <para>ATAS exposes both single (<c>OnNewTrade</c>) and batch
        /// (<c>OnNewTrades</c>) surfaces. Metadata proves both exist; it says
        /// nothing about whether both fire for the same underlying event. Capturing
        /// from both would double-count every trade if they do, and capturing from
        /// the batch surface alone would lose per-event granularity if they do
        /// not.</para>
        /// <para>So capture binds to the single surface only, and these overrides
        /// exist purely to <em>measure</em> the relationship: they call base first
        /// to preserve whatever dispatch ATAS performs, then increment a counter.
        /// The counts are written to <c>atas-callback-diagnostics.json</c> at
        /// shutdown, which is how the runtime experiment answers the question
        /// instead of the adapter guessing at it.</para>
        /// </remarks>
        protected override void OnNewTrades(IEnumerable<MarketDataArg> trades)
        {
            base.OnNewTrades(trades);

            System.Threading.Interlocked.Increment(ref _batchTradeCallbacks);
            if (trades != null)
            {
                int n = 0;
                foreach (var t in trades) { if (t != null) n++; }
                System.Threading.Interlocked.Add(ref _batchTradeItems, n);
            }
        }

        /// <summary>Counts batch depth deliveries. <b>Records nothing.</b> See <see cref="OnNewTrades"/>.</summary>
        protected override void MarketDepthsChanged(IEnumerable<MarketDataArg> depths)
        {
            base.MarketDepthsChanged(depths);

            System.Threading.Interlocked.Increment(ref _batchDepthCallbacks);
            if (depths != null)
            {
                int n = 0;
                foreach (var d in depths) { if (d != null) n++; }
                System.Threading.Interlocked.Add(ref _batchDepthItems, n);
            }
        }

        // ------------------------------------------------------------ DOM source

        /// <summary>
        /// Reads the platform's own order book. Called by the recorder on a
        /// source-time interval boundary.
        /// </summary>
        /// <remarks>
        /// <para>The book is read from the platform API rather than rebuilt from the
        /// depth-change stream on purpose. A locally reconstructed book is a
        /// function of the changes we received, so it can never contradict them and
        /// can never reveal a change that went missing. The platform's book can.</para>
        /// <para>MEASURED: <c>GetMarketDepthSnapshot()</c> returns a <b>flat</b>
        /// <c>IEnumerable&lt;MarketDataArg&gt;</c> — not separate ladders — so each
        /// row's side is read from the row itself.</para>
        /// </remarks>
        public DomBook GetBook(int depthLimit)
        {
            var provider = MarketDepthInfo;
            if (provider == null) return DomBook.Empty;

            var rows = provider.GetMarketDepthSnapshot();
            if (rows == null) return DomBook.Empty;

            var bids = new List<DomLevel>(depthLimit > 0 ? depthLimit : 64);
            var asks = new List<DomLevel>(depthLimit > 0 ? depthLimit : 64);

            foreach (var row in rows)
            {
                if (row == null) continue;

                // Rows arrive in the platform's own order and are appended in that
                // order. They are deliberately NOT sorted: sorting would impose an
                // ordering the API has not been shown to have, and would destroy the
                // evidence of what order the platform actually returned -- which is
                // one of the things the runtime experiment has to determine.
                var side = SideOf(row);
                if (side == Side.Bid)
                {
                    if (depthLimit > 0 && bids.Count >= depthLimit) continue;
                    bids.Add(new DomLevel(row.Price, row.Volume));
                }
                else if (side == Side.Ask)
                {
                    if (depthLimit > 0 && asks.Count >= depthLimit) continue;
                    asks.Add(new DomLevel(row.Price, row.Volume));
                }
            }

            return new DomBook(bids.ToArray(), asks.ToArray());
        }

        /// <summary>
        /// Book side of a row, preferring the platform's own IsBid/IsAsk flags and
        /// falling back to DataType. Returns null when the row is neither side.
        /// </summary>
        private static string SideOf(MarketDataArg row)
        {
            if (row.IsBid) return Side.Bid;
            if (row.IsAsk) return Side.Ask;
            return MapSide(row.DataType);
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

        /// <summary>
        /// Builds the raw instrument identity from what the platform reports,
        /// verbatim, in the <c>symbol@exchange</c> shape the recorder parses.
        /// </summary>
        /// <remarks>
        /// MEASURED: <c>IInstrumentInfo</c> exposes Instrument, Exchange, TickSize
        /// and TimeZone. Only the first two identify the contract; nothing is
        /// cased, trimmed or mapped, because canonical identity is derived from
        /// this downstream and must never replace it.
        /// </remarks>
        private string RawInstrumentSafe()
        {
            try
            {
                var info = InstrumentInfo;
                if (info == null) return "";

                string symbol = info.Instrument ?? "";
                string exchange = info.Exchange ?? "";

                if (symbol.Length == 0) return "";
                return exchange.Length > 0 ? symbol + "@" + exchange : symbol;
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
