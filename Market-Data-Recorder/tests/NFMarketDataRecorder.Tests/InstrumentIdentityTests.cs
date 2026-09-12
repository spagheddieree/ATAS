using System.IO;

using NFMarketDataRecorder.Core;
using Xunit;

namespace NFMarketDataRecorder.Tests
{
    /// <summary>
    /// Instrument identity: raw preserved verbatim, canonical derived alongside, and
    /// the three merges that must never happen silently.
    /// </summary>
    public class InstrumentIdentityTests
    {
        // ------------------------------------------------- raw preservation

        [Theory]
        [InlineData("dxFeed|NQU6@CME", "dxFeed", "NQU6", "CME")]
        [InlineData("UNKNOWN|#NQU6@CME", "UNKNOWN", "#NQU6", "CME")]
        [InlineData("UNKNOWN|#MNQU6@CME", "UNKNOWN", "#MNQU6", "CME")]
        [InlineData("Unknown|NQU6@CME", "Unknown", "NQU6", "CME")]
        [InlineData("NQU6", "", "NQU6", "")]
        public void Raw_identity_is_parsed_without_altering_any_component(
            string composite, string provider, string symbol, string exchange)
        {
            var raw = RawInstrumentIdentity.Parse(composite);

            Assert.Equal(provider, raw.Provider);
            Assert.Equal(symbol, raw.Symbol);
            Assert.Equal(exchange, raw.Exchange);
        }

        [Theory]
        [InlineData("dxFeed|NQU6@CME")]
        [InlineData("UNKNOWN|#MNQU6@CME")]
        [InlineData("Unknown|NQU6@CME")]
        [InlineData("NQU6")]
        public void Raw_identity_round_trips_byte_for_byte(string composite)
        {
            Assert.Equal(composite, RawInstrumentIdentity.Parse(composite).ToComposite());
        }

        [Fact]
        public void Provider_casing_variants_are_preserved_not_normalized_away()
        {
            // "Unknown|" and "UNKNOWN|" are different strings in the wild. Folding them
            // at acquisition would destroy the evidence that two partitions came from
            // differently-behaving feeds.
            var lower = RawInstrumentIdentity.Parse("Unknown|NQU6@CME");
            var upper = RawInstrumentIdentity.Parse("UNKNOWN|NQU6@CME");

            Assert.Equal("Unknown", lower.Provider);
            Assert.Equal("UNKNOWN", upper.Provider);
            Assert.NotEqual(lower.Provider, upper.Provider);
        }

        // ------------------------------------------------- canonical derivation

        [Theory]
        [InlineData("dxFeed|NQU6@CME", "NQ", SizeClass.Mini, "NQU6", SeriesKind.ActualContract)]
        [InlineData("dxFeed|MNQU6@CME", "NQ", SizeClass.Micro, "MNQU6", SeriesKind.ActualContract)]
        [InlineData("UNKNOWN|#NQU6@CME", "NQ", SizeClass.Mini, "NQU6", SeriesKind.Continuous)]
        [InlineData("UNKNOWN|#MNQU6@CME", "NQ", SizeClass.Micro, "MNQU6", SeriesKind.Continuous)]
        public void Canonical_identity_is_derived_conservatively(
            string composite, string root, string size, string contract, string series)
        {
            var canon = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse(composite));

            Assert.Equal(root, canon.Root);
            Assert.Equal(size, canon.Size);
            Assert.Equal(contract, canon.Contract);
            Assert.Equal(series, canon.Series);
        }

        [Fact]
        public void An_unrecognisable_symbol_stays_unknown_rather_than_being_guessed()
        {
            var canon = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("feed|WIDGET@XX"));

