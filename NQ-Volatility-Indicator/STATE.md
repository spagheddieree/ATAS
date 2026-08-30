# STATE — NQ Volatility Indicator

Authoritative resumability record. Update on every work session.

**Last updated:** 2026-08-30 (migrated to spagheddieree/ATAS)

---

## 1. Environment (as actually found)

The brief assumed a Windows machine with a post-migration Projects workspace and
an existing `ATAS` repository. Neither existed. Verified findings:

| Item | Finding |
|---|---|
| Host | Ephemeral **Linux** container (`Linux 6.18.44-fc-v22 x86_64`), Claude Code on the web |
| Projects root | **Does not exist.** No Windows filesystem, no `G:\`, nothing to discover |
| ATAS repository | **Did not exist** at the start of this work. Owner created `spagheddieree/ATAS` mid-session; it contained only an initial commit and a two-line README |
| Existing ATAS projects | **None.** No prior indicator, no established architecture or packaging convention to follow, so the layout below was chosen fresh (brief §18) |
| Git topology | Two independent repositories, no nesting, no submodules: `Neverflat-OS` (staging origin) and `ATAS` (final home) |
| Toolchain | `dotnet` was absent; installed `dotnet-sdk-8.0` (8.0.130) via apt. Microsoft's CDN (`builds.dotnet.microsoft.com`) is blocked by network policy; the Ubuntu archive is not |
| ATAS SDK | **Not available.** Windows-only, ships with the ATAS installation |

## 2. Git

**Final home — this repository.**

| Item | Value |
|---|---|
| Repository | `spagheddieree/ATAS` |
| Path | `ATAS/NQ-Volatility-Indicator/` |
| Branch | `claude/nq-volatility-atas-conversion-ifuil9` |
| Base commit | `37bb54d` ("Initial commit") |

### Provenance

The work was first produced in `spagheddieree/Neverflat-OS` on branch
`claude/nq-volatility-atas-conversion-ifuil9`, because no ATAS repository was
reachable at the time and the container is ephemeral. Once the Owner created
this repository the five commits were replayed here with `git format-patch` /
`git am`, so authorship, dates and messages are intact. The staging copy was
then removed from Neverflat-OS so no competing copy can drift.

| Item | Value |
|---|---|
| Neverflat-OS starting HEAD | `99f4736e73182720099ec158e91b0555079789c0` |
| Pre-existing changes there | None — tree was clean, 0 ahead / 0 behind `main` |
| Neverflat-OS files modified | None outside the staged directory, which was later removed |

## 3. Source of truth

| Item | Value |
|---|---|
| Pine source | `pine/NQ-Volatility-Range.pine` (vendored) |
| SHA-256 | `d15c58c6781ef1defd9f0fce913598a233e75f5ee3f585a759601e8700cc0525` |
| Lines | 977 |
| Read in full | Yes |

## 4. Build state

| Item | State |
|---|---|
| `NQVolatility.Core` | Builds clean, 0 warnings (`TreatWarningsAsErrors`), netstandard2.0 |
| `NQVolatility.Tests` | Builds clean, net8.0 |
| `NQVolatility.ATAS` | **Not implemented** — no SDK to compile against |
| Command | `dotnet test NQVolatility.sln` |

## 5. Test state

**97 tests, 97 passing, 0 failing, 0 skipped.** Actually executed; output captured
in `docs/evidence/`.

All 40 required cases from the brief §21 are covered — see
`docs/PARITY-TEST-MATRIX.md` §A. Five hand-derived parity fixtures pass.

## 6. Installation state

**Not attempted.** No ATAS adapter exists yet, therefore no DLLs to install, no
hashes to report, and no existing NeverFlat/ATAS DLLs at risk. ATAS was never
running and nothing was replaced.

## 7. GUI acceptance state

**Not started.** Requires Windows + ATAS.

## 8. Parity state

**Not started.** No TradingView↔ATAS comparison is possible from a headless Linux
container. Four open questions are recorded as TV-OPEN-1..4 in
`docs/PINE-PARITY-SPEC.md` §28; TV-OPEN-2 (daily-bar boundary agreement) is the
main ATR parity risk.

## 9. Blockers

1. **No ATAS SDK/assemblies and no existing ATAS project to pattern-match.**
   Blocks `NQVolatility.ATAS`. The brief (§19) requires verifying the API rather
   than guessing, so the adapter was deliberately not written.
2. **No Windows host with ATAS.** Blocks install, GUI acceptance, and all
   TradingView parity evidence.

RESOLVED: the ATAS repository did not exist and could not be created; the Owner
created `spagheddieree/ATAS` and the project now lives there.

None of these block further Core work.

## 10. Next action

On a Windows machine with ATAS installed:

1. Clone this repository; the project is already at `NQ-Volatility-Indicator/`.
2. Add `src/NQVolatility.ATAS`, referencing the ATAS assemblies and
   `NQVolatility.Core`, implementing only the mapping table in
   `src/NQVolatility.ATAS/README.md`. Write no calculation logic there.
3. Build, install with ATAS closed, verify discovery and rendering.
4. Work `docs/PARITY-TEST-MATRIX.md` §D against a TradingView chart.
