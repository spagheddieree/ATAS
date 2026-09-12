namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// Output schema identity. Bump <see cref="Current"/> on ANY change to the
    /// on-disk event shape; the manifest records it so a capture can never be
    /// silently compared against one written by different rules.
    /// </summary>
    public static class SchemaVersion
    {
        public const string Current = "rev-1";
    }
}
