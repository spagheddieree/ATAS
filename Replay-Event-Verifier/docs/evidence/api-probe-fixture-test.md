# API probe — fixture test

Captured 2026-09-12T00:45:24Z on Linux 6.18.44-fc-v24 x86_64, .NET 8.0.131.

## What this proves, and what it does not

**Proves:** `atas-api-probe` reads assembly metadata faithfully, reports the
members it is asked about, and — critically — surfaces members that the adapter
does NOT assume.

**Does not prove:** anything whatsoever about ATAS. No ATAS assembly was
available. Only running the probe against a real installation can verify the API.

## Fixture

Compiled from the adapter's own written-down assumptions
(`src/ReplayEventVerifier.ATAS/ApiStub/AtasApiAssumptions.cs`) emitted as
`ATAS.Indicators.dll`, PLUS scratch-only members the adapter never assumes, to
test the discovery sweep:

```csharp
// deliberately unassumed - aggregated and whole-book shapes
protected virtual void OnNewTicks(IEnumerable<MarketDataArg> ticks) { }
protected virtual void OnMarketDepthsChanged(IEnumerable<MarketDepthRow> book) { }
public event EventHandler<MarketDataArg> BestBidAskChanged;
```

## Command

```
dotnet run -c Release --project tools/ReplayEventVerifier.ApiProbe -- \
  --dir <fixture dir> --out report.md
```

## Result: named lookup (§1) resolved every assumed member

```
- `protected virtual Void OnNewTrade(MarketDataArg trade)`
- `protected virtual Void MarketDepthChanged(MarketDataArg depth)`
- `protected abstract Void OnCalculate(Int32 bar, Decimal value)`
- `protected virtual Void OnInitialize()`
- `protected virtual Void OnDispose()`
- `protected Void SubscribeToDrawingEvents(DrawingLayouts layouts)`
- `protected Boolean EnableCustomDrawing { get; set; }`
- `protected Boolean DenyToChangePanel { get; set; }`
- `public IMarketDepthInfoProvider MarketDepthInfo { get; set; }`
- `public InstrumentInfoData InstrumentInfo { get; set; }`
```

## Result: contract classification (§2) correctly reported an ABSENT field

| Recorder needs | Candidate member(s) found | Verdict |
|---|---|---|
| source timestamp | `Time` | present |
| price | `Price` | present |
| volume | `Volume` | present |
| aggressor / direction | `DataType`, `Direction` | present |
| source sequence / exchange id | — | **NOT FOUND** |

`source sequence` is **NOT FOUND** in the fixture, and the probe says so rather
than inventing one. That is the behaviour required by the rule that unavailable
source information is never inferred.

## Result: keyword sweep (§7) surfaced the UNASSUMED members

This is the test that matters. The sweep found both members the adapter has no
knowledge of, which is how a wrong assumption gets caught rather than confirmed:

```
TYPE ATAS.Indicators.AggregatedFeedSurface
    event EventHandler<MarketDataArg> BestBidAskChanged
    protected Void RaiseBestBidAsk(MarketDataArg a)
    protected virtual Void OnMarketDepthsChanged(IEnumerable<MarketDepthRow> book)
    protected virtual Void OnNewTicks(IEnumerable<MarketDataArg> ticks)
```

Both aggregated shapes were reported. If the real ATAS exposes only shapes like
these, that is a material finding about whether the research objective is
achievable at all — not a defect to be hidden behind the adapter.
