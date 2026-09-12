using System;
using System.IO;
using System.Threading.Tasks;

using ReplayEventVerifier.Core;
using Xunit;

namespace ReplayEventVerifier.Tests
{
    /// <summary>Required area: clean shutdown and flushing.</summary>
    public class ShutdownTests
    {
        [Fact]
        public void Shutdown_drains_every_queued_event_before_returning()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            const int n = 20000;
            for (int i = 0; i < n; i++) rec.OnTrade(Sample.Epoch.AddMilliseconds(i), i, 1m, Aggressor.Buy);

            var m = rec.Complete();

            // No polling and no sleep: Complete() must not return until the queue is
            // empty, so the count is correct the instant it returns.
            Assert.Equal(n, sink.Lines.Count);
            Assert.Equal(n, m.EventsWritten);
            Assert.True(m.Drained);
            Assert.True(m.CaptureComplete);
        }

        [Fact]
        public void Shutdown_performs_a_durable_flush()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.Complete();

            Assert.True(sink.DurableFlushes >= 1, "shutdown did not request a durable flush");
        }

        [Fact]
        public void The_manifest_is_written_last_and_its_presence_means_a_clean_exit()
        {
            using var dir = new TempDir();

            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(Sample.Epoch, 20000.25m, 2m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.True(File.Exists(Path.Combine(dir.Path, "events.jsonl")));
            Assert.True(File.Exists(Path.Combine(dir.Path, "faults.jsonl")));
            Assert.True(File.Exists(Path.Combine(dir.Path, "manifest.json")));

            string json = File.ReadAllText(Path.Combine(dir.Path, "manifest.json"));
            Assert.Contains("\"capture_complete\":true", json);
            Assert.Contains("\"schema_version\":\"" + SchemaVersion.Current + "\"", json);
            Assert.Equal(64, m.EventsSha256.Length);
        }

        [Fact]
        public void The_manifest_hash_matches_the_file_that_was_written()
        {
            using var dir = new TempDir();

            var rec = new EventRecorder(Options(dir), new StaticDom());
            for (int i = 0; i < 100; i++) rec.OnTrade(Sample.Epoch.AddMilliseconds(i), i, 1m, Aggressor.Sell);
            var m = rec.Complete();

            Assert.Equal(Hashing.Sha256File(Path.Combine(dir.Path, "events.jsonl")), m.EventsSha256);
        }

        [Fact]
        public void Events_arriving_after_shutdown_are_refused_and_faulted()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.Complete();

            rec.OnTrade(Sample.Epoch.AddSeconds(1), 2m, 1m, Aggressor.Buy);
            rec.OnDepthChange(Sample.Epoch.AddSeconds(2), Side.Bid, 3m, 1m);

            Assert.Single(sink.Lines);
            Assert.Equal(2, rec.Faults.CountOf(FaultCode.EnqueueAfterComplete));
        }

        [Fact]
        public void Complete_is_idempotent()
        {
            using var dir = new TempDir();

            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);

            Assert.NotNull(rec.Complete());
            Assert.Null(rec.Complete());
            Assert.Null(rec.Complete());
        }

        [Fact]
        public void Dispose_completes_the_run()
        {
            using var dir = new TempDir();

            using (var rec = new EventRecorder(Options(dir), new StaticDom()))
            {
                rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            }

            Assert.True(File.Exists(Path.Combine(dir.Path, "manifest.json")));
        }

        [Fact]
        public void A_drain_timeout_is_reported_rather_than_hidden()
        {
            using var dir = new TempDir();
            var blocking = new BlockingSink();

            var opt = Options(dir);
            opt.DrainTimeout = TimeSpan.FromMilliseconds(200);
            opt.QueueCapacity = 4096;

            var rec = new EventRecorder(opt, new StaticDom(), blocking);
            for (int i = 0; i < 1000; i++) rec.OnTrade(Sample.Epoch.AddMilliseconds(i), i, 1m, Aggressor.Buy);

            var m = rec.Complete();

            Assert.False(m.Drained);
            Assert.False(m.CaptureComplete);
            Assert.Equal(1, rec.Faults.CountOf(FaultCode.DrainTimeout));

            blocking.Release();
            blocking.Dispose();
        }

        [Fact]
        public async Task Shutdown_while_producers_are_still_running_stays_consistent()
        {
            // Whatever the race, the invariant must hold: every event is either
            // written or accounted for as refused or dropped. Nothing vanishes.
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            const int threads = 4, per = 5000;

            var producers = Task.Run(() => Parallel.For(0, threads, t =>
            {
                for (int i = 0; i < per; i++)
                    rec.OnTrade(Sample.Epoch.AddMilliseconds(i), t, 1m, Aggressor.Buy);
            }));

            await Task.Delay(20);
            var m = rec.Complete();
            await producers.WaitAsync(TimeSpan.FromSeconds(30));

            long refused = rec.Faults.CountOf(FaultCode.EnqueueAfterComplete);
            long accounted = m.EventsWritten + m.EventsDropped + refused;

            Assert.Equal(threads * per, accounted);
            Assert.Equal(m.EventsWritten, sink.Lines.Count);
        }

        [Fact]
        public void Options_are_validated_before_any_file_is_created()
        {
            Assert.Throws<ArgumentException>(() => new RecorderOptions { OutputDirectory = "" }.Validate());
            Assert.Throws<ArgumentException>(() =>
                new RecorderOptions { OutputDirectory = "x", SnapshotInterval = TimeSpan.Zero }.Validate());
            Assert.Throws<ArgumentException>(() =>
                new RecorderOptions { OutputDirectory = "x", QueueCapacity = 0 }.Validate());
            Assert.Throws<ArgumentException>(() =>
                new RecorderOptions { OutputDirectory = "x", SnapshotMaxCatchUp = 0 }.Validate());
        }

        private static RecorderOptions Options(TempDir dir) => new RecorderOptions
        {
            OutputDirectory = dir.Path,
            Instrument = "NQ",
            RunLabel = "test",
            SnapshotInterval = TimeSpan.FromHours(1),
            DrainTimeout = TimeSpan.FromSeconds(30),
        };
    }
}
