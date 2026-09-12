# Design

The question: **does ATAS Replay in ticks + DOM mode deliver a complete event stream,
and does it deliver the same one at an accelerated speed as at 1x?**

Everything here follows from that being a *measurement* problem. The tool has no edge,
no signal and no opinion about the market. Its only job is to record what arrived
without changing it, and to be honest about anything it failed to record.

---

## 1 · Four constraints that shaped everything

**A recorder must not perturb what it measures.** Market-data callbacks run on the
platform's feed thread. Anything slow in a callback delays the feed and changes the
timing being recorded. So no callback performs I/O, takes a contended lock, or blocks.

**Wall clock is the enemy.** The whole experiment compares a 1x run against an
accelerated one. Any wall-clock dependency makes the two structurally incomparable
rather than merely different. Source time is the only clock that drives a decision.

**Silent loss is worse than loud loss.** A capture that dropped 2% of depth updates and
does not say so will produce confident, wrong research. Every loss path is counted and
surfaced.

**The ATAS API could not be verified.** No SDK, no docs, no assemblies (evidence in
`ATAS-API-VERIFICATION.md`). So the architecture had to make an unverifiable binding
safe.

---

## 2 · Layering

```
NFMarketDataRecorder.ATAS      ~230 lines. The only ATAS types in the repository.
                              Pure translation: four callbacks in, four Core calls out.
                              No arithmetic, no state, no decisions.
        │
        ▼
NFMarketDataRecorder.Core      netstandard2.0, zero package references.
                              All behaviour. All 73 tests point here.
        ▲
        │
NFMarketDataRecorder.Harness   net8.0 CLI. Synthetic feed + compare command.
```

This is a direct answer to constraint four. Because the adapter holds no logic, a wrong
API assumption is a **compile error**, never a capture that looks right and is subtly
wrong. And because Core has no ATAS dependency, the entire tool is testable on Linux
with no platform at all.

The adapter is `IDomSource` — an interface Core defines and the adapter implements.
Core calls *out* to the platform for the book; the platform calls *in* with events. The
dependency points the right way at both boundaries.

---

## 3 · The decisions worth defending

### 3.1 · Snapshot scheduling on source time, anchored to the interval grid

**This is the decision the whole tool rests on.**

Snapshot boundaries are computed from the feed's own timestamps, floored to the
interval grid:

```
first event at 14:30:00.237, interval 1s  ->  next boundary 14:30:01.000
```

Not `firstEvent + interval`. The grid anchor means two runs whose first event differs by
microseconds still agree on every subsequent boundary.

Why it matters: snapshot boundaries become a **pure function of the event stream**. A 1x
run and a 10x run schedule snapshots at *identical source instants*, so their captures
can be compared line for line.

The alternative — a wall-clock timer — would have taken ~600 snapshots at 1x and ~60 at
10x, at unrelated points in the stream. There would be no comparison to make. Not a
worse result: **no result at all.**

A snapshot due at boundary `B` is emitted immediately before the first event at or after
`B`, so it reflects every event strictly before `B` and none at or after it. That is a
deterministic position, which is what makes it reproducible.

### 3.2 · The book comes from the platform, never from our own reconstruction

Snapshots read the platform's depth API. We never rebuild a book from the depth-change
stream.

A reconstructed book is a *function of the changes we received*. It therefore agrees
with them by construction and can never reveal a change that went missing. The
platform's book is an independent observation, and disagreement between it and the
change stream is exactly the finding the tool is hunting for.

Rebuilding would also make the snapshot a derived feature, which the brief forbids.

### 3.3 · Overflow drops, and says so

Every bounded queue chooses what to sacrifice when full: latency, memory, or data.

- *Block* → stalls the feed thread, distorts the timing being measured. Disqualified by
  constraint one.
- *Grow* → the process eventually dies mid-capture and everything is lost.
- *Drop* → loses data, but bounded and observable.

Dropping wins, but only because the loss is made loud: a counter, an aggregated
`queue_overflow` fault with the sequence range, `events_dropped` in the manifest, and
`capture_complete: false`. A knowingly incomplete capture is usable research input. A
silently incomplete one is a liability.

`queue_high_water` is recorded on every run, including clean ones, so you can see how
close a run came to overflowing before it actually does.

### 3.4 · Out-of-order timestamps are recorded, never repaired

A source timestamp earlier than the previous high raises `source_time_regression` and
**the event keeps its original stamp**.

Clamping it to the running maximum is the tempting fix and it is exactly wrong: it would
erase the defect the tool exists to detect and produce a file that looks clean. The
recorder's job is to report what arrived, not to improve it.

Same reasoning: an unmapped trade direction becomes `"unknown"` rather than being
inferred from price. An honest gap beats a plausible fabrication in a fidelity tool.

### 3.5 · Comparison excludes `seq` and `recv_ts`, and reports two verdicts

The canonical projection drops `seq` and `recv_ts`. Both are properties of the
*observer*, not the market: `recv_ts` is wall clock and *must* differ between runs;
`seq` is capture order, which can differ legitimately when the platform dispatches
same-instant events on different threads. Including either would fail every comparison
and tell you nothing.

Two verdicts are reported separately:

- **strict** — same events, same order.
- **timestamp-bucket** — same multiset of events per source timestamp, order within an
  instant ignored.

Collapsing them into one pass/fail would destroy the most useful finding. *Strict fails,
bucket passes* means every event arrived but intra-timestamp order is unstable — fine
for most research, fatal for order-book reconstruction or queue-position work. That is a
decision the researcher must make per question, so the tool reports the distinction
rather than making the decision for them.

### 3.6 · Hand-rolled JSON

No package reference in Core. Three reasons, in order of weight:

1. **Determinism is the product.** Field order, decimal formatting and timestamp
   formatting are fixed by `JsonLine.cs` rather than by a serializer's defaults. Two
   captures of the same events are byte-identical, so comparison is a text diff.
2. **Nothing extra loads into the trading platform's process.**
3. netstandard2.0 has no built-in JSON writer, so the alternative *is* a package.

Two specifics that matter more than they look:

- **Decimals are canonicalised** — trailing fractional zeros stripped, so `1.50` and
  `1.5` (same value, different scale) cannot produce two different lines. Without this,
  a comparison could fail on a formatting artefact.
- **Timestamps are fixed-width** (`yyyy-MM-ddTHH:mm:ss.fffffffZ`), so the text sorts in
  value order and a capture's monotonicity can be checked with `sort -c` and no parsing.

### 3.7 · The manifest is written last

`manifest.json` is written only on the shutdown path, after the queue has drained and
the events file has been flushed durably and hashed.

Its **presence is the signal**: a run directory with no manifest did not shut down
cleanly and its capture is not comparable. That is a stronger guarantee than a flag
inside the file, because it survives the process being killed.

### 3.8 · JSONL, not CSV or a binary format

- CSV cannot hold a variable-length DOM ladder without either a second file or ugly
  column packing.
- A binary format would be smaller and faster and would make every capture opaque to
  `grep`, `diff`, `wc` and `sort` — which is most of how a verification tool is
  actually used.
- Line-oriented means a hard process kill truncates at most one record.

The brief says no database yet, and this is why that is the right call for now: the
question is whether the data is trustworthy. Answer that with files you can read, then
choose storage.

---

## 4 · Threading

| Thread | Does | Never does |
|---|---|---|
| Platform feed thread(s) | `Interlocked` increment, source-time compare, bounded enqueue, set an event handle | I/O, allocate unboundedly, block |
| `market-data-writer` (one) | Dequeue, serialize, write, periodic flush | Touch producer state |
| Shutdown caller | Drain, flush durably, write sidecars | Race a second `Complete()` — it is idempotent |

One consumer means the sink needs no locking.

The one lock, `_scheduleGate`, covers the source clock, the snapshot scheduler *and* the
book read. The book must be read under the same lock that advanced the scheduler:
otherwise two threads crossing a boundary together could read the book in the opposite
order to the boundaries they own, and a snapshot would carry a timestamp that does not
match its contents. It is held for a few integer comparisons plus, at a boundary only,
one platform depth read — never across I/O.

The writer thread is a **background** thread, so a stuck writer can never keep the host
process alive. Shutdown drains explicitly, so this costs nothing on the clean path.

A write failure does **not** kill the writer. If it did, the queue would fill, every
later event would be dropped, and the run would end with no manifest — one bad write
would destroy the whole capture. Instead failures are counted, draining continues, and
the manifest declares the capture unusable.

---

## 5 · Output

```
<run-dir>/
  events.jsonl    raw events only, one per line, UTF-8, LF
  faults.jsonl    integrity faults, aggregated by code
  manifest.json   run metadata, counts, integrity flags, SHA-256 of events.jsonl
```

Separation of raw from derived is **structural, not conventional**: `events.jsonl`
contains market observations and nothing else, and there is no derived-feature file at
all — no delta, no imbalance, no ratio, anywhere in the codebase. `faults.jsonl` and
`manifest.json` describe the *run*, not the market.

`SerializationTests.Raw_schema_carries_no_derived_field` enforces the ban with an
assertion rather than trusting review.

LF line endings and UTF-8 without BOM are fixed regardless of host OS, so a capture
taken on Windows is byte-comparable with one taken anywhere else.

---

## 6 · What this design cannot tell you

Stated plainly, because a verification tool that oversells itself is worse than none:

- **Whether ATAS Replay is faithful.** Not answered here and not answerable without a
  Windows host running ATAS. Run `GUI-REPLAY-RUNBOOK.md`.
- **Whether the adapter binds the right ATAS APIs.** Unverified. See
  `ATAS-API-VERIFICATION.md` §3.
- **Whether the platform's own book is correct.** Snapshots are taken from it, so a
  platform-side book bug is invisible to this tool.
- **Whether events never reached the platform at all.** A recorder can only record what
  it is given. Feed-level loss upstream of ATAS is out of reach by construction.

What it *does* establish is narrow and real: **the recorder is not the variable.** Given
the same source events, its output is a pure function of them at any speed — proven by
the evidence in `docs/evidence/`. So any difference observed in a real ATAS run is
attributable to ATAS rather than to the instrument measuring it. Without that, the whole
experiment would be uninterpretable.
