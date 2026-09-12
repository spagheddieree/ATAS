# ATAS API — measured binding

**Status: the API surface is MEASURED. Runtime semantics are not.**

This supersedes the assumption checklist that preceded it. Every signature below was
read from the real ATAS assemblies; what remains open is what the values *mean* while
ATAS is running, which no amount of metadata can settle.

---

## 1 · Evidence

| | |
|---|---|
| Source | `C:\Program Files (x86)\ATAS Platform`, 238 DLLs, copied byte-for-byte |
| Verification | SHA-256 matched on both sides for the load-bearing assemblies |
| Method | `MetadataLoadContext` — metadata only; ATAS never started, no ATAS code executed |
| Report | `docs/evidence/atas-api-report.md` (2,122 lines) |
| Provenance | `docs/evidence/atas-api-report-PROVENANCE.md` |

```
ATAS.Indicators.dll     cc721fb118c3b6cca94ae02ed4cf89f53c7076729d3ab1cfa8030b95f0952756
ATAS.DataFeedsCore.dll  b598f2987437dcdc423acfe03237a01972f6c6891e0174e385b267d369b05f3f
ATAS.Types.dll          355bac332cd0f00f28093972cc0743077c73aca6419176dd2b0f39c02319be76
OFT.Core.dll            479ce76138ebde434db9a9adccab5dc0f004c118b3d7c92b25d85cb0fbd698aa
Utils.Common.dll        97a9590f03989769a16d5a02b55d5b1409c3e191710b0985da5db0d063860dc1
```

**Two limits on this evidence, both material:**

1. The probe ran on **Linux**, not Windows. Sound for metadata — `MetadataLoadContext`
   reads PE/CLI tables and never executes the target — but it is not platform-native
   verification, and nothing depending on Windows runtime behaviour is covered.
2. Metadata establishes **existence and shape only**. `MarketDataArg.Time` is provably
   a `DateTime`; whether it carries exchange, replay or arrival time is invisible here
   and decides whether any timing analysis on the dataset is valid.

## 2 · What the measurement corrected

Six assumptions were wrong. None would have been caught without this.

| # | Assumed | **Measured** |
|---|---|---|
| 1 | `MarketDataType` in `ATAS.Indicators`, `Trade=0,Bid=1,Ask=2` | **`ATAS.DataFeedsCore`**, `Bid=0, Ask=1, Trade=2` |
| 2 | `TradeDirection` in `ATAS.Indicators` | **`ATAS.DataFeedsCore`** (values were right) |
| 3 | `MarketDepthInfo.GetMarketDepth(side)` → per-side rows | **`GetMarketDepthSnapshot()` → flat `IEnumerable<MarketDataArg>`**; no `MarketDepthRow` type exists |
| 4 | `MarketDepthInfo` public get/set | **`protected` get-only `IMarketDepthInfoProvider`** |
| 5 | `InstrumentInfo` a class with `Instrument` | **`protected` get-only `IInstrumentInfo`** with `Instrument`, `Exchange`, `TickSize`, `TimeZone` |
| 6 | `EnableCustomDrawing` / `DenyToChangePanel` protected | **public** |

Confirmed correct: `OnNewTrade(MarketDataArg)`, `MarketDepthChanged(MarketDataArg)`,
`OnCalculate(int, decimal)`, `OnInitialize()`, `OnDispose()`,
`SubscribeToDrawingEvents(DrawingLayouts)`, and `MarketDataArg`'s `Time` / `Price` /
`Volume` / `DataType` / `Direction`.

## 3 · The measured surface

