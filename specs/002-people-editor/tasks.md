# Tasks: People Editor

**Input**: Design documents from `specs/002-people-editor/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic, file format and settings changes: 30
test cases, 13 in `Anniversary.Tests/SaveTests.cs` and 17 in `Anniversary.Tests/EditorRowTests.cs`. The windows and
menus are checked by hand with [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. The work was done on 2026-10-09 in the old `pokluda` repository, which kept
no history; it reached this repository with the rest of phase 1 in `67a4092` ("Moving to GitHub from a private VSO
repository", 2026-10-10).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): views and logic in `Anniversary.Shared/`, tests in `Anniversary.Tests/`, documentation in
  `README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point, the DataGrid package and the test classes

- [X] T001 Start from the core app of 001, built in steps 1–7 of the implementation order (docs/spec.md §13): the main window, the dialogs and `MessageDialog`, the commands menu with the macOS menu bar, the file lookup in `Anniversary.Shared/AnniversaryBook.cs` and the empty built-in `Anniversary.Shared/Defaults/Anniversary.Data.json`
- [X] T002 [P] Add the DataGrid package: `AvaloniaDataGridVersion` 12.1.2 in `Directory.Build.props` (versioned separately from `AvaloniaVersion` 12.1.4) and the `Avalonia.Controls.DataGrid` package reference in `Anniversary.Shared/Anniversary.Shared.csproj`
- [X] T003 Style the grid in `Anniversary.Shared/App.axaml`: include `avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml` after `FluentTheme`, give `DataGrid`, `DataGridCell` and `DataGridColumnHeader` the `AppFontSize`, set `DataGridRow` `MinHeight` 28, and give date pickers in cells `MinHeight` 0 and padding `8,2,4,2`
- [X] T004 [P] Create `Anniversary.Tests/SaveTests.cs`: a sealed `IDisposable` test class with the summary "Tests of saving the people file (spec 5.6).", and two `TempFolder`s (`_exe` and `_appData`, standing in for the executable's folder and the app-data folder) deleted in `Dispose`
- [X] T005 [P] Create `Anniversary.Tests/EditorRowTests.cs`: a sealed test class with the summary "Tests of the people editor rows (spec 7.8, 7.9)."

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Give the views the people, a nameday rule and a safe way to save

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T006 Expose the people for editing in `Anniversary.Shared/AnniversaryBook.cs`: `People` (data-file order) and `DataFilePath` (the file the people were loaded from; `null` for the built-in default), which `LoadPeople` sets from the path `ReadFile` found (FR-003, FR-015)
- [X] T007 Add `IsValidNameday(string? value)` to `Anniversary.Shared/AnniversaryBook.cs`: `null` or empty, or five characters that parse as `MM-dd` in the leap year 2000, so `02-29` is valid (FR-006)
- [X] T008 Add `SavePathFor()` to `Anniversary.Shared/AnniversaryBook.cs`: `DataFilePath` when set, otherwise `Config.DataFile` if it is an absolute path, otherwise `Path.Combine(AppDataFolder, Config.DataFile)`, where `AppDataFolder` is the last folder passed to `Load` (FR-015)
- [X] T009 Add `SavePeople(IReadOnlyList<Person> people, string path)` to `Anniversary.Shared/AnniversaryBook.cs`: `Normalize` each person (trimmed name and nameday, a blank nameday and no anniversaries become `null`, anniversaries ordered by date), serialize with `WriteOptions` (camelCase, `null` values left out, `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, indented), create the folder, write `{path}.tmp` as UTF-8 without a BOM and move it over `path`; only then replace `People`, set `DataFilePath = path` and rebuild the date index. `IOException` and `UnauthorizedAccessException` propagate and are documented with `<exception>` (FR-015, FR-016, FR-017, FR-018)
- [X] T010 [P] Add `SavedPeopleLoadAgainUnchanged` (Czech names, a nameday, a birthday and two anniversaries load again in list order, with the anniversaries ordered by date, and the reloaded `DataFilePath` is the saved path), `FileUsesTheDocumentedFormat` (`"data"` and `"name": "Žofie"`; no empty optional keys, no `\u` escapes and no `.tmp` file left), `DayReflectsSavedPeopleWithoutReloading`, `SavePathIsAppDataForBuiltInDefault`, `SavePathIsTheLoadedFile` and the `NamedayValidation` theory (`null`, `""`, `02-29` and `12-31` valid; `13-01`, `02-30`, `1-5` and `abc` invalid) to `Anniversary.Tests/SaveTests.cs`

**Checkpoint**: `AnniversaryBook` can save edited people and knows where; the user stories can begin

---

## Phase 3: User Story 1 - Add and change people in the app (Priority: P1) 🎯 MVP

**Goal**: ⋯ ▸ Edit People… opens a grid of the people; Save writes them and updates the main window

**Independent Test**: On a fresh install, add a person with a name, a nameday and a birthday and save: the file appears
in the app-data folder, and the main window shows the person at once and after a restart (quickstart scenario 1)

### Tests for User Story 1

- [X] T011 [P] [US1] Add `PersonRoundTripsThroughRow` (name, nameday, birthday `DateOnly` ↔ `DateTime?` and anniversaries survive `PersonRow.From` and `ToPerson`, and the anniversaries are copied), `EmptyNamedayBecomesNull` (the name is trimmed; a blank nameday and no anniversaries become `null`), `NamedayDateKeepsOnlyMonthAndDay`, `TwentyNinthOfFebruaryNamedayUsesALeapYear` and the `MonthDayPatternRemovesTheYear` theory (`M/d/yyyy`, `d. M. yyyy`, `dd.MM.yyyy`, `yyyy-MM-dd`, `yyyy/MM/dd` and `dd/MM/yy` lose their year) to `Anniversary.Tests/EditorRowTests.cs`

### Implementation for User Story 1

- [X] T012 [US1] Create the editable row `Anniversary.Shared/Views/PersonRow.cs` (`INotifyPropertyChanged`, XML docs): `Name`, `Nameday` (`MM-dd` text or empty), `NamedayDate` (`DateTime?` for the date picker: only the month and day are kept; the year is the current one, or the nearest earlier leap year for 29 February), `Birthday` (`DateTime?`, date only), `Anniversaries`, the static `MonthDayFormat` (the current culture's short date pattern without the year, from `MonthDayPattern`), `From(Person)` and `ToPerson()`, which trims the name and the nameday and turns empty values into `null` (FR-005, FR-006, FR-007, FR-016)
- [X] T013 [US1] Create the editor's markup `Anniversary.Shared/Views/PeopleEditorWindow.axaml`: title "Edit People", the app icon, 700 × 400 (at least 520 × 280), not in the taskbar, centred on the owner; a toolbar with **+ Add** and **− Remove**; a `DataGrid` of `PersonRow`s (compiled bindings, no generated columns, resizable columns, extended selection, horizontal grid lines) with **Name** (text, `2*`), **Nameday** (a `CalendarDatePicker` on `NamedayDate` with `MonthDayFormat` as its custom format and placeholder, 105 wide) and **Birthday** (a short-date `CalendarDatePicker`, 125 wide); a footer with the muted `FooterText`, **Save** (`cmd accent`) and **Cancel** (`cmd`, `IsCancel`) (FR-002, FR-006, FR-007, FR-013)
- [X] T014 [US1] Implement the editor in `Anniversary.Shared/Views/PeopleEditorWindow.axaml.cs`: the constructor takes the book, keeps `SavePathFor()` as the target and fills an `ObservableCollection<PersonRow>` from `book.People` (the copy); `UpdateState()` shows "{count} people · {path}" whenever rows or values change; `OnAdd` appends a row, selects it, scrolls to it and starts editing its Name; `OnRemove` removes the selected rows; `OnSave` commits the edit, calls `SavePeople` with the rows' `ToPerson()` and closes with `true`; a parameterless constructor serves the XAML previewer (FR-003, FR-004, FR-013, FR-014)
- [X] T015 [P] [US1] Add **Edit People…** to the commands menu (between separators, after Find Next) and **File ▸ Edit People…** to the macOS `NativeMenu` in `Anniversary.Shared/Views/MainWindow.axaml` (FR-001)
- [X] T016 [US1] Wire the editor in `Anniversary.Shared/Views/MainWindow.axaml.cs`: `OnEditPeople` and the macOS item, found by its header, call `EditPeople()`, which shows `PeopleEditorWindow` as a dialog and calls `Refresh()` when it returns `true` (FR-001, FR-014, FR-018)
- [X] T017 [US1] Check scenario 1 of `specs/002-people-editor/quickstart.md` by hand (docs/spec.md §12, criterion 10)

**Checkpoint**: People can be added, changed, removed and saved; the main window shows them at once

---

## Phase 4: User Story 2 - Edit a person's anniversaries (Priority: P2)

**Goal**: The Edit… button in a row opens the Anniversaries dialog, whose OK updates that person in the editor

**Independent Test**: Give a person two anniversaries, click OK and Save: the row and the main window show both; a date
used twice disables OK (quickstart scenario 2)

### Tests for User Story 2

- [X] T018 [P] [US2] Add `AnniversariesSummaryIsOrderedByDate`, `DuplicateAnniversaryDatesAreInvalid` (the second row gets the date error; with another date, `Validate` passes and `ToDictionary` has two entries) and `AnniversaryNeedsDateAndName` to `Anniversary.Tests/EditorRowTests.cs`

### Implementation for User Story 2

- [X] T019 [P] [US2] Create the editable row `Anniversary.Shared/Views/AnniversaryRow.cs` (`INotifyPropertyChanged`, `INotifyDataErrorInfo`, XML docs): `Date` (`DateTime?`, required and unique: "Date is required.", "This date is used twice."), `Name` (required: "Anniversary is required."), `HasErrors`, `From(anniversaries)` ordered by date, `Validate(rows)`, which marks the rows that share a date and returns whether all are valid, and `ToDictionary(rows)` with trimmed descriptions (FR-009)
- [X] T020 [US2] Create the dialog's markup `Anniversary.Shared/Views/AnniversariesDialog.axaml`: 400 wide, height to content, not resizable, not in the taskbar, centred on the owner; **+ Add** and **− Remove**; a `DataGrid` of `AnniversaryRow`s, 160 high, without sorting, with **Date** (a short-date `CalendarDatePicker`, 140 wide) and **Anniversary** (text); the muted `ErrorText`; **OK** (`cmd accent`, `IsDefault`) and **Cancel** (`IsCancel`) on the right (FR-009, FR-010)
- [X] T021 [US2] Implement the dialog in `Anniversary.Shared/Views/AnniversariesDialog.axaml.cs`: the title "Anniversaries – {Name}" ("Anniversaries" for a blank name); rows copied with `AnniversaryRow.From`; `Validate()` after every change enables OK and shows "Every anniversary needs a date and a name, and each date can be used only once." while invalid; Add appends and selects a row, Remove removes the selected rows; OK commits the edit, checks again, sets `Anniversaries` from `ToDictionary` and closes with `true`; Cancel closes with `false`; a parameterless constructor serves the XAML previewer (FR-009, FR-010)
- [X] T022 [US2] Add `AnniversariesSummary` (`"{description} ({date:d})"` ordered by date and joined by `", "`) and `HasAnniversaries` to `Anniversary.Shared/Views/PersonRow.cs`, both raised when `Anniversaries` changes (FR-008)
- [X] T023 [US2] Add the **Anniversaries** column (`3*`, sorted by `AnniversariesSummary`) to `Anniversary.Shared/Views/PeopleEditorWindow.axaml`, with the summary cut by an ellipsis, a muted italic "none" when there are none and an **Edit…** button; `OnEditAnniversaries` in `Anniversary.Shared/Views/PeopleEditorWindow.axaml.cs` opens `AnniversariesDialog` for the row and, on OK, sets the row's `Anniversaries` (FR-008, FR-010)
- [X] T024 [US2] Check scenario 2 of `specs/002-people-editor/quickstart.md` by hand (criterion 10)

**Checkpoint**: Anniversaries are edited in their dialog and saved with their person

---

## Phase 5: User Story 3 - Catch mistakes and discard unwanted changes (Priority: P2)

**Goal**: Invalid entries can't be saved, Cancel throws away every change, and a failed save loses nothing

**Independent Test**: Blank a name: Save is disabled; make changes and Cancel: nothing changes; save into a folder that
can't be written: a message, and the editor stays open (quickstart scenario 3)

### Tests for User Story 3

- [X] T025 [P] [US3] Add `BlankNameIsAnErrorUntilFixed`, `InvalidNamedayIsAnErrorUntilFixed` (`24-06`), `ErrorsChangedIsRaisedWhenAValueChanges` and `ClearedOrInvalidNamedayHasNoDate` (`1-5` has no date and an error on `NamedayDate`; clearing it makes the row valid) to `Anniversary.Tests/EditorRowTests.cs`

### Implementation for User Story 3

- [X] T026 [US3] Validate in `Anniversary.Shared/Views/PersonRow.cs`: implement `INotifyDataErrorInfo` with "Name is required." for a blank name and "Use MM-dd, e.g. 06-24." when `AnniversaryBook.IsValidNameday` rejects the nameday (reported for `Nameday` and `NamedayDate`), and `HasErrors`; every change raises `ErrorsChanged` and a change of `HasErrors` (FR-005, FR-006, FR-011)
- [X] T027 [US3] Keep invalid people from being saved in `Anniversary.Shared/Views/PeopleEditorWindow.axaml.cs`: `UpdateState()` disables Save while any row has errors, and `OnSave` checks again after committing the edit (FR-011)
- [X] T028 [US3] Handle a failed save and Cancel in `Anniversary.Shared/Views/PeopleEditorWindow.axaml.cs`: for `IOException` or `UnauthorizedAccessException` from `SavePeople`, show "Could not save the data file: {message}" (title "Edit People") with `MessageDialog.Show` and stay open; `OnCancel` closes with `false` without asking; Save stays without `IsDefault`, so Enter only commits a cell edit (FR-014, FR-017)
- [X] T029 [US3] Check scenario 3 of `specs/002-people-editor/quickstart.md` by hand (criterion 10)

**Checkpoint**: Nothing invalid or unwanted reaches the data file

---

## Phase 6: User Story 4 - Find people in a long list (Priority: P3)

**Goal**: Filter by name and sort by any column without changing the saved order

**Independent Test**: Type part of a name into the filter and click column headers: the view changes; Save: the file
keeps its order (quickstart scenario 4)

### Implementation for User Story 4

- [X] T030 [US4] Add the filter box (`FilterBox`, 180 wide, placeholder "Filter by name") on the right of the toolbar, enable `CanUserSortColumns`, and give the template columns their sort keys (`Nameday`, `Birthday`) in `Anniversary.Shared/Views/PeopleEditorWindow.axaml` (FR-012)
- [X] T031 [US4] Show the rows through a `DataGridCollectionView` in `Anniversary.Shared/Views/PeopleEditorWindow.axaml.cs`, sorted by Name at first; `MatchesFilter` keeps the rows whose name contains the trimmed filter text (current culture, ignoring case); `ApplyFilter` commits any edit and refreshes the view only when the text really changed, because `DataGridCollectionView.Refresh()` throws during an edit; Add clears the filter first so the new row is visible; Add and Remove change the underlying collection, whose order is the file order (FR-004, FR-012)
- [X] T032 [US4] Check scenario 4 of `specs/002-people-editor/quickstart.md` by hand

**Checkpoint**: All four user stories work

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and the final checks. The spec, the plan and the mocks were first written while specifying,
before the code (session 2026-10-09).

- [X] T033 [P] Describe the editor in `docs/spec.md` (then `specs/spec.md`): §2 (the DataGrid package), §4 (the view files and the two test classes), 5.1 (people are added with the editor), 5.6 "Saving the people file", 6 ("Editing support"), 7.2 (Edit People… in the commands menu and the macOS File menu), 7.8 and 7.9 with links to the mocks, 9 (`SaveTests`, `EditorRowTests`), 11 (C9: the people file is written only through the editor; C14), 12 (criterion 10) and 13 (step 1's early check of the `DataGrid` APIs, and step 8)
- [X] T034 [P] Update `docs/plan.md` (then `specs/plan.md`): the Decisions row "People editor" and work item 10, **people-editor** (ticked off later, with 005)
- [X] T035 [P] Add the mocks `docs/screens/people-editor.light.preview.png`, `docs/screens/people-editor.dark.preview.png` and `docs/screens/anniversaries-editor.light.preview.png` (then in `specs/screens`), exported from the HTML mock-up `docs/screens/mockup.html` while specifying, before the code; the Anniversaries dialog has no minimize and maximize buttons, because it has a fixed size
- [X] T036 [P] Describe the editor in `README.md`: in the files table, the people are edited with **⋯ ▸ Edit People…** and the app writes `Anniversary.Data.json` when you save in the editor; the Mac check list says "Edit People saves"
- [X] T037 [P] Record the editor in the handover (at the move to GitHub `specs/copilot/handover-linux.md`, now `docs/copilot/handover.md`): Edit People… in the command list, the decision that `PersonRow` and `AnniversaryRow` are the only extra types, the `DataGridCollectionView.Refresh()` gotcha, and the editor in the Linux and macOS check lists
- [X] T038 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: with the rest of phase 1, 78 tests pass, 30 of them in `Anniversary.Tests/SaveTests.cs` and `Anniversary.Tests/EditorRowTests.cs`
- [X] T039 Run the app on Windows, where phase 1 was built, tested and run live; the macOS menu-bar item and saving on Linux stay on the handover's check lists (see the platform checks in `specs/002-people-editor/quickstart.md`), and the maintainer later reported the app working on both (session 2026-10-10)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–6)**: depend on Foundational; User Stories 2–4 also build on the editor of User Story 1
- **Polish (Phase 7)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after User Story 1's `PersonRow` and editor window (T012–T014), to which it adds a column
- **User Story 3 (P2)**: after User Story 1's `PersonRow` and editor (T012, T014); independent of User Story 2
- **User Story 4 (P3)**: after User Story 1's editor (T013, T014); independent of User Stories 2 and 3

### Within Each User Story

- Tests first, then the row classes, then the markup and code-behind, then the manual check from quickstart.md
- A window's `.axaml` and `.axaml.cs` change together: the markup names the handlers, and the code uses the generated
  control fields; with compiled bindings, the markup also needs its row class

### Parallel Opportunities

- T002, T004 and T005 next to each other
- T010 next to T006–T009 (the tests are another file)
- In each story, the test task (T011, T018, T025) next to the row class, and T015 (the main window's menu items) next to
  the editor; T019 next to T018
- T033–T037 together: five different documents or folders

---

## Parallel Example: User Story 1

```bash
# The tests, the row class and the menu items are separate files:
Task: "Add the PersonRow round-trip and nameday tests to Anniversary.Tests/EditorRowTests.cs"
Task: "Create Anniversary.Shared/Views/PersonRow.cs"
Task: "Add Edit People… to the commands menu and the macOS File menu in Anniversary.Shared/Views/MainWindow.axaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (a person added on a fresh install)

### Incremental Delivery

1. Setup and Foundational: `AnniversaryBook` saves people safely and knows where
2. User Story 1: the editor with Add, Remove and Save (the MVP)
3. User Story 2: the Anniversaries dialog
4. User Story 3: validation, Cancel and failed saves
5. User Story 4: filter and sort
6. Polish: documentation, the full test run and the check on Windows

### What happened

On the evening of 2026-10-09 the maintainer asked whether a simple editor was possible and described it. The assistant
drew mocks of both windows and added the editor to the spec and plan (7.8, 7.9, the save logic in section 6, the tests,
the menu entries, C9, acceptance criterion 10, the file layout and the implementation order); the maintainer approved it
("Yes, let's do it. I like it a lot."). It was then built on Windows as step 8 of the implementation order, after the
core app, with the two test classes; the session excerpts end before the implementation, so its course isn't recorded.
The code reached this repository with the rest of phase 1 in `67a4092`.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names are as they were at the time; `SaveTests` and `EditorRowTests` haven't changed since
- The documentation paths are today's; at the time they were under `specs/`
