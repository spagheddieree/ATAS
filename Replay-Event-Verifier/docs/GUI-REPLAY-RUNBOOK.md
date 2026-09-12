# Runbook — the real ATAS Replay experiment

## This procedure was NOT executed

**No GUI access, no Windows host, no ATAS installation was available in the environment
this tool was built in.** The container is headless Linux; ATAS is a Windows desktop
application whose Replay mode is driven entirely through its interface. Nothing in this
document has been run, and no claim is made about what ATAS Replay actually does.

What *was* executed is the equivalent experiment against a synthetic feed — see
`docs/evidence/`. That run establishes that **the recorder is not the variable**: given
the same source events, it produces byte-identical captures at any speed. So when you
run the procedure below and the two captures differ, the difference is attributable to
ATAS, not to this tool. That is the only reason the synthetic run is worth anything.

Prerequisite: `docs/ATAS-API-VERIFICATION.md` completed, adapter compiling against the
real assemblies.

---

## 1 · Setup

1. Build and install per `ATAS-API-VERIFICATION.md` §4.
2. Open a chart on **NQ** (front-month contract — note which one; it belongs in the
   results table).
3. Switch to **Replay** mode. Select a date and a session with real activity — RTH,
   not an overnight lull. A dead tape proves nothing.
4. Confirm Replay is configured for **ticks + market depth**. If depth is not enabled,
   the entire experiment is void: you would be measuring a trades-only feed.
5. Add the **Replay Event Verifier** indicator to the chart.

## 2 · Settings for both runs

These must be **identical** across the two runs. Any difference invalidates the
comparison.

| Setting | Value |
|---|---|
| Output directory | `%USERPROFILE%\Documents\ReplayEventVerifier` |
| Snapshot interval (ms, source time) | `1000` |
| Snapshot depth (levels per side) | `20` |
| Queue capacity (events) | `262144` |
| Drain timeout (s) | `30` |

Only **Run label** differs: `1x` for the first run, `accel` for the second.

> A run subdirectory is created per run, stamped `yyyyMMdd-HHmmss-<label>`, so the two
> runs cannot overwrite each other.

## 3 · Run A — 1x

1. Set **Run label** to `1x`.
2. Set Replay speed to **1x**.
3. Note the exact session start time you begin from.
4. Start Replay. Let it run **ten minutes of session time**.
5. Stop Replay, then **remove the indicator from the chart** — that is what triggers
   `OnDispose`, which drains the queue and writes the manifest.
6. Confirm the run directory contains `events.jsonl`, `faults.jsonl` **and
   `manifest.json`**.

> **If `manifest.json` is absent, the run did not shut down cleanly and the capture is
> not comparable.** Do not proceed; find out why. This is the designed signal, not an
> inconvenience.

## 4 · Run B — accelerated

Identical, with two changes:

1. **Run label** = `accel`.
2. Replay speed set as high as ATAS allows.

Critically: **start from exactly the same session timestamp and cover exactly the same
ten minutes of session time.** The runs are compared on source time, so they must span
the same source interval. Different spans produce a meaningless verdict.

## 5 · Compare

```powershell
dotnet run -c Release --project src\ReplayEventVerifier.Harness -- compare `
  --a "$env:USERPROFILE\Documents\ReplayEventVerifier\<...>-1x" `
  --b "$env:USERPROFILE\Documents\ReplayEventVerifier\<...>-accel" `
  --report docs\evidence\atas-replay-comparison.txt
```

Exit codes: `0` identical · `3` complete but unordered · `4` divergent.

## 6 · Reading the verdict

Check the two manifests **first**. A comparison between captures that were not clean is
not evidence of anything.

| In either manifest | Meaning |
|---|---|
| `"capture_complete": false` | This tool lost data. Fix that before drawing any conclusion about ATAS. |
| `events_dropped > 0` | Queue overflow — raise **Queue capacity** and re-run. Expected at high speed with the default. |
| `write_failures > 0` | Disk problem. Re-run to a local disk, never a network share. |
| `queue_high_water` near `queue_capacity` | Ran close to the limit; raise it even if nothing was dropped. |

Then the verdict:

| Verdict | What it means for research | What to do |
|---|---|---|
| **IDENTICAL** | Replay speed does not affect the event stream. Accelerated replay is a faithful substitute for 1x. | Accelerate freely. |
| **COMPLETE_BUT_UNORDERED** | No event lost or added, but events sharing a source timestamp arrive in a different order. | Usable where intra-timestamp sequencing does not matter. **Not usable** for order-book reconstruction, queue-position work, or anything sensitive to event order within an instant. Decide per research question, not once globally. |
| **DIVERGENT** | The captures hold different events. | Accelerated replay is **not** a faithful substitute. Read the `per-kind counts` block: if only `trade` differs, prints are being lost; if only `depth`, book updates are; if `snapshot` counts differ, the two runs did not span the same source interval and the experiment needs re-running before anything is concluded. |

## 7 · Record the result

Fill this in and commit it to `docs/evidence/`. An unrecorded result is not a result.

```
ATAS version            :
Instrument / contract   :
Session date and window :
Replay speed A          : 1x
Replay speed B          :
Verdict                 :
events A / B            :
capture_complete A / B  :
events_dropped A / B    :
queue_high_water A / B  :
First divergence        :
Conclusion              :
```

## 8 · If the verdict is DIVERGENT

Before concluding that ATAS Replay is unfaithful, rule out the three ways this
experiment gets run wrong:

1. **Different source spans.** Compare `first_src_ts` and `last_src_ts` in the two
   manifests. If they differ, the runs did not cover the same ten minutes and the
   verdict is meaningless. This is the most common mistake.
2. **Overflow at speed.** `events_dropped > 0` in the accelerated run means *this tool*
   dropped events because the writer could not keep up — not ATAS. Raise the queue
   capacity and re-run before blaming the platform.
3. **Non-identical settings.** A different snapshot interval or depth limit between
   runs changes the output by construction. Both are recorded in each manifest; check
   them rather than trusting memory.

Only once all three are excluded is DIVERGENT a finding about ATAS. Then re-run at a
third speed: if 1x and 2x agree but 10x diverges, there is a speed threshold worth
locating, and that is a much more actionable result than a binary pass/fail.
