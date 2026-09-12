# STATE — NF Market Replay Recorder

**Conversation:** NQ quant research and ATAS replay compatibility recorder V0.1
**Repository:** `spagheddieree/atas`
**Branch:** `claude/atas-replay-event-verifier-xlgebm`
**Base:** `main` @ `37bb54d`
**Date:** 2026-09-12
**Work package:** Real ATAS build repair + dual-platform (Classic / ATAS X) architecture
**Prior work package:** Canonical rename to NF Market Replay Recorder
**Prior work package:** Real ATAS API Binding + Probe Repair
**Prior work package:** Real ATAS API Binding Verification + Windows Integration Readiness

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
| Automated tests | **Done** — 164/164 |
| Provenance hardening: RAW vs NORMALIZED source, run id, acquisition mode, instrument identity, integrity state, field register (schema `rev-2`) | **Done** |
| Dataset v0.1 acquisition contract + normalized-import seam spec | **Done (design only)** |
| Crystal Ball Historical Import Adapter | **NOT implemented, by instruction** |
| 10-minute replay, 1x vs accelerated, compared and documented | **Done against a synthetic feed** — see caveat below |
| The ATAS indicator adapter | **Written, type-checked against an ASSUMED API** |
| Real ATAS API binding verification | **DONE — measured from real assemblies** (B-2 CLOSED) |
| `atas-api-probe` — reflection tool to close B-2 in one command | **Done, fixture-tested** |
| Adapter bound to measured signatures (schema `rev-3`) | **Done** |
| Probe WindowsDesktop resolver defect | **Repaired + tested** |
| Real-ATAS build defects (net472 target, CS0104 enum ambiguity) | **Repaired** |
| Dual-platform architecture (ATAS Classic + ATAS X) | **Done — multitarget, audited** |
| `net10.0-windows` target (the default) | **COMPILE-VERIFIED against the real assemblies of both products** — Classic and ATAS X, SDK 10.0.400, `-warnaserror`, EXIT=0 / 0 warnings |
| `net8.0-windows` target | **Retained as opt-in compatibility for older net8 installations**; fails CS1705 against the currently installed net10 assemblies, and is not claimed to work with them |
| Real ATAS build against actual assemblies | **DONE for compilation, both products** — see `docs/evidence/cowork-windows-runtime-measurement.md`. Runtime load still NOT DONE |
| ATAS X runtime support | **NOT VERIFIED** — code-surface audit only |
| The real ATAS Replay experiment | **NOT RUN — blocked, no GUI** (B-1) |

## 2 · Blockers

| # | Blocker | Evidence | Owner |
|---|---|---|---|
| **B-1** | No GUI, no Windows host, no ATAS installation in this lane. The real Replay experiment cannot be run here. | Container is headless Linux; ATAS Replay is GUI-driven. | Owner / Cowork Windows worker |
| **B-2** | ~~ATAS SDK unobtainable, adapter API surface unverified.~~ **CLOSED 2026-09-12.** | Real assemblies measured on the Windows machine; adapter bound to the measured signatures; the real-mode compile now succeeds against both products' assemblies. `docs/ATAS-API-VERIFICATION.md`, `docs/evidence/cowork-windows-runtime-measurement.md` | Closed |

B-1 is environmental and permanent for a Linux container: ATAS Replay is a GUI
workflow on a Windows host. It is not a defect in this work. B-2 is closed — the
assemblies were measured where they live, on the machine that has them.

## 3 · Verification evidence

All commands and outputs: `docs/evidence/RESULTS.md`.

```
dotnet build MarketReplayRecorder.sln -c Release    ->  0 Warning(s)  0 Error(s)
dotnet test  MarketReplayRecorder.sln -c Release     ->  Passed: 164, Failed: 0
```

Ten minutes of source time at 300 evt/s, recorded three times:

| Run | Wall | Events | Dropped | `capture_complete` |
|---|---|---|---|---|
| 1x | 600.1 s | 180 599 | 0 | true |
| 60x | 10.1 s | 180 599 | 0 | true |
| unpaced (~750x) | 0.8 s | 180 599 | 0 | true |

`compare` 1x vs each accelerated run: **IDENTICAL**, 0 differing lines, exit 0 —
including all 599 DOM snapshots, because snapshots are scheduled on source time.

Negative control (queue cut to 512): 128 943 events dropped,
`capture_complete: false`, **DIVERGENT**, exit 4. The fault log's first loss (`seq`
514) and the comparer's independently-derived first divergence (canonical index 513)
agree exactly — and both reproduced across two containers whose later drop patterns
differed, which is the expected shape: the onset of loss is deterministic, which
events survive afterwards is scheduling-dependent.

Re-verified 2026-09-12 on a fresh container from a clean toolchain install: every
event count above reproduced exactly (180 599 / 44 756 / 135 244 / 599).

Re-verified again after the net10 real-mode default repair, including a backward
compatibility check against a capture written by the pre-repair binary built from
`debb57c`: **IDENTICAL**, both manifests `rev-3`. Full run:
`docs/evidence/RESULTS.md` §3.6.

Probe verification: `docs/evidence/api-probe-fixture-test.md` — `atas-api-probe` was
tested against a fixture assembly and correctly reported both the assumed surface and
two deliberately-unassumed aggregated members, which is the behaviour that makes it
worth running.

**Caveat, stated plainly: this measures the recorder, not ATAS.** The synthetic feed is
not evidence about ATAS Replay. Its only value is removing the recorder as a variable.

## 4 · Exact resume action

Everything that can be done without a Windows host is done. What remains is
**runtime** validation of both products, on the Windows machine.

### Already closed, do not redo

