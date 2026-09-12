using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using NFMarketReplayRecorder.Core;
using Xunit;

namespace NFMarketReplayRecorder.Tests
{
    /// <summary>Required area: queue overflow and write-failure integrity handling.</summary>
    public class OverflowAndFaultTests
    {
        // ------------------------------------------------------------ queue only

        [Fact]
        public void Queue_accepts_exactly_its_capacity_then_drops()
        {
            var q = new BoundedEventQueue(3);
            for (int i = 0; i < 3; i++) Assert.True(q.TryEnqueue(Trade(i)));

            Assert.False(q.TryEnqueue(Trade(99)));
            Assert.Equal(1, q.Dropped);
            Assert.Equal(3, q.Count);
        }

        [Fact]
        public void Dequeuing_frees_capacity_again()
        {
            var q = new BoundedEventQueue(2);
            q.TryEnqueue(Trade(1));
            q.TryEnqueue(Trade(2));
            Assert.False(q.TryEnqueue(Trade(3)));

            Assert.True(q.TryDequeue(out _));
            Assert.True(q.TryEnqueue(Trade(4)));
        }

        [Fact]
        public void Queue_records_its_high_water_mark()
        {
            var q = new BoundedEventQueue(100);
            for (int i = 0; i < 40; i++) q.TryEnqueue(Trade(i));
            for (int i = 0; i < 30; i++) q.TryDequeue(out _);
            for (int i = 0; i < 10; i++) q.TryEnqueue(Trade(i));

            Assert.Equal(40, q.HighWater);
        }

        [Fact]
        public void Queue_never_exceeds_capacity_under_concurrent_producers()
        {
            var q = new BoundedEventQueue(500);
            long accepted = 0;

            Parallel.For(0, 16, t =>
            {
                long local = 0;
                for (int i = 0; i < 1000; i++) if (q.TryEnqueue(Trade(i))) local++;
                System.Threading.Interlocked.Add(ref accepted, local);
            });

            Assert.Equal(500, accepted);
            Assert.Equal(500, q.Count);
            Assert.Equal(16 * 1000 - 500, q.Dropped);
        }

        [Fact]
        public void Enqueue_does_not_block_when_the_queue_is_full()
        {
            // The hard rule: a market-data callback must return immediately. If this
            // ever blocks, the recorder is distorting the timing it is measuring.
            var q = new BoundedEventQueue(1);
            q.TryEnqueue(Trade(1));

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++) q.TryEnqueue(Trade(i));
            sw.Stop();

            Assert.True(sw.Elapsed < TimeSpan.FromSeconds(5),
                "100k rejected enqueues took " + sw.Elapsed + "; the full-queue path is not non-blocking");
        }

        // ------------------------------------------------------- through recorder

        [Fact]
        public void Overflow_drops_events_records_a_fault_and_marks_the_capture_incomplete()
        {
            using var dir = new TempDir();

            // A sink that never returns lets the queue fill deterministically.
            var blocking = new BlockingSink();
            var opt = new RecorderOptions
            {
                OutputDirectory = dir.Path,
                SnapshotInterval = TimeSpan.FromHours(1),
                QueueCapacity = 16,
                DrainTimeout = TimeSpan.FromMilliseconds(250),
            };

            var rec = new EventRecorder(opt, new StaticDom(), blocking);
            for (int i = 0; i < 5000; i++) rec.OnTrade(Sample.Epoch.AddMilliseconds(i), i, 1m, Aggressor.Buy);

            blocking.Release();
            var m = rec.Complete();

            Assert.True(m.EventsDropped > 0, "expected drops with a 16-slot queue and 5000 events");
            Assert.True(rec.Faults.CountOf(FaultCode.QueueOverflow) > 0);
            Assert.False(m.CaptureComplete);

            // The loss is visible on disk, not only in memory.
            Assert.Contains(FaultCode.QueueOverflow, File.ReadAllText(Path.Combine(dir.Path, "faults.jsonl")));
            Assert.Contains("\"capture_complete\":false", File.ReadAllText(Path.Combine(dir.Path, "manifest.json")));
        }