```csharp
// ATAS.Indicators.Indicator
//   -> ExtendedIndicator -> BaseIndicator -> ChartObject -> Filters.TrackedPropertyBase

protected virtual void OnNewTrade(MarketDataArg trade);
protected virtual void OnNewTrades(IEnumerable<MarketDataArg> trades);
protected virtual void MarketDepthChanged(MarketDataArg depth);
protected virtual void MarketDepthsChanged(IEnumerable<MarketDataArg> depths);
protected virtual void OnBestBidAskChanged(MarketDataArg depth);
protected virtual void OnMarketByOrdersChanged(IEnumerable<MarketByOrder> values);

protected abstract void OnCalculate(int bar, decimal value);
protected virtual  void OnInitialize();
protected virtual  void OnDispose();
public    virtual  void Dispose();

public    bool EnableCustomDrawing { get; set; }
public    bool DenyToChangePanel   { get; set; }
protected IMarketDepthInfoProvider MarketDepthInfo { get; }   // get-only
protected IInstrumentInfo          InstrumentInfo  { get; }   // get-only

// ATAS.Indicators.MarketDataArg  — the payload AND the snapshot row type
bool      IsAsk { get; }               bool      IsBid { get; }
DateTime  Time  { get; set; }          decimal   Price { get; set; }
decimal   Volume { get; set; }         decimal   OriginPrice { get; set; }
decimal   OpenInterest { get; set; }
MarketDataType DataType { get; set; }  TradeDirection Direction { get; set; }
long?     ExchangeOrderId { get; set; }
long?     AggressorExchangeOrderId { get; set; }
// NO sequence member of any kind.

// ATAS.Indicators.IMarketDepthInfoProvider
IEnumerable<MarketDataArg> GetMarketDepthSnapshot();   // FLAT, not per-side
decimal CumulativeDomBids { get; }  decimal CumulativeDomAsks { get; }

// ATAS.Indicators.IInstrumentInfo
string Instrument { get; }  string Exchange { get; }
decimal TickSize { get; }   int TimeZone { get; }

// ATAS.DataFeedsCore
enum MarketDataType  { Bid = 0, Ask = 1, Trade = 2 }
enum TradeDirection  { Between = 0, Buy = 1, Sell = 2 }
```

## 4 · Three findings that constrain the design

### 4.1 · `source_sequence` is measured ABSENT, not unknown

`MarketDataArg` carries no sequence member. The **only** `Sequence` property in the
entire 2,122-line report belongs to `OFT.Phemex.WsMessages.Pushes.WsDepthPush` — a
crypto-exchange websocket message type not reachable from the indicator API.

`ExchangeOrderId` and `AggressorExchangeOrderId` are **order identifiers**. Mapping
either to `source_sequence` would fabricate feed ordering that was never observed, so
the register records both as available *order ids* and `source_sequence` as
`UNAVAILABLE`. Enforced by
`ProvenanceContractTests.Source_sequence_is_measured_absent_and_never_taken_from_an_order_id`.

Consequence: **gap detection by sequence number is impossible through this API.** Loss
can only be inferred by reconciling the depth-change stream against independent
snapshots — which is exactly why the snapshots exist.

### 4.2 · Snapshot row ordering is unspecified

`GetMarketDepthSnapshot()` returns a flat sequence. Metadata says nothing about
whether rows arrive best-first, bids-descending, or grouped by side at all.

The adapter therefore **records rows in the platform's own order and never sorts
them**. Sorting would impose an ordering not shown to exist and would destroy evidence
of what order the platform actually returned — one of the things the runtime
experiment has to determine.

It also makes depth truncation unsafe: "top N" assumes best-first, so the default
depth limit is **0 (record everything)** until ordering is established.

### 4.3 · Both single and batch callbacks exist

`OnNewTrade`/`OnNewTrades` and `MarketDepthChanged`/`MarketDepthsChanged` all exist.
Metadata cannot say whether both fire for the same underlying event.

- Binding both would **double-count** every event if they do.
- Binding only the batch surface would **lose per-event granularity** if they do not.

So capture binds the **single** callbacks only, and the batch overrides call `base`
then merely **count**, writing `atas-callback-diagnostics.json` at shutdown. The
runtime experiment reads those counts and settles the question by measurement rather
than the adapter guessing. See `GUI-REPLAY-RUNBOOK.md` §9D.

## 4.4 · Two defects the metadata report did not prevent

