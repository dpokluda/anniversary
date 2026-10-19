# Data Model: Single Settings File

The state as built on 2026-10-11 (commit `9c89279`), with the real type and member names. The file format is in
[contracts/usersettings-file.md](contracts/usersettings-file.md), the API in
[contracts/settings-api.md](contracts/settings-api.md), and the user-visible behaviour in [spec.md](spec.md).

## Entities

### UserSettings (`Anniversary.Shared/Model/UserSettings.cs`, stored as `usersettings.json`)

| Property | JSON key | Type | Default | Meaning |
|---|---|---|---|---|
| `Theme` | `theme` | `AppTheme` | `System` | the theme chosen in the Theme menu (001) |
| `Calendar` | `calendar` | `string?` | `null` | the file name of the calendar chosen in the Calendar menu (003); `null` until one is chosen |
| `DataFile` | `dataFile` | `string?` | `null` | the data file: a full path chosen with Open… or Save As… (007) or, **now documented**, a file name typed by hand; `null` or blank for `Anniversary.Data.json` |
| `NumberOfDaysAhead` | `numberOfDaysAhead` | `int` | `7` | **new**: the days in the "Next N days" section |
| `DisplayYearsOnBirthdays` | `displayYearsOnBirthdays` | `bool` | `true` | **new**: whether birthdays and anniversaries show the number of years |
| `FilePath` | none (`[JsonIgnore]`) | `string` | `""` | where the settings are read from and saved to |
| `LoadError` | none (`[JsonIgnore]`, private setter) | `string?` | `null` | **new**: why the file couldn't be read; while it is set, `Save()` writes nothing |

- One instance per user account and computer, read from `{app-data folder}/usersettings.json` at start-up and shared
  by `AnniversaryBook` (as `Settings`) and the main window.
- Serializer options: case-insensitive names, camelCase when writing, comments skipped and trailing commas allowed
  (**new**), indented, enums as their names (`JsonStringEnumConverter`).

### AppTheme (unchanged)

`System`, `Light` and `Dark`; written as the name, read in any letter case.

### AppConfig and the built-in `appsettings.json` (historical)

`Anniversary.Shared/Model/AppConfig.cs` and `Anniversary.Shared/Defaults/appsettings.json`, both removed by this
feature. Where each value went:

| `AppConfig` property | JSON key | Type | Default | Now |
|---|---|---|---|---|
| `CalendarFile` | `calendarFile` | `string?` | `null` (the OS region decides) | gone: the calendar is the one chosen in the app (`UserSettings.Calendar`) |
| `DataFile` | `dataFile` | `string` | `Anniversary.Data.json` | `UserSettings.DataFile`, default `Anniversary.Data.json` |
| `DisplayYearsOnBirthdays` | `displayYearsOnBirthdays` | `bool` | `true` | `UserSettings.DisplayYearsOnBirthdays` |
| `NumberOfDaysAhead` | `numberOfDaysAhead` | `int` | `7` | `UserSettings.NumberOfDaysAhead` |

`appsettings.json` was looked up like the other files: next to the executable, then in the app-data folder, then the
built-in copy (with `dataFile`, `displayYearsOnBirthdays` and `numberOfDaysAhead`, but no `calendarFile` since 006).
It was read, never written, and exposed as `AnniversaryBook.Config`. A file that couldn't be read gave the warning
`Error loading configuration: {message}` and `Using default settings.`

### Settings in `AnniversaryBook` (`Anniversary.Shared/AnniversaryBook.cs`)

| Member | Kind | Meaning |
|---|---|---|
| `Settings` | `UserSettings` property | **new**, replacing `Config`: the settings the book was loaded with, the same object as the main window's |
| `ConfigurationWarningPrefix` | `const string` | `"Error loading configuration"`, unchanged; now documented as the prefix of warnings about the settings file |
| `ConfigFileName` | `const string` | **removed** (`"appsettings.json"`) |
| `DefaultDataFile` | private `const string` | **new**: `"Anniversary.Data.json"` |
| `DataFileName` | private property | **new**: `Settings.DataFile` unless it is `null` or blank, else `DefaultDataFile` |
| `LoadConfig()` | private method | **removed** |

## Validation rules

- **Types**: `theme` is `System`, `Light` or `Dark` (any letter case), `numberOfDaysAhead` a whole number,
  `displayYearsOnBirthdays` `true` or `false`, `calendar` and `dataFile` strings or `null`. A value of another type, or
  invalid JSON, makes the whole file unreadable (`LoadError`), and all five settings take their defaults.
- **Ranges** aren't checked. docs/spec.md documents `numberOfDaysAhead` as an int ≥ 0.
- **Missing keys** take their defaults; **unknown keys** are ignored and aren't written back.
- **`calendar`** applies only if it is the file name of an available calendar (in any letter case); otherwise the
  region rule (spec 5.7) decides, without a warning.
- **`dataFile`**: a rooted path is used as is, even when the file is missing; a file name is looked up next to the
  executable, then in the app-data folder, then among the built-in files (spec 5.3).
- **`LoadError`** is set only by `Load`, to the message of a `JsonException`, `IOException` or
  `UnauthorizedAccessException`. A missing file isn't an error.

## Load states

| State | File | Values | `LoadError` | `Save()` |
|---|---|---|---|---|
| **Missing** | doesn't exist | the defaults | `null` | writes the file (the app does so at start-up) |
| **Read** | read successfully | from the file; missing keys at their defaults | `null` | writes every key |
| **Unreadable** | invalid JSON, a value of the wrong type, or an I/O or access error | the defaults | the reason | writes nothing |

| From | Event | To | Also |
|---|---|---|---|
| (start) | `UserSettings.Load` finds no file | Missing | `App` calls `Save()` at once, so the file exists with the defaults |
| (start) | the file is read | Read | |
| (start) | the file can't be read | Unreadable | `AnniversaryBook.Load` adds the warning `Error loading configuration: {path}: {reason}`, a new line and `Using default settings.` first; the main window shows it titled "Configuration Error" after it opens |
| Missing or Read | a menu choice (theme, calendar, Data File ▸ Open… or Save As…) | Read | `Save()` writes all five keys; if writing fails, that is ignored and the choice applies until the app closes |
| Unreadable | a menu choice | Unreadable | the value changes in memory only; the file stays as it is |
| Unreadable | the user fixes the file with the app closed and starts the app | Read | |
| Read | the user edits the file while the app runs, then makes a menu choice | Read | the app writes the values it read at start-up, replacing the edit |

## Relationships

- `App` loads the `UserSettings`, saves it when the file is missing, applies its theme, and passes it to
  `AnniversaryBook.Load(settings, [AppContext.BaseDirectory, appData])` and to the main window.
- `AnniversaryBook` reads `Calendar` (which calendar to load), `DataFile` (which people file), `NumberOfDaysAhead`
  (how far `GetDay` looks ahead) and `DisplayYearsOnBirthdays` (whether lines show the years), and reports
  `LoadError`.
- The main window captions the last section from `book.Settings.NumberOfDaysAhead`, and sets `Theme`, `Calendar` or
  `DataFile` (after Data File ▸ Open… or Save As…, 007) before it calls `Save()`.
