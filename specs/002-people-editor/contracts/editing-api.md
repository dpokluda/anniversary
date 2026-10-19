# Contract: `AnniversaryBook` editing API

The public members of `AnniversaryBook` (`Anniversary.Shared/AnniversaryBook.cs`) that the editor uses, as built on
2026-10-09 ([docs/spec.md](../../../docs/spec.md) 6, "Editing support"). The written file is described in
[people-file-write.md](people-file-write.md); the rows that call `IsValidNameday` are in
[data-model.md](../data-model.md).

```csharp
// State
public IReadOnlyList<Person> People { get; }          // in data-file order
public string? DataFilePath { get; }                  // null when the people came from the built-in default
public string AppDataFolder { get; }                  // the last folder passed to Load, or ""

// Saving (spec 5.6)
public string SavePathFor();
public void SavePeople(IReadOnlyList<Person> people, string path);

// Validation (spec 7.8)
public static bool IsValidNameday(string? value);
```

## `People`

The people in data-file order, which is also the order of the main window's lists. Replaced by `SavePeople`.

## `DataFilePath`

The full path of the file the people were loaded from, set by `Load`; `null` when they came from the built-in default,
or when the configured file wasn't found or couldn't be read. `SavePeople` sets it to the written path.

## `AppDataFolder`

The last folder passed to `Load(preferredCalendar, params folders)`. The desktop app passes the executable's folder and
then the user app-data folder; with no folders it is `""`, and saving to the app-data folder isn't possible ("which
only matters in tests that don't save", spec 6). docs/spec.md 6 lists it with the calendar support; only `SavePathFor`
uses it.

## `SavePathFor()`

Returns `DataFilePath` when it is set; otherwise `Config.DataFile` when it is an absolute path; otherwise
`Path.Combine(AppDataFolder, Config.DataFile)`. `Config.DataFile` is `dataFile` from `appsettings.json`, default
`Anniversary.Data.json` (`AppConfig`, historical: `Anniversary.Shared/Model/AppConfig.cs`, removed by 012).

## `SavePeople(people, path)`

- Writes `people` to `path` as described in [people-file-write.md](people-file-write.md): normalized, in the given
  order, through `{path}.tmp`, creating the folder if needed.
- Then replaces `People` with the normalized people, sets `DataFilePath = path` and rebuilds the date index, so
  `GetDay` and `Find` reflect the saved people at once.
- Throws `IOException` (the file couldn't be written) and `UnauthorizedAccessException` (the file or folder isn't
  writable), both documented with `<exception>`; after an exception nothing in the book has changed.
- Shows no UI and doesn't touch `Warnings`.

## `IsValidNameday(value)`

Returns `true` for `null` or `""`, or for exactly five characters that parse as `MM-dd` in the leap year 2000 (so
`02-29` is valid); `false` otherwise (`13-01`, `02-30`, `1-5`, `abc`).

## Callers

- `PeopleEditorWindow`: copies `People` into rows, takes the target from `SavePathFor()` (also shown in the footer), and
  calls `SavePeople` on Save. Its parameterless constructor, for the XAML previewer, loads `AnniversaryBook.Load(null)`.
- `PersonRow`: `IsValidNameday` for the nameday error and for `NamedayDate`.
- `MainWindow`: opens the editor with its book and calls `Refresh()` when the editor returns `true`.

## Tests

`Anniversary.Tests/SaveTests.cs` (13 cases) covers these members with real files; the list is in
[tasks.md](../tasks.md) and docs/spec.md §9.
