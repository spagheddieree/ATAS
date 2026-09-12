# Verification results

Captured **2026-09-12** on Linux 6.18.44 x86_64, .NET SDK **8.0.131**.
Commit: see `git log` for this directory.

---

## What was and was not measured

| | |
|---|---|
| **Measured** | The recorder's own speed-invariance, against a deterministic synthetic feed, end to end through the real `EventRecorder`. |
| **NOT measured** | Whether **ATAS Replay** delivers a complete event stream. No GUI, no Windows host, no ATAS installation was available. See `../GUI-REPLAY-RUNBOOK.md`. |

The synthetic result is not a claim about ATAS. Its value is narrower and it is a
prerequisite for the real experiment: it establishes that **the recorder is not the
variable**. Without it, a difference observed in a real ATAS run could not be
attributed to ATAS rather than to the instrument measuring it.

---

## 1 · Build and tests

```
$ dotnet build ReplayEventVerifier.sln -c Release
Build succeeded.  0 Warning(s)  0 Error(s)

$ dotnet test ReplayEventVerifier.sln -c Release
Passed!  - Failed: 0, Passed: 73, Skipped: 0, Total: 73
```

All four projects build with `TreatWarningsAsErrors` and produce zero warnings.
Files: `build.txt`, `test-run.txt`, `test-inventory.txt` (all 73 test names).

Coverage against the required areas:

| Required area | File | Tests |
|---|---|---|
| Serialization | `SerializationTests.cs` | 18 |
| Timestamp handling | `TimestampTests.cs` | 7 |
| Sequencing | `SequencingTests.cs` | 11 |
| Overflow and write failure | `OverflowAndFaultTests.cs` | 11 |
| Clean shutdown | `ShutdownTests.cs` | 10 |
| Deterministic comparisons | `DeterministicComparisonTests.cs` | 16 |
| **Total** | | **73** |

Counts are as enumerated by `dotnet test --list-tests`, so a `[Theory]` contributes one
entry per case.

---

## 2 · The ten-minute NQ replay, 1x vs accelerated

Ten minutes of source time, 300 events/s, seed `20260911`, snapshot interval 1000 ms
(source time), depth limit 20/side. Identical script in every run; only the wall-clock
pacing differs.

| Run | Speed | Wall elapsed | Trades | Depth | Snapshots | Written | Dropped | Queue high water | `capture_complete` |
|---|---|---|---|---|---|---|---|---|---|
| `1x` | 1x | **600.1 s** | 44 756 | 135 244 | 599 | 180 599 | 0 | 33 / 262 144 | **true** |
| `accel-60x` | 60x | **10.1 s** | 44 756 | 135 244 | 599 | 180 599 | 0 | 229 / 262 144 | **true** |
| `accel-max` | unpaced (~750x) | **0.8 s** | 44 756 | 135 244 | 599 | 180 599 | 0 | 143 807 / 262 144 | **true** |

Raw `events.jsonl` SHA-256 differs between all three, which is **correct and required**:
each line carries `recv_ts`, a genuine wall clock. If the raw bytes matched, `recv_ts`
would not be a real wall clock and the canonical projection would be removing nothing.

### Comparison

```
$ replay-verifier compare --a run-1x --b run-accel60     ->  VERDICT: IDENTICAL   exit 0
$ replay-verifier compare --a run-1x --b run-accelmax    ->  VERDICT: IDENTICAL   exit 0
```

```
events A            : 180599
events B            : 180599
differing lines     : 0
strict match        : yes
ts-bucket match     : yes

per-kind counts (A | B):
  depth         135244 |     135244
  snapshot         599 |        599
  trade          44756 |      44756
```

Files: `compare-1x-vs-accel60.txt`, `compare-1x-vs-accelmax.txt`, `run-manifests.txt`.

**Result: a ~750x change in wall-clock speed produced zero difference in the canonical
event stream, including all 599 DOM snapshots.** The snapshot count is identical
because snapshots are scheduled on source time; on a wall-clock schedule the three runs
would have produced roughly 599, 10 and 1 snapshots respectively and no comparison
would have been possible.

