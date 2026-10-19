# Data Model: Calendar Chooser

The state as built on 2026-10-09 and first committed at the move to GitHub (`67a4092`), with the real type and member
names. The interfaces are in [contracts/](contracts/calendar-api.md); the user-visible behaviour is in
[spec.md](spec.md).

## Entities

### CalendarData (`Anniversary.Shared/Model/CalendarData.cs`), the calendar file

| Property | JSON key | Type | Default | Meaning |
|---|---|---|---|---|
| `Name` | `name` | `string?` | `null` | **new**: the display name in the menu |
| `Locale` | `locale` | `string?` | `null` | **new**: the calendar's culture code, e.g. `cs-CZ`; informational, "reserved for automatic choice by OS locale later" (spec 5.1); [006](../006-holiday-calendar-rules/spec.md) later used it for the default by OS region |
| `Source` | `source` | `string?` | `null` | **new**: where the data came from |
| `License` | `license` | `string?` | `null` | **new**: the license of the data |
| `Data` | `calendar` | `Dictionary<string, string>` | empty | the day key (`MM-dd`) → the text shown for that day (v4) |

- Read with case-insensitive names, comments and trailing commas allowed, so v4 files without metadata load unchanged.
- The app uses the days, `name` and `locale` (for `CalendarInfo`); `source` and `license` are for people reading the
  file. The app never writes a calendar file.
- The format and the ten shipped files: [contracts/calendar-file.md](contracts/calendar-file.md).

### CalendarInfo (`Anniversary.Shared/CalendarInfo.cs`), new

`public sealed record CalendarInfo(string FileName, string DisplayName, string? Locale)`: one entry of the menu.

| Field | Meaning |
|---|---|
| `FileName` | the calendar's file name, e.g. `Anniversary.Calendar.Czech.json`; for an active calendar given by a path, that path |
| `DisplayName` | `name` from the file; otherwise the part of the file name between `Anniversary.Calendar.` and `.json` (for other names, the file name without its extension) |
| `Locale` | `locale` from the file, or `null` |

### UserSettings (`Anniversary.Shared/Model/UserSettings.cs`, stored as `usersettings.json`)

| Property | JSON key | Type | Default | Meaning |
|---|---|---|---|---|
| `Theme` | `theme` | `AppTheme` | `System` | the theme (001) |
| `Calendar` | `calendar` | `string?` | `null` | **new**: the file name of the chosen calendar; `null` until one is chosen |
| `FilePath` | none (`[JsonIgnore]`) | `string` | `""` | where the settings are read from and saved to |

- One instance per user account, in the app-data folder; written only by the app.
- Loaded at start-up (a missing or invalid file gives the defaults, without a warning) and passed to
  `AnniversaryBook.Load` as `settings.Calendar`.
- The main window sets `Calendar` and calls `Save()` after every choice. A failed save is ignored; the choice still
  applies until the app closes.
- `Calendar` isn't cleared when its file disappears: the next start falls back (see Rules), and the calendar is used
  again at a later start if the file comes back.

### AppConfig (historical: `Anniversary.Shared/Model/AppConfig.cs`, removed by 012)

Only `CalendarFile` matters here: `calendarFile` from `appsettings.json`, a file name or an absolute path, default
`Anniversary.Calendar.Czech.json`. The built-in `Anniversary.Shared/Defaults/appsettings.json` (also removed by 012)
names the same file.

### Calendar state in `AnniversaryBook` (`Anniversary.Shared/AnniversaryBook.cs`)

| Member | Type | Meaning |
|---|---|---|
| `_folders` | `string[]` | private: the non-blank folders passed to `Load`, in order of precedence ("`Load` keeps the folders for later use", spec 6); discovery and `UseCalendar` use them after `Load` |
| `Calendars` (`_calendars`) | `IReadOnlyList<CalendarInfo>` | **new**: the available calendars, sorted by display name |
| `CalendarFile` | `string` | **new**: the file name (or path) of the active calendar, as it was asked for |
| `_calendar` | `Dictionary<string, string>` | the active calendar's days; empty when it couldn't be loaded |
| `Warnings` | `IReadOnlyList<string>` | the load problems (001); `UseCalendar` clears them first |

## Rules

- **Discovery** (`RefreshCalendars()`, run by `Load` and each time the commands menu opens): in each existing folder, in
  order, every file matching `Anniversary.Calendar.*.json`; then every embedded resource whose name follows the same
  pattern. The first file of each name wins; names are compared case-insensitively. Each file is parsed to read its
  `name` and `locale`; a file that can't be read or parsed is skipped. The active calendar is added if it is missing
  (for example a full path from `calendarFile`). The list is sorted by `DisplayName` with the current culture.
- **The calendar at start-up** (`Load(preferredCalendar, folders)`): `preferredCalendar` (that is,
  `UserSettings.Calendar`) when a discovered calendar has that file name; otherwise `Config.CalendarFile`. A remembered
  calendar that isn't available gives no warning.
- **Loading a calendar** (`LoadCalendar`, from `Load` and `UseCalendar`): an absolute path is used as is; a file name is
  tried in each folder in order, then among the embedded calendars. Not found: the warning
  `Calendar file not found: {name}`; unreadable: `Error loading calendar: {message}`; either way the calendar is empty.
  `CalendarFile` is set to the name asked for in every case.
- **Switching** (`UseCalendar(fileName)`): clears `Warnings`, then loads as above. The people, the configuration and
  `DataFilePath` aren't touched.
- **What a day shows**: "Today is", Next N days and Find read the active calendar's days (spec 6.1, 6.2), so a switch
  shows at once.

## State transitions

How `AnniversaryBook` sees the active calendar:

| State | `CalendarFile` | Days | Listed in `Calendars` |
|---|---|---|---|
| **Loaded** | the name or path asked for | from the file | yes: as discovered, or added with its `name` |
| **Not found** | the name asked for | none | added, named after the file |
| **Unreadable** | the name asked for | none | as discovered (a shipped calendar of the same name), otherwise added, named after the file |

| From | Event | To | Also |
|---|---|---|---|
| (start) | `calendar` in `usersettings.json` names a discovered calendar | the result of loading it | |
| (start) | no `calendar`, or it names no discovered calendar | the result of loading `calendarFile` (default Czech) | no warning about the remembered calendar |
| any | the user chooses a calendar | the result of loading it | `Warnings` cleared first; `calendar` saved; both menus rebuilt; the window refreshed; the new warnings shown |
| any | the commands menu opens | unchanged | the calendars discovered again; both menus rebuilt |
