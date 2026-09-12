using System;
using System.Globalization;
using System.Text;

namespace NFMarketReplayRecorder.Core
{
    /// <summary>
    /// The instrument exactly as the platform named it, preserved verbatim.
    /// </summary>
    /// <remarks>
    /// <para>Nothing here is cleaned, cased, trimmed of decoration, or mapped onto a
    /// tidy internal symbol. Normalising at acquisition destroys the evidence needed
    /// to tell two partitions apart later, and real feeds produce exactly the kind of
    /// variation that makes that fatal: the same contract can arrive as
    /// <c>dxFeed|NQU6@CME</c> from one provider and <c>UNKNOWN|#NQU6@CME</c> from
    /// another, with casing differences on top.</para>
    /// <para>Canonical identity is <em>derived</em> from this
    /// (<see cref="CanonicalInstrumentIdentity"/>) and stored alongside it, never
    /// instead of it.</para>
    /// </remarks>
    public sealed class RawInstrumentIdentity
    {
        /// <summary>Provider or feed name exactly as reported, e.g. "dxFeed". Empty if unreported.</summary>
        public readonly string Provider;

        /// <summary>Symbol exactly as reported, decoration and casing intact, e.g. "#NQU6".</summary>
        public readonly string Symbol;

        /// <summary>Exchange exactly as reported, e.g. "CME". Empty if unreported.</summary>
        public readonly string Exchange;

        public RawInstrumentIdentity(string provider, string symbol, string exchange)
        {
            Provider = provider ?? "";
            Symbol = symbol ?? "";
            Exchange = exchange ?? "";
        }

        public static readonly RawInstrumentIdentity Undeclared = new RawInstrumentIdentity("", "", "");

        public bool IsDeclared { get { return Symbol.Length > 0; } }

        /// <summary>
        /// Parses the <c>provider|symbol@exchange</c> shape seen in practice, without
        /// altering any component it extracts. A string that does not match simply
        /// becomes the symbol.
        /// </summary>
        public static RawInstrumentIdentity Parse(string composite)
        {
            if (string.IsNullOrEmpty(composite)) return Undeclared;

            string provider = "";
            string rest = composite;

            int bar = rest.IndexOf('|');
            if (bar >= 0)
            {
                provider = rest.Substring(0, bar);
                rest = rest.Substring(bar + 1);
            }

            string exchange = "";
            int at = rest.LastIndexOf('@');
            if (at >= 0)
            {
                exchange = rest.Substring(at + 1);
                rest = rest.Substring(0, at);
            }

            return new RawInstrumentIdentity(provider, rest, exchange);
        }

        /// <summary>Round-trips back to the composite form, byte-for-byte where parsed from one.</summary>
        public string ToComposite()
        {
            var sb = new StringBuilder();
            if (Provider.Length > 0) sb.Append(Provider).Append('|');
            sb.Append(Symbol);
            if (Exchange.Length > 0) sb.Append('@').Append(Exchange);
            return sb.ToString();
        }
    }

    /// <summary>Contract series classification.</summary>
    public static class SeriesKind
    {
        /// <summary>A specific deliverable contract, e.g. NQU6 (September 2026).</summary>
        public const string ActualContract = "ACTUAL_CONTRACT";

        /// <summary>A stitched continuous series across contract rolls.</summary>
        public const string Continuous = "CONTINUOUS";

        /// <summary>Could not be determined from the raw identity.</summary>
        public const string Unknown = "UNKNOWN";
    }

    /// <summary>Contract size class within a product family.</summary>
    public static class SizeClass
    {
        public const string Full = "FULL";
        public const string Mini = "MINI";
        public const string Micro = "MICRO";
        public const string Unknown = "UNKNOWN";
    }

    /// <summary>
    /// Identity derived from <see cref="RawInstrumentIdentity"/> for grouping and joins.
    /// </summary>
    /// <remarks>
    /// <para>Derived, never authoritative. It exists so research can group partitions
    /// deliberately, and it is deliberately conservative: anything it cannot establish
    /// from the raw identity stays <c>UNKNOWN</c> rather than being guessed.</para>
    /// <para>Three merges must never happen silently, and each is prevented by a
    /// distinct field rather than by convention:</para>
    /// <list type="bullet">
    /// <item><b>NQ vs MNQ</b> — different products with different order books. Kept
    /// apart by <see cref="Root"/> plus <see cref="Size"/>.</item>
    /// <item><b>Actual vs continuous</b> — a stitched series is a construction, not a
    /// traded book. Kept apart by <see cref="Series"/>.</item>
    /// <item><b>Provider pooling</b> — two feeds of "the same" contract are two
    /// observations with different gaps and clocks. Kept apart by carrying the
    /// provider in <see cref="PartitionKey"/>.</item>
    /// </list>
    /// </remarks>
    public sealed class CanonicalInstrumentIdentity
    {
        /// <summary>Product root with size decoration removed, e.g. "NQ" for both NQ and MNQ.</summary>
        public readonly string Root;

