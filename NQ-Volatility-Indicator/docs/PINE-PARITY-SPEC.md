# PINE-PARITY-SPEC — "NQ Volatility Range"

**Status:** Phase 1 (behavioural parity). Authoritative.
**Canonical source:** `pine/NQ-Volatility-Range.pine`
**Source SHA-256:** `d15c58c6781ef1defd9f0fce913598a233e75f5ee3f585a759601e8700cc0525`
(CRLF, exactly as supplied; `pine/.gitattributes` marks `*.pine` as `-text` so the
checked-out file verifies against this hash rather than being normalised to LF)
**Source length:** 977 lines (976 newline-terminated + final line)
**Pine version:** v6, `overlay=true`, `max_boxes_count=500`, `max_lines_count=500`, `max_labels_count=500`

Every claim below cites the Pine line(s) it derives from. Where behaviour is
ambiguous or cannot be settled from the source alone, it is recorded as an
**[OPEN]** item rather than silently resolved — per the Phase 1 mandate.

---

## 1. Exposed inputs, defaults, ranges

| # | Pine identifier | Line | Control | Default | Range / options |
|---|---|---|---|---|---|
| 1 | `timeZone` | 7 | string | `America/Los_Angeles` | UTC, America/New_York, America/Chicago, America/Los_Angeles, Europe/London, Europe/Paris, Europe/Berlin, Asia/Tokyo, Asia/Shanghai, Asia/Singapore, Australia/Sydney |
| 2 | `atrLength` | 24 | int | `20` | minval 1, no maxval |
| 3 | `showRangeBox` | 50 | bool | `true` | — |
| 4 | `rangeBoxColor` | 51 | color | `#ffffff` @ 85 transp | — |
| 5 | `rangeBoxOpacity` | 52 | int | `85` | 0–100 |
| 6 | `showHighLowLines` | 56 | bool | `true` | — |
| 7 | `showQuarterLines` | 57 | bool | `true` | — |
| 8 | `showEqLine` | 58 | bool | `true` | — |
| 9 | `highLowLineColor` | 61 | color | `#e5ff00` @ 50 transp | — |
| 10 | `highLowLineOpacity` | 62 | int | `0` | 0–100 |
| 11 | `quarterLineColor` | 65 | color | `#ff0000` @ 50 transp | — |
| 12 | `quarterLineOpacity` | 66 | int | `0` | 0–100 |
| 13 | `eqLineColor` | 69 | color | `#ff0000` @ 50 transp | — |
| 14 | `eqLineOpacity` | 70 | int | `0` | 0–100 |
| 15 | `showRangeLevelLabels` | 73 | bool | `true` | — |
| 16 | `rangeLevelLabelSize` | 74 | string | `small` | tiny, small, normal, large, huge |
| 17 | `show233_250Zone` (Level 1) | 80 | bool | `true` | — |
| 18 | `zone1Color` | 81 | color | `#ffffff` @ 80 | — |
| 19 | `zone1Opacity` | 82 | int | `80` | 0–100 |
| 20 | `zone1InnerColor` | 83 | color | `#f23645` @ 80 | — |
| 21 | `zone1InnerOpacity` | 84 | int | `80` | 0–100 |
| 22 | `show400_450Zone` (AVR) | 87 | bool | `true` | — |
| 23 | `zone2Color` | 88 | color | `#ff7b00` @ 80 | — |
| 24 | `zone2Opacity` | 89 | int | `80` | 0–100 |
| 25 | `show600_650Zone` (AVR+) | 92 | bool | `true` | — |
| 26 | `zone3Color` | 93 | color | `#b90000` @ 80 | — |
| 27 | `zone3Opacity` | 94 | int | `80` | 0–100 |
| 28 | `show800_850Zone` (MAX) | 97 | bool | `true` | — |
| 29 | `zone4Color` | 98 | color | `#7a0000` @ 80 | — |
| 30 | `zone4Opacity` | 99 | int | `80` | 0–100 |
| 31 | `show300Line` (AVR-) | 102 | bool | `true` | — |
| 32 | `line300Color` | 103 | color | `#ffffff` @ 50 | — |
| 33 | `line300Opacity` | 104 | int | `0` | 0–100 |
| 34 | `line300Width` | 105 | int | `1` | 1–4 |
| 35 | `line300Style` | 106 | string | `Dotted` | Solid, Dotted, Dashed, Arrow Left, Arrow Right, Arrow Both |
| 36 | `showZoneLabels` | 110 | bool | `true` | — |
| 37 | `zone1LabelColor` | 111 | color | `color.white` | — |
| 38 | `zone2LabelColor` | 112 | color | `color.white` | — |
| 39 | `zone3LabelColor` | 113 | color | `color.white` | — |
| 40 | `zone4LabelColor` | 114 | color | `color.white` | — |
| 41 | `lookbackPeriod` | 143 | int | `3` | 1–10 |
| 42 | `showTable` | 147 | bool | `true` | — |
| 43 | `tablePosition` | 148 | string | `Top Right` | 9 positions (Top/Middle/Bottom × Left/Center/Right) |
| 44 | `tableTextSize` | 149 | string | `Normal` | Tiny, Small, Normal, Large, Huge |

44 exposed inputs total.

### 1a. QUIRK — "Opacity" inputs are actually **transparency**

Pine's `color.new(col, transp)` takes **transparency**: `0` = fully opaque,
`100` = fully invisible. Every input labelled "Opacity (%)" is passed directly
as that transparency argument (e.g. lines 509, 426, 543).

Consequences for the defaults:
* Range box "Opacity 85" renders at **15 % opaque**.
* High/Low, Quarter, EQ, AVR- "Opacity 0" render **fully opaque**.
* All four zones "Opacity 80" render at **20 % opaque**.

