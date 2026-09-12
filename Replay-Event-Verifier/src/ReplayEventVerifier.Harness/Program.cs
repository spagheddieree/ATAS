using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

using ReplayEventVerifier.Core;

namespace ReplayEventVerifier.Harness
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0) { Usage(); return 2; }

            try
            {
                switch (args[0])
                {
                    case "synth": return Synth(Args.Parse(args, 1));
                    case "compare": return Compare(Args.Parse(args, 1));
                    case "inspect": return Inspect(Args.Parse(args, 1));
                    default: Usage(); return 2;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("error: " + ex.Message);
                return 1;
            }
        }

        private static void Usage()
        {
            Console.Error.WriteLine(@"replay-verifier — ATAS Replay event-stream verification harness

  synth    --out DIR [--label L] [--speed X] [--minutes N] [--rate EVENTS_PER_SEC]
           [--seed N] [--interval-ms MS] [--depth N] [--queue N] [--threads N]

           Drives the recorder with a deterministic synthetic feed. --speed is a
           WALL-CLOCK multiplier: the source timestamps in the script are fixed,
           so 1 and 10 must produce identical captures. --speed 0 runs with no
           pacing at all (as fast as possible).

  compare  --a DIR_A --b DIR_B [--report FILE]

           Compares two captures in canonical form (seq and recv_ts excluded).
           Exit 0 identical, 3 complete-but-unordered, 4 divergent.

  inspect  --dir DIR
           Prints a capture's manifest and line count.");
        }

        // ------------------------------------------------------------------ synth

        private static int Synth(Args a)
        {
            string outDir = a.Require("out");
            string label = a.Get("label", "unlabelled");
            double speed = a.GetDouble("speed", 1.0);
            int minutes = a.GetInt("minutes", 10);
            int rate = a.GetInt("rate", 300);
            ulong seed = (ulong)a.GetLong("seed", 20260911);
            int intervalMs = a.GetInt("interval-ms", 1000);
            int depth = a.GetInt("depth", 20);
            int queue = a.GetInt("queue", 262144);
            int threads = a.GetInt("threads", 1);
            int ladder = a.GetInt("ladder", 20);

            Console.WriteLine("building script: {0} min at {1} evt/s, seed {2} ...", minutes, rate, seed);
            var script = SyntheticFeed.Build(seed, minutes, rate, ladder);
            Console.WriteLine("script events: {0}", script.Count.ToString(CultureInfo.InvariantCulture));

            var book = new SyntheticBook();
            var options = new RecorderOptions
            {
                OutputDirectory = outDir,
                Instrument = "NQ (synthetic)",
                RunLabel = label,
                SnapshotInterval = TimeSpan.FromMilliseconds(intervalMs),
                SnapshotDepthLimit = depth,
                QueueCapacity = queue,
                DrainTimeout = TimeSpan.FromSeconds(60),
            };

            // A fixed epoch, so the source timestamps written to disk depend only on
            // the script and never on when the run happened. Without this, two runs
            // could never be compared line for line.
            var epoch = new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc);

            var sw = Stopwatch.StartNew();
            RunManifest manifest;

            using (var recorder = new EventRecorder(options, book))
            {
                if (threads <= 1) DriveSingle(recorder, book, script, epoch, speed);
                else DriveMulti(recorder, book, script, epoch, speed, threads);

                manifest = recorder.Complete();
            }

            sw.Stop();

            Console.WriteLine();
            Console.WriteLine("wall elapsed      : {0:F1}s", sw.Elapsed.TotalSeconds);
            Console.WriteLine("trades seen       : {0}", manifest.TradesSeen);
            Console.WriteLine("depth changes seen: {0}", manifest.DepthChangesSeen);
            Console.WriteLine("snapshots taken   : {0}", manifest.SnapshotsTaken);
            Console.WriteLine("events written    : {0}", manifest.EventsWritten);
            Console.WriteLine("events dropped    : {0}", manifest.EventsDropped);
            Console.WriteLine("write failures    : {0}", manifest.WriteFailures);
            Console.WriteLine("queue high water  : {0} / {1}", manifest.QueueHighWater, manifest.QueueCapacity);
            Console.WriteLine("drained           : {0}", manifest.Drained);
            Console.WriteLine("capture_complete  : {0}", manifest.CaptureComplete);
            Console.WriteLine("events sha256     : {0}", manifest.EventsSha256);
            foreach (var f in manifest.Faults)
                Console.WriteLine("FAULT {0} x{1}", f.Code, f.Count);

            return manifest.CaptureComplete ? 0 : 5;
        }

        /// <summary>
        /// Single producer: the deterministic path. Event order into the recorder is
        /// exactly script order, so the capture is reproducible byte for byte.
        /// </summary>
        private static void DriveSingle(EventRecorder recorder, SyntheticBook book,
                                        List<ScriptEvent> script, DateTime epoch, double speed)
        {
            var pacer = new Pacer(speed);
            for (int i = 0; i < script.Count; i++)
            {
                var e = script[i];
                pacer.WaitUntil(e.OffsetTicks);
                Emit(recorder, book, e, epoch);
            }
        }

        /// <summary>
        /// Multiple producers over disjoint slices. Used to prove the recorder is
        /// thread-safe and lossless under contention; the resulting capture is
        /// intentionally NOT expected to match the single-threaded one line for
        /// line, because concurrent producers genuinely interleave.
        /// </summary>
        private static void DriveMulti(EventRecorder recorder, SyntheticBook book,
                                       List<ScriptEvent> script, DateTime epoch, double speed, int threads)
        {
            var workers = new Thread[threads];
            for (int t = 0; t < threads; t++)
            {
                int id = t;
                workers[t] = new Thread(() =>
                {
                    var pacer = new Pacer(speed);
                    for (int i = id; i < script.Count; i += threads)
                    {
                        var e = script[i];
                        pacer.WaitUntil(e.OffsetTicks);
                        Emit(recorder, book, e, epoch);
                    }
                });
                workers[t].Start();
            }
            for (int t = 0; t < threads; t++) workers[t].Join();
        }

        private static void Emit(EventRecorder recorder, SyntheticBook book, ScriptEvent e, DateTime epoch)
        {
            DateTime src = epoch.AddTicks(e.OffsetTicks);

            if (e.IsTrade)
            {
                recorder.OnTrade(src, e.Price, e.Volume, e.Aggressor);
                return;
            }

            // Record first, then mutate the book, so a snapshot due at boundary B
            // reflects every event strictly before B and none at or after it.
            recorder.OnDepthChange(src, e.Side, e.Price, e.Volume);
            book.Apply(e.Side, e.Price, e.Volume);
        }

        /// <summary>
        /// Paces emission against the wall clock at a speed multiplier. Speed
        /// affects only how long the run takes; it never touches a timestamp.
        /// </summary>
        private sealed class Pacer
        {
            private readonly double _speed;
            private readonly Stopwatch _sw = Stopwatch.StartNew();

            public Pacer(double speed) { _speed = speed; }

            public void WaitUntil(long sourceOffsetTicks)
            {
                if (_speed <= 0) return;

                double targetMs = (sourceOffsetTicks / (double)TimeSpan.TicksPerMillisecond) / _speed;
                double aheadMs = targetMs - _sw.Elapsed.TotalMilliseconds;
                if (aheadMs <= 1.0) return;
                Thread.Sleep((int)aheadMs);
            }
        }

        // ---------------------------------------------------------------- compare

        private static int Compare(Args a)
        {
            string dirA = a.Require("a");
            string dirB = a.Require("b");
            string report = a.Get("report", null);

            string fileA = Path.Combine(dirA, "events.jsonl");
            string fileB = Path.Combine(dirB, "events.jsonl");

            var canonA = StreamComparer.LoadCanonical(fileA);
            var canonB = StreamComparer.LoadCanonical(fileB);

            var result = StreamComparer.Compare(canonA, canonB);
            string text = StreamComparer.Report(result, fileA, fileB);

            Console.Write(text);
            if (!string.IsNullOrEmpty(report))
            {
                File.WriteAllText(report, text);
                Console.WriteLine("report written: " + report);
            }

            if (result.StrictMatch) return 0;
            if (result.TimestampBucketMatch) return 3;
            return 4;
        }

        private static int Inspect(Args a)
        {
            string dir = a.Require("dir");
            string manifest = Path.Combine(dir, "manifest.json");
            string events = Path.Combine(dir, "events.jsonl");

            Console.WriteLine("manifest: " + (File.Exists(manifest) ? File.ReadAllText(manifest).Trim() : "MISSING"));

            if (File.Exists(events))
            {
                long n = 0;
                using (var r = new StreamReader(events))
                    while (r.ReadLine() != null) n++;
                Console.WriteLine("events.jsonl lines: " + n.ToString(CultureInfo.InvariantCulture));
                Console.WriteLine("events.jsonl sha256: " + Hashing.Sha256File(events));
            }
            else Console.WriteLine("events.jsonl MISSING");

            return 0;
        }
    }

    /// <summary>Minimal --key value argument parser.</summary>
    internal sealed class Args
    {
        private readonly Dictionary<string, string> _m = new Dictionary<string, string>(StringComparer.Ordinal);

        public static Args Parse(string[] argv, int from)
        {
            var a = new Args();
            for (int i = from; i < argv.Length; i++)
            {
                if (!argv[i].StartsWith("--", StringComparison.Ordinal)) continue;
                string key = argv[i].Substring(2);
                string val = (i + 1 < argv.Length && !argv[i + 1].StartsWith("--", StringComparison.Ordinal))
                    ? argv[++i] : "true";
                a._m[key] = val;
            }
            return a;
        }

        public string Get(string k, string dflt) { string v; return _m.TryGetValue(k, out v) ? v : dflt; }

        public string Require(string k)
        {
            string v;
            if (!_m.TryGetValue(k, out v)) throw new ArgumentException("missing required --" + k);
            return v;
        }

        public int GetInt(string k, int dflt)
        {
            string v = Get(k, null);
            return v == null ? dflt : int.Parse(v, CultureInfo.InvariantCulture);
        }

        public long GetLong(string k, long dflt)
        {
            string v = Get(k, null);
            return v == null ? dflt : long.Parse(v, CultureInfo.InvariantCulture);
        }

        public double GetDouble(string k, double dflt)
        {
            string v = Get(k, null);
            return v == null ? dflt : double.Parse(v, CultureInfo.InvariantCulture);
        }
    }
}
