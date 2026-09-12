# ATAS API verification checklist

**Status: every ATAS symbol used by this tool is UNVERIFIED.**

This document exists because `src/NFMarketDataRecorder.ATAS/MarketDataRecorderIndicator.cs`
was written without access to the ATAS SDK. It lists every ATAS symbol the adapter
touches, what is assumed about it, and what to do when the assumption turns out to be
wrong. Work through it once on a machine with ATAS installed and the adapter is done.

---

## 1 · Why the surface could not be verified

Measured in the authoring environment, not recalled:

| Check | Command | Result |
|---|---|---|
| ATAS SDK on nuget.org | `curl https://api.nuget.org/v3-flatcontainer/atas.indicators/index.json` | **HTTP 404** |
| " | `.../atas.datafeedscore/index.json` | **HTTP 404** |
| " | `.../atas.strategies/index.json` | **HTTP 404** |
| " | `.../utils.common/index.json` | **HTTP 404** |
| Vendor docs | `curl https://atas.net/`, `https://docs.atas.net/`, `https://help.atas.net/en/` | **blocked** — proxy `connect_rejected`, organization egress policy |
| Local assemblies | `find / -xdev -iname 'ATAS*.dll' -o -iname 'OFT*.dll' -o -iname 'Utils.Common.dll'` | none |
| Windows filesystem | `mount` — cifs/smb/9p/virtiofs/drvfs/nfs | **none**; single ext4 root, no host share |
| Windows emulation | `which wine wine64` | **not installed**, no wine prefix |
| Prior ATAS project on the account | `spagheddieree/atas` @ `claude/nq-volatility-atas-conversion-ifuil9` | exists, but its ATAS adapter was **also left unimplemented for this same reason** |

The ATAS assemblies ship with the Windows installation and are not publicly
redistributable, so this is a permanent constraint of any Linux container, not a
transient network problem.

## 2 · How the design contains the risk

All behaviour lives in `NFMarketDataRecorder.Core`, which has no ATAS dependency and is
covered by 73 tests. The adapter contains **no logic at all** — no arithmetic, no
buffering, no scheduling, no decisions. It maps four ATAS callbacks onto four Core
calls and reads the depth API.

The consequence that matters: a wrong assumption below produces a **compile error**,
which you cannot miss. It cannot produce a capture that looks right and is subtly
wrong. That is the whole reason the adapter was kept this thin.

## 3 · The symbols to verify

Grouped by how likely the assumption is to be wrong.

### 3.1 · High risk — verify these first

| # | Symbol | Assumed | If wrong |
|---|---|---|---|
| 1 | `Indicator.OnNewTrade(MarketDataArg)` | `protected override`, fires once per individual print | This is the single most important assumption in the tool. If trades arrive by another route (a different override name, an event subscription, or a `MarketDataType.Trade` case inside `MarketDepthChanged`), reroute the call to `recorder.OnTrade(...)`. **If ATAS only exposes aggregated trades, the whole objective fails and that is itself the finding** — record it and stop. |
| 2 | `Indicator.MarketDepthChanged(MarketDataArg)` | `protected override`, fires once per individual level change | Same treatment: find the per-change callback and route it to `recorder.OnDepthChange(...)`. If ATAS only surfaces whole-book refreshes, say so — it means depth changes cannot be captured individually, which is a primary finding, not a defect to work around. |
| 3 | `MarketDepthInfo.GetMarketDepth(MarketDataType)` | Returns an ordered enumerable of rows, best price first | Very likely to differ in name or shape (`GetTopBids(n)` / `GetTopAsks(n)`, a `MarketDepthSnapshot` type, or an indexed collection). Adjust `ReadSide` only. **Verify the ordering explicitly** — if rows come back worst-price-first, snapshots are still correct but reversed, and `ReadSide` must reverse them so `depth_limit` truncates the right end of the book. |
| 4 | `MarketDataArg.Time` is the **feed's** timestamp | Not the local receive time | Critical. If `Time` is stamped on arrival, then at an accelerated replay it is a wall clock in disguise, every comparison in this tool is meaningless, and you must find the true exchange timestamp field. Check by replaying a known session and confirming `src_ts` matches historical prints. |

### 3.2 · Medium risk

| # | Symbol | Assumed | If wrong |
|---|---|---|---|
| 5 | `MarketDataArg.DataType` → `MarketDataType.{Bid,Ask}` | Discriminates book side | Adjust `MapSide`. Note it returns `null` for anything unrecognised and the event is then skipped — if a third meaningful side exists, add it rather than letting it fall through. |
| 6 | `MarketDataArg.Direction` → `TradeDirection.{Buy,Sell}` | Aggressor as reported | Adjust `MapAggressor`. Anything unmapped becomes `"unknown"` on purpose; **do not infer direction from price**, that would be a derived feature. |
| 7 | `MarketDataArg.Price` / `.Volume` are `decimal` | Exact, not `double` | If they are `double`, convert at the boundary and **say so in the run notes** — binary floating point would make byte-identical comparison unreliable. |
| 8 | `Indicator.OnInitialize()` | Called once before data flows | If the lifecycle hook differs, move recorder construction. It must run before the first callback or early events are lost. |
| 9 | `Indicator.OnDispose()` | Called on removal/shutdown | If ATAS does not call it reliably, the manifest will be missing and the capture will read as unclean — which is the correct signal, but add an explicit stop control if it happens routinely. |
| 10 | `Indicator.OnCalculate(int, decimal)` | `protected abstract`, must be overridden | Kept deliberately empty. If the signature differs, match it; do not add logic. |
| 11 | `InstrumentInfo.Instrument` | Instrument name string | Cosmetic — manifest only. Already wrapped in try/catch. |