**ATAS mapping:** `alpha = round(255 * (1 - value / 100))`. The label text is
kept as in Pine ("Opacity") for parity of the settings UI; the inverted
semantics are documented rather than corrected. See Phase 2 candidate P2-06.

### 1b. QUIRK — the alpha channel of every colour input is discarded

Each colour default carries a baked-in transparency (e.g. `color.new(#ffffff, 85)`,
line 51), but every *shape* render re-wraps it: `color.new(rangeBoxColor, rangeBoxOpacity)`.
`color.new` **replaces** transparency, so only the RGB of the colour input ever
reaches a box/line. The user-visible swatch is therefore misleading.

**Exception — labels do NOT re-wrap.** Range-level labels use
`textcolor=highLowLineColor` / `quarterLineColor` / `eqLineColor` (lines 431,
432, 443, 444, 453) and the AVR- labels use `textcolor=line300Color` (547, 590),
all *raw*. So with defaults, the H/L **line** is fully opaque (opacity 0) while
the H/L **label text** is 50 % transparent. This asymmetry is real Pine
behaviour and is reproduced.

Zone labels use `zone1LabelColor`..`zone4LabelColor` (`color.white`, no alpha),
so they are unaffected.

---

## 2. Internal (non-exposed) constants

| Identifier | Line | Value | Used? |
|---|---|---|---|
| `asiaSession` | 10 | `"1700-2100"` | **YES** — the active preset |
| `rangePreset` | 13 | `"Asia"` | **YES** — hard-fixed |
| `londonSession` | 14 | `"0000-0600"` | reachable only if `rangePreset` changed |
| `nyAmSession` | 15 | `"0630-0900"` | reachable only if `rangePreset` changed |
| `nyPmSession` | 16 | `"0900-1200"` | reachable only if `rangePreset` changed |
| `rangeStartHour/Minute` | 17,18 | `17`, `0` | fallback branch only (298, 311) |
| `rangeEndHour/Minute` | 19,20 | `21`, `0` | fallback branch only |
| `projectionMode` | 27 | `"Auto"` | **YES** — hard-fixed |
| `baselineVol` | 28 | `300.0` | **YES** (239) |
| `baseUpRef` | 29 | `45.0` | **YES** (244) |
| `baseDownRef` | 30 | `48.0` | **YES** (245) |
| `skewConst` | 31 | `1.067` | **DEAD — 1 occurrence, never read** |
| `scaleClampLow` | 32 | `0.80` | **YES** (241) |
| `scaleClampHigh` | 33 | `1.40` | **YES** (241) |
| `maxDailyChange` | 34 | `0.40` | **YES** (252,253,256,257) |
| `upPointDistance` | 37 | `45.0` | dead branch only (836, 840) |
| `downPointDistance` | 38 | `48.0` | dead branch only (837, 840) |
| `nyOpenHour/Minute` | 42,43 | `21`, `0` | **YES** — lock time (269) |
| `roundTo` | 46 | `0.25` | **DEAD — 1 occurrence, never read** |
| `zone1..4LabelText` | 117–120 | `Level 1`, `AVR`, `AVR+`, `MAX` | **YES** |
| `zoneLabelSize` | 121 | `"small"` | **YES** (347) |
| `zoneLabelPosition` | 122 | `"center"` | **YES** (514, 668, 877) |
| `fib200/233/250` | 126–128 | `-2.0`, `-2.33`, `-2.5` | **YES** |
| `fib300` | 130 | `-3.0` | **YES** |
| `fib400/450` | 132,133 | `-4.0`, `-4.5` | **YES** |
| `fib600/650` | 135,136 | `-6.0`, `-6.5` | **YES** |
| `fib800/850` | 138,139 | `-7.78`, `-8.5` | **YES** |

### 2a. RESOLVED — no rounding is applied to projections

The brief asked this to be determined from the source rather than assumed.

**Determination: `roundPrice` is never called and `roundTo` is never read.**
Verified by exhaustive symbol scan:
* `roundPrice` — 1 occurrence (line 229, the definition itself).
* `roundTo` — 1 occurrence (line 46, the assignment itself).

Both are **dead**. Phase 1 introduces **no rounding whatsoever**. All projected
prices are raw IEEE-754 doubles. Tick-size-aware rounding is Phase 2 candidate P2-04.

### 2b. QUIRK — `fib800` holds 7.78, not 8.0

