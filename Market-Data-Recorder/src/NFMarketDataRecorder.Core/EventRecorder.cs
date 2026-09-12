using System;
using System.IO;
using System.Threading;

namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// Supplies the current order book from the platform's own depth API.
    /// </summary>
    /// <remarks>
    /// The recorder takes the book from the platform rather than rebuilding it
    /// from the depth-change stream. That independence is the point: a snapshot
    /// reconstructed from the changes could never disagree with them, so it could
    /// never reveal a missing change. The platform's book can.
    /// </remarks>
    public interface IDomSource
    {
        /// <param name="depthLimit">Levels per side to return; 0 means no limit.</param>
        DomBook GetBook(int depthLimit);
    }

    /// <summary>
    /// The whole recorder, and the only type the platform adapter needs.
    /// </summary>
    /// <remarks>
    /// <para><b>Threading.</b> <see cref="OnTrade"/> and <see cref="OnDepthChange"/>
    /// are safe to call from any thread and never block on I/O, never allocate
    /// unboundedly, and never throw. They do a sequence increment, a scheduler
    /// check, a bounded enqueue and a set of an event handle.</para>
    /// <para><b>Ordering.</b> A snapshot due at boundary B is emitted immediately
    /// before the first event whose source time is at or after B. That places it
    /// at a deterministic point in the stream, and means the snapshot reflects the
    /// book after every event strictly before B and none at or after it.</para>
    /// </remarks>
    public sealed class EventRecorder : IDisposable
    {
        private readonly RecorderOptions _opt;
        private readonly IDomSource _dom;
        private readonly BoundedEventQueue _queue;
        private readonly BackgroundWriter _writer;
        private readonly FaultLog _faults = new FaultLog();
        private readonly SourceClock _clock = new SourceClock();
        private readonly SnapshotScheduler _scheduler;
        private readonly ILineSink _sink;
        private readonly bool _ownsSink;

        // Guards the scheduler and the source clock. Held only for the duration of a
        // few integer comparisons plus, at a boundary, one platform book read. It is
        // never held across a queue operation that could fail, and never across I/O.
        private readonly object _scheduleGate = new object();

        private readonly CaptureHeader _header;
        private string _integrity = IntegrityState.Clean;
        private readonly object _integrityGate = new object();

        private long _seq;
        private long _trades;
        private long _depthChanges;
        private long _snapshots;
        private int _completed;
        private readonly DateTime _startedWallUtc = DateTime.UtcNow;

        public EventRecorder(RecorderOptions options, IDomSource domSource)
            : this(options, domSource, null)
        {
        }

        /// <param name="sink">
        /// Overrides the default file sink. Used by tests to inject failures; when
        /// null the recorder creates and owns <c>events.jsonl</c>.
        /// </param>
        public EventRecorder(RecorderOptions options, IDomSource domSource, ILineSink sink)
        {
            if (options == null) throw new ArgumentNullException("options");
            options.Validate();

            _opt = options;
            _dom = domSource;
            _scheduler = new SnapshotScheduler(options.SnapshotInterval, options.SnapshotMaxCatchUp);
            _queue = new BoundedEventQueue(options.QueueCapacity);

            Directory.CreateDirectory(options.OutputDirectory);

            if (sink == null)
            {
                _sink = new FileLineSink(Path.Combine(options.OutputDirectory, "events.jsonl"));
                _ownsSink = true;
            }
            else
            {
                _sink = sink;
                _ownsSink = false;
            }

            var rawInstrument = RawInstrumentIdentity.Parse(options.RawInstrument);
            _header = new CaptureHeader
            {
                RunId = string.IsNullOrEmpty(options.RunId)
                    ? Guid.NewGuid().ToString("N")
                    : options.RunId,
                RunLabel = options.RunLabel ?? "",
                SourceClassValue = SourceClass.RawSource,
                AcquisitionModeValue = options.AcquisitionMode,
                RawInstrument = rawInstrument,
                CanonicalInstrument = CanonicalInstrumentIdentity.Derive(rawInstrument),
                SourceTimeVerified = options.SourceTimeVerified,
                StartedWallUtc = _startedWallUtc,
            };
            _header.Validate();

            // Every fault deterministically worsens the run's integrity state, so a
            // run cannot silently stay CLEAN after a defect is observed.
            _faults.OnFault = code =>
            {
                lock (_integrityGate)
                    _integrity = IntegrityState.Worsen(_integrity, IntegrityState.ForFault(code));
            };

            _writer = new BackgroundWriter(_queue, _sink, _faults, options.FlushEveryLines);

            // Written before any market event and before the consumer thread exists,
            // so a file is self-describing from its first byte and a truncated capture
            // still carries its provenance.
            _writer.WritePreamble(_header.ToJson());
            _writer.Start();
        }

        public FaultLog Faults { get { return _faults; } }

        /// <summary>Run provenance written as the first line of the capture.</summary>
        public CaptureHeader Header { get { return _header; } }

        /// <summary>Canonical identity of this run.</summary>
        public string RunId { get { return _header.RunId; } }

        /// <summary>
        /// Current integrity state. Moves only downward, never back toward CLEAN.
        /// </summary>
        public string Integrity { get { lock (_integrityGate) return _integrity; } }
        public BoundedEventQueue Queue { get { return _queue; } }
        public long TradesSeen { get { return Interlocked.Read(ref _trades); } }
        public long DepthChangesSeen { get { return Interlocked.Read(ref _depthChanges); } }
        public long SnapshotsTaken { get { return Interlocked.Read(ref _snapshots); } }
        public bool IsCompleted { get { return Volatile.Read(ref _completed) != 0; } }

        // ---------------------------------------------------------------- capture

        /// <summary>Records one individual trade. Non-blocking; safe from any thread.</summary>
        public void OnTrade(DateTime sourceUtc, decimal price, decimal volume, string aggressor)
        {
            if (!Admit(ref sourceUtc)) return;
            EmitDueSnapshots(sourceUtc);
            Interlocked.Increment(ref _trades);
            Submit(RawEvent.Trade(NextSeq(), sourceUtc, DateTime.UtcNow, price, volume, aggressor));
        }

        /// <summary>
        /// Records one individual trade, including the optional raw fields the
        /// platform supplied. An absent optional is written as an absent key, never
        /// as zero.
        /// </summary>
        public void OnTrade(DateTime sourceUtc, decimal price, decimal volume, string aggressor,
                            decimal? originPrice, decimal? openInterest,
                            long? exchangeOrderId, long? aggressorExchangeOrderId)
        {
            if (!Admit(ref sourceUtc)) return;
            EmitDueSnapshots(sourceUtc);
            Interlocked.Increment(ref _trades);
            Submit(RawEvent.Trade(NextSeq(), sourceUtc, DateTime.UtcNow, price, volume, aggressor,
                                  originPrice, openInterest, exchangeOrderId, aggressorExchangeOrderId));
        }

        /// <summary>Records one individual depth change. Non-blocking; safe from any thread.</summary>
        public void OnDepthChange(DateTime sourceUtc, string side, decimal price, decimal volume)
        {
            if (!Admit(ref sourceUtc)) return;
            EmitDueSnapshots(sourceUtc);
            Interlocked.Increment(ref _depthChanges);
            Submit(RawEvent.Depth(NextSeq(), sourceUtc, DateTime.UtcNow, side, price, volume));
        }

        /// <summary>Records one individual depth change carrying an exchange order id.</summary>
        public void OnDepthChange(DateTime sourceUtc, string side, decimal price, decimal volume,
                                  long? exchangeOrderId)
        {
            if (!Admit(ref sourceUtc)) return;
            EmitDueSnapshots(sourceUtc);
            Interlocked.Increment(ref _depthChanges);
            Submit(RawEvent.Depth(NextSeq(), sourceUtc, DateTime.UtcNow, side, price, volume, exchangeOrderId));
        }

        /// <summary>
        /// Common admission: reject after shutdown, reject an unusable timestamp,
        /// and note a source-time regression without altering the timestamp.
        /// </summary>
        private bool Admit(ref DateTime sourceUtc)
        {
            if (IsCompleted)
            {
                _faults.Record(FaultCode.EnqueueAfterComplete, sourceUtc, Volatile.Read(ref _seq));
                return false;
            }

            if (sourceUtc == default(DateTime) || sourceUtc == DateTime.MinValue)
            {
                _faults.Record(FaultCode.MissingSourceTime, sourceUtc, Volatile.Read(ref _seq));
                return false;
            }

            if (sourceUtc.Kind == DateTimeKind.Local) sourceUtc = sourceUtc.ToUniversalTime();
            else if (sourceUtc.Kind == DateTimeKind.Unspecified) sourceUtc = DateTime.SpecifyKind(sourceUtc, DateTimeKind.Utc);

            return true;
        }

        private long NextSeq()
        {
            return Interlocked.Increment(ref _seq);
        }

        private void Submit(RawEvent e)
        {
            if (!_queue.TryEnqueue(e))
            {
                _faults.Record(FaultCode.QueueOverflow, e.SourceUtc, e.Seq,
                    "capacity=" + _opt.QueueCapacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return;
            }
            _writer.Signal();
        }

        /// <summary>
        /// Emits any snapshot whose source-time boundary has been reached. Called
        /// before the triggering event is enqueued so the snapshot lands first.
        /// </summary>
        private void EmitDueSnapshots(DateTime sourceUtc)
        {
            System.Collections.Generic.List<DateTime> due;
            bool truncated;

            lock (_scheduleGate)
            {
                if (_clock.Observe(sourceUtc))
                {
                    _faults.Record(FaultCode.SourceTimeRegression, sourceUtc, Volatile.Read(ref _seq),
                        "high=" + JsonLine.FormatTime(_clock.High));
                }

                due = _scheduler.Advance(sourceUtc, out truncated);

                // The book must be read under the same lock that advanced the
                // scheduler. Otherwise two threads crossing a boundary together could
                // read the book in the opposite order to the boundaries they own, and
                // a snapshot would be stamped with a time that does not match its
                // contents.
                for (int i = 0; i < due.Count; i++)
                {
                    DomBook book;
                    try
                    {
                        book = _dom != null ? _dom.GetBook(_opt.SnapshotDepthLimit) : DomBook.Empty;
                    }
                    catch (Exception ex)
                    {
                        _faults.Record(FaultCode.SnapshotSourceUnavailable, due[i], Volatile.Read(ref _seq),
                            ex.GetType().Name + ": " + ex.Message);
                        continue;
                    }

                    if (book == null)
                    {
                        _faults.Record(FaultCode.SnapshotSourceUnavailable, due[i], Volatile.Read(ref _seq), "null book");
                        continue;
                    }

                    Interlocked.Increment(ref _snapshots);
                    Submit(RawEvent.Snapshot(NextSeq(), due[i], DateTime.UtcNow,
                                             book.Bids, book.Asks, _opt.SnapshotDepthLimit));
                }
            }

            if (truncated)
            {
                _faults.Record(FaultCode.SnapshotCatchUpTruncated, sourceUtc, Volatile.Read(ref _seq),
                    "cap=" + _opt.SnapshotMaxCatchUp.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        // -------------------------------------------------------------- shutdown

        /// <summary>
        /// Stops accepting events, drains the queue, flushes durably and writes
        /// faults.jsonl then manifest.json.
        /// </summary>
        /// <remarks>
        /// The manifest is written last and only on this path, so its presence in an
        /// output directory is itself the signal that the run ended through the
        /// intended shutdown rather than by the process dying. Idempotent.
        /// </remarks>
        public RunManifest Complete()
        {
            if (Interlocked.Exchange(ref _completed, 1) != 0) return null;

            bool drained = _writer.DrainAndStop(_opt.DrainTimeout);
            if (!drained)
            {
                _faults.Record(FaultCode.DrainTimeout, _clock.High, Volatile.Read(ref _seq),
                    "pending=" + _queue.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (_ownsSink) _sink.Dispose();

            var manifest = new RunManifest
            {
                SchemaVersion = SchemaVersion.Current,
                RecorderVersion = SchemaVersion.RecorderVersion,
                RunId = _header.RunId,
                SourceClassValue = _header.SourceClassValue,
                AcquisitionModeValue = _header.AcquisitionModeValue,
                RawInstrument = _header.RawInstrument,
                CanonicalInstrument = _header.CanonicalInstrument,
                SourceTimeBasis = _header.SourceTimeBasis,
                SourceTimeVerified = _header.SourceTimeVerified,
                RunLabel = _opt.RunLabel ?? "",
                StartedWallUtc = _startedWallUtc,
                EndedWallUtc = DateTime.UtcNow,
                FirstSourceUtc = _clock.Started ? _clock.First : DateTime.MinValue,
                LastSourceUtc = _clock.Started ? _clock.High : DateTime.MinValue,
                SnapshotIntervalMs = (long)_opt.SnapshotInterval.TotalMilliseconds,
                SnapshotDepthLimit = _opt.SnapshotDepthLimit,
                QueueCapacity = _opt.QueueCapacity,
                QueueHighWater = _queue.HighWater,
                TradesSeen = TradesSeen,
                DepthChangesSeen = DepthChangesSeen,
                SnapshotsTaken = SnapshotsTaken,
                EventsWritten = _writer.Written,
                EventsDropped = _queue.Dropped,
                WriteFailures = _writer.WriteFailures,
                Drained = drained,
                Faults = _faults.Snapshot(),
            };

            // A capture is "complete" only if nothing was lost on either path. The
            // flag exists so downstream research can refuse a lossy file outright
            // instead of having to reason about the fault list.
            manifest.CaptureComplete = drained
                                       && manifest.EventsDropped == 0
                                       && manifest.WriteFailures == 0;

            // Integrity is the richer signal: CLEAN / DEGRADED / CORRUPT, derived
            // deterministically from the faults actually observed. A run that lost
            // events is CORRUPT; one that merely saw an anomaly is DEGRADED.
            lock (_integrityGate)
            {
                if (!manifest.CaptureComplete)
                    _integrity = IntegrityState.Worsen(_integrity, IntegrityState.Corrupt);
                manifest.IntegrityStateValue = _integrity;
            }

            WriteSidecars(manifest);
            return manifest;
        }

        private void WriteSidecars(RunManifest manifest)
        {
            string faultsPath = Path.Combine(_opt.OutputDirectory, "faults.jsonl");
            try
            {
                using (var fs = new FileLineSink(faultsPath))
                {
                    foreach (var f in manifest.Faults) fs.WriteLine(f.ToJson());
                    fs.Flush(true);
                }
            }
            catch (IOException ex)
            {
                manifest.SidecarError = "faults.jsonl: " + ex.Message;
            }

            // The field-availability register travels with every capture, so a
            // consumer can tell what this partition actually contains instead of
            // discovering a silent absence mid-analysis.
            try
            {
                File.WriteAllText(Path.Combine(_opt.OutputDirectory, "field-register.jsonl"),
                                  RecorderFieldRegister.ToJsonLines(), new System.Text.UTF8Encoding(false));
            }
            catch (IOException ex)
            {
                manifest.SidecarError = (manifest.SidecarError ?? "") + " field-register: " + ex.Message;
            }

            string eventsPath = Path.Combine(_opt.OutputDirectory, "events.jsonl");
            if (_ownsSink && File.Exists(eventsPath))
            {
                try { manifest.EventsSha256 = Hashing.Sha256File(eventsPath); }
                catch (IOException ex) { manifest.SidecarError = (manifest.SidecarError ?? "") + " events sha: " + ex.Message; }
            }

            try
            {
                File.WriteAllText(Path.Combine(_opt.OutputDirectory, "manifest.json"),
                                  manifest.ToJson(), new System.Text.UTF8Encoding(false));
            }
            catch (IOException)
            {
                // Nothing further can be reported: the manifest IS the report channel.
            }
        }

        public void Dispose()
        {
            Complete();
        }
    }
}
