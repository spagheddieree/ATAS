# Dataset v0.1 — acquisition contract

What the recorder emits, and the provenance rules any other source must satisfy to
enter the same canonical dataset.

> **Scope.** Dataset v0.1 itself — storage, normalization, the canonical research
> tables — is **not implemented** and is deliberately out of scope for this
> repository so far. This document specifies the **acquisition-side contract**: the
> shape and provenance of what the recorder produces, hardened so that a future
> Dataset v0.1 can ingest it, and so that a future normalized historical import
> cannot be mistaken for it.
>
> Schema: **`rev-2`**. Recorder: **`0.1.0`**.

---

## 1 · The one distinction everything else rests on

```
RAW_SOURCE                                NORMALIZED_SOURCE
event-level observations, as delivered    aggregates produced upstream
                                          (bars, footprint price levels)
        │                                          │
        │  independent ATAS recorder               │  read-only historical import
        ▼                                          ▼
        └──────────────► canonical dataset ◄───────┘
                      provenance preserved on both
```

**Both may populate the same canonical dataset. Neither may be relabelled as the
other.**

The hazard is concrete. A normalized store holding per-bar, per-price volume with
bid/ask splits and trade counts *looks* like it contains the trades that produced
those aggregates. It does not. A trade count is not the trades. Bid volume at a price
level is not the prints that made it. No transformation recovers them, and a dataset
that lets such a partition claim `RAW_SOURCE` will silently serve reconstructed
aggregates to research that asked for observations.

So source class is **declared, mandatory, and enforced**:

| Rule | Where |
|---|---|
| Recorder always declares `RAW_SOURCE` | `CaptureHeader.SourceClassValue` |
| A `NORMALIZED_SOURCE` may not emit `trade`, `depth` or `snapshot` | `Validation.MayEmit` |
| Violations throw rather than degrade | `Validation.RequireMayEmit` |

`ProvenanceContractTests.A_normalized_source_may_not_emit_event_level_observations`
holds this line.

## 2 · File layout

One directory per run:

```
events.jsonl        line 1: capture header (provenance)
                    line 2+: raw market events
faults.jsonl        integrity faults, aggregated by code
field-register.jsonl what this partition's fields actually contain
manifest.json       run summary; written LAST, so its presence means clean shutdown
```

**Why a header rather than per-event provenance.** Run identity, acquisition mode,
source class and instrument identity are constant for a whole run and would multiply
file size several times over if repeated on every line of a multi-million-row
capture. The file carries them once; the logical row is **`header ⊗ event`**. The
file stays self-describing — a capture separated from its manifest still knows what
it is — and ingestion performs the join.

The header is **excluded from stream comparison**: it carries `run_id` and wall-clock
values that necessarily differ between two captures of the same market events.

## 3 · Timestamps

Three distinct concepts. **Never collapsed into one.**

| Concept | Field | Status |
|---|---|---|
| Strongest available source/exchange timestamp | `src_ts` | present; **meaning unverified** |
| Normalized market/event timestamp | — | not yet distinguishable from `src_ts` |
| Recorder receive timestamp | `recv_ts` | present, wall clock |

The header declares what `src_ts` actually is, rather than leaving a consumer to
assume:

```json
"source_time_basis": "PLATFORM_EVENT_TIME",
"source_time_verified": false
```

`source_time_verified: false` is load-bearing. Until the ATAS API probe confirms that
the platform's event timestamp is the **feed** clock and not an **arrival** clock,
this capture cannot support latency analysis. During a historical replay the receive
clock is present-day while the event clock is historical — differences of weeks are
normal and meaningless — so:

> **`recv_ts − src_ts` is not latency.** It is only interpretable alongside
> `acquisition_mode`, and under `REPLAY` it is meaningless by construction.

A normalized event timestamp is **not** invented as a third field while it would be a
copy of the second. When the source clock is verified and the two genuinely differ,
the field is added and the register promotes it.

## 4 · Acquisition mode

`LIVE` · `REPLAY` · `UNKNOWN` — declared by the operator, **never inferred**.

Banned inference sources: timestamp differences, chart symbol, account name, provider
identity, historical date. Under replay every one of them is ambiguous or actively
misleading.

The default is `UNKNOWN`, never `LIVE`. An undeclared run reports
`"acquisition_mode_declared": false` rather than guessing, because a silently
mislabelled replay is worse than an honestly unlabelled one.

## 5 · Run and sequence identity

| Field | Meaning |
|---|---|
| `run_id` | Canonical identity of one capture run. Machine-generated, unique. **What downstream joins on.** |
| `run_label` | Free-text operator label. **Never identity** — two passes can share one. |
| `recorder_seq` | Monotone capture order **within a run**. A total order of *observation*. |

`recorder_seq` is named explicitly to prevent the failure it would otherwise invite:
being mistaken for an exchange sequence number. It orders what the recorder saw, not
what the exchange emitted. **No source sequence is claimed** — see §7.

### Statistical independence

Two replay passes over the same historical interval produce two runs with different
`run_id`s and the same market events. Both are kept.

> **Deduplicate statistical independence, not acquisition passes.**

The observations are genuine, separately-acquired evidence and carry integrity value
— disagreement between passes is exactly how a replay defect surfaces. They are *not*
two independent market events. Collapsing them at acquisition destroys the evidence;
treating them as independent corrupts any statistic computed over them.

The contract therefore preserves enough identity for the research layer to decide:
`run_id` distinguishes the passes, `src_ts` plus the canonical partition key
identifies the underlying market event.

## 6 · Instrument identity

Raw and canonical are stored **side by side**. Canonical is derived; raw is never
overwritten.

