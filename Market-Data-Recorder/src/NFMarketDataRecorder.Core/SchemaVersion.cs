namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// Output schema identity. Bump <see cref="Current"/> on ANY change to the
    /// on-disk event shape; the manifest records it so a capture can never be
    /// silently compared against one written by different rules.
    /// </summary>
    public static class SchemaVersion
    {
        public const string Current = "rev-3";

        /// <summary>
        /// Build identity of the recorder that produced a capture. Recorded so a
        /// dataset can tell which code wrote a partition, independently of the wire
        /// schema, when a defect is found later.
        /// </summary>
        public const string RecorderVersion = "0.1.0";
    }
}
