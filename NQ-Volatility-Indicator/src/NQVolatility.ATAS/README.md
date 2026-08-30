# NQVolatility.ATAS — NOT YET IMPLEMENTED

This directory is intentionally empty of code.

## Why

The brief (§19) requires: *"Before inventing APIs: inspect the existing ATAS
projects and locally available ATAS SDK/assemblies/documentation. **Verify rather
than guess.**"*

Neither is available in the environment this work was produced in:

* No ATAS SDK, assemblies, or documentation are present.
* No existing ATAS indicator project exists anywhere on the account to copy
  proven patterns from (verified: the GitHub account holds nine repositories,
  none of which is an ATAS repository).
* The ATAS assemblies are Windows-only and ship with the ATAS installation.

Writing `OnCalculate` / `OnRender` code against a remembered API surface would be
exactly the guesswork the brief prohibits, and it could not be compiled or
verified here. So it has deliberately not been written.

## What is ready for it

Everything platform-independent is done, tested, and lives in
`NQVolatility.Core`. The adapter's remaining job is mechanical translation:

| Adapter responsibility | Core already provides |
|---|---|
| Feed bars | `SessionEngine.OnBar(Ohlc, double? yesterdayAtr)` |
| Daily ATR acquisition | `DailyAtrSeries` — call `OnDailyBarClosed` per closed daily bar, `AdvanceChartBar()` per chart bar |
| Settings | `IndicatorSettings` (all 44 Pine inputs with exact defaults) |
| Timezone | hand `PineTime` a resolved `TimeZoneInfo` |
| What to draw | `SessionDrawModel.Build(session, settings)` → a flat list of `DrawPrimitive` |
| Info table contents | `SessionEngine.GetInfoTableData()` + `InfoTableData.Format` |

`DrawPrimitive` is deliberately reduced to three kinds — `Box`, `Line`, `Label` —
with boxes pre-normalised (`Y1 <= Y2`) and colours pre-resolved to straight RGBA.
The adapter should contain no calculation logic whatsoever: it maps
`DrawPrimitive` to `RenderRectangle` / `RenderLine` / `RenderString`, converts
UTC timestamps to bar indices, and nothing else.

## To implement it

1. On a Windows machine with ATAS installed, add a project referencing the ATAS
   assemblies (typically `ATAS.Indicators.dll`, `ATAS.Indicators.Technical.dll`,
   `Utils.Common.dll` from the ATAS install directory).
2. Reference `NQVolatility.Core` (targets `netstandard2.0`, so it loads in any
   ATAS runtime).
3. Implement the mapping table above.
4. Keep ATAS X portability in mind (§20): avoid WPF-only custom editors; if a
   feature needs a platform-specific implementation, isolate it behind an
   interface.

See `docs/PINE-PARITY-SPEC.md` §24 for the full Pine→ATAS mechanism map and §25
for the six behaviours that cannot be reproduced exactly.
