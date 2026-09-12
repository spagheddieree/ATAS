namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// The kinds of record that may appear in <c>events.jsonl</c>.
    /// Only the first three are market data. Nothing here is derived: there is
    /// no delta, no imbalance, no ratio, no aggregate anywhere in this library.
    /// </summary>
    public static class EventKind
    {
        /// <summary>
        /// The first line of a capture file: run identity, provenance and instrument
        /// identity that apply to every event in the file. Not a market event.
        /// </summary>
        public const string Header = "header";

        /// <summary>One individual trade (print) as reported by the platform.</summary>
        public const string Trade = "trade";

        /// <summary>One individual market-depth change as reported by the platform.</summary>
        public const string Depth = "depth";

        /// <summary>
        /// A full order-book snapshot read from the platform's own depth API at a
        /// source-time interval boundary. Never reconstructed locally.
        /// </summary>
        public const string Snapshot = "snapshot";
    }

    /// <summary>Book side of a depth record.</summary>
    public static class Side
    {
        public const string Bid = "bid";
        public const string Ask = "ask";
    }

    /// <summary>
    /// Trade aggressor as reported by the platform. <see cref="Unknown"/> is a
    /// first-class value: replay feeds do not always carry direction, and
    /// guessing it would be a derived feature.
    /// </summary>
    public static class Aggressor
    {
        public const string Buy = "buy";
        public const string Sell = "sell";
        public const string Unknown = "unknown";
    }
}
