# Contract: `AnniversaryBook` calendar API

The public members the app uses to list, choose and remember calendars, as built on 2026-10-09 and committed at the move
to GitHub (`67a4092`): `Anniversary.Shared/AnniversaryBook.cs`, `Anniversary.Shared/CalendarInfo.cs` and
`Anniversary.Shared/Model/UserSettings.cs`. Entities and rules: [data-model.md](../data-model.md); system spec:
[docs/spec.md](../../../docs/spec.md) 5.3, 5.5, 5.7 and 6 ("Calendar support").

```csharp
// Loading (spec 5.3, 5.7)
public static AnniversaryBook Load(string? preferredCalendar, params string[] folders);

// State
public IReadOnlyList<CalendarInfo> Calendars { get; }  // the available calendars, sorted by display name
public string CalendarFile { get; }                    // the file name (or path) of the active calendar
public IReadOnlyList<string> Warnings { get; }         // from 001

// Calendar operations
public void RefreshCalendars();
public void UseCalendar(string fileName);

// Anniversary.Shared/CalendarInfo.cs
public sealed record CalendarInfo(string FileName, string DisplayName, string? Locale);

// Anniversary.Shared/Model/UserSettings.cs
public string? Calendar { get; set; }
```

## `Load(preferredCalendar, folders)`

- `folders`: in order of precedence; the last one is the app-data folder. Blank entries are ignored. The book keeps them
  to discover calendars and to load another one later. The desktop app passes `AppContext.BaseDirectory` and the
  app-data folder; the XAML previewer and some tests pass none, which leaves only the embedded calendars.
- Order of work: the configuration (`appsettings.json`), the calendars (`RefreshCalendars()`), the active calendar, the
  people.
- `preferredCalendar`: the calendar chosen by the user (`UserSettings.Calendar`), or `null`. It is used when one of the
  discovered calendars has that file name (compared case-insensitively); otherwise `Config.CalendarFile` is used
  (`calendarFile` from `appsettings.json`, default `Anniversary.Calendar.Czech.json`). A preferred calendar that isn't
  available gives no warning.
- Never throws. A calendar that isn't found or can't be read adds `Calendar file not found: {name}` or
  `Error loading calendar: {message}` to `Warnings` and leaves the calendar empty.

## `Calendars` and `CalendarInfo`

- One `CalendarInfo` per available calendar: the files named `Anniversary.Calendar.*.json` in the folders (in order) and
  the embedded resources with such names, the first of each file name winning (case-insensitive); files that can't be
  read are left out. The active calendar is added when it isn't among them (for example a full path from
  `calendarFile`).
- `FileName`: the file name (or, for an added active calendar, the name or path it was asked for). `DisplayName`: `name`
  from the file, else the `{Name}` part of `Anniversary.Calendar.{Name}.json`, else the file name without its extension.
  `Locale`: `locale` from the file, or `null`.
- Sorted by `DisplayName` with `StringComparison.CurrentCulture`.

## `CalendarFile`

The name the active calendar was asked for: a file name, or a full path when `calendarFile` gives one. It is set even
when that calendar couldn't be found or read, so the menu can check it.

## `RefreshCalendars()`

Discovers the calendars again, as `Load` does, and replaces `Calendars`. It changes neither the active calendar nor
`Warnings`. The main window calls it each time the commands menu opens.

## `UseCalendar(fileName)`

- Clears `Warnings`, then loads `fileName`: an absolute path as is; a file name from the first folder that has it,
  otherwise from the embedded calendars.
- Not found: `Calendar file not found: {fileName}`. Unreadable (`JsonException`, `IOException`,
  `UnauthorizedAccessException`, `NotSupportedException`): `Error loading calendar: {message}`. Either way the calendar
  is empty; nothing is thrown.
- Sets `CalendarFile` and adds the calendar to `Calendars` if it is missing. The people, `Config` and `DataFilePath`
  don't change; `GetDay` and `Find` use the new calendar from the next call.

## `UserSettings.Calendar` (`calendar` in `usersettings.json`)

| Key | Type | Default | Meaning |
|---|---|---|---|
| `calendar` | string or `null` | `null` (or missing) | the file name of the calendar chosen in the menu; `null` until one is chosen |

```json
{ "theme": "System", "calendar": "Anniversary.Calendar.Czech.json" }
```

- `usersettings.json` lives in the app-data folder and is written only by the app: after a choice the main window sets
  `Calendar` and calls `Save()`, which writes the whole file (camelCase, indented). If saving fails, the choice still
  applies until the app closes.
- Read at start-up with case-insensitive names; a missing or invalid file gives the defaults, without a warning. A file
  written before this feature has no `calendar` and means "nothing chosen".
- `appsettings.json` is never written; its `calendarFile` is the fallback when `calendar` is `null` or names a calendar
  that isn't available.

## Tests

`Anniversary.Tests/CalendarTests.cs` (10 cases) covers these members with real files and the embedded calendars; the
list is in [tasks.md](../tasks.md) and docs/spec.md §9.