Line 138: `fib800 = -7.78`, despite the identifier, the section comment
("`-8.0 and -8.5 zone`", line 137) and the input comment ("`-8.0 and -8.5 zone
(Zone 4 - New MAX)`", line 96) all saying 8.0. The **value 7.78 is what
executes** and is what Phase 1 reproduces. The `800` in the identifier and in
`show800_850Zone` is a stale name.

### 2c. QUIRK — `skewConst = 1.067` is dead

`48 / 45 = 1.0666…`, so `skewConst` was evidently intended as the hard-coded
up/down skew. It is never used: the actual skew emerges from `baseUpRef` and
`baseDownRef` and is *recomputed* as a diagnostic (`skewRatio`, line 261).

### 2d. `pointsToPrice` is an identity function

Line 382–383: `pointsToPrice(points) => points`. Projection distances are
consumed as **raw price units**, never converted to ticks. Called at 459–460.
Phase 1 must not introduce tick conversion.

---

## 3. Timezone semantics

* All wall-clock derivation uses the named IANA zone from `timeZone`:
  `hour(time, timeZone)`, `minute(time, timeZone)`, `dayofweek(time, timeZone)`,
  `year/month/dayofmonth(t, timeZone)`, and `timestamp(timeZone, y, m, d, h, m)`.
* Named zones carry full DST rules. Phase 1 **must not** substitute fixed UTC
  offsets. The ATAS implementation resolves the zone via `TimeZoneInfo`
  (see §26 for the Windows/Linux ID mapping).
* Bar timestamps: Pine's `time` is the bar's **open** time. Every wall-clock
  test in this indicator is therefore evaluated against **bar open**, never
  close. Reproduced exactly.

---

## 4. Session semantics

`getSessionTimes()` (301–311) → `parseSessionString("1700-2100")` (285–298) →
`[17, 0, 21, 0]`.

`isInCustomRange()` (365–375):
```
currentMinutes = hour(time,tz)*60 + minute(time,tz)
startMinutes   = 1020   (17:00)
endMinutes     = 1260   (21:00)
startMinutes > endMinutes ?  currentMinutes >= start OR currentMinutes <= end     // cross-midnight
                          :  currentMinutes >= start AND currentMinutes <= end    // same-day  <-- ACTIVE
```
For the shipped Asia preset `1020 < 1260`, so the **same-day** branch is taken.

**In-range window is `17:00 <= t <= 21:00`, inclusive of the whole 21:00 minute**
(because the comparison is on minute granularity, `21:00:59` still yields 1260).

`isMarketActive()` (377–379) — see §20.

`inRangeNow = isInCustomRange() and isMarketActive()` (768).

### 4a. No shipped preset crosses midnight

Asia `1700-2100`, London `0000-0600`, NY AM `0630-0900`, NY PM `0900-1200` —
all have `startMinutes < endMinutes`. The cross-midnight branches at lines 372–373,
341–342 and 787–788 are therefore **unreachable in every preset the script
contains**. Core still implements them (they are part of the specified
behaviour and are unit-tested), but they are dead on the shipped path.

---

## 5. Session lifecycle

### 5.1 Creation (772–792)

```
newSessionCondition = inRangeNow and (na(currentSession) or currentSession.sessionComplete)
```

On trigger:
1. `newStartTime = timestamp(tz, year(time,tz), month(time,tz), dayofmonth(time,tz), 17, 0)`
   — i.e. 17:00 **on the calendar date of the current bar, in `tz`**.
2. Lines 778–782: finalise the previous session's drawings to `newStartTime`
   — **UNREACHABLE**, see §15.
3. `currentSession := RangeSession.new()`, `startTime := newStartTime`.
4. Lines 787–788: `if startHour > endHour and hour(time,tz) < startHour` →
   subtract 24 h. `17 > 21` is false → **dead** for all shipped presets.
5. `endTime := getSessionEnd(startTime)` (338–343):
   `delta = (21*60+0) - (17*60+0) = 240`; `delta > 0` so no `+1440`;
   `endTime = startTime + 240*60*1000` = start + 4 h.
6. `array.push(sessions, currentSession)` then `cleanupOldSessions()`.

### 5.2 Accumulation (795–811)

Guard: `not na(currentSession) and not currentSession.projectionsLocked`.
Inner guard: `time <= sessionEndTime and inRangeNow`.

```
highPrice := math.max(highPrice, high)
lowPrice  := math.min(lowPrice,  low)
```
Seeds are `highPrice = 0.0` and `lowPrice = 999999.0` (lines 156–157) — see §23.4.

Range box (805–811): created on first qualifying bar with
`box.new(startTime, highPrice, time, lowPrice)`; thereafter `set_top`,
`set_bottom`, `set_right(time)`. Right edge tracks the **bar open time**.

### 5.3 Lock (815–863)

```
not na(currentSession) and not projectionsLocked and atOrAfterLock and not inRangeNow
```
`isAtOrAfterLockTime()` (266–271): `hour*60+minute >= 21*60`. This is a
**same-day** test — true only for `21:00 <= t <= 23:59`. Combined with
`not inRangeNow` (which excludes the 21:00 minute), the effective lock window is:

> **the first bar whose local wall-clock time is in `[21:01, 23:59]`.**

Lock actions, in source order:
1. `finalHigh := highPrice`, `finalLow := lowPrice`, `lockTime := time`.
2. `projectionMode == "Auto"` → `calculateAutoUValues(yesterdayATR, prevUpDistance, prevDownDistance)`;
   assign `lockedUpDistance`, `lockedDownDistance`, `atrValue`, `scaleFactor`,
   `skewRatio`; then `prevUpDistance := uUp`, `prevDownDistance := uDown`.
   The `else` (Manual) branch at 834–840 is **dead**.
3. `projectionsLocked := true`.
4. Range box finalised: `set_right(currentSession.endTime)` — snaps the right
   edge back from the last in-range bar's open to the nominal 21:00 —
   `set_top(finalHigh)`, `set_bottom(finalLow)`.
5. `nextSessionStart = getNextSessionStartTime(getSessionEnd(startTime))`.
6. `createRangeLevelLines(currentSession, nextSessionStart)`.
7. `createFibZones(currentSession, nextSessionStart)`.
8. `sessionComplete := true`, `zonesFinalizedToNextSession := true`.

### 5.4 Immutability after lock

Because the accumulation block (795) is gated on `not projectionsLocked` and the
lock block (815) is gated on the same flag, once `projectionsLocked` is set
**nothing** in the script writes `finalHigh`, `finalLow`, `lockedUpDistance` or
`lockedDownDistance` again. Core enforces this structurally (the completed
session is an immutable value type) and it is unit-tested.

### 5.5 QUIRK (HIGH SEVERITY) — the lock can never fire, permanently wedging the indicator

The lock requires a bar opening in `[21:01, 23:59]` local. If the chart
timeframe produces no such bar, `projectionsLocked` stays `false` **forever**:
* `sessionComplete` is never set, so `newSessionCondition` (which requires
  `na(currentSession) or currentSession.sessionComplete`) can never fire again;
* the indicator therefore produces **exactly one incomplete session for the
  entire chart** and no projections at all.

Concretely, with `timeZone = America/Los_Angeles`:
* 1 m / 5 m / 15 m / 30 m / 1 h charts — a bar opens at 21:01/21:05/21:15/21:30/22:00 → **OK**.
* 4 h charts aligned to CME (…17:00, 21:00, 01:00…) — the bar after 21:00 opens
  at 01:00, minute-of-day 60 < 1260 → **never locks**.
* Daily and above — `isInCustomRange` never true → no sessions at all.

This is faithfully reproduced (Core exposes it via `SessionEngine` and it is
covered by a regression test). It is **not** repaired in Phase 1. Phase 2
candidate P2-07.

---

## 6. ATR semantics

```pine
atrDaily     = request.security(syminfo.tickerid, "D", ta.atr(atrLength), lookahead=barmerge.lookahead_off)   // 219
yesterdayATR = atrDaily[1]                                                                                    // 221
```

### 6.1 `ta.atr` = Wilder RMA of True Range

```
TR_i  = max(high_i - low_i, |high_i - close_{i-1}|, |low_i - close_{i-1}|)
TR_0  = high_0 - low_0                                  (no prior close)
RMA_n = SMA(TR_1..TR_n)                                 (seed, n = atrLength)
RMA_i = (RMA_{i-1} * (n - 1) + TR_i) / n                (i > n)
```
Pine's `ta.rma` seeds with the SMA of the first `n` values and is `na` before
that. Reproduced bit-for-bit in `WilderAtr`.

### 6.2 `lookahead_off`

The daily value is only published to the intraday series **after the daily bar
closes**. So on any intraday bar during daily period `D`, `atrDaily` carries the
ATR computed through the last **closed** daily bar, `D-1`.

### 6.3 CRITICAL — `[1]` indexes *chart* bars, not daily bars

`atrDaily[1]` is the value of the series one **intraday chart bar** ago, not one
day ago. Therefore:

* For all bars except the first of a new daily period, `atrDaily[1] == atrDaily`
  (the series is flat within a day) = ATR through `D-1`.
* On the **first chart bar of daily period `D`**, `atrDaily` has just stepped to
  ATR-through-`D-1`, while `atrDaily[1]` still holds ATR-through-`D-2`.

So `yesterdayATR` is **"the lookahead-off daily-ATR series as of the previous
chart bar"** — which the comment at line 220 calls "yesterday's completed ATR".
The two coincide except on the daily-rollover bar.

**What matters for this indicator:** the lock fires at 21:01–23:59 local. For
NQ on CME the daily bar rolls at 17:00 America/Chicago = 15:00
America/Los_Angeles, which is **not** inside the lock window, so the lock bar is
never the daily-rollover bar and `yesterdayATR == atrDaily` = **ATR through the
last closed daily bar**. Core models the general `[1]`-on-chart-bars rule
(so the distinction is preserved for other timezones/instruments) and a test
pins the rollover case.

No lookahead, no future leakage; historical and realtime evaluation are
identical because the value is only ever read on the lock bar.

### 6.4 Missing-ATR behaviour — **[OPEN-1]**

Before the daily ATR series is seeded (fewer than `atrLength + 1` closed daily
bars), `yesterdayATR` is `na`. `calculateAutoUValues` then computes
`fRaw = na / 300 = na` and calls `clampValue(na, 0.8, 1.4)` =
`math.max(0.8, math.min(1.4, na))`.

Pine's documented behaviour for `math.max`/`math.min` with an `na` argument is
not stated unambiguously in the reference. Two readings:
* **(A) na propagates** → `f = na` → all distances `na` → `createFibZones`
  guard (457) fails → **no zones drawn**, table shows `N/A`, and
  `prevUpDistance/prevDownDistance` are set to `na` (so the *next* session
  re-initialises from its own base).
* **(B) na is ignored as an extremum** → `f` collapses to a clamp bound, zones
  would be drawn from a fabricated scale factor.

**Phase 1 adopts (A).** Rationale: the table's explicit `"N/A"` handling
(lines 411–413, 958, 964, 970) and the `na` guards on `lockedUpDistance` in
`createFibZones` only make sense if `na` reaches those fields. Interpretation
(A) is implemented and tested; **(B) is recorded here as unverified.**
Resolving it requires a TradingView observation on a chart with insufficient
daily history — logged in `PARITY-TEST-MATRIX.md` as TV-OPEN-1.

---

## 7. Auto projection algorithm (237–263)

```
fRaw          = atr / 300.0
f             = clamp(fRaw, 0.80, 1.40)
uUpAutoBase   = 45.0 * f
uDownAutoBase = 48.0 * f

localPrevUp   = na(prevUp)   ? uUpAutoBase   : prevUp
localPrevDown = na(prevDown) ? uDownAutoBase : prevDown

uUpToday   = clamp(uUpAutoBase,   localPrevUp   * 0.60, localPrevUp   * 1.40)
uDownToday = clamp(uDownAutoBase, localPrevDown * 0.60, localPrevDown * 1.40)

skewRatio  = uDownToday / uUpToday
```
`clampValue(v, lo, hi) = math.max(lo, math.min(hi, v))` (233–234).

* Up and down are constrained **independently** against their own previous
  locked value. They are never collapsed to one symmetric distance.
* On the first session `prevUp`/`prevDown` are `na`, so the day-over-day clamp
  is a no-op and `uUpToday == uUpAutoBase`, `uDownToday == uDownAutoBase`.
* `prevUpDistance`/`prevDownDistance` are **script-global `var`s** (215–216),
  not per-session. They are updated only at lock (832–833), so the "previous"
  value is always the most recently *locked* session's value, regardless of how
  many calendar days elapsed (weekends and holidays included).
* With defaults, `skewRatio` is exactly `48/45 = 1.0666…` whenever neither
  day-over-day clamp binds.

---

## 8. Price projection formulas (467–489)

```
Up(k)   = finalLow  + lockedUpDistance   * |k|
Down(k) = finalHigh - lockedDownDistance * |k|
```
with `k ∈ { -2.0, -2.33, -2.5, -3.0, -4.0, -4.5, -6.0, -6.5, -7.78, -8.5 }`
stored negative and passed through `math.abs`.

Anchors are asymmetric and must not be swapped: **upward projections anchor on
`finalLow`; downward projections anchor on `finalHigh`.**

---

## 9. Projection structure

| Structure | Multipliers | Geometry |
|---|---|---|
| Level 1 outer | 2.0 → 2.5 | box |
| Level 1 inner shade | 2.33 → 2.5 | box drawn on top of the outer |
| AVR- | 3.0 | single line |
| AVR | 4.0 → 4.5 | box |
| AVR+ | 6.0 → 6.5 | box |
| MAX | 7.78 → 8.5 | box |

Both an upward and a downward set are produced per session, using their
respective distances and anchors.

### 9a. QUIRK — boxes are declared with `top < bottom`

`box.new(left, top, right, bottom)`. Upward Level 1 (509) passes
`top = upLevel200`, `bottom = upLevel250`, but `upLevel250 > upLevel200`, so
`top` holds the **lower** price. The same inversion applies to every zone box in
both directions (509, 511, 519, 527, 535, 552, 554, 562, 570, 578). Pine renders
inverted boxes correctly. ATAS rendering must normalise to
`(min, max)` before drawing.

### 9b. The inner shade has no independent visibility test

The 2.33–2.5 inner box is created inside the `showUpLevel1` / `showDownLevel1`
branch (511, 554) with no check of its own. If Level 1 is eligible, the inner
shade is drawn — even in the (impossible for positive distances) case where only
part of it clears the range.

---

## 10. Zone visibility (494–504)

```
showUpLevel1   = upLevel250 > finalHigh and upLevel200 > finalHigh
showUpLine300  = upLevel300 > finalHigh
showUpAVR      = upLevel450 > finalHigh and upLevel400 > finalHigh
showUpAVRPlus  = upLevel650 > finalHigh and upLevel600 > finalHigh
showUpMAX      = upLevel850 > finalHigh and upLevel800 > finalHigh

showDownLevel1  = downLevel250 < finalLow and downLevel200 < finalLow
showDownLine300 = downLevel300 < finalLow
showDownAVR     = downLevel450 < finalLow and downLevel400 < finalLow
showDownAVRPlus = downLevel650 < finalLow and downLevel600 < finalLow
showDownMAX     = downLevel850 < finalLow and downLevel800 < finalLow
```

A zone is eligible only when **both** of its boundaries lie strictly outside the
completed range. Comparisons are **strict** (`>` / `<`); a boundary exactly equal
to `finalHigh`/`finalLow` is **not** eligible.

Note that for any positive distance the wider boundary dominates, so each
two-term test reduces to its lower-multiplier term (e.g. `showUpAVR ≡ upLevel400 > finalHigh`).
Core keeps **both** terms explicitly so the predicate stays structurally
identical to Pine and remains correct if a distance is ever ≤ 0.

Both the box and its label are suppressed together (the label is created inside
the same `if`).

---

## 11. Range levels (417–453)

```
rangeSize = finalHigh - finalLow
H   = finalHigh
L   = finalLow
q1Price = finalLow + rangeSize * 0.25
eqPrice = finalLow + rangeSize * 0.50
q3Price = finalLow + rangeSize * 0.75
```
Line styles: H/L solid (426–427), quarters dotted (438–439), EQ solid (449).

### 11a. QUIRK (CONFIRMED) — Q1/Q3 labels are swapped

Line 443: the label at **`q1Price` (25 %)** reads **`"Q3"`**.
Line 444: the label at **`q3Price` (75 %)** reads **`"Q1"`**.

The *prices* are correct; only the two label texts are transposed. Confirmed
directly in the source, not inferred.

Phase 1 reproduces the swap. Core therefore separates the two concerns:
`RangeLevels` exposes `Q1Price`/`Q3Price` by **value** (25 % / 75 %), and a
distinct `RangeLevelLabelMap` supplies the display text. A
`LabelMode.PineCompatible` (default) and `LabelMode.Corrected` are both defined;
only `PineCompatible` is reachable in Phase 1. Switching modes requires no
change to the calculations. Phase 2 candidate P2-05.

---

## 12. Historical retention (760–763, 792)

```pine
cleanupOldSessions() =>
    while array.size(sessions) > lookbackPeriod
        oldSession = array.shift(sessions)
        deleteSession(oldSession)
```
Called **only** at session creation (792), *after* the new session is pushed.
So the retained count — including the in-progress session — is capped at
`lookbackPeriod`. With the default 3: two completed sessions plus the live one.

`deleteSession` (688–757) removes all 11 possible boxes, 7 lines and 15 labels.
Object growth is bounded: worst case (lookback 10) = 110 boxes, 70 lines,
150 labels, all under the 500 caps.

---

## 13. Next-session extension (314–336)

```pine
getNextSessionStartTime(currentEndTime) =>
    endHourTz = hour(currentEndTime, tz)                       // 21
    if startHour > endHour and endHourTz < startHour           // 17 > 21 -> FALSE, dead
        timestamp(tz, endY, endM, endD, startHour, startMinute)
    else
        nextDayTime = currentEndTime + 86_400_000              // raw ms
        nextDow = dayofweek(nextDayTime, tz)
        if nextDow == saturday (7) -> nextDayTime += 2 days
        else if nextDow == sunday (1) -> nextDayTime += 1 day
        timestamp(tz, year(nextDayTime,tz), month(nextDayTime,tz), dayofmonth(nextDayTime,tz), 17, 0)
```
Called once, at lock, with `currentEndTime = getSessionEnd(startTime)` (21:00
of the session day). Result:

| Session day | +24 h lands on | Adjust | Next session start |
|---|---|---|---|
| Mon–Thu | Tue–Fri | none | next day 17:00 |
| **Fri** | **Sat** | **+2 d** | **Mon 17:00** |
| **Sat** | **Sun** | **+1 d** | **Mon 17:00** |
| Sun | Mon | none | Mon 17:00 |

Weekend skipping is applied to the *extension target only*, never to session
creation (§20).

The `+24 h` is raw millisecond arithmetic, but the result is immediately decomposed
back to `year/month/dayofmonth` **in `tz`** and recombined via `timestamp`, so a
DST shift only moves the intermediate instant to 20:00 or 22:00 on the same
calendar date — the extracted date is unchanged. **DST-safe by construction.**

Zones and range-level lines are drawn from `rangeSession.endTime` (21:00) to
`nextSessionStart`. Labels sit at `nextSessionStart + 5 min` (line 422:
`labelOffset = 5*60*1000`).

### 13a. `getSessionEnd` is NOT timezone-aware

Line 343: `startTs + deltaMinutes*60*1000`. A DST transition inside the session
window would produce a session end one hour off. For America/Los_Angeles the
transition is at 02:00 local, never inside 17:00–21:00, so **the shipped preset
is unaffected**. Recorded because it would bite London (`0000-0600`) in Europe/London,
where the 01:00 transition *is* inside the window. Phase 2 candidate P2-08.

---

## 14. Label placement and update

* Range-level labels: `x = nextSessionStart + 5 min`, `style_label_left`,
  `color = color.new(color.white, 100)` (fully transparent background), size
  from `rangeLevelLabelSize`.
* AVR- labels: same offset and style, text `"AVR-"`, size from
  `zoneLabelSize` (small), textcolor `line300Color` raw.
* Zone labels: `style_text_outline`, size small, and because
  `zoneLabelPosition == "center"`, `x = int((sessionEndTime + nextSessionStart) / 2)`,
  `y` = midpoint of the **outer** boundaries (e.g. `(upLevel200 + upLevel250)/2`,
  line 515; the downward Level 1 label likewise uses 200/250, line 558 — not the
  inner shade).
* The re-centring pass (877–924) runs every bar for every finalised session,
  reading `box.get_left`/`get_right` of the first non-`na` zone box and rewriting
  every zone label's `x`. Since the boxes never move (§15), this recomputes the
  identical value — **idempotent, effectively a no-op**, but it does execute.
  If every zone of a session is suppressed, `referenceBox` stays `na` and nothing
  is repositioned; the labels don't exist either, so this is consistent.

---

## 15. `updateZonesAndLines` is DEAD CODE — there is no dynamic extension

`zonesFinalizedToNextSession` is set `true` at line 863, in the **same block**
that sets `sessionComplete := true` (862). Both call sites of
`updateZonesAndLines` require `sessionComplete AND NOT zonesFinalizedToNextSession`:

* line 780 — `prevSession.sessionComplete and not prevSession.zonesFinalizedToNextSession`
* line 870 — `sess.sessionComplete and not sess.zonesFinalizedToNextSession`

No code path ever sets `sessionComplete` without also setting
`zonesFinalizedToNextSession` on the same bar. **The conjunction is therefore
unsatisfiable and `updateZonesAndLines` (593–685) never executes.**

**Behavioural consequence:** zones and range-level lines are drawn **once**, at
lock, spanning `[sessionEnd, nextSessionStart]`, and are never lengthened toward
the live bar. The right edge is fixed at the *computed* next session start even
if price has not reached it, and does not follow the chart forward.

This resolves the brief's §14 ("replicate line extension behaviour"): **the
correct Phase 1 behaviour is no extension at all.** Core models the drawn extent
as a fixed, immutable `[From, To]` interval on the completed session.

---

## 16. Information table (928–977)

* Scans `sessions` newest→oldest, taking the first with `projectionsLocked`
  (935–941), and displays that session's `atrValue`, `lockedUpDistance`,
  `lockedDownDistance`.
* 3 columns × 4 rows: header (`Metric` / `Value` / `Unit`) plus `ATR20`,
  `Up Proj`, `Down Proj`; unit `pts` on all three.
* Number format `"#.##"`; `na` renders as `"N/A"`.
* Created once (`var infoTable`, 224); afterwards only `set_position` (947) and
  cell rewrites. Deleted and set to `na` when `showTable` is turned off (974–977).

### 16a. QUIRK — the row label `"ATR20"` is hard-coded

Line 959 writes the literal `"ATR20"` regardless of `atrLength`. With
`atrLength = 14` the table still reads `ATR20`. Reproduced verbatim.

### 16b. QUIRK — `displayAtr/Up/Down` are `var` and never reset

Declared `var` at 930–932 inside the `if showTable` block, so they persist
across bars. If a later scan finds no locked session the previous values remain
displayed. In practice unreachable (sessions only accumulate), but it means the
table can never revert from a number to `N/A` once populated.

### 16c. `getTableData` (410–414) is dead

Defined but never called; the table is populated inline at 958–973.

---

## 17. Historical vs realtime

The script contains no `barstate.*` guards. Every block runs on every bar in
both historical and realtime evaluation. The only realtime-sensitive constructs are:

* `request.security(..., lookahead_off)` — non-repainting by construction (§6).
* The range box's right edge tracking `time` while accumulating — this
  redraws as the live bar advances, then snaps to the nominal 21:00 at lock
  (step 4 of §5.3).
* Intrabar updates: on the live bar, `high`/`low` update tick-by-tick, so the
  accumulating range can widen within a bar. On historical bars only the final
  values are seen. This is the standard Pine live/historical divergence and is
  **not** removable. In ATAS the equivalent is `CalculateAtEveryTick`; the
  adapter honours the same semantics and the difference is documented in
  `PARITY-TEST-MATRIX.md`.

Once locked, a session's values are frozen (§5.4), so no historical repaint is
possible after the lock bar.

---

## 18. Weekend behaviour — QUIRK (CONFIRMED)

```pine
isMarketActive() =>
    dayOfWeekTz = dayofweek(time, timeZone)
    dayOfWeekTz >= 1 and dayOfWeekTz <= 7
```
Pine's `dayofweek` returns 1 (Sunday) … 7 (Saturday). The test `>= 1 and <= 7`
is satisfied by **every possible value** — it is a tautology. `isMarketActive()`
is a constant `true` and `inRangeNow` reduces to `isInCustomRange()`.

The evident intent was a weekday filter; the implementation filters nothing.
Sessions are therefore created on **any** calendar day that has bars in
17:00–21:00, Saturday included.

Reproduced verbatim in Phase 1. **No CME trading-calendar model is introduced.**
Note the asymmetry: weekend *skipping* does exist, but only in
`getNextSessionStartTime` (§13), never in session creation. Phase 2 candidate P2-03.

---

## 19. Cross-midnight behaviour

All cross-midnight machinery exists and is implemented in Core, but is
**unreachable with the shipped presets** (§4a):
* `isInCustomRange` wrap branch (372–373)
* `getSessionEnd` `+1440` correction (341–342)
* start-time `-24 h` correction (787–788)
* `getNextSessionStartTime` same-day branch (322–323)

Covered by unit tests against synthetic presets so the logic is proven, and
flagged so a Phase 2 preset exposure does not need re-derivation.

---

## 20. DST implications

| Mechanism | DST-safe? | Why |
|---|---|---|
| `hour`/`minute`/`dayofweek` in `tz` | yes | zone-aware |
| `timestamp(tz, y, m, d, h, mi)` | yes | zone-aware |
| `getSessionEnd` (+ms) | **no** | §13a — harmless for Asia preset |
| `getNextSessionStartTime` (+24 h then re-decompose) | yes | §13 |

On a US spring-forward Sunday the 17:00–21:00 window is unaffected (transition
at 02:00). On fall-back, likewise. The **first Asia session after a US DST
change shifts by one hour relative to UTC**, which is correct named-zone
behaviour and must be preserved — a fixed-offset implementation would be wrong.

---

## 21. Pine object lifecycle

| Object | Created | Mutated | Deleted |
|---|---|---|---|
| `rangeBox` | first accumulating bar (807) | `set_top/bottom/right` while accumulating (809–811); finalised at lock (847–849) | `cleanupOldSessions` → `deleteSession` (690) |
| zone boxes (10) | at lock, if eligible (509–578) | never (§15) | `deleteSession` |
| `upLine300`/`downLine300` | at lock, if eligible (543, 586) | never | `deleteSession` |
| range-level lines (5) | at lock (426–449) | never | `deleteSession` |
| zone labels (8) | at lock with their box | `set_x` by the idempotent re-centring pass (909–924) | `deleteSession` |
| AVR- labels (2) | at lock with their line | never (the 646–649 path is dead) | `deleteSession` |
| range-level labels (5) | at lock (431–453) | never (638–643 dead) | `deleteSession` |
| `infoTable` | first bar with `showTable` (945) | `set_position` + cells every bar | on `showTable` false (976) |

---

## 22. Complete quirk / dead-code register

| ID | Severity | Description | § |
|---|---|---|---|
| Q-01 | High | Lock can never fire on timeframes with no bar in 21:01–23:59 → indicator permanently wedged | 5.5 |
| Q-02 | High | `updateZonesAndLines` unreachable → no dynamic extension | 15 |
| Q-03 | Medium | Q1/Q3 label texts transposed | 11a |
| Q-04 | Medium | `isMarketActive()` is a tautology — no weekday filter | 18 |
| Q-05 | Medium | "Opacity" inputs are really transparency (inverted) | 1a |
| Q-06 | Medium | Colour-input alpha discarded for shapes but honoured for labels | 1b |
| Q-07 | Low | `fib800 = -7.78` contradicts its name and comments | 2b |
| Q-08 | Low | `"ATR20"` table label hard-coded | 16a |
| Q-09 | Low | Zone boxes declared with `top < bottom` | 9a |
| Q-10 | Low | `highPrice=0.0` / `lowPrice=999999.0` sentinels can leak (§23.4) | 23.4 |
| Q-11 | Low | `displayAtr/Up/Down` are `var`, never reset to `na` | 16b |
| Q-12 | Info | `getSessionEnd` not DST-aware | 13a |
| D-01 | Dead | `roundPrice` never called; `roundTo` never read → **no rounding** | 2a |
| D-02 | Dead | `skewConst = 1.067` never read | 2c |
| D-03 | Dead | Manual projection branch (834–840), `upPointDistance`, `downPointDistance` | 5.3 |
| D-04 | Dead | `getTableData` never called | 16c |
| D-05 | Dead | All cross-midnight branches (shipped presets) | 19 |
| D-06 | Dead | London / NY AM / NY PM presets (`rangePreset` fixed to "Asia") | 2 |
| D-07 | Dead | `pointsToPrice` is identity | 2d |

### 23.4 Sentinel seeds

`highPrice = 0.0`, `lowPrice = 999999.0` (156–157). If a session were ever
locked without accumulating a single bar, `finalHigh = 0` and
`finalLow = 999999`, giving `rangeSize = -999999` and nonsensical levels. The
creation condition guarantees at least one in-range bar (the creating bar itself
falls through to the accumulation block on the same execution), so this is
unreachable on the shipped path. Core uses explicit `null` instead of sentinels
and treats "locked with no bars" as an invalid state; a test pins that the
sentinel path is unreachable.

---

## 23. Additional observations

1. **Partial first session.** If chart data begins mid-window (say 20:00), the
   session is still created with `startTime = 17:00` and accumulates only from
   20:00. The box is drawn from 17:00 but reflects a partial range. Faithful.
2. **`prevUp/prevDown` survive gaps.** Being script-global `var`s, the
   day-over-day clamp chains across weekends, holidays and data gaps to the last
   *locked* session, not the previous calendar day.
3. **Parameter shadowing.** `createRangeLevelLines(rangeSession, endTime)` and
   `createFibZones(rangeSession, endTime)` take a parameter named `endTime`
   while `rangeSession.endTime` also exists and means something different
   (session end vs. next session start). Cosmetic, but a real readability trap.
4. See §23.4 above.
5. **Daily-and-above timeframes produce nothing** — `isInCustomRange` can never
   be true for a bar opening at the daily boundary.

---

## 24. ATAS equivalents

| Pine mechanism | ATAS / Core equivalent |
|---|---|
| `indicator(overlay=true)` | `Indicator` with `OnlineCalculation`, drawn on the price panel |
| `input.*` | `[Display]`/`[Parameter]` properties on the indicator class |
| `hour/minute/dayofweek(t, tz)` | `TimeZoneInfo.ConvertTimeFromUtc` in `IClock`/`SessionCalendar` |
| `timestamp(tz, y, m, d, h, mi)` | `TimeZoneInfo`-aware construction with DST-invalid/ambiguous handling |
| `request.security("D", ta.atr(n))` | daily bars via ATAS `SecurityDataProvider`, folded through `WilderAtr`; `[1]` modelled as previous-chart-bar snapshot |
| `box.new` | `RenderRectangle` in `OnRender` (normalised `top`/`bottom`) |
| `line.new` | `RenderLine` with `RenderPen` (dash pattern per style) |
| `label.new` | `RenderString` with measured extents |
| `table.*` | custom overlay rendered in `OnRender` at a chart-relative anchor |
| `xloc.bar_time` | timestamp→bar-index lookup, then `ChartInfo.GetXByBar` |
| `color.new(c, transp)` | `Color.FromArgb(255*(1-transp/100), r, g, b)` |
| `array<RangeSession>` + cleanup | `SessionStore` bounded ring |
| `var` globals | Core engine fields, reset on `OnInitialize`/recalculate |

---

## 25. Behaviour that cannot be reproduced exactly in ATAS

| # | Pine behaviour | Why not exact | Mitigation |
|---|---|---|---|
| 1 | `line.style_arrow_left/right/both` | ATAS `RenderPen` has no arrow-cap primitive | Rendered as solid; arrowheads drawn manually as two short strokes at the terminus. Visual approximation — documented, not claimed as parity. |
| 2 | Label auto-layout / collision avoidance | TradingView repositions overlapping labels; ATAS `RenderString` does not | Labels drawn at the computed anchor. Dense sessions may overlap where TV would nudge. |
| 3 | Font metrics | Pine `size.tiny…huge` are TV-internal | Mapped to a documented px ladder (9/11/13/16/20). Text extents will differ slightly. |
| 4 | `table` positioning | Pine tables float in chart-relative space with TV's own padding | Custom overlay honours the 9 anchors; pixel offsets differ. |
| 5 | Intrabar `high`/`low` on the live bar | Depends on tick delivery | `CalculateAtEveryTick`; identical in principle, tick-stream dependent in practice. |
| 6 | Daily bar boundary definition | TV's "D" for CME futures vs. ATAS daily aggregation may disagree on session boundary | **Primary parity risk for ATR.** Must be verified empirically against TradingView — see TV-OPEN-2 in `PARITY-TEST-MATRIX.md`. |

---

## 26. Timezone identifier portability

Pine uses IANA names. .NET on Windows historically used its own IDs. The ATAS
adapter resolves via `TimeZoneInfo.FindSystemTimeZoneById` with an IANA→Windows
fallback table (ICU-backed .NET 6+ accepts IANA on Windows, but the fallback
keeps older hosts and ATAS X on Linux working). Core itself is given an
already-resolved `TimeZoneInfo`, so it has no platform dependency.

---

## 27. Phase 2 candidates (NOT implemented)

| ID | Candidate |
|---|---|
| P2-01 | Expose Asia/London/NY AM/NY PM presets |
| P2-02 | Arbitrary custom session times |
| P2-03 | CME-aware trading-calendar / real weekend filter (fixes Q-04) |
| P2-04 | Instrument tick-size-aware rounding (activates D-01) |
| P2-05 | Corrected Q1/Q3 naming (fixes Q-03) |
| P2-06 | Rename "Opacity" → "Transparency" or invert (fixes Q-05) |
| P2-07 | Robust lock trigger not requiring a bar in 21:01–23:59 (fixes Q-01) |
| P2-08 | Timezone-aware `getSessionEnd` (fixes Q-12) |
| P2-09 | Manual projection mode |
| P2-10 | Live extension of completed-session lines (changes Q-02) |
| P2-11 | Shared reusable ATAS range/session components |

**No Phase 2 item may alter Phase 1 compatibility mode without Owner approval.**

---

## 28. Open items requiring TradingView observation

| ID | Question | § |
|---|---|---|
| TV-OPEN-1 | Does Pine `math.max`/`math.min` propagate `na`? Determines missing-ATR behaviour. Phase 1 assumes yes. | 6.4 |
| TV-OPEN-2 | Does TradingView's "D" bar for NQ match ATAS's daily aggregation boundary? Determines ATR parity. | 25.6 |
| TV-OPEN-3 | Confirm the Q1/Q3 label swap renders as read from source. | 11a |
| TV-OPEN-4 | Confirm the 4 h-chart wedge (Q-01) reproduces on TradingView. | 5.5 |
