# Research: People Editor

Decisions made while the editor was specified and built on 2026-10-09. Sources: the session of 2026-10-09 (the request,
the mocks and the approval; the excerpts end before the implementation), the Decisions table and work breakdown in
[docs/plan.md](../../docs/plan.md), the system spec as it stood at the move to GitHub (`git show 67a4092:specs/spec.md`;
"spec" below), the handover of that time (`specs/copilot/handover-linux.md`, now
[docs/copilot/handover.md](../../docs/copilot/handover.md)), the mocks in [docs/screens/](../../docs/screens/) and the
code at `67a4092`. Alternatives are listed only where a source names them.

## A grid editor with a nested Anniversaries dialog

- **Decision**: **Edit People…** opens a grid with one row per person (Name, Nameday, Birthday, Anniversaries). The
  Anniversaries cell shows a one-line summary and an **Edit…** button that opens a second dialog with the columns Date
  and Anniversary.
- **Rationale**: It is the design the maintainer described ("a dialog with grid view … another dialog (key/value) with
  anniversary date and value is the anniversary name"). The assistant drew mocks of both windows (the editor in light
  and dark) and the maintainer approved them: "Yes, let's do it. I like it a lot." With the built-in people list empty,
  the editor is how people get into a fresh install, besides copying a v4 data file into the app-data folder or next
  to the executable.
- **Alternatives considered**: no editor, as in v4 (spec C14 lists the editor as new); the maintainer asked whether it
  was "possible or too complicated". No other layout was recorded.