```json
"raw_instrument": "UNKNOWN|#MNQU6@CME",
"raw_provider": "UNKNOWN", "raw_symbol": "#MNQU6", "raw_exchange": "CME",
"canonical_root": "NQ", "canonical_size_class": "MICRO",
"canonical_contract": "MNQU6", "canonical_series": "CONTINUOUS",
"canonical_partition_key": "UNKNOWN/CME/NQ/MICRO/CONTINUOUS/MNQU6"
```

Raw identity is preserved verbatim — casing included. `Unknown|` and `UNKNOWN|` stay
distinct, because folding them at acquisition destroys evidence that two partitions
came from differently-behaving feeds.

Three merges must never happen silently. Each is prevented by a distinct component of
`partition_key`, not by convention:

| Must not merge | Prevented by | Test |
|---|---|---|
| NQ with MNQ | `size_class` | `NQ_and_MNQ_never_share_a_partition_key` |
| Actual with continuous | `series` | `Continuous_and_actual_contracts_never_share_a_partition_key` |
| Two providers | `provider` | `Different_providers_never_share_a_partition_key` |
| Two contract months | `contract` | `Different_contract_months_never_share_a_partition_key` |

Derivation is conservative: anything not establishable from the raw identity stays
`UNKNOWN` rather than being guessed.

## 7 · Field availability — heterogeneous completeness

`field-register.jsonl` accompanies every capture and states, per contract field, what
this partition actually contains:

| State | Meaning |
|---|---|
| `AVAILABLE_DIRECTLY` | Source supplies it. Verified. |
| `DERIVABLE_WITHOUT_INFORMATION_LOSS` | Computable, nothing lost. Verified. |
| `DERIVABLE_WITH_INFORMATION_LOSS` | Computable approximately. Verified. |
| `UNAVAILABLE` | Verified absent. |
| `UNKNOWN_NOT_YET_VERIFIED` | Not established either way. **Never treated as present.** |

This is what makes a dataset of unevenly rich partitions workable. A consumer asks
the register whether a partition can answer a question, instead of discovering a
silent null mid-analysis.

**A field is promoted only on evidence.** Because the ATAS API surface is still
unverified, much of the trade and depth contract is currently `UNKNOWN` —
`source_sequence`, `exchange_trade_id`, `level`, `update_type`, and critically
`source_timestamp` itself. The manifest advertises this:

```json
"has_unverified_contract_fields": true
```

Wanting a field for the dataset contract is not evidence. `UNKNOWN` is a publishable
state, not a gap to tidy away before shipping.

A future `NORMALIZED_SOURCE` partition will legitimately register bars, footprint
levels, bid/ask volume, trade count, event time, receipt time, mode and lineage as
available, while registering individual trades, raw aggressor, source trade sequence,
raw L2 updates, snapshots and MBO as `UNAVAILABLE`. **That partition is still
useful** — it simply cannot answer questions its register says it cannot answer.

## 8 · Integrity

`CLEAN` → `DEGRADED` → `CORRUPT`. Monotone: a run never returns to a better state.

| State | Meaning |
|---|---|
| `CLEAN` | No integrity defect observed. |
| `DEGRADED` | Imperfect but complete — a source-time regression, an unavailable snapshot. Nothing lost. |
| `CORRUPT` | Events were lost or unwritten. The stream has holes. |

Mapping is deterministic (`IntegrityState.ForFault`), split on whether data was
*lost*: overflow, write failure and drain timeout are `CORRUPT`; source-time
regression, missing source time, snapshot unavailability and post-shutdown arrivals
are `DEGRADED`.

A run **cannot silently stay clean after a defect** — every fault occurrence worsens
the state through an observer on the fault log, and the final state is recomputed at
shutdown.

### Not yet represented

Reconnect boundaries, feed resets and detected source-sequence gaps are **planned,
not implemented**. All three require API capabilities that are still unverified — a
gap cannot be detected without a source sequence, and a reconnect cannot be observed
without a connection-state callback. They are listed in the register as `UNKNOWN`
rather than silently omitted, and are the first additions once the probe report lands.

## 9 · Future import seam

Dataset v0.1 must be able to accept a normalized historical import **without the
recorder knowing anything about it**.

```
historical SQLite ──► read-only adapter ──► NORMALIZED_SOURCE partition ──┐
                                                                          ├──► canonical dataset
ATAS ──► recorder ──────────────────────► RAW_SOURCE partition ───────────┘
```

The seam is the **source-class contract in §1 plus the field register in §7** — not a
code dependency. Nothing in `NFMarketDataRecorder.Core` imports, references, or knows
about any historical store, and the recorder acquires nothing from one.

Requirements on any future adapter:

1. Declare `NORMALIZED_SOURCE`. It may not emit `trade`, `depth` or `snapshot`.
2. Use its own `IMPORT_RUN_ID` for canonical run identity, and preserve the source's
   own pass/lineage identifier **separately** as source lineage. An import id must
   never be presented as a recorder run.
3. Register `UNAVAILABLE` honestly for everything the source does not hold.
4. Open the source strictly read-only (`mode=ro` plus `PRAGMA query_only=ON`), since
   the source may be actively writing. Never modify it; never add indexes to it for
   this project's convenience.
5. **Exclude simulated/research trade tables from market-data ingestion.** A table
   named for trades may hold hypothetical research rows, not market tape. Table names
   are not evidence of semantics; the adapter must exclude them explicitly rather
   than infer from naming.

Rule 1 makes rule 5 structurally enforced rather than merely documented: even if a
simulated-trade table were read by mistake, a `NORMALIZED_SOURCE` cannot emit
`trade`, so those rows cannot enter the dataset as market tape.

**The adapter itself is deliberately not implemented.**
