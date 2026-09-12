# Provenance — atas-api-report.md

Read this before interpreting `atas-api-report.md`. It records **how** the report
was produced, because the report's own header lines are misleading about the host.

## What the report measured

Real ATAS assemblies from Eddie's Windows machine `m18`:

- Install directory: `C:\Program Files (x86)\ATAS Platform`
- 238 `*.dll`, 289 MB, copied byte-for-byte into the execution container
- SHA-256 verified identical on both sides for the load-bearing assemblies:

| Assembly | SHA-256 |
|---|---|
| ATAS.Indicators.dll | `cc721fb118c3b6cca94ae02ed4cf89f53c7076729d3ab1cfa8030b95f0952756` |
| ATAS.DataFeedsCore.dll | `b598f2987437dcdc423acfe03237a01972f6c6891e0174e385b267d369b05f3f` |
| ATAS.Types.dll | `355bac332cd0f00f28093972cc0743077c73aca6419176dd2b0f39c02319be76` |
| OFT.Core.dll | `479ce76138ebde434db9a9adccab5dc0f004c118b3d7c92b25d85cb0fbd698aa` |
| Utils.Common.dll | `97a9590f03989769a16d5a02b55d5b1409c3e191710b0985da5db0d063860dc1` |

ATAS was **not** started. The probe reads metadata only (`MetadataLoadContext`).

## Where the probe ran — and why that is not Windows

The report header says `Probe host OS: Ubuntu 24.04.4 LTS` and
`ATAS directory: /home/claude/atas-probe-input`. That is accurate and is a
**deviation from the work package**, for a hard reason:

- The Cowork session's device shell is an isolated **Linux VM**, not Windows.
- Computer-use grants for `Windows PowerShell` / `Terminal` / `Command Prompt`
  resolve at `click` tier only — visible and clickable, but **keystrokes are
  refused**, so no PowerShell command could be typed on `m18`.
- No other native-Windows execution surface was available to this session.

`MetadataLoadContext` reads PE/CLI metadata tables directly and never executes
the target assemblies, so the emitted signatures are a property of the
assemblies, not of the host OS. The evidence is therefore sound for API-binding
purposes, but it is **not** platform-native verification of ATAS runtime
behaviour. Anything in the adapter that depends on Windows runtime behaviour
still needs a native check.

## Deviation 2 — the committed probe does not run as written

Running the probe exactly as its README specifies fails, on Linux **and,
by inspection, on Windows too**:

```
error: FileNotFoundException: Could not find assembly 'PresentationCore,
Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'.
```

`Probe.Run()` builds its resolver from the ATAS directory's DLLs plus
`RuntimeEnvironment.GetRuntimeDirectory()`. For a plain `net8.0` console app
that directory is `Microsoft.NETCore.App` — which does **not** contain
`PresentationCore`, `PresentationFramework`, `WindowsBase` or `System.Xaml`.
ATAS's own assemblies reference those WPF types, so resolution fails and
`Program.Main` catches the exception, writes **no report at all**, and exits 1.
On Windows the same resolver is built the same way, so the same failure is
expected there. This is a genuine defect, not a Linux artifact.

**Fix belongs to chat (4)** — likely `<UseWPF>` / `net8.0-windows` on the probe
project, or adding the WindowsDesktop framework directory to `resolverPaths`,
plus writing a partial report instead of swallowing the run on exception.

### The environment-only workaround used instead

No repository file was edited. The scanned directory (a container-side copy) had
48 assemblies added to it from the **same machine's** .NET 8 Desktop runtime:

- Source: `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.31`
- Only names not already present in the ATAS directory were added — zero overwrites
- Scanned directory therefore holds 286 DLLs (238 ATAS + 48 desktop-runtime)

These additions are invisible to the report: §0 only loads assemblies whose name
starts with `ATAS.`, `OFT.` or `Utils.`, and every later section iterates only
that set. They exist solely so references resolve.

## Deviation 3 — NuGet was unreachable

`api.nuget.org` is denied by this session's egress gateway, so
`System.Reflection.MetadataLoadContext 8.0.0` could not be restored. A local
offline feed was created outside the repository from the copy that ships with the
.NET 8 SDK (`/usr/lib/dotnet/sdk/8.0.131/System.Reflection.MetadataLoadContext.dll`),
with a `NuGet.config` placed **above** the clone at `/home/claude/NuGet.config`.
The committed `.csproj` restored and built unmodified — 0 warnings, 0 errors.

## Repository state

- No local checkout of `spagheddieree/ATAS` exists on `m18`. Searched
  `C:\Projects`, `C:\Users\eddie\source\repos`, Documents, Desktop, Downloads,
  `G:\`. `C:\Projects\ATAS` holds six *other* indicator repos with different
  remotes. A fresh clone was therefore made in the container.
- The report was generated twice. First at `25e3a1f` — the HEAD chat (4)
  expected. The remote branch then advanced to `ba329d5`
  (*"rename Replay Event Verifier to NF Market Data Recorder"*), pushed by
  another Claude session at 01:39 UTC while this work was in progress. The probe
  was re-run at `ba329d5` after a fast-forward; both runs produced a
  byte-identical 138,047-byte report, confirming the rename is behaviour-neutral
  for the probe. **The delivered report belongs to `ba329d5`.**
- Working tree clean at completion. Nothing committed, nothing pushed.

## Exact commands

```bash
# auto-discovery, as the README specifies (fails on Linux — no Windows SpecialFolders)
dotnet run -c Release --project tools/NFMarketDataRecorder.ApiProbe -- --list
#   -> "No ATAS installation found automatically."  exit 2

# the run that produced the report
dotnet run -c Release --project tools/NFMarketDataRecorder.ApiProbe -- \
  --dir /home/claude/atas-probe-input \
  --out /mnt/user-data/outputs/atas-api-report.md
#   -> "Report written"  exit 0
```

.NET SDK: 8.0.131 (Ubuntu `dotnet-sdk-8.0`, `/usr/bin/dotnet`).
