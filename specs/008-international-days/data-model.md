# Data Model: International Days

The state as built on 2026-10-10 (commit `fb5312e`), with the real type and member names. The file's full content is
in [contracts/international-calendar-file.md](contracts/international-calendar-file.md), the menu in
[contracts/calendar-menu.md](contracts/calendar-menu.md); the user-visible behaviour is in [spec.md](spec.md).

## Entities

### CalendarData (`Anniversary.Shared/Model/CalendarData.cs`, unchanged)

The calendar file format of spec 5.1 (003 added the metadata, 006 the rule keys). For International Days:

| Property | JSON key | Value |
|---|---|---|
| `Name` | `name` | `International Days` |
| `Locale` | `locale` | none: the key is absent, so `null` |
| `Source` | `source` | the UN list of international days with its address, "as of 10 October 2026", plus fun days kept by tradition |
| `License` | `license` | `MIT, Copyright (c) David Pokluda (part of Anniversary; see LICENSE)` |
| `Data` | `calendar` | 61 entries: 58 `MM-dd` keys and 3 rule keys |

The 61 entries:

| Kind | Keys | Days |
|---|---|---|
| UN days on a fixed date | 54 `MM-dd` keys (`03-21` holds two days) | 55 |
| UN day that moves | `05-Sat-2`, `10-Sat-2` | 1 (World Migratory Bird Day, twice a year) |
| fun days on a fixed date | `03-14`, `05-04`, `07-17`, `09-19` | 4 |
| fun day that moves | `08-Fri-1` | 1 (International Beer Day) |
| **Total** | **61** (58 dates + 3 rules) | **61** (56 UN days + 5 fun days) |

### CalendarInfo (`Anniversary.Shared/CalendarInfo.cs`, unchanged)

`record CalendarInfo(string FileName, string DisplayName, string? Locale)`, one per available calendar (003). With this
feature `Locale` also decides the group a calendar is listed in: **with a locale** (a country) or **without one**
(`null` or empty).

| Calendar | `FileName` | `DisplayName` | `Locale` | Group |
|---|---|---|---|---|
| the 11 country calendars | `Anniversary.Calendar.{Austrian…US}.json` | e.g. `Czechia (Česko)`, `United States` | e.g. `cs-CZ`, `en-US` | with a locale |
| International Days | `Anniversary.Calendar.International.json` | `International Days` | `null` | without |
| a user's file with metadata but no `locale` | e.g. `Anniversary.Calendar.Test.json` | its `name`, e.g. `Test` | `null` | without |
| a v4-style file without metadata | `Anniversary.Calendar.{X}.json` | from the file name: `X` | `null` | without |

### The calendar list (`AnniversaryBook.Calendars` in `Anniversary.Shared/AnniversaryBook.cs`)

`IReadOnlyList<CalendarInfo>`, built by `RefreshCalendars()` from the files found (exe folder, then app-data, then the
embedded ones; merged by file name, 003) plus the active calendar if it isn't among them (`AddActiveCalendarIfMissing`,
for example one given by an absolute path).

## Rules

- **Order** (new): `AddActiveCalendarIfMissing` sorts the list every time it runs, that is at the end of
  `RefreshCalendars()` and of each calendar load:
  1. calendars with a locale before those without (`OrderBy(c => string.IsNullOrEmpty(c.Locale))`);
  2. within each group, by `DisplayName` with `StringComparer.CurrentCulture`.

  Before, the list was sorted by display name only, so International Days would have been listed among the countries.
- **Separator** (new): `MainWindow.UpdateCalendarMenus` walks the list in order and adds one separator to each menu
  (`Separator` in the ⋯ submenu, `NativeMenuItemSeparator` in the macOS View ▸ Calendar submenu) right before a
  calendar without a locale that follows one with a locale. Because the list is sorted, there is at most one separator,
  and none when either group is empty.
- **Starting calendar** (unchanged, 006): when no calendar was chosen or configured, `CalendarForRegion` takes the
  first calendar in the list whose `Locale` is set and has the region of `CultureInfo.CurrentCulture`, otherwise the
  US calendar. International Days has no locale, so it is never picked.
- **Names for a day** (unchanged, 006): the day's `MM-dd` entry first, then each rule that falls on the day, joined
  with ", ". So 10 October 2026 gives `World Mental Health Day, World Migratory Bird Day`; a day without an entry gives
  an empty string.

## Validation

- **When the script writes the file** (`Write-Calendar ... -Holidays` in `calendars/Build-Calendars.ps1`): each key is
  a valid `MM-dd` (a date in the leap year 2000) or a rule key; no value is empty; `-Holidays` skips the "full year"
  check that name-day calendars must pass; keys are written in ordinal order; the written file must parse back with
  the same number of entries. The `locale` line is written only when a locale is given (new).
- **When the app loads it**: unknown keys would be reported as a warning (006); the shipped file loads without warnings
  (`InternationalCalendarHasTheseDays` asserts `Assert.Empty(book.Warnings)`).

## Relationships

- `Calendars` feeds both calendar menus; choosing an item calls `UseCalendar(fileName)` and saves `calendar` in
  `usersettings.json` (003), which works for International Days like any other file.
- The embedded copy comes from `calendars/` through the `Anniversary.Calendar.*.json` wildcard in
  `Anniversary.Shared/Anniversary.Shared.csproj`; a file with the same name next to the executable or in the app-data
  folder would win (003).

## State transitions

None: the calendar is static data, and a calendar's group follows from its `locale` alone.