| | Status |
|---|---|
| API probe against real assemblies | run; `docs/evidence/atas-api-report.md` |
| Adapter bound to measured signatures | done (`rev-3`) |
| Enum ambiguity (CS0104) | fixed, guarded by 19 tests |
| Target framework | `net10.0-windows` default, compile-verified against **both** products |
| Real-mode compile | EXIT=0 / 0 warnings vs Classic and vs ATAS X, SDK 10.0.400, `-warnaserror` |

### The remaining steps, in order

1. **Verify the runtime has not moved again.** Re-read the `tfm` from
   `OFT.Platform.runtimeconfig.json` and `OFT.PlatformX.runtimeconfig.json` and the
   `TargetFrameworkAttribute` on the assemblies. Classic changed from net8 to net10
   mid-validation once already; a stale reading is how this step fails.
2. **Build for each product** against its own install directory — the two products
   ship different assemblies under identical names:
   `-p:UseRealAtas=true -p:AtasInstallDir="<that product's directory>"`. No TFM
   override unless step 1 says otherwise.
3. **Install** the two DLLs into the product-specific folder, `%APPDATA%\ATAS\Indicators`
   or `%APPDATA%\ATAS X\Indicators`, or via the platform's "Add custom indicator"
   workflow. Back up anything already there. Do not cross-install.
4. **Load on each product independently** and record what happened, including whether
   a restart was needed. A pass on one says nothing about the other.
5. **Set Acquisition mode = REPLAY.** It is never inferred; an unset run records
   `UNKNOWN`.
6. **Run `docs/GUI-REPLAY-RUNBOOK.md` end to end** on one fixed 10-minute NQ interval:
   1x, 60x, and unpaced where genuinely supported. Answer §9 A–E per product and
   commit the filled-in results block into `docs/evidence/`.
7. **Sanity checks** from `docs/ATAS-API-VERIFICATION.md` §5. The highest-value one is
   replaying the depth-change stream into your own book and diffing it against the
   recorded snapshots — the only check that can actually detect a missing update.

Two outcomes would be findings that end the investigation early rather than problems to
solve: ATAS exposing only **aggregated** trades, or only **whole-book refreshes**
instead of individual depth changes. Either means Replay cannot supply a complete raw
event stream, which is the answer to the original question. Record it and stop.

## 5 · Decisions taken

**Canonical name: NF Market Replay Recorder (Owner decision, 2026-09-12).** Second
rename of this component. Directory `Market-Data-Recorder/` → `Market-Replay-Recorder/`,
solution `MarketDataRecorder.sln` → `MarketReplayRecorder.sln`, projects and namespaces
`NFMarketDataRecorder.*` → `NFMarketReplayRecorder.*`, indicator class
`MarketDataRecorderIndicator` → `MarketReplayRecorderIndicator`, ATAS display name
and capture directory to match, harness CLI `nf-recorder` → `nf-replay-recorder`. All
moves used `git mv`.

**The serialized contract was deliberately NOT touched.** No field in the capture
header, the manifest or any event carries the product name — verified before renaming
anything — so `schema_version` stays `rev-3`, no semantic field was renamed, and every
capture written before this rename remains readable by the renamed code. A display
name changing is not a reason to break stored data.

Reference forms: **NF Market Replay Recorder** (full), *Market Replay Recorder*
(normal), *the Recorder* (short).

**Earlier, first rename: Replay Event Verifier → NF Market Data Recorder
(2026-09-12).** Historical record — these are the names as they were *then*, not
current paths. Directory `Replay-Event-Verifier/` → `Market-Data-Recorder/`,
projects and namespaces `ReplayEventVerifier.*` → `NFMarketDataRecorder.*`, indicator
class `ReplayVerifierIndicator` → `MarketDataRecorderIndicator`, ATAS display name
"Replay Event Verifier" → "NF Market Data Recorder", harness CLI `replay-verifier` →
`nf-recorder`, capture directory `~/Documents/ReplayEventVerifier` →
`~/Documents/NFMarketDataRecorder`. All superseded by the current rename above.

That first rename corrected the framing — verifying ATAS Replay is the recorder's
first assignment, not its identity — and the current one sharpens it further: Replay
is the acquisition path the component is built around, so the name now says so.

Assembly names are deliberately distinctive (`NFMarketReplayRecorder.*` rather than a
bare `MarketReplayRecorder.*`), because ATAS loads every DLL in one indicators folder
and a generic name is a collision risk there.

**The GitHub repository rename is NOT done** — it requires Owner action in repository
settings and cannot be performed from here. See §7.

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
unrelated refactors. No PR opened, nothing merged. Dataset v0.1 and Feature Engine
v0.1 not begun.

`Core` still has **zero** package references and JSON is hand-rolled for determinism.
Two dependencies exist and both are outside the shipped indicator: xunit (tests) and
`System.Reflection.MetadataLoadContext` (the API probe — the only supported way to
reflect over .NET Framework assemblies from a .NET 8 process without loading or
executing them). Neither can reach the assembly that loads into ATAS.

## 7 · Open Owner action — GitHub repository rename

The in-repo rename is complete and pushed. Renaming the **GitHub repository** itself
(`spagheddieree/atas` → `NF-Market-Replay-Recorder`) must be done by the Owner:
repository **Settings → General → Repository name**.

GitHub keeps redirects for git operations after a rename, so existing clones and the
pushed branch keep working; only the canonical URL changes.

One thing to weigh first: this repository currently holds **two** indicator projects —
`Market-Replay-Recorder/` on this branch and `NQ-Volatility-Indicator/` on
`claude/nq-volatility-atas-conversion-ifuil9`. Naming the whole repository after one
of them is a reasonable call if the recorder is the repository's centre of gravity and
the NQ volatility work is incidental; if both are meant to be first-class, a neutral
name such as `NF-ATAS-Tools` keeps room for both. Owner's call — the work here is
unaffected either way.
