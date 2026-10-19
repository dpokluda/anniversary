# Data Model: People Editor

The state as built on 2026-10-09 (in this repository since `67a4092`), with the real type and member names. The
interfaces are in [contracts/](contracts/editing-api.md); the user-visible behaviour is in [spec.md](spec.md).

## Entities

### Person and AnniversaryData (from 001, unchanged)

`Anniversary.Shared/Model/Person.cs` and `Anniversary.Shared/Model/AnniversaryData.cs`; the people file is
`{ "data": [ … ] }` (v4 format).

| Property | JSON key | Type | Rule |
|---|---|---|---|
| `Name` | `name` | `string` | required |
| `Birthdate` | `birthdate` | `DateOnly?` | optional; `yyyy-MM-dd`; v4's year `9999` means "year unknown" |
| `Namedate` | `namedate` | `string?` | optional; `MM-dd` |
| `Anniversaries` | `anniversaries` | `Dictionary<DateOnly, string>?` | optional; date (`yyyy-MM-dd`) → description |
| `AnniversaryData.People` | `data` | `List<Person>` | the people, in data-file order |

### PersonRow (`Anniversary.Shared/Views/PersonRow.cs`, new, editor only)

One editable row of the people editor; implements `INotifyPropertyChanged` and `INotifyDataErrorInfo`.