        [Fact]
        public void A_write_failure_does_not_kill_the_writer_and_later_events_still_land()
        {
            // If a write failure killed the writer thread, the queue would fill, every
            // later event would be dropped, and the run would end with no manifest.
            using var dir = new TempDir();

            // The capture header is written first, so it absorbs the first simulated
            // failure: 3 failures = the header plus the first 2 trades. The header
            // failing is itself worth exercising -- a capture whose provenance line is
            // missing must still fail loudly rather than produce an unlabelled file.
            var sink = new FailingSink { FailFirst = 3 };

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            for (int i = 0; i < 10; i++) rec.OnTrade(Sample.Epoch.AddMilliseconds(i), i, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.Equal(3, m.WriteFailures);
            Assert.Equal(8, sink.Written.Count);   // 10 trades - 2 that failed
            Assert.Equal(8, m.EventsWritten);      // header is provenance, never counted
            Assert.False(m.CaptureComplete);
            Assert.Equal(IntegrityState.Corrupt, m.IntegrityStateValue);
            Assert.Equal(3, rec.Faults.CountOf(FaultCode.WriteFailure));
        }

        [Fact]
        public void A_flush_failure_is_recorded_rather_than_thrown()
        {
            using var dir = new TempDir();
            var sink = new FailingSink { FailFlush = true };

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.True(m.WriteFailures > 0);
            Assert.False(m.CaptureComplete);
        }

        [Fact]
        public void A_failing_depth_api_faults_the_snapshot_without_losing_market_events()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var opt = Options(dir);
            opt.SnapshotInterval = TimeSpan.FromSeconds(1);

            var rec = new EventRecorder(opt, new ThrowingDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.OnTrade(Sample.Epoch.AddSeconds(2), 2m, 1m, Aggressor.Buy);
            rec.Complete();

            Assert.Equal(2, rec.Faults.CountOf(FaultCode.SnapshotSourceUnavailable));
            Assert.Equal(2, sink.MarketLines.Count); // both trades survived
            Assert.DoesNotContain(sink.MarketLines, l => l.Contains("\"kind\":\"snapshot\""));
        }

        [Fact]
        public void An_oversized_source_time_gap_raises_the_catchup_fault()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var opt = Options(dir);
            opt.SnapshotInterval = TimeSpan.FromSeconds(1);
            opt.SnapshotMaxCatchUp = 2;

            var rec = new EventRecorder(opt, new StaticDom { Book = Sample.Book() }, sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.OnTrade(Sample.Epoch.AddHours(1), 2m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.Equal(1, rec.Faults.CountOf(FaultCode.SnapshotCatchUpTruncated));
            Assert.Equal(2, m.SnapshotsTaken);
        }

        [Fact]
        public void Faults_are_aggregated_by_code_with_first_and_last_bounds()
        {
            var log = new FaultLog();
            log.Record(FaultCode.QueueOverflow, Sample.Epoch, 10, "first");
            log.Record(FaultCode.QueueOverflow, Sample.Epoch.AddSeconds(5), 20, "second");
            log.Record(FaultCode.WriteFailure, Sample.Epoch.AddSeconds(1), 15);

            var all = log.Snapshot();

            Assert.Equal(2, all.Count);
            Assert.Equal(FaultCode.QueueOverflow, all[0].Code); // ordinal ordering
            Assert.Equal(2, all[0].Count);
            Assert.Equal(10, all[0].FirstSeq);
            Assert.Equal(20, all[0].LastSeq);
            Assert.Equal("first", all[0].FirstDetail);
            Assert.Equal(3, log.TotalCount);
        }

        private static RawEvent Trade(int i) =>
            RawEvent.Trade(i, Sample.Epoch, Sample.Epoch, i, 1m, Aggressor.Buy);

        private static RecorderOptions Options(TempDir dir) => new RecorderOptions
        {
            OutputDirectory = dir.Path,
            SnapshotInterval = TimeSpan.FromHours(1),
            DrainTimeout = TimeSpan.FromSeconds(15),
        };
    }

    /// <summary>A sink whose writes park until released, so the queue can be filled on demand.</summary>
    internal sealed class BlockingSink : ILineSink
    {
        private readonly System.Threading.ManualResetEventSlim _gate = new System.Threading.ManualResetEventSlim(false);

        public void WriteLine(string line) => _gate.Wait(TimeSpan.FromSeconds(30));
        public void Flush(bool durable) { }
        public void Release() => _gate.Set();
        public void Dispose() => _gate.Dispose();
    }
}