The first compile against the real assemblies failed twice over. Both are worth
recording because both were *findable* in the evidence and were missed.

### Duplicate enum declarations — CS0104

ATAS declares **two** independent public enums named `MarketDataType` and **two**
named `TradeDirection`: one pair in `ATAS.DataFeedsCore`, one in `ATAS.Indicators`.
Not type forwards — separate declarations. Importing both namespaces and using the
bare name is ambiguous, and the adapter did exactly that at six call sites.

`ATAS.Indicators.MarketDataArg` types its `DataType` and `Direction` properties as
the **`ATAS.Indicators`** pair, so that is the family to bind to. The members are
numerically identical across both families (`Bid=0/Ask=1/Trade=2`,
`Between=0/Buy=1/Sell=2`), which is precisely why this hid: the wrong binding would
have *behaved* correctly. Only the type identity differs, so only a compiler could
catch it.

**Why the report missed it.** `Signatures.TypeName()` printed `t.Name`, so §2 read
`public MarketDataType DataType` — a name that cannot say which of two declarations
it means. The collision was visible elsewhere in the same report (line 187 and line
205 both list a `MarketDataType`), but nothing connected them.

Fixed in three places, so the same class of defect cannot recur silently:

1. **Probe** — a new §0.2 enumerates every simple name declared by more than one
   assembly, and any such name is printed namespace-qualified everywhere else in
   the report.
2. **Stub** — both enum pairs are now declared, so unqualified use is CS0104 in the
   *local* build too. Verified by deliberately reintroducing the defect on a scratch
   copy and confirming the identical CS0104 pair.
3. **Adapter** — binds through explicit aliases; `ATAS.DataFeedsCore` is no longer
   imported, since nothing here needs a type from it.

### Wrong real-mode target framework

The project targeted `net472` in real mode. The installed ATAS assemblies are
**`.NETCoreApp,Version=v8.0`**, so that build could not legally consume them:
CS0012 (`System.Runtime 8.0.0.0` missing), CS0115 (no suitable method to override),
CS0534 (abstract member unresolved).

Real mode now targets **`net8.0-windows`** with `UseWPF`. The Windows-specific TFM
is chosen on a measured reference requirement, not because ATAS is a Windows app:
resolving `ATAS.Indicators` metadata requires `PresentationCore`,
`PresentationFramework`, `WindowsBase` and `System.Xaml` — the same requirement that
broke the API probe — and `Indicator` carries WPF types in its own member
signatures while this adapter derives from it. It is a deliberate superset: a
missing WPF reference fails the build, an unnecessary one does not. If a real
Windows build shows plain `net8.0` resolves everything, it can be narrowed.

The explicit `System.ComponentModel.DataAnnotations` reference is gone — it existed
only because of `net472`; under .NET 8 those attributes are in the shared framework.

`Core` stays on `netstandard2.0`, which .NET 8 consumes fine. Only its stale comment
calling ATAS ".NET Framework based" was corrected.

## 5 · What is still UNKNOWN

| Question | Why it matters | Settled by |
|---|---|---|
| What `MarketDataArg.Time` means | If it is an arrival clock, **no timing analysis on this dataset is valid** | Runbook §9C |
| Whether Replay fires `OnNewTrade` per historical print | Aggregation would end the investigation | §9A |
| Whether depth arrives incrementally, and how removals appear | Determines whether a book can be reconstructed | §9B |
| Snapshot ladder ordering | Blocks safe depth truncation and any level index | §9B |
| Single vs batch dispatch relationship | Decides the correct long-term binding | §9D |
| `OriginPrice` vs `Price` | Both captured; picking the wrong one would corrupt every price | §9A |

## 6 · Re-verifying natively

The delivered report came from copied assemblies on Linux. A native re-run confirms
the repaired resolver on the real machine:

```powershell
dotnet run -c Release --project tools\NFMarketReplayRecorder.ApiProbe
```

Section **0 · Resolver** must show a real `WindowsDesktop framework:` path and
`WPF assemblies: all present`. See `WINDOWS-INSTALL.md` §3.
