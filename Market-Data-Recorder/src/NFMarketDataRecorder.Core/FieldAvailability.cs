using System;
using System.Collections.Generic;
using System.Text;

namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// How well a contract field is actually supported by the source, as evidenced.
    /// </summary>
    /// <remarks>
    /// The whole point is to stop a field appearing in output because the dataset
    /// contract wants it. A field is only ever promoted on evidence, and
    /// <see cref="Unknown"/> is a real, expected, publishable state — not a gap to be
    /// tidied away before shipping.
    /// </remarks>
    public static class Availability
    {
        /// <summary>The source supplies this field directly. Verified.</summary>
        public const string AvailableDirectly = "AVAILABLE_DIRECTLY";

        /// <summary>Computable from available fields with nothing lost. Verified.</summary>
        public const string DerivableWithoutLoss = "DERIVABLE_WITHOUT_INFORMATION_LOSS";

        /// <summary>Computable only approximately; the derivation discards information. Verified.</summary>
        public const string DerivableWithLoss = "DERIVABLE_WITH_INFORMATION_LOSS";

        /// <summary>Verified absent. The source demonstrably does not carry it.</summary>
        public const string Unavailable = "UNAVAILABLE";

        /// <summary>Not yet verified either way. Never treated as present.</summary>
        public const string Unknown = "UNKNOWN_NOT_YET_VERIFIED";

        public static int Rank(string a)
        {
            switch (a)
            {
                case AvailableDirectly: return 4;
                case DerivableWithoutLoss: return 3;
                case DerivableWithLoss: return 2;
                case Unavailable: return 1;
                case Unknown: return 0;
                default: return 0;
            }
        }

        /// <summary>True where the field may legitimately carry a value in output.</summary>
        public static bool IsPresent(string a)
        {
            return a == AvailableDirectly || a == DerivableWithoutLoss || a == DerivableWithLoss;
        }
    }

    /// <summary>One field's classification, with the evidence that justifies it.</summary>
    public sealed class FieldClassification
    {
        public readonly string Field;
        public readonly string State;
        public readonly string Evidence;

        public FieldClassification(string field, string state, string evidence)
        {
            Field = field;
            State = state;
            Evidence = evidence ?? "";
        }

        public string ToJson()
        {
            var sb = new StringBuilder(200);
            var j = new JsonLine(sb);
            j.Str("field", Field).Str("state", State).Str("evidence", Evidence);
            j.End();
            return sb.ToString();
        }
    }

    /// <summary>
    /// The recorder's declared field-availability register, emitted with every run.
    /// </summary>
    /// <remarks>
    /// <para>This is what makes heterogeneous source completeness workable. A dataset
    /// assembled from partitions of differing richness needs each partition to state
    /// what it actually contains, so a consumer can decide whether a partition can
    /// answer a given question instead of discovering a silent null mid-analysis.</para>
    /// <para>Every classification below is pinned to the evidence in
    /// <c>docs/ATAS-API-VERIFICATION.md</c>. Where the ATAS API surface is still
    /// unverified the field is <see cref="Availability.Unknown"/> — <b>not</b>
    /// available — and it stays that way until the API probe is run against a real
    /// installation. Promoting a field here without that evidence would be the exact
    /// failure this register exists to prevent.</para>
    /// </remarks>
    public static class RecorderFieldRegister
    {
        private const string ProbePending =
            "ATAS API unverified: no SDK reachable; run tools/NFMarketDataRecorder.ApiProbe (docs/ATAS-API-VERIFICATION.md)";

        private const string RecorderOwned =
            "produced by the recorder itself, independent of the ATAS API";

        private const string AssumedArgField =
            "assumed present on the ATAS market-data event; UNVERIFIED until the API probe runs";

        /// <summary>Classification for the raw trade contract.</summary>
        public static List<FieldClassification> TradeEvent()
        {
            return new List<FieldClassification>
            {
                new FieldClassification("schema_version",        Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("run_id",                Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("recorder_seq",          Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("receive_timestamp",     Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("acquisition_mode",      Availability.AvailableDirectly, "declared by the operator; never inferred"),
                new FieldClassification("integrity_state",       Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("raw_instrument",        Availability.AvailableDirectly, "captured verbatim from operator/platform declaration"),
                new FieldClassification("canonical_instrument",  Availability.DerivableWithoutLoss, "derived from raw identity; raw retained alongside"),

                new FieldClassification("source_timestamp",      Availability.Unknown, AssumedArgField + "; whether it is the feed clock or arrival clock is the single highest-risk open question"),
                new FieldClassification("price",                 Availability.Unknown, AssumedArgField),
                new FieldClassification("volume",                Availability.Unknown, AssumedArgField),
                new FieldClassification("aggressor_side",        Availability.Unknown, AssumedArgField + "; never inferred from price"),

                new FieldClassification("event_timestamp",       Availability.Unknown, "distinct normalized event time; indistinguishable from source_timestamp until the source clock is verified"),
                new FieldClassification("source_sequence",       Availability.Unknown, ProbePending + "; no exchange sequence is assumed to exist"),
                new FieldClassification("exchange_trade_id",     Availability.Unknown, ProbePending),
                new FieldClassification("exchange_order_id",     Availability.Unknown, ProbePending),
                new FieldClassification("provider_feed",         Availability.Unknown, ProbePending),
            };
        }

        /// <summary>Classification for the raw depth-update contract.</summary>
        public static List<FieldClassification> DepthUpdate()
        {
            return new List<FieldClassification>
            {
                new FieldClassification("schema_version",        Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("run_id",                Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("recorder_seq",          Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("receive_timestamp",     Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("acquisition_mode",      Availability.AvailableDirectly, "declared by the operator; never inferred"),
                new FieldClassification("integrity_state",       Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("raw_instrument",        Availability.AvailableDirectly, "captured verbatim"),
                new FieldClassification("canonical_instrument",  Availability.DerivableWithoutLoss, "derived; raw retained"),

                new FieldClassification("source_timestamp",      Availability.Unknown, AssumedArgField),
                new FieldClassification("side",                  Availability.Unknown, AssumedArgField),
                new FieldClassification("price",                 Availability.Unknown, AssumedArgField),
                new FieldClassification("volume",                Availability.Unknown, AssumedArgField),

                new FieldClassification("level",                 Availability.Unknown, ProbePending),
                new FieldClassification("update_type",           Availability.Unknown, ProbePending + "; whether depth arrives as incremental changes or whole-book refreshes is a primary open finding"),
                new FieldClassification("source_sequence",       Availability.Unknown, ProbePending),
                new FieldClassification("provider_feed",         Availability.Unknown, ProbePending),
            };
        }

        /// <summary>Classification for the depth-snapshot contract.</summary>
        public static List<FieldClassification> DepthSnapshot()
        {
            return new List<FieldClassification>
            {
                new FieldClassification("schema_version",        Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("run_id",                Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("snapshot_id",           Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("recorder_seq",          Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("source_timestamp",      Availability.AvailableDirectly, "the source-time interval boundary the snapshot was scheduled on; recorder-computed from observed source time"),
                new FieldClassification("receive_timestamp",     Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("acquisition_mode",      Availability.AvailableDirectly, "declared by the operator"),
                new FieldClassification("integrity_state",       Availability.AvailableDirectly, RecorderOwned),
                new FieldClassification("raw_instrument",        Availability.AvailableDirectly, "captured verbatim"),
                new FieldClassification("canonical_instrument",  Availability.DerivableWithoutLoss, "derived; raw retained"),
                new FieldClassification("depth_limit",           Availability.AvailableDirectly, "recorder configuration"),

                new FieldClassification("side",                  Availability.Unknown, AssumedArgField),
                new FieldClassification("level",                 Availability.Unknown, "ladder ORDERING returned by the depth API is unverified; index meaning cannot be asserted until then"),
                new FieldClassification("price",                 Availability.Unknown, AssumedArgField),
                new FieldClassification("volume",                Availability.Unknown, AssumedArgField),
                new FieldClassification("provider_feed",         Availability.Unknown, ProbePending),
            };
        }

        /// <summary>All three contracts, keyed by contract name.</summary>
        public static Dictionary<string, List<FieldClassification>> All()
        {
            return new Dictionary<string, List<FieldClassification>>(StringComparer.Ordinal)
            {
                { "TradeEvent", TradeEvent() },
                { "DepthUpdate", DepthUpdate() },
                { "DepthSnapshot", DepthSnapshot() },
            };
        }

        /// <summary>
        /// True when at least one field in any contract is still unverified, i.e. the
        /// recorder's own field claims are not yet fully evidenced.
        /// </summary>
        public static bool HasUnverifiedFields()
        {
            foreach (var kv in All())
                foreach (var f in kv.Value)
                    if (f.State == Availability.Unknown) return true;
            return false;
        }

        /// <summary>Writes the register as JSON lines, one contract per line.</summary>
        public static string ToJsonLines()
        {
            var sb = new StringBuilder(4096);
            foreach (var kv in All())
            {
                sb.Append("{\"contract\":");
                JsonLine.WriteString(sb, kv.Key);
                sb.Append(",\"fields\":[");
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(kv.Value[i].ToJson());
                }
                sb.Append("]}\n");
            }
            return sb.ToString();
        }
    }
}
