# Data Model: Data File Sharing

The state as built on 2026-10-10 (commit `bb8676b`), with the real type and member names. The interfaces are in
[contracts/](contracts/data-file-api.md); the user-visible behaviour is in [spec.md](spec.md).

## Entities

### Person and AnniversaryData (unchanged)

The people file keeps the v4 format: `{ "data": [ { "name", "namedate", "birthdate", "anniversaries" } ] }`
(`Anniversary.Shared/Model/Person.cs`, `Anniversary.Shared/Model/AnniversaryData.cs`, where `People` is the `data`
list). It is read case-insensitively, with comments and trailing commas allowed, and written by `SavePeople` (002).
This feature doesn't change the format; Open… only adds a check before it uses a file (see Rules).

### UserSettings (`Anniversary.Shared/Model/UserSettings.cs`, stored as `usersettings.json`)

| Property | JSON key | Type | Default | Meaning |
|---|---|---|---|---|
| `Theme` | `theme` | `AppTheme` | `System` | the theme (001) |
| `Calendar` | `calendar` | `string?` | `null` | the chosen calendar file name (003) |
| `DataFile` | `dataFile` | `string?` | `null` | **new**: the full path of the data file chosen with Open… or Save As…; `null` or blank uses the configured one |
| `FilePath` | none (`[JsonIgnore]`) | `string` | `""` | where the settings are read from and saved to |

- One instance per user account and computer, in the app-data folder; so a shared file can have a different path on
  each computer.
- Loaded at start-up (a missing or invalid file gives the defaults, without a warning) and passed to
  `AnniversaryBook.Load` as `settings.DataFile`.
- The main window sets `DataFile` and calls `Save()` after a successful Open… or Save As… (`UseDataFile`). Save
  failures are ignored; the choice still applies for the current session.

### AppConfig (historical: `Anniversary.Shared/Model/AppConfig.cs`, removed by 012)

Only `DataFile` matters here: the file name or absolute path of the people file from `appsettings.json` (default
`Anniversary.Data.json`), used when `UserSettings.DataFile` is blank.

### Data file state in `AnniversaryBook` (`Anniversary.Shared/AnniversaryBook.cs`)

| Member | Type | Meaning |
|---|---|---|
| `People` | `IReadOnlyList<Person>` | the people, in data-file order |
| `DataFilePath` | `string?` | the data file in use: the file the people were loaded from or saved to, or a chosen or absolute file that was missing or couldn't be read; `null` when the people came from the built-in default (or from a relative configured name that wasn't found) |
| `DataFileLoadFailed` | `bool` | **new**: the data file wasn't found or couldn't be read at the last load |
| `_dataFileStamp` | `(DateTime WriteTime, long Length)?` | **new, private**: the file's `LastWriteTimeUtc` and length when the people were loaded or saved; `null` when there was no file |
| `Warnings` | `IReadOnlyList<string>` | load problems; `ReloadPeopleIfChanged()` clears them before it loads again |

`SavePathFor()` (from 002) returns `DataFilePath` when it is set; otherwise the configured `dataFile` if it is an
absolute path, else `{app-data folder}/{dataFile}`.

## Rules

- **Which file**: `Load` uses `preferredDataFile` (that is, `UserSettings.DataFile`) unless it is `null` or blank,
  otherwise `Config.DataFile`.
- **Finding it**: an absolute path is used as is and nothing else is tried. A file name is tried in each folder in
  order (the executable's folder, then the app-data folder), then as a built-in default (`Anniversary.Data.json` is
  built in, with no people).
- **What Open… accepts**: a readable file that parses as JSON (comments and trailing commas allowed), whose root is an
  object with a property named `data` (in any letter case) holding an array, and whose people can then be read.
  Anything else throws, and nothing changes.
- **Changed elsewhere** (`HasDataFileChanged()`): the file at `DataFilePath` exists and its `(LastWriteTimeUtc,
  Length)` differs from `_dataFileStamp`. A missing file is never a change; a file that appears after being missing is
  one, because its stamp was `null`.
- **Stamp renewed by**: the start-up load, a reload, `OpenDataFile` and `SavePeople` (Save in the editor and Save As…).

## State transitions

How `AnniversaryBook` sees the data file:

| State | `DataFilePath` | `DataFileLoadFailed` | Stamp | People |
|---|---|---|---|---|
| **Built-in** | `null` | `false` | `null` | the built-in empty list |
| **Loaded** | the file | `false` | the file's | from the file |
| **Missing** | the chosen or absolute path (`null` for a relative name) | `true` | `null` | none |
| **Unreadable** | the file | `true` | the file's | none |

| From | Event | To | Also |
|---|---|---|---|
| (start) | start-up load uses the built-in default | Built-in | |
| (start) | start-up load finds and reads the file | Loaded | |
| (start) | the chosen or configured file isn't found | Missing | warning `Data file not found: {name}` |
| (start) | the file is found but can't be read (invalid JSON, I/O error, …) | Unreadable | warning `Error loading data: {message}` |
| Missing (with a path) | the file appears; then the main window is activated or Edit People… opens | Loaded or Unreadable | earlier warnings cleared, new ones shown |
| Loaded or Unreadable | the file changes elsewhere; then the main window is activated or Edit People… opens | Loaded or Unreadable | earlier warnings cleared, new ones shown |
| Loaded | the file disappears | Loaded (unchanged) | not a change |
| any | Open… succeeds | Loaded (the opened file) | `UserSettings.DataFile` saved |
| any | Open… fails | unchanged | message "Could not open the data file: …" |
| any | Save in the editor or Save As… succeeds | Loaded (the written file) | Save As… also saves `UserSettings.DataFile` |

Before writing, Save in the editor asks:

1. **Overwrite** or Cancel when `HasDataFileChanged()` (checked first);
2. otherwise **Save Anyway** or Cancel when `DataFileLoadFailed` (Missing or Unreadable);
3. otherwise nothing.
