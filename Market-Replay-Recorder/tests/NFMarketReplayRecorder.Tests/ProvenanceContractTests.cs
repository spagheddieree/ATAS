using System;
using System.Collections.Generic;
using System.IO;

using NFMarketReplayRecorder.Core;
using Xunit;

namespace NFMarketReplayRecorder.Tests
{
    /// <summary>
    /// Contracts that exist because of the normalized-historical-source interoperability audit:
    /// raw versus normalized provenance, acquisition mode, run identity and
    /// integrity state.
    /// </summary>
    public class ProvenanceContractTests
    {
        // ------------------------------------------------- source class

        [Fact]
        public void The_recorder_always_declares_itself_a_raw_source()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.Equal(SourceClass.RawSource, m.SourceClassValue);
            Assert.Contains("\"source_class\":\"RAW_SOURCE\"", sink.HeaderLine);
        }

        [Fact]
        public void A_normalized_source_may_not_emit_event_level_observations()
        {
            // The rule that stops an aggregated historical store from being ingested
            // as if it were a trade tape. Bars and per-price footprint summaries are
            // not the trades that produced them, and no transformation recovers them.
            Assert.False(Validation.MayEmit(SourceClass.NormalizedSource, EventKind.Trade));
            Assert.False(Validation.MayEmit(SourceClass.NormalizedSource, EventKind.Depth));
            Assert.False(Validation.MayEmit(SourceClass.NormalizedSource, EventKind.Snapshot));

            Assert.Throws<InvalidOperationException>(
                () => Validation.RequireMayEmit(SourceClass.NormalizedSource, EventKind.Trade));
        }

        [Fact]
        public void A_raw_source_may_emit_every_event_kind()
        {
            Assert.True(Validation.MayEmit(SourceClass.RawSource, EventKind.Trade));
            Assert.True(Validation.MayEmit(SourceClass.RawSource, EventKind.Depth));
            Assert.True(Validation.MayEmit(SourceClass.RawSource, EventKind.Snapshot));
        }

        [Fact]
        public void An_unrecognised_source_class_may_emit_nothing()
        {
            Assert.False(Validation.MayEmit("SOMETHING_ELSE", EventKind.Trade));
            Assert.False(SourceClass.IsValid("SOMETHING_ELSE"));
        }

        // ------------------------------------------------- acquisition mode

        [Fact]
        public void Acquisition_mode_defaults_to_unknown_never_to_live()
        {
            // Defaulting to LIVE would silently mislabel every replay capture.
            var opt = new RecorderOptions { OutputDirectory = "x" };
            Assert.Equal(AcquisitionMode.Unknown, opt.AcquisitionMode);
            Assert.False(AcquisitionMode.IsDeclared(opt.AcquisitionMode));
        }

        [Fact]
        public void An_undeclared_acquisition_mode_is_reported_as_undeclared()
        {
            using var dir = new TempDir();

            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.Equal(AcquisitionMode.Unknown, m.AcquisitionModeValue);
            Assert.Contains("\"acquisition_mode_declared\":false",
                            File.ReadAllText(Path.Combine(dir.Path, "manifest.json")));
        }

        [Fact]
        public void A_declared_replay_mode_is_carried_into_header_and_manifest()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var opt = Options(dir);
            opt.AcquisitionMode = AcquisitionMode.Replay;

            var rec = new EventRecorder(opt, new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.Equal(AcquisitionMode.Replay, m.AcquisitionModeValue);
            Assert.Contains("\"acquisition_mode\":\"REPLAY\"", sink.HeaderLine);
        }

        [Fact]
        public void An_invalid_acquisition_mode_is_rejected_rather_than_coerced()
        {
            var opt = new RecorderOptions { OutputDirectory = "x", AcquisitionMode = "BACKFILL" };
            Assert.Throws<ArgumentException>(() => opt.Validate());
        }

