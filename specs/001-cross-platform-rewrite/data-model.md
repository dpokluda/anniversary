# Data Model: Cross-Platform Rewrite

The state as built on 2026-10-09 (first committed as `67a4092`), with the real type and member names. Members that
002 and 003 added to the same types at the same time are left out or marked. The interfaces are in
[contracts/](contracts/anniversarybook-api.md); the user-visible behaviour is in [spec.md](spec.md).

## Entities

### Person (`Anniversary.Shared/Model/Person.cs`)

Ported from v4 with XML docs.

| Property | JSON key | Type | Meaning |
|---|---|---|---|
| `Name` | `name` | `string` (default `""`) | the name shown in every list |
| `Birthdate` | `birthdate` | `DateOnly?` | `yyyy-MM-dd`; the year `9999` means "year unknown" (v4) |
| `Namedate` | `namedate` | `string?` | the name day as `MM-dd` |
| `Anniversaries` | `anniversaries` | `Dictionary<DateOnly, string>?` | `yyyy-MM-dd` → description, e.g. `"svatba"` |

### AnniversaryData (`Anniversary.Shared/Model/AnniversaryData.cs`)

The people file: `People` (`List<Person>`, JSON `data`). It was v4's `Model.Anniversary`, renamed so it doesn't clash
with the `Anniversary` namespace (C8). The list order is the display order.

### CalendarData (`Anniversary.Shared/Model/CalendarData.cs`)

The calendar file: `Data` (`Dictionary<string, string>`, JSON `calendar`), `MM-dd` → the text for the day, for example
`"10-09": "Štefan/Sára"`. It was v4's `Model.Calendar`, renamed so it doesn't clash with Avalonia's `Calendar` control
(C8). The optional `Name`, `Locale`, `Source` and `License` next to it came with 003.

### AppConfig (historical: `Anniversary.Shared/Model/AppConfig.cs`, removed by 012)

v4's configuration, read from `appsettings.json` and never written (C9).

| Property | JSON key | Type | Default |
|---|---|---|---|
| `CalendarFile` | `calendarFile` | `string` (file name or absolute path) | `Anniversary.Calendar.Czech.json` |
| `DataFile` | `dataFile` | `string` (file name or absolute path) | `Anniversary.Data.json` |
| `DisplayYearsOnBirthdays` | `displayYearsOnBirthdays` | `bool` | `true` |
| `NumberOfDaysAhead` | `numberOfDaysAhead` | `int` (≥ 0) | `7` |

The defaults are the real file names; v4's `AppConfig` defaulted to `calendar.txt` and `data.txt` (C5). A key missing
from the file keeps its default.

### UserSettings and AppTheme (`Anniversary.Shared/Model/UserSettings.cs`, stored as `usersettings.json`)

| Property | JSON key | Type | Default | Meaning |
|---|---|---|---|---|
| `Theme` | `theme` | `AppTheme` | `System` | the chosen theme, written as a string |
| `FilePath` | none (`[JsonIgnore]`) | `string` | `""` | where the settings are read from and saved to |

- `AppTheme`: `System` (follow the OS light or dark setting, including changes while the app runs), `Light`, `Dark`.
- One file per user account, in the app-data folder. `Calendar` in the same class came with 003.
- `Load(filePath)` reads property names in any letter case; a missing file, invalid JSON or an I/O or access error
  gives the defaults, without a warning, because only the app writes the file. `FilePath` is always set.
- `Save()` creates the folder if needed and writes camelCase, indented JSON with the enum as a string. I/O and access
  errors are ignored, so the choice still applies until the app closes.

### DayInfo (`Anniversary.Shared/DayInfo.cs`)

Everything the main window shows for one day, as returned by `AnniversaryBook.GetDay`.

| Field | Type | Content |
|---|---|---|
| `CalendarNames` | `string` | the calendar text for the day, or `""` |
| `Namedays` | `IReadOnlyList<string>` | the people with their name day, one entry per line |
| `Birthdays` | `IReadOnlyList<string>` | birthdays, then anniversaries, one entry per line |
| `NextDaysPeople` | `IReadOnlyList<string>` | the people with an event in the next N days |
| `NextDaysNames` | `IReadOnlyList<string>` | the calendar names in the next N days |

### AnniversaryBook (`Anniversary.Shared/AnniversaryBook.cs`)

