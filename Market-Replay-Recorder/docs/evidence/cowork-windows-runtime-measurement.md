# Windows runtime measurement — ATAS Classic and ATAS X

**Measured by:** the Cowork Windows execution worker, on the machine with both ATAS
products installed.
**Recorded here by:** the Claude Code lane, which has no Windows host and measured
none of this itself.
**Source tree measured:** `claude/atas-replay-event-verifier-xlgebm` @ `debb57c`,
unmodified. No source change was made by the measuring worker.
**Date:** 2026-09-12.

This file exists because the repository previously asserted that the installed ATAS
assemblies are .NETCoreApp 8.0. That was true when written and is no longer true.
The measurements below are what replaced it.

---

## 1 · Product runtimes

| | ATAS Classic | ATAS X |
|---|---|---|
| Runtime config | `OFT.Platform.runtimeconfig.json` | `OFT.PlatformX.runtimeconfig.json` |
| `tfm` | `net10.0` | `net10.0` |
| Frameworks | `Microsoft.NETCore.App 10.0.0`, `Microsoft.WindowsDesktop.App 10.0.0` | `Microsoft.NETCore.App 10.0.0`, `Microsoft.WindowsDesktop.App 10.0.0` |

## 2 · Reference assemblies

All eight report `.NETCoreApp,Version=v10.0`.

**ATAS Classic**

| Assembly | SHA-256 |
|---|---|
| `ATAS.Indicators.dll` | `f775d4e579fc20c2eb6f54848f6a7248927bb923ce6d1d2e8583ed25dc65f440` |
| `ATAS.DataFeedsCore.dll` | `992a4a25e5fcc8ac33ed406d010d3e636e76c8906d18ee048d1d8158db8c1309` |
| `Utils.Common.dll` | `126e147bd54a951d99852eaa42c6859ad59eee044630ff090ac8fcc16f58d1ec` |
| `OFT.Attributes.dll` | `1da03785bc0ba2d8e76435c0b16e8f6fb7121bb477974d0fd021c1b5422fe868` |

**ATAS X**

| Assembly | SHA-256 |
|---|---|
| `ATAS.Indicators.dll` | `759a2c90bab8d84348fd04df500a34bdfc72122ebc7f5f286be7718db0bd7a72` |
| `ATAS.DataFeedsCore.dll` | `9e723c307fef35bceb16d671bfe1188c81262468daa2e07cd879a0e9a475de65` |
| `Utils.Common.dll` | `9f71350ab75a4e4b0553e69b1ec074dcfa3e4c13c29caa66d0b6192b0c41f4bc` |
| `OFT.Attributes.dll` | `20e81401b3c40a1f1c2751cde4bb9c7cf0a236ffedc26916a06ef6de76d13167` |

The two products ship **different** assemblies under identical file names. They are
distinct reference sets and must be built against separately.

## 3 · The platform moved mid-validation

| When | Classic `ATAS.Indicators.dll` TFM | SHA-256 |
|---|---|---|
| 2026-09-12T01:35Z | `.NETCoreApp,Version=v8.0` | `cc721fb118c3b6cca94ae02ed4cf89f53c7076729d3ab1cfa8030b95f0952756` |
| 2026-09-12, later | `.NETCoreApp,Version=v10.0` | `f775d4e579fc20c2eb6f54848f6a7248927bb923ce6d1d2e8583ed25dc65f440` |

Both readings are accurate for their moment. **The earlier one is not an error and
is not being retracted** — it is the evidence that ATAS updates underneath an
installation, which is why every runtime reading must be re-taken rather than
inherited.

## 4 · Version labels are not runtime signals

| Product | `ATAS.Indicators` assembly identity | Actual TFM |
|---|---|---|
| Classic | `8.0.14.399` | `.NETCoreApp,Version=v10.0` |
| ATAS X | `8.0.15.643` | `.NETCoreApp,Version=v10.0` |

Both require `System.Runtime` 10.0 despite the `8.0.x` identity. Authoritative
signals are the runtimeconfig `tfm` and the assembly `TargetFrameworkAttribute`.

## 5 · Measured build results, unmodified source at `debb57c`

Windows toolchain: .NET SDK `10.0.400`, `Microsoft.NETCore.App.Ref 10.0.11`,
`Microsoft.WindowsDesktop.App.Ref 10.0.11`.

**Then-current default `net8.0-windows` — FAILED on both products**

```
CS1705: ATAS.Indicators uses System.Runtime, Version=10.0.0.0
        which is higher than referenced System.Runtime, Version=8.0.0.0
EXIT=1   (ATAS Classic)
EXIT=1   (ATAS X, same error class)
```

**`net10.0-windows` semantics, same reference set, `-warnaserror` — PASSED on both**

| Build | Exit | Warnings |
|---|---|---|
| Core | 0 | — |
| Adapter vs ATAS Classic | 0 | 0 |
| Adapter vs ATAS X | 0 | 0 |

## 6 · What this evidence does and does not establish

**Establishes.** `net10.0-windows` is COMPILE-VERIFIED against the real installed
assemblies of both products. This supersedes the prior classification
*STRUCTURALLY SUPPORTED — NOT COMPILE VERIFIED*.

**Does not establish.** Anything about runtime. Neither product has loaded the
recorder. No indicator has been installed, no chart has been attached, no Replay
has been run. Compiling against an assembly is not executing inside the host that
owns it.

Still `UNKNOWN` on both products: load success, `MarketDataArg.Time` semantics,
trade fidelity, depth fidelity, snapshot ordering, single-vs-batch callback
semantics, speed invariance.
