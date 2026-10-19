# Contract: Shipped holiday entries and the build script

The calendar data this feature shipped, as built on 2026-10-10: the new `calendars/Anniversary.Calendar.US.json`, the
two entries added to `calendars/Anniversary.Calendar.Czech.json`, and how `calendars/Build-Calendars.ps1` writes them.
Key format: [calendar-rule-keys.md](calendar-rule-keys.md); sources and checks:
[calendars/README.md](../../../calendars/README.md); system spec: [docs/spec.md](../../../docs/spec.md) 5.1 and 5.7.

## `Anniversary.Calendar.US.json`

| Field | Value |
|---|---|
| `name` | `United States` |
| `locale` | `en-US` |
| `source` | `5 U.S.C. 6103(a) (federal holidays); 36 U.S.C. 109, 110, 117, 144 (observances); 15 U.S.C. 260a (daylight saving time); traditional dates` |
| `license` | `MIT, Copyright (c) David Pokluda (part of Anniversary; see LICENSE)` |

The 30 entries, in the file's order, with the source the script gives for each. "Tradition" means a date fixed by long
custom; the 2026 dates of the rules are the ones the tests check.

| Key | Text | Source | 2026 |
|---|---|---|---|
| `01-01` | New Year's Day | 5 U.S.C. 6103(a) | |
| `01-Mon-3` | Martin Luther King Jr. Day | 5 U.S.C. 6103(a): Birthday of Martin Luther King, Jr. | 19 January |
| `02-02` | Groundhog Day | tradition | |
| `02-14` | Valentine's Day | tradition | |
| `02-Mon-3` | Presidents' Day | 5 U.S.C. 6103(a): Washington's Birthday | 16 February |
| `03-17` | St. Patrick's Day | tradition | |
| `03-Sun-2` | Daylight Saving Time begins | 15 U.S.C. 260a(a) | 8 March |
| `04-01` | April Fools' Day | tradition | |
| `04-22` | Earth Day | tradition (since 1970) | |
| `05-05` | Cinco de Mayo | tradition | |
| `05-Mon-last` | Memorial Day | 5 U.S.C. 6103(a) | 25 May |
| `05-Sun-2` | Mother's Day | 36 U.S.C. 117 | 10 May |
| `06-14` | Flag Day | 36 U.S.C. 110 | |
| `06-19` | Juneteenth | 5 U.S.C. 6103(a): Juneteenth National Independence Day | |
| `06-Sun-3` | Father's Day | 36 U.S.C. 109 | 21 June |
| `07-04` | Independence Day | 5 U.S.C. 6103(a) | |
| `09-11` | Patriot Day | 36 U.S.C. 144 | |
| `09-Mon-1` | Labor Day | 5 U.S.C. 6103(a) | 7 September |
| `10-31` | Halloween | tradition | |
| `10-Mon-2` | Columbus Day | 5 U.S.C. 6103(a) | 12 October |
| `11-11` | Veterans Day | 5 U.S.C. 6103(a) | |
| `11-Sun-1` | Daylight Saving Time ends | 15 U.S.C. 260a(a) | 1 November |
| `11-Thu-4` | Thanksgiving | 5 U.S.C. 6103(a): Thanksgiving Day | 26 November |
| `11-Thu-4+1` | Black Friday | tradition: the day after Thanksgiving | 27 November |
| `12-24` | Christmas Eve | tradition | |
| `12-25` | Christmas Day | 5 U.S.C. 6103(a) | |
| `12-31` | New Year's Eve | tradition | |
| `easter` | Easter | tradition (Gregorian computus) | 5 April |
| `easter-2` | Good Friday | tradition | 3 April |
| `easter-47` | Mardi Gras | tradition: the Tuesday before Ash Wednesday | 17 February |

That is 11 federal holidays, 6 observances designated by law and 13 traditional dates; 16 `MM-dd` keys and 14 rules.
Holidays are listed on their legal date: when one falls on a weekend, the weekday off that OPM lists (for example
Friday 3 July 2026) isn't an entry.

## Additions to `Anniversary.Calendar.Czech.json`

| Key | Text | 2026 | Source |
|---|---|---|---|
| `easter-2` | Velký pátek | 3 April: "Richard, Velký pátek" | zákon č. 245/2000 Sb., § 2 |
| `easter+1` | Velikonoční pondělí | 6 April: "Vendula, Velikonoční pondělí" | zákon č. 245/2000 Sb., § 2 |

- The 366 v4 entries are unchanged. In ordinal order the two keys come after `12-31`, `easter+1` first.
- `source` becomes `Anniversary v4 (Anniversary.Calendar.Czech.json), plus Velký pátek and Velikonoční pondělí (zákon
  č. 245/2000 Sb., § 2)`; `name`, `locale` and `license` are unchanged.
- The other nine calendars are unchanged. `calendars/THIRD-PARTY-NOTICES.md` is unchanged: both files carry the app's
  own MIT license.

## `Build-Calendars.ps1`

Run from `calendars/` with PowerShell 7: `pwsh -File Build-Calendars.ps1` (parameters `-OutputFolder` and
`-CzechSource` as before). It writes all calendar files next to itself and prints one line per file with its number
of entries and its name.

- **US**: written from the ordered table in the script, each line with its source in a comment, by
  `Write-Calendar 'US' 'United States' 'en-US' {source} {license} $us -Holidays`.
- **Czech**: read from the Czech file itself (the v4 values, so the file is its own source), the two keys set, and
  written with the new `source`. Running the script again changes nothing.
- **`-Holidays`**: skips the full-year check (at least 360 `MM-dd` keys) that name-day calendars must pass. The other
  checks apply to every file: each key is `MM-dd` or a rule, no text is empty, and the written file must parse back
  with the same number of entries. Keys are written in ordinal order.

## Tests

`Anniversary.Tests/CalendarRuleTests.cs` loads the embedded files with `AnniversaryBook.Load("{file name}")`:

- `UsCalendarHasTheFederalHolidays`: the 11 federal holidays in 2025, 2026 and 2027 (33 cases) on the dates in OPM's
  federal holiday schedules, fixed-date holidays on their legal date.
- `UsCalendarHasTheFloatingObservances`: Mardi Gras, Daylight Saving Time begins, Good Friday, Easter, Mother's Day,
  Father's Day, Daylight Saving Time ends and Black Friday in 2026.
- `CzechCalendarHasTheEasterHolidays`: no warnings, the two days above, and Velikonoční pondělí found on 29 March 2027.