### 3.3 · Low risk — cosmetic or easily dropped

| # | Symbol | Assumed | If wrong |
|---|---|---|---|
| 12 | `[DisplayName]`, `[Display]`, `[Range]` | Standard attributes drive the settings UI | Purely presentational. Delete them if ATAS uses its own attribute set. |
| 13 | `EnableCustomDrawing`, `SubscribeToDrawingEvents(DrawingLayouts.None)`, `DenyToChangePanel` | Opt out of rendering | If these members do not exist, **just delete the three lines**. They are an optimisation, not a requirement — the indicator draws nothing either way. |

## 3.4 · Why the stub is pinned to the real attribute shapes

The stub already caught one real defect. `[Display]` was originally applied to the
indicator **class**; the stub permitted it (`AttributeTargets.All`) while the real
framework attribute targets only members, so a real build would have failed with
CS0592. The stub's `AttributeUsage` is now copied from the framework attribute and the
class-level `[Display]` is gone.

The lesson generalises: **a stub that is more permissive than the real API hides
errors.** If you tighten an assumption below to match reality, tighten the stub with
it rather than only fixing the call site.

A standing check that the stub is not masking anything else
(`docs/evidence/real-atas-mode-probe.txt`):

```bash
dotnet build src/NFMarketDataRecorder.ATAS/NFMarketDataRecorder.ATAS.csproj -c Release \
  -p:UseRealAtas=true -p:AtasInstallDir=/nonexistent
```

Every error must be **CS0246** (type or namespace not found) naming an ATAS type. Any
other error code is a defect in the adapter source, not an SDK-availability problem,
and should be fixed before anyone takes this to a Windows machine.

## 4 · Procedure

### Step 1 — run the probe (do this first)

`tools/NFMarketDataRecorder.ApiProbe` reads the real assemblies' metadata and reports
what the API actually is. It replaces manual API archaeology with one command, and it
answers rows 1–13 below from measurement rather than inspection.

```powershell
cd Market-Data-Recorder
dotnet run -c Release --project tools/NFMarketDataRecorder.ApiProbe
```

It finds the installation itself — no path is assumed. It reads metadata only: ATAS
need not be running, no ATAS code executes, and nothing but the report file is
written. See `tools/NFMarketDataRecorder.ApiProbe/README.md`.

Send `atas-api-report.md` back. Every row below is then corrected against measured
signatures instead of being rediscovered by compile errors.

### Step 2 — build against the real assemblies

```powershell
# Confirm Core and the tests are green first, so any failure from here on is
# unambiguously an ATAS-binding problem and not a regression.
dotnet test NFMarketDataRecorder.sln -c Release

# The probe's report names the exact directory to use here.
dotnet build src/NFMarketDataRecorder.ATAS/NFMarketDataRecorder.ATAS.csproj -c Release `
  -p:UseRealAtas=true `
  -p:AtasInstallDir="<directory reported by the probe>"
```

Each compile error maps to a numbered row above. Fix it **in the adapter only** —
if a fix seems to require changing `Core`, stop: that means logic is leaking into the
adapter, which is exactly what this layering prevents.

Then:

3. Copy `NFMarketDataRecorder.ATAS.dll` and `NFMarketDataRecorder.Core.dll` into the ATAS
   indicators folder (typically `%USERPROFILE%\Documents\ATAS\Indicators`).
4. Update the table above: change **UNVERIFIED** to the confirmed signature, with the
   ATAS version you verified against.
5. Run `docs/GUI-REPLAY-RUNBOOK.md`.

## 5 · Sanity checks once it compiles

Compiling is not the same as being correct. Before trusting a capture:

| Check | How | What it proves |
|---|---|---|
| Trades are individual, not aggregated | Replay one minute; compare the trade count against the platform's own tape for that minute | Assumption 1 |
| Depth changes are individual | Same, against the DOM | Assumption 2 |
| `src_ts` is the feed's clock | Replay a historical session and spot-check `src_ts` against known print times | Assumption 4 — **the one that invalidates everything if wrong** |
| Snapshot ladders are the right way up | Confirm the first bid in a snapshot is the highest and the first ask the lowest | Assumption 3 |
| Snapshots agree with the changes | Replay the change stream into your own book and diff it against the recorded snapshots | Both the depth path and the snapshot path at once — and this is the check that actually answers whether the change stream is complete |

The last row is the highest-value check in this document. It is the one that can detect
a silently missing depth update, because the snapshot comes from the platform's book
rather than from the changes.