---

## 3 · Negative control

An IDENTICAL verdict is only meaningful if the comparison can fail. Same script, same
unpaced speed, queue reduced from 262 144 to **512** to force overflow:

| Run | Written | Dropped | Queue high water | `capture_complete` |
|---|---|---|---|---|
| `starved` | 53 409 | **127 190** | 512 / 512 | **false** |

```
$ replay-verifier compare --a run-1x --b run-starved     ->  VERDICT: DIVERGENT   exit 4

events A            : 180599
events B            : 53409
differing lines     : 180086
per-kind counts (A | B):
  depth         135244 |      39851   <-- DIFFERS
  snapshot         599 |        544   <-- DIFFERS
  trade          44756 |      13014   <-- DIFFERS

first divergence at canonical line index 513
```

Fault record written by the recorder:

```json
{"code":"queue_overflow","count":127190,
 "first_src_ts":"2026-03-10T14:30:01.7082070Z","last_src_ts":"2026-03-10T14:39:59.9917691Z",
 "first_seq":514,"last_seq":180599,"first_detail":"capacity=512"}
```

Two independent mechanisms agree exactly: the recorder's fault log reports the first
loss at **`seq` 514**, and the comparer — which never sees the fault log and works only
from the two event files — locates the first divergence at canonical index **513**
(0-based, i.e. line 514). The integrity accounting and the comparison corroborate each
other rather than sharing a common failure mode.

This also demonstrates the intended integrity behaviour end to end: the run **did not
block, did not crash and did not silently truncate**. It dropped 70% of events, counted
every one, named the sequence range, and marked the capture `capture_complete: false`
so the file cannot be mistaken for a faithful record.

File: `compare-1x-vs-starved-negative-control.txt`.

---

## 4 · Reproducing

```bash
cd Replay-Event-Verifier
dotnet build ReplayEventVerifier.sln -c Release
dotnet test  ReplayEventVerifier.sln -c Release

P="dotnet run --no-build -c Release --project src/ReplayEventVerifier.Harness --"

$P synth --out /tmp/run-1x       --label 1x       --minutes 10 --rate 300 --speed 1
$P synth --out /tmp/run-accel60  --label accel-60x --minutes 10 --rate 300 --speed 60
$P synth --out /tmp/run-accelmax --label accel-max --minutes 10 --rate 300 --speed 0
$P synth --out /tmp/run-starved  --label starved  --minutes 10 --rate 300 --speed 0 --queue 512

$P compare --a /tmp/run-1x --b /tmp/run-accel60    # expect IDENTICAL, exit 0
$P compare --a /tmp/run-1x --b /tmp/run-accelmax   # expect IDENTICAL, exit 0
$P compare --a /tmp/run-1x --b /tmp/run-starved    # expect DIVERGENT, exit 4
```

The `1x` run takes ten minutes of wall clock by construction.

Per-line content is reproducible across machines; the `events.jsonl` SHA-256 values
above are **not**, because `recv_ts` is a wall clock. Compare with `compare`, never by
hashing the raw file.

---

## 5 · What remains unproven

1. **Whether ATAS Replay is faithful.** The actual objective. Blocked on a Windows host
   with ATAS. Procedure: `../GUI-REPLAY-RUNBOOK.md`.
2. **Whether the adapter binds the correct ATAS APIs.** It compiles only against a
   written-down set of assumptions. Checklist: `../ATAS-API-VERIFICATION.md`.
3. **Behaviour under real feed-thread concurrency.** The deterministic runs above use a
   single producer. Multi-producer safety is covered by unit tests
   (`Concurrent_producers_lose_nothing_and_produce_unique_sequence_numbers`,
   `Shutdown_while_producers_are_still_running_stays_consistent`) and by the
   harness's `--threads` option, but ATAS's real dispatch pattern is unknown. Note that
   under genuine multi-threaded dispatch a `COMPLETE_BUT_UNORDERED` verdict may reflect
   the platform's threading rather than a Replay defect — which is precisely why the
   comparer reports that verdict separately instead of calling it a failure.
