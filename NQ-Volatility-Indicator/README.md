# NQ Volatility Indicator

An ATAS port of the TradingView Pine Script v6 indicator **"NQ Volatility Range"**,
built for deterministic behavioural parity rather than visual resemblance.

## Status

| Layer | State |
|---|---|
| Pine source analysis | **Complete** — `docs/PINE-PARITY-SPEC.md` |
| `NQVolatility.Core` (all calculation + draw modelling) | **Complete**, 97 tests passing |
| Parity fixtures | **Complete** — 5 fixtures, hand-derived |
| `NQVolatility.ATAS` (adapter) | **Blocked** — no ATAS SDK available; see `src/NQVolatility.ATAS/README.md` |
| ATAS install / GUI acceptance | **Not started** — requires Windows + ATAS |
| TradingView parity evidence | **Not started** — requires both platforms |

This is **IMPLEMENTATION IN PROGRESS**, not FULL ACCEPTANCE COMPLETE.

## Layout

```
ATAS/
└── NQ-Volatility-Indicator/
├── NQVolatility.sln
├── pine/NQ-Volatility-Range.pine      # vendored canonical source
├── src/NQVolatility.Core/             # netstandard2.0, zero dependencies
├── src/NQVolatility.ATAS/             # adapter — not yet implemented (see its README)
├── tests/NQVolatility.Tests/          # net8.0, xunit
├── fixtures/                          # platform-independent parity fixtures
└── docs/
    ├── PINE-PARITY-SPEC.md            # authoritative behavioural spec
    ├── PARITY-TEST-MATRIX.md          # requirement → test mapping
    └── evidence/                      # captured build/test output
```

## Architecture

`Core` owns every decision; the adapter owns none.

* **`SessionEngine`** — the bar-by-bar state machine, mirroring the Pine main
  logic in source order. Feed it `OnBar(Ohlc, double? yesterdayAtr)`.
* **`DailyAtrSeries`** — reproduces `request.security("D", ta.atr(n), lookahead_off)`
  followed by `[1]`, including the fact that `[1]` indexes *chart* bars.
* **`AutoProjection`** — the ATR→distance algorithm with both clamps.
* **`RangeLevels` / `ProjectionLevels` / `ZoneEligibility`** — pure functions.
* **`SessionDrawModel`** — turns a locked session into a flat list of
  `DrawPrimitive` (Box / Line / Label) with colours already resolved and boxes
  already normalised. The adapter renders these and does nothing else.

`Core` targets `netstandard2.0` and has no package references, so it loads under
any ATAS runtime and is fully testable without ATAS installed.

## Build and test

```bash
dotnet test NQVolatility.sln
```

Requires the .NET SDK (developed against 8.0.130). No ATAS assemblies needed.

## Phase discipline

Phase 1 is **exact Pine parity** — including the source's bugs. Nineteen quirks
and dead-code findings are catalogued in `docs/PINE-PARITY-SPEC.md` §22, among
them:

* the lock can never fire on chart timeframes with no bar in 21:01–23:59
  (the indicator wedges permanently);
* zones never extend — Pine's `updateZonesAndLines` is unreachable;
* the Q1/Q3 labels are transposed;
* `isMarketActive()` is a tautology, so there is no weekday filter;
* the "Opacity" inputs are really transparency;
* `roundPrice` is dead, so **no rounding occurs**.

None of these are fixed here. Eleven Phase 2 candidates are listed in §27 and
must not alter Phase 1 behaviour without Owner approval.