| Member | Type | Meaning |
|---|---|---|
| `Config` | `AppConfig` | the configuration that was loaded, or the defaults |
| `Warnings` | `IReadOnlyList<string>` | the problems found while loading |
| `People` | `IReadOnlyList<Person>` | the people in data-file order; the main window uses it for the empty-list hint |
| `_folders` (private) | `string[]` | the folders passed to `Load`, blank ones removed; the last one is the app-data folder |
| `_calendar` (private) | `Dictionary<string, string>` | `MM-dd` → the day's text |
| `_peopleByDate` (private) | `Dictionary<string, List<Person>>` | `MM-dd` → the people with a birthday, name day or anniversary on that day |

### Main window state (`Anniversary.Shared/Views/MainWindow.axaml.cs`)

| Field | Type | Meaning |
|---|---|---|
| `_book` | `AnniversaryBook` | the loaded data |
| `_settings` | `UserSettings` | the theme, saved when it changes |
| `_day` | `DateOnly` | the shown day |
| `_findName`, `_findFromCurrentDate`, `_findWholeWords`, `_findCalendarOnly` | `string`, `bool` | the last Find values, kept between Find and Find Next as in v4 |

## Relationships

- `AppConfig` names the calendar and the people file that `AnniversaryBook.Load` reads.
- `AnniversaryData.People` holds the `Person`s; `_peopleByDate` indexes them by `MM-dd`.
- `GetDay` combines `_calendar`, `_peopleByDate` and `Config` into one `DayInfo`.
- `MainWindow` holds one `AnniversaryBook`, one `UserSettings` and the shown day; `App` creates all three at start-up.

## Rules

- **Keys**: calendar keys and `namedate` are `MM-dd`; `birthdate` and the anniversary keys are `yyyy-MM-dd`. A date is
  compared through its `MM-dd` key, formatted with the invariant culture.
- **Index**: each person is added under the key of their birth date, their name day (when set) and each anniversary
  date, at most once per key, in data-file order.
- **Lists for a day**: Namedays are the indexed people whose `Namedate` equals the key; Birthdays are their birthdays
  on the key, then their anniversaries on the key in dictionary order.
- **Years**: shown only when `DisplayYearsOnBirthdays` is on and the date's year is not after the shown day's year
  (C16): `{Name} ({day.Year − year})`, `{Name} - {description} ({day.Year − year})`.
- **Next N days**: for i = 1 … `NumberOfDaysAhead`, the people indexed at the day's key (each name once) and the
  calendar text split at `,`, trimmed, empty pieces skipped, each name once; both in date order.
- **29 February**: the key `02-29` comes up only on real dates, so it matches only in leap years.
- **Reading**: property names in any letter case; comments and trailing commas allowed.

## State transitions

### Loading each file (`AnniversaryBook.Load`)

The configuration (always `appsettings.json`) is loaded first, then the calendar it names (`calendarFile`), then the
people file it names (`dataFile`).

| Lookup result | Configuration | Calendar | People file |
|---|---|---|---|
| `calendarFile` or `dataFile` is an absolute path that exists | – | that file | that file |
| a copy next to the executable, else in the app-data folder | that copy | that copy | that copy |
| the built-in default with that name | v4's file | v4's Czech calendar | the empty list |
| not found (only another name, or an absolute path) | – (a copy is built in) | `Calendar file not found: {name}`; empty calendar | `Data file not found: {name}`; no people |
| found but can't be read | `Error loading configuration: {message}` + newline + `Using default settings.`; defaults | `Error loading calendar: {message}`; empty calendar | `Error loading data: {message}`; no people |

An absolute path that doesn't exist is reported as not found; the folders aren't searched instead. The warnings are
shown after the main window opens, one message each.

### Theme (`UserSettings.Theme`)

| From | Event | To | Also |
|---|---|---|---|
| (start) | `usersettings.json` missing or unreadable | System | no message |
| (start) | a saved theme | that theme | applied before the main window is created |
| any | Theme ▸ System / Light / Dark (macOS: View ▸ Appearance) | the chosen theme | every window updates; the file is saved; both menus check the choice |
| System | the OS switches between light and dark | System | the app re-themes on its own |

### The shown day (`MainWindow._day`)

| Event | New shown day |
|---|---|
| start-up, Today | today |
| Yesterday, Tomorrow | one day earlier or later |
| Change Date, OK | the picked day, or the same day if the picker was cleared |
| Change Date, Cancel | unchanged |
| Find or Find Next finds a day | the found day |
| Find with an empty name, or no match | unchanged (a message) |