            Assert.Equal(SizeClass.Unknown, canon.Size);
            Assert.Equal(SeriesKind.Unknown, canon.Series);
            Assert.Equal("", canon.Contract);
        }

        [Fact]
        public void Canonical_derivation_never_replaces_the_raw_identity()
        {
            var raw = RawInstrumentIdentity.Parse("UNKNOWN|#MNQU6@CME");
            var canon = CanonicalInstrumentIdentity.Derive(raw);

            // Canonical normalizes casing and strips decoration; raw keeps all of it.
            Assert.Equal("UNKNOWN", canon.Provider);
            Assert.Equal("#MNQU6", raw.Symbol);
            Assert.Equal("MNQU6", canon.Contract);
        }

        // ------------------------------------------- the merges that must not happen

        [Fact]
        public void NQ_and_MNQ_never_share_a_partition_key()
        {
            // Different products, different order books. Pooling them would silently
            // blend two instruments' liquidity into one series.
            var nq = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|NQU6@CME"));
            var mnq = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|MNQU6@CME"));

            Assert.Equal(nq.Root, mnq.Root);              // same family...
            Assert.NotEqual(nq.Size, mnq.Size);           // ...different size class
            Assert.NotEqual(nq.PartitionKey, mnq.PartitionKey);
        }

        [Fact]
        public void Continuous_and_actual_contracts_never_share_a_partition_key()
        {
            // A stitched continuous series is a construction with roll artefacts, not
            // a traded book.
            var actual = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|NQU6@CME"));
            var cont = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|#NQU6@CME"));

            Assert.Equal(SeriesKind.ActualContract, actual.Series);
            Assert.Equal(SeriesKind.Continuous, cont.Series);
            Assert.NotEqual(actual.PartitionKey, cont.PartitionKey);
        }

        [Fact]
        public void Different_providers_never_share_a_partition_key()
        {
            // Two feeds of "the same" contract are two observations with different
            // gaps and clocks, not one series.
            var a = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|NQU6@CME"));
            var b = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("Rithmic|NQU6@CME"));

            Assert.NotEqual(a.PartitionKey, b.PartitionKey);
        }

        [Fact]
        public void Different_contract_months_never_share_a_partition_key()
        {
            var u6 = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|NQU6@CME"));
            var z6 = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|NQZ6@CME"));

            Assert.NotEqual(u6.PartitionKey, z6.PartitionKey);
        }

        [Fact]
        public void The_same_instrument_from_the_same_feed_does_share_a_partition_key()
        {
            // The key must actually group, or it would be useless.
            var a = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|NQU6@CME"));
            var b = CanonicalInstrumentIdentity.Derive(RawInstrumentIdentity.Parse("dxFeed|NQU6@CME"));

            Assert.Equal(a.PartitionKey, b.PartitionKey);
        }

        // ------------------------------------------------- end to end

        [Fact]
        public void A_capture_records_both_identities()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var opt = new RecorderOptions
            {
                OutputDirectory = dir.Path,
                RawInstrument = "UNKNOWN|#MNQU6@CME",
                SnapshotInterval = System.TimeSpan.FromHours(1),
            };

            var rec = new EventRecorder(opt, new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            // raw, verbatim
            Assert.Equal("UNKNOWN|#MNQU6@CME", m.RawInstrument.ToComposite());
            Assert.Contains("\"raw_symbol\":\"#MNQU6\"", sink.HeaderLine);

            // canonical, derived alongside
            Assert.Equal("NQ", m.CanonicalInstrument.Root);
            Assert.Equal(SizeClass.Micro, m.CanonicalInstrument.Size);
            Assert.Equal(SeriesKind.Continuous, m.CanonicalInstrument.Series);
            Assert.Contains("\"canonical_partition_key\":", sink.HeaderLine);

            string manifest = File.ReadAllText(Path.Combine(dir.Path, "manifest.json"));
            Assert.Contains("\"raw_instrument\":\"UNKNOWN|#MNQU6@CME\"", manifest);
            Assert.Contains("\"canonical_size_class\":\"MICRO\"", manifest);
        }
    }
}
