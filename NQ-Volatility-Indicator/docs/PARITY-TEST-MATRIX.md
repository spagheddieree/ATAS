# PARITY-TEST-MATRIX

Maps the 40 automated test requirements from the brief (§21) to the tests that
actually exist and pass, then records the TradingView↔ATAS acceptance items that
remain open.

**Automated status:** 97 tests, 97 passing, 0 failing, 0 skipped.
Evidence: `docs/evidence/test-results.txt`, `docs/evidence/build-and-test-run.txt`.

---

## A. Required cases 1–40

| # | Requirement | Test | Status |
|---|---|---|---|
| 1 | ATR scale lower clamp | `ProjectionMathTests.AtrScale_LowerClamp` | PASS |
| 2 | ATR scale normal / no clamp | `ProjectionMathTests.AtrScale_NoClamp` | PASS |
| 3 | ATR scale upper clamp | `ProjectionMathTests.AtrScale_UpperClamp` | PASS |
| 4 | First-session projection initialisation | `ProjectionMathTests.FirstSession_UsesOwnBaseAsPrevious` | PASS |
| 5 | +40 % daily clamp | `ProjectionMathTests.DailyClamp_UpperBoundBinds` | PASS |
| 6 | −40 % daily clamp | `ProjectionMathTests.DailyClamp_LowerBoundBinds` | PASS |
| 7 | Independent up/down distances | `ProjectionMathTests.UpAndDown_AreClampedIndependently` | PASS |
| 8 | 2.0 projection | `ProjectionMathTests.EveryMultiplier_ProjectsCorrectly(-2.0)` | PASS |
| 9 | 2.33 projection | `…(-2.33)` | PASS |
| 10 | 2.5 projection | `…(-2.5)` | PASS |
| 11 | 3.0 projection | `…(-3.0)` | PASS |
| 12 | 4.0 projection | `…(-4.0)` | PASS |
| 13 | 4.5 projection | `…(-4.5)` | PASS |
| 14 | 6.0 projection | `…(-6.0)` | PASS |
| 15 | 6.5 projection | `…(-6.5)` | PASS |
| 16 | 7.78 projection | `…(-7.78)` | PASS |
| 17 | 8.5 projection | `…(-8.5)` | PASS |
| 18 | Upward anchor from finalLow | `ProjectionMathTests.UpwardProjections_AnchorOnFinalLow` | PASS |
| 19 | Downward anchor from finalHigh | `ProjectionMathTests.DownwardProjections_AnchorOnFinalHigh` | PASS |
| 20 | Completely-outside-range zone rule | `RangeAndZoneTests.ZoneEligibility_UpwardRequiresBothBoundariesAboveHigh` | PASS |
| 21 | High/Low calculations | `RangeAndZoneTests.RangeLevels_HighAndLow` | PASS |
| 22 | 25 % level | `RangeAndZoneTests.RangeLevels_TwentyFivePercent` | PASS |
| 23 | EQ | `RangeAndZoneTests.RangeLevels_Equilibrium` | PASS |
| 24 | 75 % level | `RangeAndZoneTests.RangeLevels_SeventyFivePercent` | PASS |
| 25 | Q1/Q3 Pine label quirk | `RangeAndZoneTests.Quirk_Q1Q3_LabelsAreTransposed_InPineCompatibleMode` | PASS |
| 26 | 17:00–21:00 session | `SessionTimingTests.AsiaWindow_IsSeventeenToTwentyOne_Inclusive` | PASS |
| 27 | Generic cross-midnight session | `SessionTimingTests.CrossMidnightWindow_WrapsCorrectly` | PASS |
| 28 | Named timezone behaviour | `SessionTimingTests.NamedTimezone_ChangesWhichInstantsAreInRange` | PASS |
| 29 | DST transition | `SessionTimingTests.DstTransition_ShiftsTheSessionByOneHourInUtc` | PASS |
| 30 | Next-session calculation | `SessionTimingTests.NextSessionStart_SkipsWeekends` (5 cases) | PASS |
| 31 | Pine-compatible weekend behaviour | `SessionTimingTests.Quirk_IsMarketActive_IsATautology_WeekendsIncluded` | PASS |
| 32 | Historical lookback cleanup | `SessionEngineTests.Retention_NeverExceedsLookbackPeriod` | PASS |
| 33 | Deterministic recalculation | `SessionEngineTests.Recalculation_IsDeterministic` | PASS |
| 34 | No ATR lookahead | `AtrTests.NoLookahead_PublishedValueNeverIncludesTheOpenDailyBar` | PASS |
| 35 | Yesterday-completed-ATR semantics | `AtrTests.YesterdayAtr_IsThePreviousChartBarsPublishedValue` | PASS |
| 36 | Missing ATR behaviour | `AtrTests.MissingAtr_ProducesNaProjections_AndNoZones` | PASS |
| 37 | Locked high/low immutability | `SessionEngineTests.LockedSession_RejectsFurtherAccumulation` | PASS |
| 38 | Locked projection immutability | `SessionEngineTests.LockedSession_RejectsRelocking` | PASS |
| 39 | Zone eligibility above range | `RangeAndZoneTests.ZoneEligibility_UpwardRequiresBothBoundariesAboveHigh` | PASS |
| 40 | Zone eligibility below range | `RangeAndZoneTests.ZoneEligibility_DownwardRequiresBothBoundariesBelowLow` | PASS |

All 40 required behaviours are covered.

## B. Additional tests discovered during implementation

