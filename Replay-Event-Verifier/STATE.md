# STATE — Replay Event Verifier

**Conversation:** NQ quant research and ATAS replay compatibility recorder V0.1
**Repository:** `spagheddieree/atas`
**Branch:** `claude/atas-replay-event-verifier-xlgebm`
**Base:** `main` @ `37bb54d`
**Date:** 2026-09-12

---

## 1 · Objective and outcome

**Objective.** Build a minimal ATAS C# indicator that verifies whether ATAS Replay in
ticks + DOM mode provides a complete event stream for downstream quantitative research.
No trading logic.

**Outcome.** The instrument is built, tested and evidenced. **The question it exists to
answer is not yet answered**, because answering it needs a Windows host running ATAS
and none was available. What has been established is the prerequisite: the recorder is
speed-invariant, so it can be ruled out as the cause of any difference the real
experiment finds.

| Deliverable | State |
|---|---|
| Raw event recorder (trades, depth changes, DOM snapshots) | **Done** |
| Source-time snapshot scheduling at configurable interval | **Done** |
| Raw / derived separation (no delta, imbalance or ratios anywhere) | **Done**, enforced by a test |
| Background writer + thread-safe non-blocking queue | **Done** |
| Integrity fault detection (overflow, write failure, and six more) | **Done** |
| Graceful shutdown and flushing | **Done** |
| File outputs + manifest, no database | **Done** |
| Automated tests (6 required areas) | **Done** — 73/73 |
| 10-minute replay, 1x vs accelerated, compared and documented | **Done against a synthetic feed** — see caveat below |
| The ATAS indicator adapter | **Written, type-checked against an ASSUMED API** |
| The real ATAS Replay experiment | **NOT RUN — blocked, no GUI** |

## 2 · Blockers

| # | Blocker | Evidence | Owner |
|---|---|---|---|
| **B-1** | No GUI, no Windows host, no ATAS installation. The real Replay experiment cannot be run. | Container is headless Linux. ATAS Replay is GUI-driven. | Owner — needs a Windows machine |
| **B-2** | ATAS SDK unobtainable, so the adapter's API surface is unverified. | nuget.org 404 on all four ATAS packages; `atas.net` / `docs.atas.net` / `help.atas.net` egress-blocked; no local assemblies. Full table: `docs/ATAS-API-VERIFICATION.md` §1 | Same Windows machine |

Both are environmental and permanent for a Linux container — the ATAS assemblies ship
with the Windows install and are not redistributable. Neither is a defect in this work.

## 3 · Verification evidence

All commands and outputs: `docs/evidence/RESULTS.md`.

```
dotnet build ReplayEventVerifier.sln -c Release   ->  0 Warning(s)  0 Error(s)
dotnet test  ReplayEventVerifier.sln -c Release   ->  Passed: 73, Failed: 0
```

Ten minutes of source time at 300 evt/s, recorded three times:

| Run | Wall | Events | Dropped | `capture_complete` |
|---|---|---|---|---|
| 1x | 600.1 s | 180 599 | 0 | true |
| 60x | 10.1 s | 180 599 | 0 | true |
| unpaced (~750x) | 0.8 s | 180 599 | 0 | true |

`compare` 1x vs each accelerated run: **IDENTICAL**, 0 differing lines, exit 0 —
including all 599 DOM snapshots, because snapshots are scheduled on source time.

Negative control (queue cut to 512): 127 190 events dropped,
`capture_complete: false`, **DIVERGENT**, exit 4. The fault log's first loss (`seq`
514) and the comparer's independently-derived first divergence (canonical index 513)
agree exactly.

**Caveat, stated plainly: this measures the recorder, not ATAS.** The synthetic feed is
not evidence about ATAS Replay. Its only value is removing the recorder as a variable.

## 4 · Exact resume action

On a Windows machine with ATAS installed:

1. Work `docs/ATAS-API-VERIFICATION.md` §4 — build the adapter with
   `-p:UseRealAtas=true` and fix each compile error against the numbered rows.
   Fix the **adapter only**; if a fix seems to need a `Core` change, stop and
   reconsider, because logic is leaking into the adapter.
2. Run `docs/ATAS-API-VERIFICATION.md` §5 sanity checks. The highest-value one is
   replaying the depth-change stream into your own book and diffing it against the
   recorded snapshots — that is the check that can actually detect a missing update.
3. Run `docs/GUI-REPLAY-RUNBOOK.md` end to end and commit the filled-in results block
   from §7 into `docs/evidence/`.

Two outcomes would be findings that end the investigation early rather than problems to
solve: ATAS exposing only **aggregated** trades, or only **whole-book refreshes**
instead of individual depth changes. Either means Replay cannot supply a complete raw
event stream, which is the answer to the original question. Record it and stop.

## 5 · Decisions taken

**Built in `spagheddieree/atas`, not in `Neverflat-OS`.** The task named a
`Neverflat-OS` branch, but a prior lane in that repository had already decided this
exact question and moved its ATAS work out
(`claude/nq-volatility-atas-conversion-ifuil9` @ `38e196d`: *"an ATAS C# indicator does
not belong in the NeverFlat OS tree"*). The same branch name is used here, matching
that lane's own precedent of replaying into `spagheddieree/atas` under its original
branch name. **`Neverflat-OS` was not modified** — its working tree is clean and its
branch is unchanged at `99f4736`.

**Snapshots scheduled on source time, anchored to the interval grid.** Makes snapshot
boundaries a pure function of the event stream, so 1x and accelerated runs snapshot at
identical source instants. A wall-clock timer would have made the comparison impossible
in principle, not merely noisier.

**The book is read from the platform's depth API, never reconstructed.** A book rebuilt
from the change stream agrees with it by construction and can never reveal a missing
change.

**Overflow drops rather than blocks, and says so.** Blocking would stall the feed
thread and distort the timing being measured. Every drop is counted and surfaced as
`capture_complete: false`.

**Out-of-order timestamps are recorded, never clamped.** Repairing them would erase the
defect the tool exists to detect.

Full rationale: `docs/DESIGN.md`.

## 6 · Out of scope, deliberately

No database (brief says not yet — and the point of V0.1 is to establish whether the
data is trustworthy before choosing storage). No derived features of any kind. No
unrelated refactors. No new runtime dependencies — `Core` has zero package references
and JSON is hand-rolled for determinism; xunit is test-only. No PR opened.
