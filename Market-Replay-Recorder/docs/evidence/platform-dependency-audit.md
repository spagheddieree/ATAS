# Platform dependency audit — ATAS Classic / ATAS X compatibility

Captured 2026-09-12. Evidence for the claim that one assembly can serve both
products.

## Why this matters

ATAS X performs automatic conversion for many ordinary Windows-specific visual
types, but **custom WPF editors and unsupported WPF UI constructs can prevent
cross-platform loading**. So the question is not "does it use WPF at all" — the
platform handles ordinary cases — but "does it contain the specific constructs that
block conversion".

## Result: none present

Searched across `src/` and `tests/`, excluding the API stub:

| Construct | Occurrences | Assessment |
|---|---|---|
| `System.Windows` | 0 | — |
| `DataTemplateSelector` | 0 | — |
| XAML / `.xaml` files | 0 | — |
| `System.Xaml` | 1 | **not a dependency** — a string literal in the probe's WPF-assembly resolver list |
| `UserControl` | 0 | — |
| `Avalonia` | 0 | — |
| `Dispatcher` | 0 | — |
| `EditorAttribute` / custom editor | 0 | — |
| `Window` | 18 | **not a dependency** — all in comments/strings (`Windows`, `WindowsDesktop`, "on Windows") |

The adapter imports **no** UI namespace. Verified directly:

```
grep -nE "using (System\.Windows|System\.Xaml|Avalonia)" MarketReplayRecorderIndicator.cs
  -> no matches
```

## Why the recorder is UI-light by construction

It was never a drawing indicator. From the constructor:

```csharp
EnableCustomDrawing = false;
SubscribeToDrawingEvents(DrawingLayouts.None);
```

It does not override `OnRender`, declares no custom property editor, and produces no
visual output. Its only outputs are files. That is a consequence of the original
design goal — capture raw events without interpreting or displaying them — and it
happens to be exactly what makes one assembly plausible on both products.

## Settings surface

Settings use `[Display]` and `[Range]` from
`System.ComponentModel.DataAnnotations`, which under .NET 8 come from the shared
framework rather than a WPF assembly. No custom editor, no template selector, no
platform-specific control.

## Build configuration

- **Platform:** SDK default (**AnyCPU**). No `PlatformTarget`, no
  `RuntimeIdentifier`, no `Platforms` — confirmed by grep. Nothing here is bitness
  sensitive, and AnyCPU matches ATAS guidance for ordinary custom indicators.
- **`UseWPF`** is enabled in real mode, but only so the compiler can resolve WPF
  types appearing in `ATAS.Indicators.Indicator`'s own member signatures
  (`OnRender`, `ProcessKeyDown`, `GetCursor`). The recorder uses none of them.

## Honest limitation

A `-windows` TFM produces a **Windows-only** assembly. That is correct for ATAS X
**on Windows**, which is what is being validated. ATAS X on Linux or macOS would
need a non-Windows TFM with `UseWPF` off, and whether the ATAS reference assemblies
permit that is **unverified** — no non-Windows ATAS reference set was available.

## Status

**Architecturally compatible with both products** — no construct known to block
ATAS X loading is present.

This is a code-surface audit, **not** a runtime result. Neither product has loaded
the assembly yet. Runtime support must be observed per product and reported
separately.