        /// <summary>Size class. This is what keeps NQ and MNQ apart despite a shared root.</summary>
        public readonly string Size;

        /// <summary>Contract code as identified, e.g. "NQU6". Empty when not determinable.</summary>
        public readonly string Contract;

        public readonly string Series;
        public readonly string Exchange;
        public readonly string Provider;

        public CanonicalInstrumentIdentity(string root, string size, string contract,
                                           string series, string exchange, string provider)
        {
            Root = root ?? "";
            Size = size ?? SizeClass.Unknown;
            Contract = contract ?? "";
            Series = series ?? SeriesKind.Unknown;
            Exchange = exchange ?? "";
            Provider = provider ?? "";
        }

        /// <summary>
        /// The grouping key. Every axis that must not be pooled appears in it, so two
        /// partitions that differ on any of them cannot collide.
        /// </summary>
        public string PartitionKey
        {
            get
            {
                return string.Join("/", new[]
                {
                    Provider.Length > 0 ? Provider : "-",
                    Exchange.Length > 0 ? Exchange : "-",
                    Root.Length > 0 ? Root : "-",
                    Size,
                    Series,
                    Contract.Length > 0 ? Contract : "-",
                });
            }
        }

        /// <summary>
        /// Derives canonical identity conservatively. Anything not establishable from
        /// the raw identity is left UNKNOWN rather than assumed.
        /// </summary>
        public static CanonicalInstrumentIdentity Derive(RawInstrumentIdentity raw)
        {
            if (raw == null) raw = RawInstrumentIdentity.Undeclared;

            // Strip leading decoration only — '#' marks a continuous series in the
            // symbol forms observed, so it is recorded, not discarded.
            string symbol = raw.Symbol;
            bool continuousMark = symbol.StartsWith("#", StringComparison.Ordinal);
            string bare = continuousMark ? symbol.Substring(1) : symbol;
            bare = bare.Trim();

            string upper = bare.ToUpperInvariant();

            string size = SizeClass.Unknown;
            string root = "";
            string contract = "";

            if (upper.Length > 0)
            {
                // Futures contract codes end in <month letter><year digit(s)>.
                string stem = upper;
                int trailing = 0;
                while (trailing < stem.Length && IsDigit(stem[stem.Length - 1 - trailing])) trailing++;

                if (trailing > 0 && stem.Length > trailing + 1)
                {
                    char monthCode = stem[stem.Length - trailing - 1];
                    if (monthCode >= 'A' && monthCode <= 'Z')
                    {
                        contract = stem;
                        stem = stem.Substring(0, stem.Length - trailing - 1);
                    }
                }

                // Size prefix. M = micro, E-mini products carry no prefix in these codes,
                // so the default for a recognised root is MINI only where the product is
                // known to be an e-mini; otherwise it stays UNKNOWN rather than guessed.
                if (stem.Length > 1 && stem[0] == 'M')
                {
                    size = SizeClass.Micro;
                    root = stem.Substring(1);
                }
                else
                {
                    root = stem;
                    if (root == "NQ" || root == "ES" || root == "RTY" || root == "YM")
                        size = SizeClass.Mini;
                }
            }

            string series = continuousMark
                ? SeriesKind.Continuous
                : (contract.Length > 0 ? SeriesKind.ActualContract : SeriesKind.Unknown);

            return new CanonicalInstrumentIdentity(
                root, size, contract, series,
                raw.Exchange.ToUpperInvariant(),
                raw.Provider.ToUpperInvariant());
        }

        private static bool IsDigit(char c) { return c >= '0' && c <= '9'; }

        public string ToJson()
        {
            var sb = new StringBuilder(160);
            var j = new JsonLine(sb);
            j.Str("root", Root)
             .Str("size_class", Size)
             .Str("contract", Contract)
             .Str("series", Series)
             .Str("exchange", Exchange)
             .Str("provider", Provider)
             .Str("partition_key", PartitionKey);
            j.End();
            return sb.ToString();
        }
    }
}
