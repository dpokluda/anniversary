# Data Model: Holiday Calendar Rules

The state as built on 2026-10-10 (commit `5bb1afe`), with the real type and member names. The interfaces are in
[contracts/](contracts/calendar-rule-keys.md); the user-visible behaviour is in [spec.md](spec.md).

## Entities

### CalendarData (`Anniversary.Shared/Model/CalendarData.cs`, a calendar file)

| Property | JSON key | Type | Meaning |
|---|---|---|---|
| `Name` | `name` | `string?` | display name in the calendar menu (003) |
| `Locale` | `locale` | `string?` | culture code; **now** its region makes the calendar the default for that OS region |
| `Source` | `source` | `string?` | where the data came from (003) |
| `License` | `license` | `string?` | under what terms (003) |
| `Data` | `calendar` | `Dictionary<string, string>` | the entries: key → display text; keys are `MM-dd` dates or, **new**, rule keys |

The type itself is unchanged; its documentation now calls it "a calendar file (name days or holidays)" and names the
rule keys (spec 5.1). Property names are read case-insensitively, with comments and trailing commas allowed.

### Rule key

A key matching `RuleKeyPattern` in `Anniversary.Shared/AnniversaryBook.cs` (grammar in
[contracts/calendar-rule-keys.md](contracts/calendar-rule-keys.md)). The parsed `Match` is kept, and its named groups
are the rule:

| Group | Values | Meaning |
|---|---|---|
| `month` | `01`–`12` | the month; absent for `easter` |
| `weekday` | `mon`, `tue`, `wed`, `thu`, `fri`, `sat`, `sun` (any case) | the weekday, mapped to `DayOfWeek` through `WeekdayNames` |
| `nth` | `1`–`4` or `last` | which of those weekdays in the month |
| `offset` | `+` or `-` followed by 1–99 (no leading zero) | days added after the date is found; optional |

### The loaded calendar in `AnniversaryBook` (`Anniversary.Shared/AnniversaryBook.cs`)

| Member | Type | Meaning |
|---|---|---|
| `_calendar` | `Dictionary<string, string>` | the `MM-dd` entries (as before) |
| `_calendarRules` | `List<(Match Rule, string Names)>` | **new**: the rule entries, in file order |
| `CalendarFile` | `string` | the file name of the active calendar (003) |
| `Calendars` | `IReadOnlyList<CalendarInfo>` | the discovered calendars, sorted by display name (003) |
| `Warnings` | `IReadOnlyList<string>` | load problems; **new**: the unknown-keys warning |
| `FallbackCalendarFile` | `const string` | **new**: `Anniversary.Calendar.US.json` |
| `MaxReportedKeys` | `const int` | **new**: 5, the most keys the warning lists |

Both collections are replaced each time a calendar loads: at start-up (`Load`) and when the user switches calendars
(`UseCalendar`, 003). People and the people index are untouched.

### CalendarInfo (`Anniversary.Shared/CalendarInfo.cs`, unchanged)

`sealed record CalendarInfo(string FileName, string DisplayName, string? Locale)`, one per discovered calendar (003).
Its `Locale` is now compared with the OS region.

### AppConfig (historical: `Anniversary.Shared/Model/AppConfig.cs`, removed by 012)

| Property | JSON key | Before | Now |
|---|---|---|---|
| `CalendarFile` | `calendarFile` | `string`, default `"Anniversary.Calendar.Czech.json"` | `string?`, default `null`: "or `null` to choose it by the OS region (spec 5.7)" |

`DataFile`, `DisplayYearsOnBirthdays` and `NumberOfDaysAhead` are unchanged.

### Built-in `appsettings.json` (historical: `Anniversary.Shared/Defaults/appsettings.json`, removed by 012)

Before: `calendarFile` (Czech), `dataFile`, `displayYearsOnBirthdays`, `numberOfDaysAhead`. Now `calendarFile` is gone,
so a fresh install has no configured calendar:

```json
{
  "dataFile": "Anniversary.Data.json",
  "displayYearsOnBirthdays": true,
  "numberOfDaysAhead": 7
}
```