- **Source**: session 2026-10-09; docs/plan.md, Decisions, "People editor" ("Mocked and specified … user-approved for
  phase 1; implemented after the core app"); spec C14.

## Editor-only row classes

- **Decision**: The grids edit `PersonRow` and `AnniversaryRow` objects in `Anniversary.Shared/Views/`, not `Person` and
  its anniversaries dictionary. Both implement `INotifyPropertyChanged` and `INotifyDataErrorInfo`, hold the cell rules,
  and convert with `PersonRow.From(Person)` / `ToPerson()` and `AnniversaryRow.From` / `ToDictionary`.
- **Rationale**: "Two-way binding needs change notification, and the date picker needs `DateTime?`", and the DataGrid
  shows `INotifyDataErrorInfo` errors as cell errors (spec 7.8). The spec calls them "the one exception to 'no extra
  types'"; the handover lists them as the only extra types.
- **Alternatives considered**: binding the grid to `Person` directly, which the reasons above rule out.
- **Source**: spec 7.8; the handover's decisions; the constitution's Principle I, which names them as its example of a
  justified type.

## The Avalonia DataGrid over a collection view

- **Decision**: `Avalonia.Controls.DataGrid` 12.1.x (12.1.2, versioned separately from Avalonia 12.1.4), with its Fluent
  theme included in `App.axaml`. The editor's grid shows a `DataGridCollectionView` over an
  `ObservableCollection<PersonRow>`: column headers sort the view, its `Filter` filters it, and Add and Remove change
  the collection, whose order is the file order.
- **Rationale**: Sorting and filtering the view leave the order of the people, and so the file and the main window's
  lists, unchanged (spec 5.6, 7.8). The package is the only one added for the editor and is on the spec's list of
  allowed packages.
- **Alternatives considered**: none recorded. The implementation order asked to try these APIs first (a `DataGrid` over
  a `DataGridCollectionView` with a template column holding a `CalendarDatePicker` and a button, showing an
  `INotifyDataErrorInfo` error on a cell) and to record any Avalonia 12 difference in the spec; no record says how that
  check went.
- **Source**: spec 2, 7.8 and 13 (step 1).

## Where Save writes

- **Decision**: `SavePathFor()` returns the file the people were loaded from (`DataFilePath`). If they came from the
  built-in default, it returns `{app-data folder}/{dataFile}` (`Anniversary.Data.json` by default), and `SavePeople`
  creates the folder; an absolute `dataFile` is written in place. The editor shows this path in its footer.
- **Rationale**: The built-in list is embedded in the app and empty (the maintainer's decision), so it can't be saved
  over. The app-data folder is the per-user folder that the file lookup already reads (spec 5.3), so the saved file is
  found again at the next start; a file the user put next to the executable or into the app-data folder is edited where
  it is.
- **Alternatives considered**: none recorded.
- **Source**: docs/plan.md, Decisions, "People editor" ("Saves the data file (to app-data when the data came from the
  empty default)"); spec 5.6; `SaveTests` (`SavePathIsAppDataForBuiltInDefault`, `SavePathIsTheLoadedFile`).

## The file format and the order of the people

- **Decision**: The format of v4's data file: `{ "data": [ … ] }` with camelCase names, indented UTF-8, empty optional
  values left out, dates as `yyyy-MM-dd`, anniversaries ordered by date, and Czech letters written as they are
  (`JavaScriptEncoder.UnsafeRelaxedJsonEscaping`). People are written in list order.
- **Rationale**: The data file stays in the v4 format (spec 5.1), so a saved file loads again unchanged. The list order
  is the order of the main window's lists (spec 6.1: "in data-file order"), so sorting the grid must not reorder the
  file. The relaxed encoder keeps Czech letters readable instead of `\u` escapes.
- **Alternatives considered**: none recorded.
- **Source**: spec 5.1, 5.6 and 6.1; `SaveTests` (`SavedPeopleLoadAgainUnchanged`, `FileUsesTheDocumentedFormat`).

## Writing through a temporary file

- **Decision**: `SavePeople` writes `{target}.tmp` and then moves it over the target.
- **Rationale**: "so a failure never leaves a half-written file" (spec 5.6). Write errors (`IOException`,
  `UnauthorizedAccessException`) reach the editor, which reports them and stays open.
- **Alternatives considered**: none recorded.
- **Source**: spec 5.6; `FileUsesTheDocumentedFormat` (no `.tmp` file is left behind).

## The nameday as a month-and-day date picker

- **Decision**: The Nameday cell is a `CalendarDatePicker` bound to `PersonRow.NamedayDate`. Only the month and day are
  kept (stored as `MM-dd`) and shown, in `PersonRow.MonthDayFormat`: the current culture's short date pattern without
  the year, for example `M/d` or `d. M.`. For 29 February the picker gets a leap year. `AnniversaryBook.IsValidNameday`
  accepts an empty value or an `MM-dd` date that exists in a leap year.
- **Rationale**: "so it's consistent with the Birthday column" (spec 7.8).
- **Alternatives considered**: typing `MM-dd` text, as the mock shows (values such as `06-24` and the placeholder
  `MM-dd`; [people-editor.light.preview.png](../../docs/screens/people-editor.light.preview.png)).
- **Source**: spec 7.8; `EditorRowTests` (`NamedayDateKeepsOnlyMonthAndDay`,
  `TwentyNinthOfFebruaryNamedayUsesALeapYear`, `MonthDayPatternRemovesTheYear`).

## Validation in the rows; Save and OK disabled

- **Decision**: The rows report errors per cell: a blank name, an invalid nameday, an anniversary without a date or a
  description, and a date used twice (checked over the whole list by `AnniversaryRow.Validate`). Save and OK are
  disabled while any row has an error, and both check again when clicked.
- **Rationale**: Invalid entries never reach the file (acceptance criterion 10: "Invalid namedays, blank names and
  duplicate anniversary dates disable Save/OK"). Anniversary dates must be unique "because they are the dictionary
  keys" (spec 7.9).
- **Alternatives considered**: none recorded.
- **Source**: spec 7.8, 7.9 and 12; `EditorRowTests`.

## No confirmations, and Save isn't the default button

- **Decision**: Remove and Cancel don't ask; the editor works on a copy until Save. Save isn't `IsDefault`.
- **Rationale**: Remove has "no confirmation, because Cancel undoes everything"; "Save isn't `IsDefault`, because Enter
  commits a cell edit in the grid" (spec 7.8).
- **Alternatives considered**: none recorded.
- **Source**: spec 7.8.

## Updating the main window without a reload

- **Decision**: `SavePeople` replaces `People`, sets `DataFilePath` and rebuilds the date index; the editor closes with
  `true`, and the main window calls `Refresh()`.
- **Rationale**: "On success, close and refresh the main window" (spec 7.8); "after saving, `GetDay` reflects the new
  data without reloading" (spec 9).
- **Alternatives considered**: none recorded.
- **Source**: spec 6 (`SavePeople` "writes per 5.6, then replaces People and rebuilds the index"), 7.8 and 9;
  `DayReflectsSavedPeopleWithoutReloading`.

## Refreshing the filter during an edit

- **Decision**: The filter commits the current edit first and refreshes the view only when the filter text really
  changed.
- **Rationale**: `DataGridCollectionView.Refresh()` throws during an edit, and `TextChanged` arrives through the
  dispatcher (the handover's Avalonia 12 gotchas and the comment in `ApplyFilter`).
- **Alternatives considered**: none recorded.
- **Source**: the handover; `Anniversary.Shared/Views/PeopleEditorWindow.axaml.cs`.

## Verification approach

- **Decision**: Tests with real files for saving (`SaveTests`) and with real rows for the editor's rules
  (`EditorRowTests`); the windows by hand, following acceptance criterion 10.
- **Rationale**: Logic is tested with real objects and real files, without mocks (spec 3 and 9); what only the UI shows
  is checked by hand against the acceptance criteria (spec 12).
- **Alternatives considered**: none recorded.
- **Source**: spec 9 and 12; the handover (phase 1 built, tested and run live on Windows; the editor on the Linux and
  macOS check lists).