        [Fact]
        public void Replay_mode_is_not_inferrable_from_the_data()
        {
            // A capture whose source timestamps are 68 days behind the wall clock --
            // the shape of a historical replay -- still records UNKNOWN when the
            // operator did not declare a mode. Nothing in the event stream is allowed
            // to imply the mode.
            using var dir = new TempDir();

            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(DateTime.UtcNow.AddDays(-68), 1m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.Equal(AcquisitionMode.Unknown, m.AcquisitionModeValue);
        }

        // ------------------------------------------------- run identity

        [Fact]
        public void Every_run_gets_a_run_id_even_when_none_was_supplied()
        {
            using var dir = new TempDir();
            var rec = new EventRecorder(Options(dir), new StaticDom());
            var m = rec.Complete();

            Assert.False(string.IsNullOrEmpty(m.RunId));
        }

        [Fact]
        public void Two_runs_get_different_run_ids()
        {
            using var d1 = new TempDir();
            using var d2 = new TempDir();

            var a = new EventRecorder(Options(d1), new StaticDom());
            var b = new EventRecorder(Options(d2), new StaticDom());
            string idA = a.RunId, idB = b.RunId;
            a.Complete(); b.Complete();

            Assert.NotEqual(idA, idB);
        }

        [Fact]
        public void Run_label_is_not_run_identity()
        {
            // Two replay passes over the same interval share a label but must remain
            // distinguishable, or acquisition evidence collapses into one observation.
            using var d1 = new TempDir();
            using var d2 = new TempDir();

            var o1 = Options(d1); o1.RunLabel = "pass";
            var o2 = Options(d2); o2.RunLabel = "pass";

            var a = new EventRecorder(o1, new StaticDom());
            var b = new EventRecorder(o2, new StaticDom());
            var ma = a.Complete(); var mb = b.Complete();

            Assert.Equal(ma.RunLabel, mb.RunLabel);
            Assert.NotEqual(ma.RunId, mb.RunId);
        }

        [Fact]
        public void An_explicit_run_id_is_honoured()
        {
            using var dir = new TempDir();
            var opt = Options(dir);
            opt.RunId = "run-abc-123";

            var rec = new EventRecorder(opt, new StaticDom());
            Assert.Equal("run-abc-123", rec.RunId);
            rec.Complete();
        }

        // ------------------------------------------------- header

        [Fact]
        public void The_header_is_the_first_line_of_the_capture()
        {
            using var dir = new TempDir();
            var sink = new MemorySink();

            var rec = new EventRecorder(Options(dir), new StaticDom(), sink);
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.Complete();

            Assert.Contains("\"kind\":\"header\"", sink.Lines[0]);
            Assert.Single(sink.MarketLines);
        }

        [Fact]
        public void The_header_is_excluded_from_canonical_comparison()
        {
            // Two captures of identical market events have different run_ids and
            // start times. If the header took part in comparison, every comparison
            // would fail and the tool would be useless.
            using var d1 = new TempDir();
            using var d2 = new TempDir();

            foreach (var d in new[] { d1, d2 })
            {
                var rec = new EventRecorder(Options(d), new StaticDom());
                rec.OnTrade(Sample.Epoch, 20000.25m, 3m, Aggressor.Buy);
                rec.OnTrade(Sample.Epoch.AddMilliseconds(1), 20000.5m, 1m, Aggressor.Sell);
                rec.Complete();
            }

            var a = StreamComparer.LoadCanonical(Path.Combine(d1.Path, "events.jsonl"));
            var b = StreamComparer.LoadCanonical(Path.Combine(d2.Path, "events.jsonl"));

            Assert.Equal(2, a.Count);   // header not loaded
            Assert.True(StreamComparer.Compare(a, b).StrictMatch);
        }

        [Fact]
        public void A_header_requires_run_identity()
        {
            var h = new CaptureHeader { RunId = "" };
            Assert.Throws<InvalidOperationException>(() => h.Validate());
        }

        // ------------------------------------------------- integrity state

        [Fact]
        public void A_clean_run_reports_clean()
        {
            using var dir = new TempDir();
            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            var m = rec.Complete();

            Assert.Equal(IntegrityState.Clean, m.IntegrityStateValue);
            Assert.True(m.CaptureComplete);
        }

        [Fact]
        public void Integrity_state_only_ever_worsens()
        {
            Assert.Equal(IntegrityState.Degraded,
                         IntegrityState.Worsen(IntegrityState.Clean, IntegrityState.Degraded));
            Assert.Equal(IntegrityState.Corrupt,
                         IntegrityState.Worsen(IntegrityState.Degraded, IntegrityState.Corrupt));

            // The rule that matters: a defect can never be undone.
            Assert.Equal(IntegrityState.Corrupt,
                         IntegrityState.Worsen(IntegrityState.Corrupt, IntegrityState.Clean));
            Assert.Equal(IntegrityState.Degraded,
                         IntegrityState.Worsen(IntegrityState.Degraded, IntegrityState.Clean));
        }

        [Theory]
        [InlineData(FaultCode.QueueOverflow, IntegrityState.Corrupt)]
        [InlineData(FaultCode.WriteFailure, IntegrityState.Corrupt)]
        [InlineData(FaultCode.DrainTimeout, IntegrityState.Corrupt)]
        [InlineData(FaultCode.SourceTimeRegression, IntegrityState.Degraded)]
        [InlineData(FaultCode.SnapshotSourceUnavailable, IntegrityState.Degraded)]
        [InlineData(FaultCode.MissingSourceTime, IntegrityState.Degraded)]
        public void Each_fault_maps_to_a_deterministic_integrity_state(string fault, string expected)
        {
            Assert.Equal(expected, IntegrityState.ForFault(fault));
        }

        [Fact]
        public void A_lossless_anomaly_degrades_but_does_not_corrupt()
        {
            // An out-of-order source timestamp makes the capture imperfect, not holed.
            using var dir = new TempDir();

            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(Sample.Epoch.AddSeconds(5), 1m, 1m, Aggressor.Buy);
            rec.OnTrade(Sample.Epoch.AddSeconds(2), 2m, 1m, Aggressor.Sell);
            var m = rec.Complete();

            Assert.Equal(IntegrityState.Degraded, m.IntegrityStateValue);
            Assert.True(m.CaptureComplete);   // nothing was lost
        }

        [Fact]
        public void Lost_events_corrupt_the_run()
        {
            using var dir = new TempDir();
            var blocking = new BlockingSink();

            var opt = Options(dir);
            opt.QueueCapacity = 8;
            opt.DrainTimeout = TimeSpan.FromMilliseconds(200);

            var rec = new EventRecorder(opt, new StaticDom(), blocking);
            for (int i = 0; i < 2000; i++) rec.OnTrade(Sample.Epoch.AddMilliseconds(i), i, 1m, Aggressor.Buy);
            blocking.Release();
            var m = rec.Complete();

            Assert.Equal(IntegrityState.Corrupt, m.IntegrityStateValue);
            Assert.False(m.CaptureComplete);
            blocking.Dispose();
        }

        [Fact]
        public void A_run_cannot_stay_clean_after_a_defect_even_if_it_ends_quietly()
        {
            using var dir = new TempDir();

            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(Sample.Epoch.AddSeconds(5), 1m, 1m, Aggressor.Buy);
            rec.OnTrade(Sample.Epoch.AddSeconds(2), 2m, 1m, Aggressor.Sell);  // regression
            for (int i = 0; i < 100; i++)                                      // then all normal
                rec.OnTrade(Sample.Epoch.AddSeconds(10 + i), i, 1m, Aggressor.Buy);

            Assert.Equal(IntegrityState.Degraded, rec.Integrity);
            Assert.Equal(IntegrityState.Degraded, rec.Complete().IntegrityStateValue);
        }

        // ------------------------------------------------- field register

        [Fact]
        public void Measured_present_fields_are_promoted()
        {
            // Promotion requires measurement. These were read off the real
            // MarketDataArg, so they are AVAILABLE rather than UNKNOWN.
            var trade = Index(RecorderFieldRegister.TradeEvent());

            Assert.Equal(Availability.AvailableDirectly, trade["price"].State);
            Assert.Equal(Availability.AvailableDirectly, trade["volume"].State);
            Assert.Equal(Availability.AvailableDirectly, trade["aggressor_side"].State);
            Assert.Equal(Availability.AvailableDirectly, trade["origin_price"].State);
            Assert.Equal(Availability.AvailableDirectly, trade["exchange_order_id"].State);
        }

        [Fact]
        public void Source_sequence_is_measured_absent_and_never_taken_from_an_order_id()
        {
            // The single most dangerous available substitution. ExchangeOrderId is an
            // ORDER identifier; using it as a sequence would fabricate feed ordering
            // that was never observed.
            var trade = Index(RecorderFieldRegister.TradeEvent());
            var depth = Index(RecorderFieldRegister.DepthUpdate());

            Assert.Equal(Availability.Unavailable, trade["source_sequence"].State);
            Assert.Equal(Availability.Unavailable, depth["source_sequence"].State);
            Assert.False(Availability.IsPresent(trade["source_sequence"].State));

            // ...while the order id itself is legitimately available, and the evidence
            // says explicitly that the two must not be conflated.
            Assert.Equal(Availability.AvailableDirectly, trade["exchange_order_id"].State);
            Assert.Contains("NOT a sequence", trade["exchange_order_id"].Evidence);
        }

        [Fact]
        public void A_measured_property_does_not_make_its_semantics_verified()
        {
            // MarketDataArg.Time provably exists. What it MEANS at runtime -- exchange
            // clock, replay clock or arrival clock -- metadata cannot say, and the
            // whole dataset's timing validity rests on it.
            var trade = Index(RecorderFieldRegister.TradeEvent());

            Assert.Equal(Availability.Unknown, trade["source_timestamp"].State);
            Assert.Contains("SEMANTICS", trade["source_timestamp"].Evidence);

            // Likewise snapshot ladder ordering: rows are measured, order is not.
            var snap = Index(RecorderFieldRegister.DepthSnapshot());
            Assert.Equal(Availability.AvailableDirectly, snap["side"].State);
            Assert.Equal(Availability.Unknown, snap["level"].State);
            Assert.Contains("ORDERING", snap["level"].Evidence);
        }

        [Fact]
        public void Absent_and_unknown_are_both_treated_as_not_present()
        {
            Assert.False(Availability.IsPresent(Availability.Unknown));
            Assert.False(Availability.IsPresent(Availability.Unavailable));
            Assert.True(Availability.IsPresent(Availability.AvailableDirectly));
        }

        [Fact]
        public void Recorder_owned_fields_are_available_directly()
        {
            var trade = Index(RecorderFieldRegister.TradeEvent());

            Assert.Equal(Availability.AvailableDirectly, trade["run_id"].State);
            Assert.Equal(Availability.AvailableDirectly, trade["recorder_seq"].State);
            Assert.Equal(Availability.AvailableDirectly, trade["acquisition_mode"].State);
            Assert.Equal(Availability.DerivableWithoutLoss, trade["canonical_instrument"].State);
        }

        [Fact]
        public void Every_classification_carries_evidence()
        {
            foreach (var kv in RecorderFieldRegister.All())
                foreach (var f in kv.Value)
                    Assert.False(string.IsNullOrWhiteSpace(f.Evidence),
                                 kv.Key + "." + f.Field + " has no evidence");
        }

        [Fact]
        public void All_three_contracts_are_registered()
        {
            var all = RecorderFieldRegister.All();
            Assert.True(all.ContainsKey("TradeEvent"));
            Assert.True(all.ContainsKey("DepthUpdate"));
            Assert.True(all.ContainsKey("DepthSnapshot"));
            Assert.True(RecorderFieldRegister.HasUnverifiedFields());
        }

        [Fact]
        public void The_register_is_written_beside_every_capture()
        {
            using var dir = new TempDir();
            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.OnTrade(Sample.Epoch, 1m, 1m, Aggressor.Buy);
            rec.Complete();

            string path = Path.Combine(dir.Path, "field-register.jsonl");
            Assert.True(File.Exists(path));

            string text = File.ReadAllText(path);
            Assert.Contains("TradeEvent", text);
            Assert.Contains("UNKNOWN_NOT_YET_VERIFIED", text);
        }

        [Fact]
        public void The_manifest_flags_that_contract_fields_remain_unverified()
        {
            using var dir = new TempDir();
            var rec = new EventRecorder(Options(dir), new StaticDom());
            rec.Complete();

            Assert.Contains("\"has_unverified_contract_fields\":true",
                            File.ReadAllText(Path.Combine(dir.Path, "manifest.json")));
        }

        [Fact]
        public void Source_time_is_not_claimed_verified_while_the_api_is_unverified()
        {
            using var dir = new TempDir();
            var rec = new EventRecorder(Options(dir), new StaticDom());
            var m = rec.Complete();

            Assert.False(m.SourceTimeVerified);
            Assert.Equal(SourceTimeBases.PlatformEventTime, m.SourceTimeBasis);
        }

        private static Dictionary<string, FieldClassification> Index(List<FieldClassification> fields)
        {
            var d = new Dictionary<string, FieldClassification>(StringComparer.Ordinal);
            foreach (var f in fields) d[f.Field] = f;
            return d;
        }

        private static RecorderOptions Options(TempDir dir) => new RecorderOptions
        {
            OutputDirectory = dir.Path,
            RawInstrument = "TEST|NQU6@CME",
            SnapshotInterval = TimeSpan.FromHours(1),
            DrainTimeout = TimeSpan.FromSeconds(15),
        };
    }
}