| Member | Type | Meaning and rule |
|---|---|---|
| `Name` | `string` | the name; `null` becomes `""`; error "Name is required." when blank |
| `Nameday` | `string` | `MM-dd` or `""`; error "Use MM-dd, e.g. 06-24." when `AnniversaryBook.IsValidNameday` rejects the trimmed value |
| `NamedayDate` | `DateTime?` | the nameday for the date picker: `null` when empty or invalid, otherwise that month and day in the current year (for 29 February the nearest earlier leap year). Setting it keeps only the month and day; `null` clears the nameday |
| `Birthday` | `DateTime?` | the birthday, date part only; optional |
| `Anniversaries` | `Dictionary<DateOnly, string>` | date → description; `null` becomes an empty dictionary |
| `AnniversariesSummary` | `string`, read-only | `"{description} ({date:d})"` in the current culture, ordered by date, joined by `", "` |
| `HasAnniversaries` | `bool`, read-only | whether there are any anniversaries |
| `HasErrors` | `bool`, read-only | whether the name or the nameday has an error |
| `MonthDayFormat` | `static string` | `MonthDayPattern` of the current culture's short date pattern, e.g. `M/d` (en-US) or `d. M.` (cs-CZ) |
| `MonthDayPattern(pattern)` | `static string` | the pattern without the year: `M/d/yyyy` → `M/d`, `d. M. yyyy` → `d. M.`, `yyyy-MM-dd` → `MM-dd` |
| `From(Person)`, `ToPerson()` | | the conversions in [Rules](#rules) |
| `GetErrors(propertyName)` | | the error of `Name`, or of `Nameday` and `NamedayDate` |

Every change raises `PropertyChanged` and `ErrorsChanged` for the property and `PropertyChanged` for `HasErrors`.
`Nameday` also raises `NamedayDate`; `Anniversaries` also raises `AnniversariesSummary` and `HasAnniversaries`.

### AnniversaryRow (`Anniversary.Shared/Views/AnniversaryRow.cs`, new, editor only)

One editable row of the Anniversaries dialog; implements `INotifyPropertyChanged` and `INotifyDataErrorInfo`.

| Member | Type | Meaning and rule |
|---|---|---|
| `Date` | `DateTime?` | the date, date part only; error "Date is required." when empty, "This date is used twice." when another row has the same date |
| `Name` | `string` | the description; `null` becomes `""`; error "Anniversary is required." when blank |
| `HasErrors` | `bool`, read-only | whether the date or the description has an error |
| `From(anniversaries)` | `static List<AnniversaryRow>` | one row per anniversary, ordered by date; none for `null` |
| `Validate(rows)` | `static bool` | marks the rows whose date is used twice and returns whether all rows are valid |
| `ToDictionary(rows)` | `static Dictionary<DateOnly, string>` | the valid rows as date → trimmed description |

### Editing state in AnniversaryBook (`Anniversary.Shared/AnniversaryBook.cs`)

| Member | Type | Meaning |
|---|---|---|
| `People` | `IReadOnlyList<Person>` | the people, in data-file order |
| `DataFilePath` | `string?` | the file the people were loaded from or last saved to; `null` when they came from the built-in default, or when the configured file wasn't found or couldn't be read |
| `AppDataFolder` | `string` | the last folder passed to `Load` (the user app-data folder), or `""` |
| `Config.DataFile` | `string` | the data file name or absolute path from `appsettings.json`, default `Anniversary.Data.json` (`AppConfig`, historical: `Anniversary.Shared/Model/AppConfig.cs`, removed by 012) |

## Rules

- **Nameday** (`AnniversaryBook.IsValidNameday`): `null` or `""`, or exactly five characters that parse as `MM-dd` in
  the year 2000 (a leap year). So `02-29` and `12-31` are valid; `13-01`, `02-30`, `1-5` and `abc` aren't.
- **Name**: not blank. **Birthday**: optional, any date.
- **Anniversaries**: each has a date and a non-blank description, and a person's dates are unique.
- **Save enabled**: no `PersonRow` has errors. **OK enabled**: `AnniversaryRow.Validate` returns `true`.
- **Person → row** (`PersonRow.From`): copies the values; a `null` nameday becomes `""`; the birthday becomes a
  `DateTime` at midnight; the anniversaries are copied.
- **Row → person** (`ToPerson`): trims the name and the nameday; an empty nameday becomes `null`; the birthday becomes a
  `DateOnly`; no anniversaries become `null`; the anniversaries are copied.
- **Before writing** (`Normalize` in `SavePeople`): the name and the nameday are trimmed, a blank nameday becomes
  `null`, anniversaries are ordered by date and none become `null`; `null` values aren't written.
- **Save target** (`SavePathFor`): `DataFilePath` when set; otherwise `Config.DataFile` if it is an absolute path;
  otherwise `{AppDataFolder}/{Config.DataFile}`.

## Relationships

- The editor holds one `PersonRow` per `Person` in an `ObservableCollection<PersonRow>` (the copy), in data-file order.
  Add appends and Remove removes; a `DataGridCollectionView` over the collection sorts and filters only the view.
- A row's anniversaries are edited in the dialog as `AnniversaryRow`s (another copy); OK replaces the row's
  `Anniversaries` dictionary.
- Save turns the rows back into `Person`s (`ToPerson`) in collection order and passes them to `SavePeople`, which writes
  them and makes them the book's `People`.

## State transitions

### The editor

| From | Event | To | Also |
|---|---|---|---|
| (main window) | Edit People… | Editing | rows copied from `People`; the save target taken from `SavePathFor()` |
| Editing | a row gets an error | Editing, Save disabled | the cell is marked |
| Editing, Save disabled | the errors are fixed | Editing | Save enabled |
| Editing | Save succeeds | Closed | file written; `People`, `DataFilePath` and the date index replaced; main window refreshed |
| Editing | Save fails (`IOException`, `UnauthorizedAccessException`) | Editing | message; the old file stays as it was |
| Editing | Cancel or Esc | Closed | nothing changes |

### The Anniversaries dialog

| From | Event | To | Also |
|---|---|---|---|
| Editing | Edit… in a row | Dialog | rows copied from the row's anniversaries, ordered by date |
| Dialog | a date or description is missing, or a date is used twice | Dialog, OK disabled | the cells are marked and the note is shown |
| Dialog | OK | Editing | the row's `Anniversaries` replaced; its summary updates |
| Dialog | Cancel or Esc | Editing | nothing changes |

### The data file, as the editor sees it

| At start-up | `DataFilePath` | Save target | After a successful Save |
|---|---|---|---|
| People loaded from a file | that file | that file | unchanged |
| The built-in empty list | `null` | `{AppDataFolder}/{Config.DataFile}`, or `Config.DataFile` if absolute | the written file |
| The configured file wasn't found (warning `Data file not found: {name}`) | `null` | as for the built-in list | the written file |
| The file couldn't be read (warning `Error loading data: {message}`) | `null` | as for the built-in list, which replaces the unreadable file if it is the same file | the written file |

At the next start the file lookup (spec 5.3) decides again: the written file is found unless a file with the same name
next to the executable comes first.
