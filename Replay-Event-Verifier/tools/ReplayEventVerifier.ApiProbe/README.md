# atas-api-probe

Reads the **real ATAS assemblies** and reports what the API actually is.

This exists because the recorder's ATAS adapter was written in an environment with
no ATAS SDK, so its API surface is assumed rather than known. Running this probe on
a machine with ATAS installed replaces every assumption with a measured fact.

## Safety

It reads **metadata only**, via `System.Reflection.MetadataLoadContext`:

- It does not start ATAS, and ATAS need not be running.
- It does not load or execute any ATAS code — no static initialisers run.
- It does not read or modify charts, settings, workspaces, licences or market data.
- The only thing it writes is the report file you name.

## Run it

```powershell
cd Replay-Event-Verifier
dotnet run -c Release --project tools/ReplayEventVerifier.ApiProbe
```

It searches for the installation itself. To see what it found without generating a
report:

```powershell
dotnet run -c Release --project tools/ReplayEventVerifier.ApiProbe -- --list
```

To point it at a specific directory (the one containing `ATAS.Indicators.dll`):

```powershell
dotnet run -c Release --project tools/ReplayEventVerifier.ApiProbe -- `
  --dir "<path to ATAS install>" --out atas-api-report.md
```

Exit codes: `0` report written · `2` no installation found or no metadata readable ·
`1` unexpected error.

## What the report contains

| § | Content | Answers |
|---|---|---|
| 0 | Assemblies loaded, with type counts | Which ATAS assemblies exist and are readable |
| 1 | `Indicator` base type, inheritance chain, the members the adapter uses, and **every** virtual/abstract member | Lifecycle and callback assumptions (checklist rows 1, 2, 8–10, 13) |
| 2 | `MarketDataArg` properties and fields with exact CLR types, classified against the recorder contract | Timestamp, price, volume, direction, and whether a **source sequence** exists at all (rows 4–7) |
| 3 | `MarketDataType`, `TradeDirection` and any other Bid/Ask/Trade enum, with underlying values | Book side and aggressor mapping (rows 5, 6) |
| 4 | Every depth/book/level type and member | The snapshot API and its shape (row 3) |
| 5 | Instrument/security types | Instrument metadata (row 11) |
| 6 | Attribute types ATAS ships | Whether the settings UI needs ATAS attributes rather than framework ones (row 12) |
| 7 | **Keyword sweep** across all trade/depth/tick members | The correct entry point, *even if the adapter assumed the wrong name* |

§7 is the one that matters most. A named lookup can only confirm or deny a guess;
the sweep shows what is actually there. It is also what surfaces the two findings
that would decide the research objective outright:

- a trade callback taking a **collection** rather than a single event → trades are
  aggregated, not individual;
- a depth callback taking a **whole book** rather than one level → depth changes are
  not individually observable.

Either of those is a material finding about ATAS Replay, not a bug to work around.

## Why it is not in `ReplayEventVerifier.sln`

It is a development tool, never shipped into the ATAS process, and it carries the
only NuGet dependency in the repository outside tests
(`System.Reflection.MetadataLoadContext`). Keeping it out of the solution preserves
the rule that `Core` has zero package references and that everything in the solution
is part of the deliverable. Build it explicitly with the command above.

## How it was tested without ATAS

The probe was verified against a fixture assembly compiled from
`src/ReplayEventVerifier.ATAS/ApiStub/AtasApiAssumptions.cs` — the adapter's written
down assumptions — emitted as `ATAS.Indicators.dll`, plus deliberately-unassumed
members (an aggregated `OnNewTicks(IEnumerable<MarketDataArg>)` and a whole-book
`OnMarketDepthsChanged(IEnumerable<MarketDepthRow>)`).

The probe correctly reported the assumed surface **and** surfaced both unassumed
members in the §7 sweep, which is the behaviour that makes it useful. Evidence:
`docs/evidence/api-probe-fixture-test.md`.

That test proves the probe reports metadata faithfully. It proves nothing about
ATAS — only running it against the real installation can do that.