### UserSettings and DayInfo (unchanged types)

- `UserSettings.Calendar` (`usersettings.json` → `calendar`, 003): the file name chosen in the menu, or `null`.
- `DayInfo.CalendarNames`: now documented as "the calendar text for the day (names or holidays)"; it comes from
  `CalendarNamesOn(day)`, as do `NextDaysNames`.

## Rules

- **Sorting keys at load** (`AddCalendarEntries`), for each entry in file order:
  1. a non-empty key that `IsValidNameday` accepts (five characters, `MM-dd`, a date that exists in a leap year, so
     `02-29` counts) → `_calendar`;
  2. otherwise a key that matches `RuleKeyPattern` → `_calendarRules`;
  3. otherwise → unknown. All unknown keys of one load give a single warning:
     `Unknown dates in {fileName}: {first five keys, joined with ", "}`, followed by ` and {n} more` when there are
     more than five.
- **The date of a rule in a year** (`TryResolveRule`):
  - `MM-Ddd-n`: the first `Ddd` on or after the 1st of `MM`, plus `7 × (n − 1)` days;
  - `MM-Ddd-last`: the last `Ddd` on or before the last day of `MM`;
  - `easter`: `EasterSunday(year)`, the anonymous Gregorian algorithm (Meeus, Jones and Butcher);
  - then the offset is added. No date when the year is outside 1–9999 or the result is outside `DateOnly`'s range.
- **Names for a day** (`CalendarNamesOn(day)`): the day's `MM-dd` entry if it isn't empty, then each rule entry with a
  non-empty text whose date in the day's year, the year before or the year after is the day, in file order; joined
  with `", "`. An empty string when there is nothing.
- **Where they are used**: `GetDay` (`CalendarNames`, and `NextDaysNames`, which splits each day's names on ",", trims
  them and removes duplicates) and `Find` (a day matches if its names match). People are still indexed by `MM-dd`.
- **Region of a culture name** (`RegionOf`): `CultureInfo.GetCultureInfo(name)`; `null` for a neutral culture
  (`cs`), the invariant culture (`""`) or an unknown name; otherwise `RegionInfo.TwoLetterISORegionName` (`cs-CZ` and
  `en-CZ` → `CZ`). Calendars are compared by `RegionOf(Locale)`.

## Choosing the active calendar

`Load(preferredCalendar, folders)` loads the configuration and discovers the calendars, then takes the first that
applies (spec 5.7 as of `5bb1afe`):

| Step | Condition | Active calendar |
|---|---|---|
| 1 | `preferredCalendar` (the app passes `UserSettings.Calendar`) is set and among `Calendars` | that calendar |
| 2 | `Config.CalendarFile` isn't blank (a user's `appsettings.json` sets it) | that file; a missing file gives `Calendar file not found: {name}` and an empty calendar |
| 3 | a calendar in `Calendars`, by display name, has `RegionOf(Locale)` equal to `RegionOf(CultureInfo.CurrentCulture.Name)` | the first such calendar |
| 4 | otherwise | `FallbackCalendarFile` (`Anniversary.Calendar.US.json`) |

How the active calendar changes:

| From | Event | To |
|---|---|---|
| (start) | `Load` | the calendar from the table above, its entries sorted as in Rules |
| any | the user chooses a calendar in the menu | that calendar (`UseCalendar` clears `Warnings` and loads it); `UserSettings.Calendar` saved |
| any | the next start | step 1 again: the chosen calendar, if its file is still found |
| a chosen calendar | its file is removed, then the app starts | steps 2–4 |

## Shipped calendar data

- `calendars/Anniversary.Calendar.US.json` (**new**): "United States", `en-US`, 30 entries: 16 `MM-dd` dates and 14
  rules.
- `calendars/Anniversary.Calendar.Czech.json`: the 366 v4 `MM-dd` entries unchanged, plus `easter-2` and `easter+1`.
- The other nine calendars are unchanged.

The entries, their sources and the build script are in [contracts/holiday-calendars.md](contracts/holiday-calendars.md).
