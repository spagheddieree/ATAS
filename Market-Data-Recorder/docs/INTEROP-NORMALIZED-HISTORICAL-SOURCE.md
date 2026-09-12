# Interoperability — the normalized historical store

Findings from a sibling read-only audit, and what they changed here.

> **Status of these findings.** They were produced by a separate audit lane and are
> recorded here as **received**, not re-verified in this repository. No database was
> accessed from this work package, and nothing in this repository reads, imports or
> depends on that store. Where a claim below is load-bearing for a design decision,
> the decision is stated so that it stays correct even if a detail later proves wrong.

---

## 1 · The decisive finding

**The historical store is not a raw trade or L2 recorder.** Verified in the sibling
lane by both physical schema inspection and production-code tracing.

It does **not** persist:

- individual trades / raw trade tape
- raw L2 / depth updates
- periodic DOM or depth snapshots
- raw MBO events
- any event stream permitting arbitrary order-book reconstruction

It **is** principally a normalized bar and footprint store: on the order of 1.17M
bars and 66.95M footprint price levels, the best partition being roughly 476k
one-minute bars over 60 trading days with ~20.88M footprint levels. Footprint rows
carry price level, volume, bid volume, ask volume and trade count.

Two specifics worth keeping:

- Tape events **are received** by that indicator, but the production path feeds them
  into bar reconstruction and never persists the individual tick. The only
  market-data persistence entry point saves observations/bars. There is no trade-save
  path.
- Depth API usage exists only in **capability probing**. That an MBO feed could be
  available at runtime proved *API capability*, not *recording*. Raw depth was never
  persisted.

### Consequence

> **The independent recorder remains mandatory, at unchanged scope.**

That store cannot reduce the recorder's raw acquisition requirements, and this
repository must not be redesigned on an assumption that historical trades or depth
already exist somewhere. If anything the finding makes **depth snapshots more
important**, not less: since no raw depth history exists anywhere, the recorder's
periodic full-book snapshots are the only available reconstruction anchors, gap
recovery points, and cross-check against accumulated deltas.

## 2 · What is genuinely reusable

Aggregated bars and footprint levels are real research data. They are simply
`NORMALIZED_SOURCE`: aggregation already happened upstream, and the individual events
that produced those aggregates are gone.

A trade count is not the trades. Bid volume at a price level is not the prints that
built it. **Do not attempt to reconstruct individual trade events from footprint
aggregates** — the information is not recoverable, and a plausible reconstruction is
worse than an honest absence because it will be believed.

## 3 · What changed in this repository

Every change is a provenance or contract hardening. **No code depends on that store.**

| Finding | Change here |
|---|---|
| Aggregated store could be mistaken for raw tape | `SourceClass` declared and enforced; a `NORMALIZED_SOURCE` cannot emit `trade`, `depth` or `snapshot` (§1 of the contract) |
| No raw depth history exists anywhere | Depth snapshots reaffirmed as required; snapshot contract registered explicitly |
| That store separates event time from receipt time | Recorder keeps `src_ts`/`recv_ts` distinct and adds `source_time_basis` + `source_time_verified` |
| Its `exchange_utc` / `provider_utc` / `replay_utc` were 0% populated — a column's existence is not evidence it was captured | Field-availability register: a field is `UNKNOWN` until evidenced, never present because the schema lists it |
| It preserved replay/live separation via explicit mode metadata even with an empty `replay_utc` | `AcquisitionMode` explicit, operator-declared, defaulting to `UNKNOWN`, inference banned |
| It used `pass_id` with `UNIQUE(event_key, pass_id)` | Recorder keeps its stronger `run_id` + `recorder_seq`; a future import must use a distinct `IMPORT_RUN_ID` and keep the source's pass id separately as lineage |
| Instrument identity varied: `dxFeed\|NQU6@CME`, `UNKNOWN\|#NQU6@CME`, `UNKNOWN\|#MNQU6@CME`, plus `Unknown\|`/`UNKNOWN\|` casing | Raw identity preserved verbatim; canonical derived alongside; NQ/MNQ, actual/continuous and cross-provider merges structurally prevented |
| It holds ~7,390 simulated research trade rows | Import boundary must exclude simulated/research tables explicitly; structurally reinforced because a `NORMALIZED_SOURCE` cannot emit `trade` at all |
| A receipt/event gap of ~68 days was observed under backfill | `recv_ts − src_ts` documented as **not latency**; interpretable only alongside `acquisition_mode` |

## 4 · The table-name lesson

The simulated-trades finding generalises, and is worth stating as a rule:

> **Table and column names are not evidence of semantics.**

A table named for trades held hypothetical research rows. Columns named for exchange
and provider timestamps were entirely unpopulated. Both would have been believed by
an importer that trusted naming.

This is why the field register records **evidence** alongside every classification,
and why `UNKNOWN_NOT_YET_VERIFIED` is a first-class state rather than something to
resolve before shipping.

## 5 · Boundary

- Nothing here reads that database. No connection string, no SQLite dependency, no
  schema knowledge in code.
- The recorder acquires **nothing** from it and never will — a future import adapter
  populates the canonical dataset, not the recorder.
- The adapter is **not implemented** in this work package.
- When it is built: strictly read-only access (`mode=ro`, `PRAGMA query_only=ON`)
  because that store may be actively writing; never modify it; never add indexes to
  it for this project's convenience.

Seam specification: `DATASET-V0.1-CONTRACT.md` §9.
