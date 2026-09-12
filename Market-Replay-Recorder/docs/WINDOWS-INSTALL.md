# Windows install — building and loading the recorder into ATAS

Everything here is for the machine that has ATAS installed. Nothing in this document
requires copying framework DLLs anywhere.

**Two products, tested separately.** ATAS Classic and ATAS X are distinct
installations with distinct directories and possibly distinct .NET runtimes. The
recorder is built from one shared codebase and is expected to work on both, but
**a pass on one is not evidence about the other** — each must be installed and
validated on its own, and reported separately.

---

## 0 · Prerequisites

| Need | Why | Check |
|---|---|---|
| ATAS installed (Classic and/or X) | supplies the assemblies to build against | `ATAS.Indicators.dll` exists in the install directory |
| .NET SDK 8.x | builds the solution and the probe | `dotnet --version` |
| .NET **Desktop** Runtime 8.x | ATAS assemblies reference WPF types; both the probe and the `net8.0-windows` adapter build need it | `dotnet --list-runtimes` shows `Microsoft.WindowsDesktop.App 8.x` |

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

The first attempt at this step failed and the two defects it exposed are fixed:
the project targeted `net472` while the installed ATAS assemblies are .NETCoreApp
8.0, and the adapter used enum names that are ambiguous between `ATAS.Indicators`
and `ATAS.DataFeedsCore`. See `docs/ATAS-API-VERIFICATION.md` §4.4.

**This build has still not been executed successfully on Windows.** The fixes are
verified locally — the stub build now reproduces the exact CS0104 pair when the
defect is reintroduced — but a diagnostic compile is not an MSBuild run on the real
machine. If it fails, each error maps to a row in `ATAS-API-VERIFICATION.md`.

## 5 · Which artifact, and where it goes

ATAS ships **two products**, and supported installations may run on **different
.NET runtimes**. The indicator's target framework must match the runtime of the
installation it is loaded into, so pick the artifact by measurement, not by the
version number on the splash screen.

### 5.1 · Detect the runtime — the authority is the runtimeconfig

| Product | Runtime config file |
|---|---|
| ATAS **Classic** | `OFT.Platform.runtimeconfig.json` |
| **ATAS X** | `OFT.PlatformX.runtimeconfig.json` |

```powershell
# in the installation directory of whichever product you are testing
Get-Content .\OFT.Platform.runtimeconfig.json  | ConvertFrom-Json | % { $_.runtimeOptions.tfm }   # Classic
Get-Content .\OFT.PlatformX.runtimeconfig.json | ConvertFrom-Json | % { $_.runtimeOptions.tfm }   # ATAS X
```

The `tfm` value selects the build. **Do not infer the runtime from the product
version label.**

### 5.2 · The build matrix

| Runtime (`tfm`) | Artifact directory | Classic | ATAS X |
|---|---|---|---|
| `net8.0` | `bin\Release\net8.0-windows\` | compile target verified locally | compile target verified locally |
| `net10.0` | `bin\Release\net10.0-windows\` | **structurally supported, NOT verified** | **structurally supported, NOT verified** |

Precise status, because the distinction matters:

- **Compile-verified (stub):** the adapter compiles against the measured API surface
  for `net8.0-windows`. Verified on Linux against the stub, not yet by MSBuild on
  Windows against the real assemblies.
- **Structurally supported:** the project multitargets on request and the code has
  no runtime-version-specific constructs, but `net10.0-windows` has **never been
  compiled** — no .NET 10 SDK and no net10 ATAS reference set was available. Do not
  report it as working.
- **Runtime-verified:** nothing yet, on either product. That is what the next Cowork
  validation is for.

To produce the net10 artifact on a machine with a .NET 10 SDK (note the escaped
semicolon — MSBuild splits an unescaped one into a second switch):

```powershell
dotnet build .\src\NFMarketReplayRecorder.ATAS\NFMarketReplayRecorder.ATAS.csproj `
  -c Release -p:UseRealAtas=true `
  -p:AtasInstallDir="C:\Program Files (x86)\ATAS Platform" `
  "-p:AtasTargetFrameworks=net8.0-windows%3Bnet10.0-windows"
```

### 5.3 · The files

Exactly **two**, from the directory matching the detected `tfm`:

| File | Role |
|---|---|
| `NFMarketReplayRecorder.ATAS.dll` | the indicator ATAS loads |
| `NFMarketReplayRecorder.Core.dll` | all recorder behaviour |

**Both are required.** `Core` carries the queue, writer, scheduler, provenance and
integrity logic; the adapter alone will not load. Nothing else ships — `Core` has
zero package references by design.

The ATAS-referenced assemblies are **not** copied: they are referenced with
`<Private>false</Private>` so the platform's own copies are used.

### 5.4 · Install directories

| Product | Directory |
|---|---|
| ATAS **Classic** | `%APPDATA%\ATAS\Indicators` — i.e. `C:\Users\<user>\AppData\Roaming\ATAS\Indicators` |
| **ATAS X** | `%APPDATA%\ATAS X\Indicators` — i.e. `C:\Users\<user>\AppData\Roaming\ATAS X\Indicators` |

> **Correction.** Earlier revisions of this document named
> `%USERPROFILE%\Documents\ATAS\Indicators`. That was not verified against the
> installed platform and should not be used unless the installation actually proves
> it. Verify the directory exists before writing to it.

Where the platform offers an **"Add custom indicator"** workflow in its UI, prefer
it — it puts the assembly where that installation expects it. The manual paths above
are the fallback and the thing to verify against.

**Load semantics differ between products.** Classic generally needs a restart after
a DLL is placed. ATAS X may pick up a newly installed DLL without the same restart
behaviour. Do not assume they behave identically; observe and record what actually
happened.

The indicator appears as **NF Market Replay Recorder** on both.

### 5.5 · Do not cross-install

An assembly built for one runtime dropped into an installation running the other
will fail to load, and the failure can look like a code defect rather than a
mismatch. Detect the `tfm` first (§5.1), then take the matching directory.

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
