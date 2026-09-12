using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using NFMarketDataRecorder.Core;

namespace NFMarketDataRecorder.Tests
{
    /// <summary>A temp directory that cleans itself up.</summary>
    public sealed class TempDir : IDisposable
    {
        public string Path { get; }

        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "rev-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string File(string name) => System.IO.Path.Combine(Path, name);

        public void Dispose()
        {
            try { Directory.Delete(Path, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>Captures lines in memory so a test can assert on exact output.</summary>
    public sealed class MemorySink : ILineSink
    {
        private readonly object _gate = new object();
        public readonly List<string> Lines = new List<string>();
        public int Flushes;
        public int DurableFlushes;

        public void WriteLine(string line) { lock (_gate) Lines.Add(line); }

        public void Flush(bool durable)
        {
            lock (_gate) { Flushes++; if (durable) DurableFlushes++; }
        }

        public void Dispose() { }
    }

    /// <summary>Fails the first <c>FailFirst</c> writes, then succeeds.</summary>
    public sealed class FailingSink : ILineSink
    {
        private int _seen;
        public int FailFirst;
        public bool FailFlush;
        public readonly List<string> Written = new List<string>();

        public void WriteLine(string line)
        {
            if (Interlocked.Increment(ref _seen) <= FailFirst)
                throw new IOException("simulated write failure");
            lock (Written) Written.Add(line);
        }

        public void Flush(bool durable)
        {
            if (FailFlush) throw new IOException("simulated flush failure");
        }

        public void Dispose() { }
    }

    /// <summary>A fixed order book.</summary>
    public sealed class StaticDom : IDomSource
    {
        public DomBook Book = DomBook.Empty;
        public int Calls;

        public DomBook GetBook(int depthLimit)
        {
            Interlocked.Increment(ref Calls);
            return Book;
        }
    }

    /// <summary>A depth source that always throws, for the unavailable-DOM path.</summary>
    public sealed class ThrowingDom : IDomSource
    {
        public DomBook GetBook(int depthLimit) => throw new InvalidOperationException("depth API unavailable");
    }

    public static class Sample
    {
        public static readonly DateTime Epoch = new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc);

        public static DomBook Book() => new DomBook(
            new[] { new DomLevel(19999.75m, 10m), new DomLevel(19999.50m, 20m) },
            new[] { new DomLevel(20000.00m, 15m), new DomLevel(20000.25m, 25m) });
    }
}
