using System.Collections.Concurrent;
using System.Threading;

namespace ReplayEventVerifier.Core
{
    /// <summary>
    /// A bounded multi-producer / single-consumer queue that never blocks a
    /// producer.
    /// </summary>
    /// <remarks>
    /// The hard rule for this tool is that a market-data callback must return
    /// immediately. Every bounded-queue design has to choose what to sacrifice
    /// when it fills: latency (block), memory (grow), or data (drop). Blocking
    /// would stall the platform's feed thread and corrupt the very timing we are
    /// measuring; growing without limit would eventually take the process down
    /// mid-capture. So this queue drops, counts what it dropped, and makes the
    /// loss visible in the manifest. A capture that is knowingly incomplete is
    /// usable research input; one that is silently incomplete is not.
    /// </remarks>
    public sealed class BoundedEventQueue
    {
        private readonly ConcurrentQueue<RawEvent> _q = new ConcurrentQueue<RawEvent>();
        private readonly int _capacity;
        private int _count;
        private long _dropped;
        private int _highWater;

        public BoundedEventQueue(int capacity)
        {
            _capacity = capacity < 1 ? 1 : capacity;
        }

        public int Capacity { get { return _capacity; } }
        public int Count { get { return Volatile.Read(ref _count); } }
        public long Dropped { get { return Interlocked.Read(ref _dropped); } }
        public int HighWater { get { return Volatile.Read(ref _highWater); } }

        /// <summary>
        /// Enqueues without ever blocking. Returns false when the queue was full
        /// and the event was dropped.
        /// </summary>
        public bool TryEnqueue(RawEvent e)
        {
            // Reserve a slot first. If the reservation overshoots capacity we give it
            // straight back, so _count can momentarily exceed capacity but never
            // settles above it, and no producer ever waits on another.
            int now = Interlocked.Increment(ref _count);
            if (now > _capacity)
            {
                Interlocked.Decrement(ref _count);
                Interlocked.Increment(ref _dropped);
                return false;
            }

            // Track the deepest the queue ever got: the single best signal for
            // whether the capacity is adequate for a given replay speed.
            int hw = Volatile.Read(ref _highWater);
            while (now > hw)
            {
                int seen = Interlocked.CompareExchange(ref _highWater, now, hw);
                if (seen == hw) break;
                hw = seen;
            }

            _q.Enqueue(e);
            return true;
        }

        public bool TryDequeue(out RawEvent e)
        {
            if (_q.TryDequeue(out e))
            {
                Interlocked.Decrement(ref _count);
                return true;
            }
            return false;
        }
    }
}
