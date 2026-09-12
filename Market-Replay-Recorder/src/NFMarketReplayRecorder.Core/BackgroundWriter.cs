using System;
using System.Text;
using System.Threading;

namespace NFMarketReplayRecorder.Core
{
    /// <summary>
    /// The single consumer. Owns the only thread that ever touches the sink, so
    /// the sink needs no locking and market-data callbacks never perform I/O.
    /// </summary>
    public sealed class BackgroundWriter
    {
        private readonly BoundedEventQueue _queue;
        private readonly ILineSink _sink;
        private readonly FaultLog _faults;
        private readonly int _flushEvery;
        private readonly StringBuilder _sb = new StringBuilder(256);

        private Thread _thread;
        private readonly ManualResetEventSlim _work = new ManualResetEventSlim(false);
        private volatile bool _draining;
        private volatile bool _stopped;
        private long _written;
        private long _writeFailures;

        /// <param name="flushEvery">
        /// Lines between buffer flushes. Small enough that a hard process kill loses
        /// little, large enough that flushing is not the bottleneck.
        /// </param>
        public BackgroundWriter(BoundedEventQueue queue, ILineSink sink, FaultLog faults, int flushEvery)
        {
            _queue = queue;
            _sink = sink;
            _faults = faults;
            _flushEvery = flushEvery < 1 ? 1 : flushEvery;
        }

        public long Written { get { return Interlocked.Read(ref _written); } }
        public long WriteFailures { get { return Interlocked.Read(ref _writeFailures); } }
        public bool Stopped { get { return _stopped; } }

        /// <summary>
        /// Writes one line directly to the sink before the consumer thread exists.
        /// </summary>
        /// <remarks>
        /// Must be called before <see cref="Start"/>. At that point this is the only
        /// thread touching the sink, so the preamble needs no synchronisation and is
        /// guaranteed to land ahead of every queued event.
        /// </remarks>
        public void WritePreamble(string line)
        {
            if (_thread != null)
                throw new InvalidOperationException("Preamble must be written before the writer thread starts.");

            try
            {
                _sink.WriteLine(line);
                _sink.Flush(false);
                // Deliberately NOT counted in _written: the header is provenance, not
                // a market event, and events_written must keep satisfying
                // written + dropped == events seen.
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _writeFailures);
                _faults.Record(FaultCode.WriteFailure, DateTime.MinValue, 0, "preamble: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        public void Start()
        {
            if (_thread != null) throw new InvalidOperationException("Writer already started.");
            _thread = new Thread(Loop)
            {
                // Background: a stuck writer must never keep the host process alive.
                // Shutdown drains explicitly, so this costs nothing on the clean path.
                IsBackground = true,
                Name = "market-data-writer",
            };
            _thread.Start();
        }

        /// <summary>Wakes the writer. Called from producer threads; must stay cheap.</summary>
        public void Signal()
        {
            _work.Set();
        }

        private void Loop()
        {
            long sinceFlush = 0;

            while (true)
            {
                RawEvent e;
                if (_queue.TryDequeue(out e))
                {
                    if (WriteOne(e)) sinceFlush++;

                    if (sinceFlush >= _flushEvery)
                    {
                        sinceFlush = 0;
                        SafeFlush(false);
                    }
                    continue;
                }

                if (_draining) break;

                // Queue empty and not draining: sleep until signalled. The timeout is
                // a safety net against a lost wake-up, not the normal path.
                _work.Wait(50);
                _work.Reset();
            }

            SafeFlush(true);
            _stopped = true;
        }

        private bool WriteOne(RawEvent e)
        {
            try
            {
                _sb.Length = 0;
                EventSerializer.Write(_sb, e);
                _sink.WriteLine(_sb.ToString());
                Interlocked.Increment(ref _written);
                return true;
            }
            catch (Exception ex)
            {
                // A write failure must not kill the writer thread: if it died, the
                // queue would fill, every later event would be dropped, and the run
                // would end with no manifest at all. Count it, keep draining, and let
                // the manifest declare the capture unusable.
                Interlocked.Increment(ref _writeFailures);
                _faults.Record(FaultCode.WriteFailure, e.SourceUtc, e.Seq, ex.GetType().Name + ": " + ex.Message);
                return false;
            }
        }

        private void SafeFlush(bool durable)
        {
            try
            {
                _sink.Flush(durable);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _writeFailures);
                _faults.Record(FaultCode.WriteFailure, DateTime.MinValue, -1, "flush: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Drains the queue and stops the thread. Returns false if the drain did not
        /// finish within <paramref name="timeout"/>.
        /// </summary>
        public bool DrainAndStop(TimeSpan timeout)
        {
            if (_thread == null) return true;
            _draining = true;
            _work.Set();
            return _thread.Join(timeout);
        }
    }
}
