# Contract: `AnniversaryBook` API

The public members the views and tests use, as built on 2026-10-09 (`Anniversary.Shared/AnniversaryBook.cs` and
`Anniversary.Shared/DayInfo.cs`). States and rules: [data-model.md](../data-model.md); system spec:
[docs/spec.md](../../../docs/spec.md) §5.3, §5.4, §6, §6.1 and §6.2.

```csharp
namespace Anniversary;

public sealed class AnniversaryBook
{
    public const string ConfigFileName = "appsettings.json";
    public const string ConfigurationWarningPrefix = "Error loading configuration";

    public AppConfig Config { get; }
    public IReadOnlyList<string> Warnings { get; }
    public IReadOnlyList<Person> People { get; }                 // in data-file order

    public static AnniversaryBook Load(string? preferredCalendar, params string[] folders);
    public DayInfo GetDay(DateOnly day);
    public DateOnly? Find(string name, DateOnly from, bool wholeWordsOnly, bool calendarOnly);
}

public sealed record DayInfo(
    string CalendarNames,
    IReadOnlyList<string> Namedays,
    IReadOnlyList<string> Birthdays,
    IReadOnlyList<string> NextDaysPeople,
    IReadOnlyList<string> NextDaysNames);
```

The same class also has the members that 002 (editing and saving the people) and 003 (calendars) added at the same
time. `People` is listed in docs/spec.md §6 with the editing support; the main window also uses it for the empty-list
hint.

## `Load(preferredCalendar, folders)`

- `folders`: the folders to look in, in order of precedence; blank entries are ignored, and the last one is the
  user's app-data folder. The desktop app passes `AppContext.BaseDirectory` (next to the executable) and
  `App.AppDataFolder`; tests pass their temporary folders; with no folders only the built-in defaults are used (the
  main window's parameterless constructor for the XAML previewer calls `Load(null)`).
- `preferredCalendar`: the calendar chosen in the app, which came with 003; `null` uses `Config.CalendarFile`, which
  is all this feature needs.
- Order: the configuration (`ConfigFileName`), then the calendar (`Config.CalendarFile`), then the people
  (`Config.DataFile`). Each file is found on its own:
  1. an absolute path is used as is, and nothing else is tried;
  2. otherwise the first folder that has a file with that name;
  3. otherwise the embedded resource with that file name (`appsettings.json`, `Anniversary.Calendar.Czech.json` and
     `Anniversary.Data.json` are built in, under their plain file names);
  4. otherwise "not found".
- JSON is read with property names in any letter case, comments and trailing commas.
- Never throws for a load problem: `JsonException`, `IOException`, `UnauthorizedAccessException` and
  `NotSupportedException` are caught, and each problem adds one entry to `Warnings`:

| Situation | Warning | Result |
|---|---|---|
| configuration can't be read | `Error loading configuration: {message}` + newline + `Using default settings.` | default `AppConfig` |
| calendar not found | `Calendar file not found: {name}` | empty calendar |
| calendar can't be read | `Error loading calendar: {message}` | empty calendar |
| people file not found | `Data file not found: {name}` | no people |
| people file can't be read | `Error loading data: {message}` | no people |

The configuration is never "not found", because a copy is built in. Configuration warnings start with
`ConfigurationWarningPrefix`, so the main window can give them the title "Configuration Error" instead of "Data Error".

## `GetDay(day)`

With `key = day.ToString("MM-dd")` (invariant culture):

- `CalendarNames`: the calendar text for `key`, or `""`.
- `Namedays`: for each person indexed at `key`, in data-file order, the name if `Namedate == key`.
- `Birthdays`: for each person indexed at `key`, in data-file order: the birthday on `key` as `"{Name} ({years})"`,
  then each anniversary on `key`, in dictionary order, as `"{Name} - {description} ({years})"`. Without `" ({years})"`
  when `Config.DisplayYearsOnBirthdays` is `false` or the date's year is after `day.Year`.
- `NextDaysPeople` and `NextDaysNames`: for `i = 1 … Config.NumberOfDaysAhead`, the names of the people indexed at
  `day.AddDays(i)`'s key (each once), and the calendar text for that key split at `,`, trimmed, empty pieces skipped
  (each once, the first occurrence wins); both in date order.
- A person is indexed at most once per key, so they appear once per list even with several events on a day.
- `02-29` matches only in leap years.

## `Find(name, from, wholeWordsOnly, calendarOnly)`

- Returns `null` for a `null`, empty or blank `name`.
- Checks each day from `from` (inclusive) to 31 December of `from.Year + 1` (inclusive) and returns the first day
  whose calendar text matches, or, unless `calendarOnly`, on which a person indexed at that day has a matching name.
  Returns `null` when nothing matches.
- Matching ignores letter case (ordinal): with `wholeWordsOnly` the text is split at `' '`, `','`, `';'`, `'\t'`, `'/'`
  and `'-'` (empty pieces removed) and one piece must equal `name`; otherwise the text must contain `name`.

## What the class never does

- Read the clock: the caller passes the day as a `DateOnly`. Only the UI reads today, with
  `DateOnly.FromDateTime(DateTime.Today)`, at start-up and for the Today command.
- Show UI: problems go to `Warnings`.
- Write `appsettings.json` (C9).

## Tests

`DayTests` (11 cases), `FindTests` (13) and `LoadTests` (9) cover these members with real files; the list is in
[tasks.md](../tasks.md).
