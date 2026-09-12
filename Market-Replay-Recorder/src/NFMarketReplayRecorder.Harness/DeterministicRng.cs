namespace NFMarketReplayRecorder.Harness
{
    /// <summary>
    /// xorshift64*, implemented here rather than using <c>System.Random</c>.
    /// </summary>
    /// <remarks>
    /// <c>System.Random</c>'s algorithm is not contractually stable across .NET
    /// versions, and it changed in .NET 6. A synthetic replay whose event script
    /// silently differed between runtimes would make every comparison meaningless,
    /// so the generator is pinned here in twelve lines instead.
    /// </remarks>
    public sealed class DeterministicRng
    {
        private ulong _s;

        public DeterministicRng(ulong seed)
        {
            _s = seed == 0 ? 0x9E3779B97F4A7C15UL : seed;
        }

        public ulong NextUInt64()
        {
            _s ^= _s >> 12;
            _s ^= _s << 25;
            _s ^= _s >> 27;
            return _s * 2685821657736338717UL;
        }

        /// <summary>Uniform in [0, bound).</summary>
        public int Next(int bound)
        {
            return (int)(NextUInt64() % (ulong)bound);
        }

        /// <summary>Uniform in [0, 1).</summary>
        public double NextDouble()
        {
            return (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
        }
    }
}
