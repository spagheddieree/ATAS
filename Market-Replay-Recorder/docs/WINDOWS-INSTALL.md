# Windows install — building and loading the recorder into ATAS

Everything here is for the machine that has ATAS installed. Nothing in this document
requires copying framework DLLs anywhere.

---

## 0 · Prerequisites

| Need | Why | Check |
|---|---|---|
| ATAS installed | supplies the assemblies to build against | `C:\Program Files (x86)\ATAS Platform\ATAS.Indicators.dll` exists |
| .NET SDK 8.x | builds the solution and the probe | `dotnet --version` |
| .NET **Desktop** Runtime 8.x | ATAS assemblies reference WPF types; the probe resolves against it | `dotnet --list-runtimes` shows `Microsoft.WindowsDesktop.App 8.x` |

If `Microsoft.WindowsDesktop.App` is missing, install the .NET Desktop Runtime from
Microsoft. The probe discovers it automatically — **do not** copy
`PresentationCore` or friends into the ATAS folder. An earlier run worked around the
resolver bug that way; the bug is now fixed and the workaround is obsolete.

## 1 · Get the code

```powershell
git clone -b claude/atas-replay-event-verifier-xlgebm https://github.com/spagheddieree/ATAS.git
cd ATAS\Market-Replay-Recorder
```

## 2 · Confirm the baseline before touching ATAS

```powershell
dotnet build MarketReplayRecorder.sln -c Release
dotnet test  MarketReplayRecorder.sln -c Release
```

Expect **0 warnings** and **145 passing**. Doing this first means any later failure is
unambiguously an ATAS-binding problem rather than a regression.

## 3 · Re-run the probe natively (5 minutes, worth it)

The delivered metadata report was produced on Linux from copied assemblies. That is
sound for metadata, but a native run confirms the repaired resolver on the real
machine and costs almost nothing:

```powershell
dotnet run -c Release --project tools\NFMarketReplayRecorder.ApiProbe
```

It finds the ATAS install itself. In the report, section **0 · Resolver** should say
`WindowsDesktop framework:` with a real path and `WPF assemblies: all present`. If it
instead lists missing assemblies, install the Desktop Runtime and re-run.

The probe reads metadata only: ATAS need not be running, no ATAS code executes, and
nothing but the report file is written.

## 4 · Build the indicator against the real assemblies

```powershell
dotnet build src\NFMarketReplayRecorder.ATAS\NFMarketReplayRecorder.ATAS.csproj -c Release `
  -p:UseRealAtas=true `
  -p:AtasInstallDir="C:\Program Files (x86)\ATAS Platform"
```

This is the step that has never been executed. The adapter is written against
measured signatures, so it should compile; if it does not, each error maps to a row in
`docs/ATAS-API-VERIFICATION.md` and indicates either a transcription slip or a
different ATAS version.

## 5 · The install artifact

Exactly **two** files, from `src\NFMarketReplayRecorder.ATAS\bin\Release\net472\`:

| File | Role |
|---|---|
| `NFMarketReplayRecorder.ATAS.dll` | the indicator ATAS loads |
| `NFMarketReplayRecorder.Core.dll` | all recorder behaviour; the adapter is a thin shell over it |

**Both are required.** `Core` carries the queue, writer, scheduler, provenance and
integrity logic; the adapter alone will not load.

No other dependency ships: `Core` has zero package references by design, precisely so
that nothing extra enters the ATAS process.

Copy both into the ATAS indicators folder — typically:

```
%USERPROFILE%\Documents\ATAS\Indicators
```

Then restart ATAS. The indicator appears as **NF Market Replay Recorder**.

> The ATAS-referenced assemblies (`ATAS.Indicators.dll` etc.) are **not** copied —
> the project references them with `<Private>false</Private>` so the platform's own
> copies are used at runtime.

## 6 · Settings

| Setting | Value for the experiment | Notes |
|---|---|---|
| Output directory | `%USERPROFILE%\Documents\NFMarketReplayRecorder` | a per-run subdirectory is created |
| **Acquisition mode** | **`REPLAY`** | **must be set manually** — never inferred, and an unset run records `UNKNOWN` |
| Run label | `1x`, then `accel-60x` | reporting only; `run_id` is the real identity |
| Snapshot interval (ms, source time) | `1000` | source time, so it is speed-invariant |
| Snapshot depth (levels per side) | **`0`** | see below |
| Queue capacity (events) | `262144` | raise if a run reports drops |
| Drain timeout (s) | `30` | |

> **Why depth limit 0 (record everything).** Snapshot row ordering from
> `GetMarketDepthSnapshot()` is measured to be *unspecified*. Truncating to "top N"
> assumes best-first ordering, and if that assumption is wrong the truncation silently
> discards the most important levels. Recording the full ladder costs disk and keeps
> the capture honest. Set a limit only once ordering is established.

## 7 · Output

One directory per run:

```
events.jsonl                     line 1 = capture header, then raw events
faults.jsonl                     integrity faults by code
field-register.jsonl             what this partition's fields actually contain
manifest.json                    written LAST — its presence means clean shutdown
atas-callback-diagnostics.json   single vs batch callback counts
```

Check `manifest.json` before trusting any capture:

- `"integrity_state": "CLEAN"` — nothing went wrong
- `"capture_complete": true` — nothing was lost
- `"acquisition_mode": "REPLAY"` — you set it
- `events_dropped: 0` — otherwise raise the queue capacity and re-run

`atas-callback-diagnostics.json` answers a question no amount of metadata could:
whether ATAS fires the single and batch callbacks for the same events. The recorder
binds the **single** callbacks only and merely counts the batch ones, so nothing is
double-counted either way — but the counts tell us which binding is correct long-term.

## 8 · Next

`docs/GUI-REPLAY-RUNBOOK.md` — the 1x vs accelerated experiment.
