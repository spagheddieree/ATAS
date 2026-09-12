# NF Market Replay Recorder

Also referred to as **the recorder** or **the recorder indicator**.

An ATAS indicator that captures **raw market events only** — every individual trade,
every individual market-depth change, and a periodic snapshot of the platform's own
order book — to plain files, faithfully and without interpretation.

**No trading logic. No signals. No derived features** — no delta, no imbalance, no
ratios, anywhere. It draws nothing.

## Current mission — Phase 0, data-source validation

The recorder's first job is to answer one question about its own input:

> Does ATAS Replay in ticks + DOM mode deliver a complete event stream, and does it
> deliver the *same* one at an accelerated speed as it does at 1x?

That is why the repository ships a deterministic comparison harness alongside the
indicator. Recording faithfully is the durable capability; verifying that ATAS Replay
is worth recording is what it is being pointed at first. Until that question is
answered, no dataset built from Replay can be trusted, so nothing downstream of it has
been started.

> **Naming history (2026-09-12).** *Replay Event Verifier* → *NF Market Data
> Recorder* → **NF Market Replay Recorder** (current). Old identifiers, useful only
> for searching prior commits: `ReplayEventVerifier.*` / `Replay-Event-Verifier/`,
> then `NFMarketDataRecorder.*` / `Market-Data-Recorder/`.
>
> The serialized capture format was **not** changed by either rename: no field in the
> header, manifest or event stream carries the product name, so `schema_version`
> remains `rev-3` and captures written under the old names stay readable.

---

## Status

| Component | State | Evidence |
|---|---|---|
| `Core` — recorder, scheduler, queue, writer, comparer, provenance | **Done, tested** | 145/145 tests pass · `docs/evidence/` |
| `Harness` — synthetic feed + compare CLI | **Done, tested** | `docs/evidence/` |
| `ATAS` — the indicator adapter | **Written, type-checked against an ASSUMED API** | `docs/ATAS-API-VERIFICATION.md` |
| Real ATAS Replay experiment | **NOT RUN** — no GUI, no Windows, no ATAS available | `docs/GUI-REPLAY-RUNBOOK.md` |

**The ATAS API surface is unverified.** nuget.org carries no ATAS packages (HTTP 404)
and the vendor documentation is egress-blocked, so the adapter was written against an
API surface that could not be checked. All behaviour is in `Core`, which has no ATAS
dependency, so a wrong assumption is a localised compile error rather than a subtly
wrong capture. `docs/ATAS-API-VERIFICATION.md` lists every symbol to confirm.

---

## Layout

```
src/NFMarketReplayRecorder.Core/      netstandard2.0, no package references.
                                   All behaviour. No ATAS dependency.
src/NFMarketReplayRecorder.ATAS/      The indicator. The only ATAS types in the repo.
                                   Pure translation, no logic.
src/NFMarketReplayRecorder.Harness/   net8.0 CLI: synthetic feed, compare, inspect.
tests/NFMarketReplayRecorder.Tests/   73 xunit tests.
docs/                              Design, API checklist, runbook, evidence.
```

## Build and test

```bash
dotnet build NFMarketReplayRecorder.sln -c Release
dotnet test  NFMarketReplayRecorder.sln -c Release
```

Builds and passes on Linux with no ATAS present — the adapter compiles in **stub mode**
against `src/NFMarketReplayRecorder.ATAS/ApiStub/AtasApiAssumptions.cs`. That file is a
written-down set of assumptions, **not** the ATAS SDK. It proves the adapter is
coherent C#; it proves nothing about the real API.

To build against real ATAS, on Windows:

```powershell
dotnet build src\NFMarketReplayRecorder.ATAS\NFMarketReplayRecorder.ATAS.csproj -c Release `
  -p:UseRealAtas=true -p:AtasInstallDir="C:\Program Files (x86)\ATAS Platform"
```

## Output

One directory per run:

```
events.jsonl         line 1: capture header (run provenance)
                     line 2+: raw events, one JSON object per line
