using System;

namespace NFMarketReplayRecorder.Core
{
    /// <summary>
    /// How much processing had already happened before the data reached this dataset.
    /// </summary>
    /// <remarks>
    /// <para>This is the single most important provenance axis, and it exists because
    /// of a concrete hazard: a normalized historical store holding bars and footprint
    /// aggregates can look, at a glance, like it contains the trades that produced
    /// those aggregates. It does not. Aggregated bid/ask volume at a price level is
    /// not a trade tape, and no amount of transformation recovers the individual
    /// prints.</para>
    /// <para>Labelling such a source <see cref="RawSource"/> would let research
    /// silently treat reconstructed aggregates as observed events. The recorder is
    /// therefore required to declare its class, and <see cref="Validation"/> refuses
    /// event kinds a normalized source cannot honestly produce.</para>
    /// </remarks>
    public static class SourceClass
    {
        /// <summary>
        /// Event-level observations captured as the platform delivered them, with no
        /// aggregation applied by the producer.
        /// </summary>
        public const string RawSource = "RAW_SOURCE";

        /// <summary>
        /// Historical import where aggregation or transformation already occurred
        /// upstream — bars, footprint levels, summarised volumes.
        /// </summary>
        public const string NormalizedSource = "NORMALIZED_SOURCE";

        public static bool IsValid(string value)
        {
            return value == RawSource || value == NormalizedSource;
        }
    }

    /// <summary>
    /// Whether the data was acquired from a live feed or a historical replay.
    /// </summary>
    /// <remarks>
    /// Declared explicitly by the operator and <b>never inferred</b>. Inference from
    /// timestamp gaps, symbol, account, provider or date is banned: during a
    /// historical replay the receive clock is present-day while the event clock is
    /// historical, so every one of those signals is either ambiguous or actively
    /// misleading. A run whose mode was not declared stays
    /// <see cref="Unknown"/> and is reported as such rather than guessed at.
    /// </remarks>
    public static class AcquisitionMode
    {
        public const string Live = "LIVE";
        public const string Replay = "REPLAY";

        /// <summary>Not declared. A first-class value, never a silent default to LIVE.</summary>
        public const string Unknown = "UNKNOWN";

        public static bool IsValid(string value)
        {
            return value == Live || value == Replay || value == Unknown;
        }

        public static bool IsDeclared(string value)
        {
            return value == Live || value == Replay;
        }
    }

    /// <summary>
    /// Deterministic integrity state of a capture run.
    /// </summary>
    /// <remarks>
    /// <para>Replaces a bare pass/fail flag. The ordering is monotone —
    /// <see cref="Clean"/> → <see cref="Degraded"/> → <see cref="Corrupt"/> — and a
    /// run can only ever move downward. Once an integrity defect is observed the run
    /// cannot return to clean, no matter what happens afterwards.</para>
    /// <para>The distinction that matters to research: <b>degraded</b> means
    /// something was observed that makes the capture imperfect but still
    /// interpretable — a source-time regression, a snapshot the platform could not
    /// supply. <b>Corrupt</b> means events were actually lost or unwritten, so the
    /// stream has holes and cannot be treated as a complete record of the interval it
    /// claims to cover.</para>
    /// </remarks>
    public static class IntegrityState
    {
        public const string Clean = "CLEAN";
        public const string Degraded = "DEGRADED";
        public const string Corrupt = "CORRUPT";

        public static int Rank(string state)
        {
            switch (state)
            {
                case Clean: return 0;
                case Degraded: return 1;
                case Corrupt: return 2;
                default: return 2; // an unrecognised state is treated as the worst case
            }
        }

        /// <summary>Monotone worsening. Never improves a state.</summary>
        public static string Worsen(string current, string candidate)
        {
            return Rank(candidate) > Rank(current) ? candidate : current;
        }

        /// <summary>
        /// The state a given fault code implies, on its own.
        /// </summary>
        /// <remarks>
        /// The split is by whether data was <em>lost</em>. Overflow, write failure and
        /// a drain timeout all mean events that happened are not in the file, so the
        /// record has holes: CORRUPT. A source-time regression or an unavailable
        /// snapshot means the record is intact but imperfect: DEGRADED.
        /// </remarks>
        public static string ForFault(string faultCode)
        {
            switch (faultCode)
            {
                case FaultCode.QueueOverflow:
                case FaultCode.WriteFailure:
                case FaultCode.DrainTimeout:
                    return Corrupt;

                case FaultCode.SourceTimeRegression:
                case FaultCode.MissingSourceTime:
                case FaultCode.SnapshotCatchUpTruncated:
                case FaultCode.SnapshotSourceUnavailable:
                case FaultCode.EnqueueAfterComplete:
                    return Degraded;

                default:
                    return Degraded;
            }
        }
    }

    /// <summary>Contract rules that must hold regardless of who produced the data.</summary>
    public static class Validation
    {
        /// <summary>
        /// Whether a source of the given class may legitimately emit the given event kind.
        /// </summary>
        /// <remarks>
        /// The rule that stops a normalized historical import from masquerading as
        /// raw acquisition: individual trades, individual depth changes and order-book
        /// snapshots are observations, not aggregates. A source that only ever held
        /// bars and per-price summaries cannot produce them, so emitting them under
        /// <see cref="SourceClass.NormalizedSource"/> is a contract violation rather
        /// than a formatting choice.
        /// </remarks>
        public static bool MayEmit(string sourceClass, string eventKind)
        {
            if (sourceClass == SourceClass.RawSource) return true;

            if (sourceClass == SourceClass.NormalizedSource)
            {
                // A normalized source can never honestly claim event-level observations.
                return eventKind != EventKind.Trade
                    && eventKind != EventKind.Depth
                    && eventKind != EventKind.Snapshot;
            }

            return false;
        }

        public static void RequireMayEmit(string sourceClass, string eventKind)
        {
            if (!MayEmit(sourceClass, eventKind))
            {
                throw new InvalidOperationException(
                    "source_class " + (sourceClass ?? "<null>") + " may not emit event kind '" +
                    (eventKind ?? "<null>") + "': event-level observations cannot be produced by " +
                    "an aggregated source.");
            }
        }
    }
}