| Area | Test | Why it was added |
|---|---|---|
| Q-01 wedge | `SessionEngineTests.Quirk_FourHourChart_NeverLocks_AndWedgesTheIndicator` | The lock needs a bar in 21:01–23:59; on a 4 h chart none exists and the indicator stalls forever |
| Q-02 | `SessionEngineTests.Quirk_UpdateZonesAndLines_IsNeverReached` | Proves the extension code is unreachable, so "no extension" is the correct parity behaviour |
| Q-02 | `SessionEngineTests.Quirk_DrawnExtentIsFixedFromSessionEndToNextSessionStart` | Pins the fixed draw interval |
| Q-05 | `DrawModelTests.OpacityInput_IsAppliedAsTransparency` | "Opacity" inputs are inverted |
| Q-06 | `DrawModelTests.ShapeDiscardsInputAlpha_ButLabelKeepsIt` | Shapes and labels resolve colour differently |
| Q-07 | `ProjectionMathTests.Fib800_IsSevenPointSevenEight_NotEight` | The identifier lies; the value is 7.78 |
| Q-09 | `DrawModelTests.Boxes_AreNormalisedSoY1IsAlwaysTheLowerPrice` | Pine declares inverted boxes |
| Q-10 | `SessionEngineTests.Quirk_SentinelSeedsNeverLeak_OnTheShippedPath` | 0.0 / 999999.0 seeds must never reach output |
| D-01 | `ProjectionMathTests.Projections_AreNotRounded` | `roundPrice` is dead — no rounding |
| D-02 | `ProjectionMathTests.SkewRatio_IsComputed_NotTheDeadConstant` | `skewConst` is dead |
| D-05 | `SessionEngineTests.Quirk_CrossMidnightStartAdjustment_IsNeverReached` | Cross-midnight branches are dead on shipped presets |
| ATR | `AtrTests.YesterdayAtr_LagsByExactlyOneChartBarAcrossRollover` | `[1]` indexes chart bars, not daily bars |
| ATR | `AtrTests.WilderAtr_SeedsWithSmaThenSmooths` | RMA seed semantics |
| Retention | `SessionEngineTests.Retention_HonoursEveryAllowedLookback` (1,2,5,10) | Full input range |
| Lifecycle | `SessionEngineTests.Session_AccumulatesOnlyInRangeBars_AndLocksAfterTwentyOne` | Out-of-window bars must never be folded in |
| Weekend | `SessionEngineTests.WeekendSessions_AreCreated_BecauseIsMarketActiveIsATautology` | Saturday/Sunday sessions really are created |
| Draw | `DrawModelTests.SuppressedZones_ProduceNeitherBoxNorLabel` | Label suppression follows box suppression |
| Draw | `DrawModelTests.VisibilityToggles_RemoveTheirPrimitives` | Every visual toggle |

## C. Parity fixtures

Platform-independent, hand-derived from the spec (NOT generated from the
implementation), executed by `FixtureTests.Fixture_MatchesExpectedOutput`.

| Fixture | What it pins |
|---|---|
| `01-asia-baseline.json` | Full baseline: lock, levels, all 20 projected prices, mixed visibility (Level 1 / AVR- / AVR suppressed; AVR+ / MAX shown) |
| `02-friday-weekend-extension.json` | Friday session extends to Monday 17:00 |
| `03-dst-spring-forward.json` | Two sessions 23 h apart in UTC across the US spring-forward |
| `04-daily-clamp-asymmetric.json` | +40 % and −40 % clamps binding in opposite directions; all up zones suppressed, all down zones shown |
| `05-missing-atr.json` | na ATR ⇒ na distances, no zones, range levels still drawn |

These are the truth layer for the later TradingView↔ATAS comparison: the same
inputs can be reproduced on a TradingView chart and the expected columns checked
directly.

## D. TradingView ↔ ATAS acceptance — NOT STARTED

None of this can be produced in a headless Linux container. Every row requires
ATAS installed on Windows plus a TradingView chart.

| Item | Status |
|---|---|
| Asia start / end | NOT VERIFIED |
| finalHigh / finalLow | NOT VERIFIED |
| 25 % / EQ / 75 % | NOT VERIFIED |
| Yesterday ATR | NOT VERIFIED |
| Locked Up Proj / Down Proj | NOT VERIFIED |
| Level 1 up/down | NOT VERIFIED |
| AVR- up/down | NOT VERIFIED |
| AVR up/down | NOT VERIFIED |
| AVR+ up/down | NOT VERIFIED |
| MAX up/down | NOT VERIFIED |
| Visibility suppression when a zone overlaps the range | NOT VERIFIED |
| Next-session extension | NOT VERIFIED |
| Historical lookback | NOT VERIFIED |
| Labels | NOT VERIFIED |
| Visual toggles | NOT VERIFIED |
| DST case | NOT VERIFIED |

## E. Open questions requiring TradingView observation

| ID | Question | Phase 1 assumption |
|---|---|---|
| TV-OPEN-1 | Does Pine `math.max`/`math.min` propagate `na`? | Yes — na propagates, so a missing ATR yields no zones |
| TV-OPEN-2 | Does TradingView's "D" bar for NQ share ATAS's daily boundary? | Assumed yes; **primary ATR parity risk** |
| TV-OPEN-3 | Does the Q1/Q3 label swap render as read from source? | Yes |
| TV-OPEN-4 | Does the 4 h-chart wedge (Q-01) reproduce on TradingView? | Yes |