faults.jsonl         integrity faults, aggregated by code
field-register.jsonl what this partition's fields actually contain
manifest.json        counts, provenance, integrity state, SHA-256 of events.jsonl
```

Every capture declares its provenance: `run_id`, `source_class` (always `RAW_SOURCE`),
`acquisition_mode` (`LIVE`/`REPLAY`/`UNKNOWN`, operator-declared and never inferred),
raw instrument identity verbatim plus a derived canonical identity, and an integrity
state of `CLEAN`/`DEGRADED`/`CORRUPT`. Full specification:
[`docs/DATASET-V0.1-CONTRACT.md`](docs/DATASET-V0.1-CONTRACT.md).

`manifest.json` is written **last** and only on the clean shutdown path, so its
presence means the run ended properly. A run directory without one is not comparable.

Always check `capture_complete` before using a capture. `false` means this tool lost
data — dropped events, a write failure, or a drain timeout — and the capture must not
be treated as faithful.

### Event shapes

```json
{"kind":"header","schema_version":"rev-3","run_id":"...","source_class":"RAW_SOURCE","acquisition_mode":"REPLAY", ...}
{"recorder_seq":1,"kind":"trade","src_ts":"...","recv_ts":"...","price":20000.25,"volume":3,"aggressor":"buy"}
{"recorder_seq":2,"kind":"depth","src_ts":"...","recv_ts":"...","side":"bid","price":20000,"volume":0}
{"recorder_seq":3,"kind":"snapshot","src_ts":"...","recv_ts":"...","depth_limit":20,"bids":[[p,v],...],"asks":[[p,v],...]}
```

The logical row is `header ⊗ event` — run provenance is constant per file and carried
once rather than repeated on every line.

- `src_ts` — the **platform's** timestamp. The only clock any decision is made on.
- `recv_ts` — wall clock at capture. Diagnostic only; excluded from comparison.
- `recorder_seq` — capture order. A total order of *observation*, **not** an exchange
  sequence; no source sequence is claimed to exist.
- `recv_ts − src_ts` is **not latency** — under `REPLAY` it is meaningless by
  construction.
- `volume: 0` on a depth record means the level was removed.
- `aggressor: "unknown"` is a real value — direction is never inferred from price.

## The harness

Drives the recorder with a deterministic synthetic feed so the whole pipeline can be
exercised without ATAS.

```bash
# --speed is a WALL-CLOCK multiplier. Source timestamps are fixed, so 1 and 60
# must produce identical captures. --speed 0 means no pacing at all.
dotnet run -c Release --project src/NFMarketReplayRecorder.Harness -- \
  synth --out captures/1x --label 1x --minutes 10 --rate 300 --speed 1

dotnet run -c Release --project src/NFMarketReplayRecorder.Harness -- \
  synth --out captures/accel --label accel --minutes 10 --rate 300 --speed 60

dotnet run -c Release --project src/NFMarketReplayRecorder.Harness -- \
  compare --a captures/1x --b captures/accel
```

`compare` exits `0` identical · `3` complete but unordered · `4` divergent.

The synthetic feed is **not** a claim about ATAS. It establishes that the recorder is
not the variable — given the same source events it produces byte-identical output at
any speed — so that a difference seen in a real ATAS run is attributable to ATAS.

## Design

Three decisions carry most of the weight; `docs/DESIGN.md` covers them and the rest.

**Snapshots are scheduled on source time, anchored to the interval grid.** Boundaries
are a pure function of the event stream, so a 1x and a 10x run snapshot at identical
source instants and the captures compare line for line. A wall-clock timer would put
~600 snapshots in one run and ~60 in the other at unrelated points — no comparison
would be possible even in principle.

**The book is read from the platform's depth API, never rebuilt from the depth-change
stream.** A reconstructed book is a function of the changes we received, so it agrees
with them by construction and can never reveal a change that went missing.

**The queue drops on overflow rather than blocking, and says so loudly.** Blocking
would stall the feed thread and distort the timing being measured. Every drop is
counted and surfaced as `capture_complete: false`. A knowingly incomplete capture is
usable; a silently incomplete one is a liability.

## Next step

`docs/GUI-REPLAY-RUNBOOK.md` — the real experiment, which needs a Windows host running
ATAS. Complete `docs/ATAS-API-VERIFICATION.md` first.
