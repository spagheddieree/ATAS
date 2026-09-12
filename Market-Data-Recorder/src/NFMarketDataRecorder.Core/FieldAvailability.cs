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
    /// <para>Every classification is pinned to evidence. Most are now MEASURED
    /// against the real ATAS assemblies (<c>docs/evidence/atas-api-report.md</c>,
    /// SHA-256 verified), which promoted price, volume, direction, book side and the
    /// exchange order ids, and demoted <c>source_sequence</c> and
    /// <c>exchange_trade_id</c> from unknown to <b>measured absent</b>.</para>
    /// <para>What metadata cannot settle stays <see cref="Availability.Unknown"/>:
    /// a property provably existing is not the same as its runtime meaning being
    /// known. <c>MarketDataArg.Time</c> is measured present, yet whether it carries
    /// exchange, replay or arrival time is unresolved and decides whether any timing
    /// analysis on this dataset is valid at all. Snapshot row ordering is the same
    /// shape of question. Both are for the GUI Replay experiment, not for
    /// assumption.</para>
    /// </remarks>
    public static class RecorderFieldRegister
    {
        private const string RecorderOwned =
            "produced by the recorder itself, independent of the ATAS API";

        /// <summary>Measured on the real assemblies; see docs/evidence/atas-api-report.md.</summary>
        private const string MeasuredArg =
            "MEASURED on ATAS.Indicators.MarketDataArg (api-report section 2), SHA-256 verified assemblies";

        /// <summary>
        /// The distinction this register exists to keep: a CLR property provably
        /// exists, but what its value MEANS at runtime is a different question that
        /// metadata cannot answer.
        /// </summary>
        private const string MeasuredSemanticsUnknown =
            "PROPERTY MEASURED on MarketDataArg, but its RUNTIME SEMANTICS are unverified; " +
            "metadata cannot establish meaning. Resolve via the GUI Replay experiment";

        private const string MeasuredAbsent =
            "MEASURED ABSENT: no such member on MarketDataArg or the indicator depth API in the " +
            "full 2,122-line metadata report";

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

                new FieldClassification("price",                 Availability.AvailableDirectly, MeasuredArg + ": Decimal Price"),
                new FieldClassification("volume",                Availability.AvailableDirectly, MeasuredArg + ": Decimal Volume"),
                new FieldClassification("aggressor_side",        Availability.AvailableDirectly, MeasuredArg + ": TradeDirection Direction {Between=0,Buy=1,Sell=2}; mapped directly, never inferred from price"),
                new FieldClassification("origin_price",          Availability.AvailableDirectly, MeasuredArg + ": Decimal OriginPrice; captured alongside Price, NOT substituted for it -- their relationship is unverified"),
                new FieldClassification("open_interest",         Availability.AvailableDirectly, MeasuredArg + ": Decimal OpenInterest"),
                new FieldClassification("exchange_order_id",     Availability.AvailableDirectly, MeasuredArg + ": Nullable<Int64> ExchangeOrderId -- an ORDER identifier, NOT a sequence"),
                new FieldClassification("aggressor_exchange_order_id", Availability.AvailableDirectly, MeasuredArg + ": Nullable<Int64> AggressorExchangeOrderId"),

                // The property exists and is captured; what the value MEANS does not.
                new FieldClassification("source_timestamp",      Availability.Unknown, MeasuredSemanticsUnknown +
                    ". DateTime Time is measured present, but whether it carries exchange time, replay time, platform-normalized time or arrival time is THE highest-risk open question -- if it is an arrival clock, no timing analysis on this dataset is valid"),

                new FieldClassification("event_timestamp",       Availability.Unknown, "a normalized event time distinct from source_timestamp; not introduced while it would merely duplicate it"),

                // Measured absent, not merely unverified.
                new FieldClassification("source_sequence",       Availability.Unavailable, MeasuredAbsent +
                    ". The only Sequence property in the whole report belongs to OFT.Phemex.WsMessages.Pushes.WsDepthPush, a crypto websocket message type unreachable from the indicator API. ExchangeOrderId is an order id and must NEVER be mapped here"),
                new FieldClassification("exchange_trade_id",     Availability.Unavailable, MeasuredAbsent + ". No trade-id member exists on MarketDataArg"),
                new FieldClassification("provider_feed",         Availability.Unknown, "not exposed on MarketDataArg; whether a connector/provider identity is reachable from an indicator is unverified"),
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

                new FieldClassification("side",                  Availability.AvailableDirectly, MeasuredArg + ": Boolean IsBid / Boolean IsAsk, with MarketDataType DataType {Bid=0,Ask=1,Trade=2} as fallback"),
                new FieldClassification("price",                 Availability.AvailableDirectly, MeasuredArg + ": Decimal Price"),
                new FieldClassification("volume",                Availability.AvailableDirectly, MeasuredArg + ": Decimal Volume"),
                new FieldClassification("exchange_order_id",     Availability.AvailableDirectly, MeasuredArg + ": Nullable<Int64> ExchangeOrderId"),

                new FieldClassification("source_timestamp",      Availability.Unknown, MeasuredSemanticsUnknown + ". DateTime Time measured present; meaning unverified"),

                new FieldClassification("level",                 Availability.Unavailable, MeasuredAbsent + ". MarketDataArg carries no ladder index"),
                new FieldClassification("update_type",           Availability.Unknown,
                    "no add/change/delete discriminator on MarketDataArg. Removal is presumed to arrive as volume 0, which is UNVERIFIED. Whether depth arrives incrementally at all, and whether MarketDepthChanged and MarketDepthsChanged both fire for one event, are runtime questions the GUI experiment must answer -- the adapter therefore binds the SINGLE callback only and merely counts the batch one"),
                new FieldClassification("source_sequence",       Availability.Unavailable, MeasuredAbsent),
                new FieldClassification("provider_feed",         Availability.Unknown, "not exposed on MarketDataArg"),
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

                new FieldClassification("side",                  Availability.AvailableDirectly, "MEASURED: IMarketDepthInfoProvider.GetMarketDepthSnapshot() returns a FLAT IEnumerable<MarketDataArg>; each row carries IsBid/IsAsk"),
                new FieldClassification("price",                 Availability.AvailableDirectly, MeasuredArg + ": Decimal Price on each snapshot row"),
                new FieldClassification("volume",                Availability.AvailableDirectly, MeasuredArg + ": Decimal Volume on each snapshot row"),

                new FieldClassification("level",                 Availability.Unknown,
                    "ROW ORDERING of GetMarketDepthSnapshot() is not expressed in metadata. Best-first, bids-descending and asks-ascending are all UNVERIFIED, so a positional level index cannot be asserted. Rows are recorded in the platform's own order and never sorted; depth truncation is therefore unsafe until ordering is established"),
                new FieldClassification("provider_feed",         Availability.Unknown, "not exposed on MarketDataArg"),
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
