# Contract: settings API

The public members the app and the views use for the settings, as built on 2026-10-11
(`Anniversary.Shared/Model/UserSettings.cs`, `Anniversary.Shared/AnniversaryBook.cs`). States and rules:
[data-model.md](../data-model.md); the file: [usersettings-file.md](usersettings-file.md); system spec:
[docs/spec.md](../../../docs/spec.md) 5.5, 6 and 8.

```csharp
// Anniversary.Model
public class UserSettings
{
    public AppTheme Theme { get; set; }                  // System
    public string? Calendar { get; set; }                // null: not chosen yet
    public string? DataFile { get; set; }                // null: Anniversary.Data.json
    public int NumberOfDaysAhead { get; set; }           // 7 (new)
    public bool DisplayYearsOnBirthdays { get; set; }    // true (new)
    [JsonIgnore] public string FilePath { get; set; }    // ""
    [JsonIgnore] public string? LoadError { get; }       // null (new; private setter)

    public static UserSettings Load(string filePath);
    public void Save();
}

// Anniversary
public sealed class AnniversaryBook
{
    public const string ConfigurationWarningPrefix = "Error loading configuration";
    public UserSettings Settings { get; }                // new; replaces AppConfig Config

    public static AnniversaryBook Load(UserSettings settings, IReadOnlyList<string> folders); // new
    public static AnniversaryBook Load(string? preferredCalendar, params string[] folders);  // now default settings
}
```

Removed: `AnniversaryBook.Load(string? preferredCalendar, string? preferredDataFile, IReadOnlyList<string> folders)`,
`AnniversaryBook.Config`, `AnniversaryBook.ConfigFileName` and the type `AppConfig`.

## `UserSettings.Load(filePath)`

- Never throws for a missing or unreadable file. A missing file gives the defaults with `LoadError == null`. A
  `JsonException`, `IOException` or `UnauthorizedAccessException` gives the defaults with `LoadError` set to the
  exception's message.
- `FilePath` is always set to `filePath`.
- Reads names case-insensitively, skips comments, allows trailing commas, and reads `theme` by name in any letter case.

## `UserSettings.Save()`

- Does nothing while `LoadError` is set.
- Otherwise creates the folder of `FilePath` if needed and writes every key: camelCase, indented, the theme by name,
  `null` for a calendar or data file not chosen.
- `IOException` and `UnauthorizedAccessException` are ignored: the values stay in memory for the session.

## `AnniversaryBook.Load(settings, folders)`

- Keeps `settings` itself as `Settings` (not a copy): the main window changes and saves the same object.
- If `settings.LoadError` is set, the first entry in `Warnings` is
  `{ConfigurationWarningPrefix}: {settings.FilePath}: {settings.LoadError}`, then `Environment.NewLine` and
  `Using default settings.`
- Calendar: `settings.Calendar` when it is the file name of a discovered calendar (in any letter case), otherwise the
  calendar for the region of `CultureInfo.CurrentCulture`, otherwise `Anniversary.Calendar.US.json` (spec 5.7).
- People: from `settings.DataFile`, or from `Anniversary.Data.json` when it is `null` or blank (spec 5.3);
  `SavePathFor()` uses the same name when no data file is in use.
- `GetDay` looks `Settings.NumberOfDaysAhead` days ahead (stopping at 31 December 9999) and shows the years only when
  `Settings.DisplayYearsOnBirthdays` is `true`.
- Never throws; other problems are warnings as before (spec 5.4).

## `AnniversaryBook.Load(preferredCalendar, params folders)`

Loads with `new UserSettings { Calendar = preferredCalendar }`: default settings with no `FilePath`, so the people come
from `Anniversary.Data.json`. Used by the tests and by the views' parameterless constructors for the XAML previewer.

## Callers

- `App.OnFrameworkInitializationCompleted`: `UserSettings.Load(Path.Combine(appData, "usersettings.json"))`, then
  `Save()` if the file doesn't exist, `ApplyTheme(settings.Theme)`,
  `AnniversaryBook.Load(settings, [AppContext.BaseDirectory, appData])` and
  `new MainWindow(book, DateOnly.FromDateTime(DateTime.Today), settings)`.
- `MainWindow`: the caption `Next {book.Settings.NumberOfDaysAhead} days`; `ShowWarnings()` titles warnings that start
  with `ConfigurationWarningPrefix` "Configuration Error" and all others "Data Error"; `ChooseTheme`,
  `ChooseCalendar` and `UseDataFile` set `Theme`, `Calendar` or `DataFile` and call `Save()`.
