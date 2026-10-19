# Contract: `AnniversaryBook` data-file API

The public members the views use for the data file, as built on 2026-10-10 (`Anniversary.Shared/AnniversaryBook.cs`),
plus `MessageDialog.Confirm`. States and rules: [data-model.md](../data-model.md); system spec:
[docs/spec.md](../../../docs/spec.md) 5.3–5.6 and 6 ("Data file support").

```csharp
// Loading (spec 5.3)
public static AnniversaryBook Load(string? preferredCalendar, string? preferredDataFile, IReadOnlyList<string> folders);
public static AnniversaryBook Load(string? preferredCalendar, params string[] folders); // passes no data file

// State
public IReadOnlyList<Person> People { get; }
public string? DataFilePath { get; }
public bool DataFileLoadFailed { get; }
public IReadOnlyList<string> Warnings { get; }

// Data file operations
public void OpenDataFile(string path);
public bool HasDataFileChanged();
public bool ReloadPeopleIfChanged();
public string SavePathFor();                                        // from 002
public void SavePeople(IReadOnlyList<Person> people, string path); // from 002; now also renews the stamp
```

## `Load(preferredCalendar, preferredDataFile, folders)`

- `preferredCalendar`: unchanged (006).
- `preferredDataFile`: the full path chosen in the app (`UserSettings.DataFile`); `null` or blank uses `dataFile` from
  `appsettings.json` (`Config.DataFile`).
- `folders`: in order of precedence; the last one is the app-data folder. The desktop app passes the executable's
  folder and the app-data folder.
- Never throws. Problems go to `Warnings` (`Data file not found: {name}`, `Error loading data: {message}`).
  `DataFilePath`, `DataFileLoadFailed` and the stamp are set as in the data model.
- The `params` overload calls this one with no data file.

## `DataFilePath` and `DataFileLoadFailed`

- `DataFilePath`: the data file in use, that is, the file the people were loaded from or saved to, or a chosen or
  absolute file that was missing or couldn't be read. `null` when the people came from the built-in default.
- `DataFileLoadFailed`: `true` when the data file wasn't found or couldn't be read at the last load; `false` after a
  successful load, `OpenDataFile` or `SavePeople`.

## `OpenDataFile(path)`

- Reads `path` and requires valid JSON (comments and trailing commas allowed) whose root is an object with a `data`
  array (the name in any letter case); then reads the people.
- On success: `People` replaced, `DataFilePath = path`, `DataFileLoadFailed = false`, stamp renewed, the date index
  rebuilt. `Warnings` isn't touched.
- Documented exceptions, after which nothing has changed: `JsonException` (invalid JSON, no `data` list with the message
  `This isn't an Anniversary data file (it has no "data" list).`, or people that can't be read), `IOException`
  (including a missing file) and `UnauthorizedAccessException`. The main window also catches `NotSupportedException`.

## `HasDataFileChanged()`

`true` when the file at `DataFilePath` exists and its last-write time (UTC) or size differs from the stamp taken when
the people were loaded or saved. `false` when `DataFilePath` is `null` or the file doesn't exist.

## `ReloadPeopleIfChanged()`

If `HasDataFileChanged()`: clears `Warnings`, loads the people from `DataFilePath` exactly as at start-up (with the
same warnings), and returns `true`. Otherwise returns `false` and changes nothing. The calendar isn't reloaded.

## `SavePathFor()` and `SavePeople(people, path)`

- `SavePathFor()`: `DataFilePath` when set; otherwise `Config.DataFile` when it is an absolute path; otherwise
  `{AppDataFolder}/{Config.DataFile}`.
- `SavePeople`: as in 002 (format, write to `{path}.tmp` and replace, `IOException` and `UnauthorizedAccessException`
  propagate, then `People` and `DataFilePath = path`). It now also clears `DataFileLoadFailed` and renews the stamp, so
  a Save As… target is a loaded data file and the app's own save isn't a change.

## `MessageDialog.Confirm` (`Anniversary.Shared/Views/MessageDialog.cs`)

```csharp
public static Task<bool> Confirm(Window owner, string title, string message, string confirmText);
```

The same small modal window as `MessageDialog.Show` (wrapped text, width 240–400, margin 16, not resizable, not in the
taskbar, centred on the owner) with two buttons on the right, spacing 8: `{confirmText}` and **Cancel**. Cancel is
`accent`, `IsDefault` and `IsCancel`. Returns `true` only when the confirm button is clicked. `Show` now builds its
window with the same helpers.

## Tests

`Anniversary.Tests/DataFileTests.cs` (16 cases) covers these members with real files; the list is in
[tasks.md](../tasks.md) and docs/spec.md §9.
