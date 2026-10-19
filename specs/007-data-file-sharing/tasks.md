# Tasks: Data File Sharing

**Input**: Design documents from `specs/007-data-file-sharing/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic, file format and settings changes: the
16 new test cases are in `Anniversary.Tests/DataFileTests.cs`. The native dialogs, the menus and the questions are
checked by hand with [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. Everything went into one commit, `bb8676b` ("Adding support for
external/shared files", 2026-10-10).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): views and logic in `Anniversary.Shared/`, tests in `Anniversary.Tests/`, documentation in
  `README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and the test class

- [X] T001 Start from the state 006 left: `Anniversary.slnx` building without warnings and 165 tests passing with `dotnet test --project Anniversary.Tests`
- [X] T002 [P] Create `Anniversary.Tests/DataFileTests.cs`: a sealed `IDisposable` test class with the summary "Tests of choosing, sharing and watching the data file (spec 5.3–5.6, 7.11)", two `TempFolder`s (`_appData` and `_cloud`, a stand-in for a cloud-drive folder) deleted in `Dispose`, and the `ChangeElsewhere` helper, which writes new content and sets the last-write time a minute later, like another computer's save synced by the cloud app

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Remember a chosen data file per computer, load the people from it, and track the file in use

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 [P] Add `string? DataFile` to `UserSettings` in `Anniversary.Shared/Model/UserSettings.cs` ("the full path of the data file chosen with Open or Save As, or `null` to use the configured one"), saved as `dataFile` in `usersettings.json` (FR-006)
- [X] T004 Add `Load(string? preferredCalendar, string? preferredDataFile, IReadOnlyList<string> folders)` to `Anniversary.Shared/AnniversaryBook.cs`: the people come from `preferredDataFile` unless it is `null` or blank, otherwise from `Config.DataFile`; the `params` overload passes no data file, and the private constructor takes `IEnumerable<string>` (FR-006, FR-007)
- [X] T005 Track the data file in use in `Anniversary.Shared/AnniversaryBook.cs`: `DataFilePath` now means the file in use (loaded from or saved to, or a chosen or absolute file that was missing or couldn't be read), so `SavePathFor()` and the editor save into it; add `DataFileLoadFailed` and the private `_dataFileStamp` (last-write time in UTC and size from `StampOf(path)`, `null` when there is no file), set by `LoadPeople(name)` and by `SavePeople`, which already set `DataFilePath` (002) and now also clears `DataFileLoadFailed` and renews the stamp (FR-008, FR-010)
- [X] T006 Pass the remembered file at start-up in `Anniversary.Shared/App.axaml.cs`: `AnniversaryBook.Load(settings.Calendar, settings.DataFile, [AppContext.BaseDirectory, appData])` (FR-006)
- [X] T007 [P] Add `ChosenDataFileWinsOverTheConfiguredOne` (the chosen file wins over `dataFile` from the config and is `DataFilePath` and the save target, with no warnings; renamed `ChosenDataFileIsUsedInsteadOfTheDefaultOne` by 012) and `UserSettingsRemembersDataFile` (saving and loading the settings keeps `DataFile`) to `Anniversary.Tests/DataFileTests.cs`

**Checkpoint**: A data file named in `usersettings.json` loads at start-up and is the save target; user stories can
begin

---

## Phase 3: User Story 1 - Open a data file someone sent (Priority: P1) 🎯 MVP

**Goal**: ⋯ ▸ Data File ▸ Open… (macOS File ▸ Open Data File…) uses a picked data file where it is and remembers it

**Independent Test**: On a computer without Anniversary data, open a data file from another folder: its people appear
at once and after a restart, and a person added in Edit People… is saved into that file (quickstart scenario 1)

### Tests for User Story 1

- [X] T008 [P] [US1] Add `OpenDataFileUsesTheFileWhereItIs` (after Open… and a save, the shared file holds the new person, the birthday shows "Babička (76)" on 14 March 2026, and no `Anniversary.Data.json` appears in the app-data folder), `OpenDataFileRejectsOtherFilesAndChangesNothing` (invalid JSON, a calendar, a `data` that isn't a list and a JSON array each throw `JsonException`, and the file in use and the people stay) and `OpenDataFileReportsAMissingFile` (`IOException`; `DataFilePath` stays `null`) to `Anniversary.Tests/DataFileTests.cs`

### Implementation for User Story 1

- [X] T009 [US1] Add `OpenDataFile(string path)` to `Anniversary.Shared/AnniversaryBook.cs`: read the file; check with `JsonDocument` (comments and trailing commas allowed) that the root is an object with a `data` array, matched in any letter case, otherwise throw `JsonException("This isn't an Anniversary data file (it has no \"data\" list).")`; then replace the people, set `DataFilePath`, clear `DataFileLoadFailed`, take the stamp and rebuild the index, so a failure changes nothing (FR-003, FR-004)
- [X] T010 [P] [US1] Add the **Data File** submenu with **Open…** (`OpenDataFileItem`) below Edit People… in the commands menu, and a separator and **Open Data File…** (`Gesture="Meta+O"`) after Edit People… in the macOS File menu, in `Anniversary.Shared/Views/MainWindow.axaml` (FR-001)
- [X] T011 [US1] Implement Open… in `Anniversary.Shared/Views/MainWindow.axaml.cs`: the `DataFileType` file type ("Anniversary data": `*.json`, `public.json`, `application/json`); `OpenDataFile()` with `StorageProvider.OpenFilePickerAsync` (title "Open Data File", that type plus all files) and `TryGetLocalPath()`; for `JsonException`, `IOException`, `UnauthorizedAccessException` or `NotSupportedException` the message "Could not open the data file: {message}" (title "Open Data File"); on success `UseDataFile(path)`, which saves `UserSettings.DataFile` and refreshes the window (FR-003, FR-004, FR-006)
- [X] T012 [US1] Wire Open… in `Anniversary.Shared/Views/MainWindow.axaml.cs`: the `OnOpenDataFile` handler, the macOS "Open Data File…" item found by its header, the gesture text Ctrl+O (⌘O on macOS) and Ctrl or ⌘ + O in `OnKeyDown` (FR-001, FR-002)
- [X] T013 [US1] Check scenario 1 of `specs/007-data-file-sharing/quickstart.md` by hand (docs/spec.md §12, criterion 13)

**Checkpoint**: A family member can open the file they were sent and keep using it

---

## Phase 4: User Story 2 - Use one data file from several computers (Priority: P2)

**Goal**: Save As… moves the people to a shared file, and changes made elsewhere show up without a restart

**Independent Test**: Save As… into a cloud-drive folder, change that file elsewhere and switch back to the app: the
main window shows the change (quickstart scenario 2)

### Tests for User Story 2

- [X] T014 [P] [US2] Add `SavingToANewPathMakesItTheDataFile`, `MissingChosenFileIsReportedAndKept` (the warning "Data file not found: {path}", no people, `DataFileLoadFailed`, the path kept as the save target, not a change), `MissingFileIsLoadedOnceItAppears`, `FileChangedElsewhereIsReloaded` (the new person and their nameday on 24 June show; a second reload does nothing), `ReloadClearsOldWarnings`, `SavingIsNotAChange` and `DeletedFileIsNotAChange` to `Anniversary.Tests/DataFileTests.cs`

### Implementation for User Story 2

- [X] T015 [US2] Keep a missing chosen or absolute file as the data file in `LoadPeople(name)` in `Anniversary.Shared/AnniversaryBook.cs`: the warning `Data file not found: {name}`, `DataFileLoadFailed`, and `DataFilePath` set to the rooted path, so the file is saved there and picked up when it appears (FR-011)
- [X] T016 [US2] Add `HasDataFileChanged()` (the file exists and its last-write time or size differs from the stamp; a file that is gone isn't a change) and `ReloadPeopleIfChanged()` (if changed: clear `Warnings`, load the people again with the start-up warnings and return `true`) to `Anniversary.Shared/AnniversaryBook.cs` (FR-009, FR-010)
- [X] T017 [P] [US2] Add **Save As…** (`SaveDataFileAsItem`) to the Data File submenu and **Save Data File As…** (`Gesture="Meta+Shift+S"`) to the macOS File menu in `Anniversary.Shared/Views/MainWindow.axaml` (FR-001)
- [X] T018 [US2] Implement Save As… in `Anniversary.Shared/Views/MainWindow.axaml.cs`: `SaveDataFileAs()` with `StorageProvider.SaveFilePickerAsync` (title "Save Data File As", suggested name `Path.GetFileName(SavePathFor())`, default extension `json`, the data file type, `ShowOverwritePrompt`), `SavePeople(People, path)`, the message "Could not save the data file: {message}" (title "Save Data File As") for `IOException` or `UnauthorizedAccessException`, then `UseDataFile(path)`; the `OnSaveDataFileAs` handler, the macOS item, the gesture text Ctrl+Shift+S (⇧⌘S) and Ctrl or ⌘ + Shift + S in `OnKeyDown` (FR-002, FR-005, FR-006)
- [X] T019 [US2] Reload changes made elsewhere in `Anniversary.Shared/Views/MainWindow.axaml.cs`: `Activated += OnActivated`, which calls `ReloadPeopleIfChanged()` and, after a reload, refreshes the window and shows the warnings; `EditPeople()` does the same before it opens the editor (FR-009, FR-011)
- [X] T020 [US2] Check scenario 2 of `specs/007-data-file-sharing/quickstart.md` by hand (docs/spec.md §12, criterion 14)

**Checkpoint**: One file on a cloud drive works from several computers

---

## Phase 5: User Story 3 - Save without losing changes made elsewhere (Priority: P2)

**Goal**: The editor asks before overwriting a file that changed elsewhere or couldn't be loaded

**Independent Test**: Change the data file while Edit People… is open and click Save; start with a missing or broken
file and click Save in the editor: both times it asks first (quickstart scenario 3)

### Tests for User Story 3

- [X] T021 [P] [US3] Add `UnreadableFileKeepsItsPath` (the warning "Error loading data: …", no people, `DataFileLoadFailed`, and the file stays the save target) to `Anniversary.Tests/DataFileTests.cs`

### Implementation for User Story 3

- [X] T022 [US3] Keep an unreadable file as the data file in `LoadPeople(name)` in `Anniversary.Shared/AnniversaryBook.cs`: the warning `Error loading data: {message}`, `DataFileLoadFailed`, and `DataFilePath` set to the file that was read (FR-012)
- [X] T023 [P] [US3] Add `Confirm(Window owner, string title, string message, string confirmText)` to `Anniversary.Shared/Views/MessageDialog.cs`: the same small window with `{confirmText}` and **Cancel** on the right (spacing 8), Cancel being `accent`, default and cancel; it returns `true` only for the confirm button; `Show` now uses the shared `CreateButton` and `CreateWindow` helpers (FR-015)
- [X] T024 [US3] Check the file before saving in `OnSave` in `Anniversary.Shared/Views/PeopleEditorWindow.axaml.cs`: if `HasDataFileChanged()`, ask **Overwrite** ("The data file was changed elsewhere, for example on another computer, after it was loaded: … Saving replaces those changes with yours."); otherwise, if `DataFileLoadFailed`, ask **Save Anyway** ("The data file couldn't be loaded, so its people aren't in this list: … If it's on a cloud drive that hasn't finished syncing, cancel and try again later."); title "Edit People"; Cancel keeps the editor open (FR-013, FR-014)
- [X] T025 [US3] Check scenario 3 of `specs/007-data-file-sharing/quickstart.md` by hand (docs/spec.md §12, criterion 14)

**Checkpoint**: No save silently replaces changes made elsewhere or a file that couldn't be loaded

---

## Phase 6: User Story 4 - Find the data file (Priority: P3)

**Goal**: Show in Folder opens the data file's folder in the file manager

**Independent Test**: Choose Show in Folder: the folder opens, or a message says it doesn't exist yet (quickstart
scenario 4)

### Implementation for User Story 4

- [X] T026 [US4] Add **Show in Folder** to the Data File submenu and **Show Data File in Finder** to the macOS File menu in `Anniversary.Shared/Views/MainWindow.axaml` (FR-001)
- [X] T027 [US4] Implement Show in Folder in `Anniversary.Shared/Views/MainWindow.axaml.cs`: `ShowDataFile()` opens the folder of `SavePathFor()` with `Launcher.LaunchDirectoryInfoAsync`, or, if that folder doesn't exist, shows "The data file's folder doesn't exist yet:" and the folder (title "Data File"); the `OnShowDataFile` handler and the macOS item (FR-016)
- [X] T028 [US4] Check scenario 4 of `specs/007-data-file-sharing/quickstart.md` by hand

**Checkpoint**: All four user stories work

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and the final checks

- [X] T029 [P] Update `docs/spec.md` (then `specs/spec.md`): 5.3 (a chosen data file wins and is used as is), 5.4 (the results for a missing or unreadable data file), 5.5 (renamed "User settings (theme, calendar and data file)", `DataFile`, one path per computer), 5.6 (the target path and change tracking), 6 ("Data file support" and the `DataFilePath` comment), 7.2 (the menu row, the shortcuts and the macOS File menu), 7.6 (`Confirm`), 7.8 (the questions before saving), the new 7.11 "Data File menu (sharing the people)", 8 (the start-up call), 9 (`DataFileTests`), 11 (C20) and 12 (criteria 13 and 14)
- [X] T030 [P] Update `docs/plan.md` (then `specs/plan.md`): the Decisions row "Sharing the data file" (where it is, the user's choice over asking or a separate Import; per computer; no merging; double-click waits for the installers) and work item 14, **data-file** ✅
- [X] T031 [P] Update `docs/copilot/handover.md` (then `specs/copilot/handover.md`): 181 tests, the Data File commands in the command list, the data-files paragraph (a chosen file wins; open v4's file with Data File ▸ Open…; what the app writes), the decision "Data file sharing", and the Avalonia note that `NativeMenu.Items` holds separators too, so code filters with `OfType<NativeMenuItem>()`
- [X] T032 [P] Update `README.md`: the macOS File menu items, the files table (any file chosen with Open… works; `usersettings.json` holds the chosen data file), the new "Sharing your data" section (with family; on several computers) and "Moving from v4" (open the v4 file with Open…)
- [X] T033 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: 181 tests pass, 16 of them new in `Anniversary.Tests/DataFileTests.cs`
- [X] T034 Run the real windows headless to confirm the menus and the questions, and leave the native dialogs and Show in Finder, which can't run headless, to the manual checks in `specs/007-data-file-sharing/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–6)**: depend on Foundational; User Story 3 also uses T015 and T016 from User Story 2
- **Polish (Phase 7)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after Foundational; independent of User Story 1 (Save As… gives it a file to share)
- **User Story 3 (P2)**: after Foundational and User Story 2's `HasDataFileChanged()` (T016) and missing-file handling
  (T015)
- **User Story 4 (P3)**: after Foundational; uses `SavePathFor()` from 002 and no other story

### Within Each User Story

- Tests first, then `AnniversaryBook`, then the views, then the manual check from quickstart.md
- `MainWindow.axaml` and `MainWindow.axaml.cs` change together: the markup names the handlers, and the code uses the
  generated item fields

### Parallel Opportunities

- T002 next to T001
- T003 and T007 next to T004 and T005
- In each story, the test task and the markup or dialog task (T010, T017, T023) next to the `AnniversaryBook` tasks
- T029–T032 together: four different documents

---

## Parallel Example: User Story 1

```bash
# The tests and the menu markup can be written next to the logic:
Task: "Add the Open… tests to Anniversary.Tests/DataFileTests.cs"
Task: "Add OpenDataFile(string path) to Anniversary.Shared/AnniversaryBook.cs"
Task: "Add Data File ▸ Open… and Open Data File… to Anniversary.Shared/Views/MainWindow.axaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (a family copy)

### Incremental Delivery

1. Setup and Foundational: a data file named in `usersettings.json` is used
2. User Story 1: Open… (the family copy)
3. User Story 2: Save As… and reloading (several computers)
4. User Story 3: the questions before saving
5. User Story 4: Show in Folder
6. Polish: documentation, the full test run and the headless check

### What happened

The maintainer described both use cases and asked to discuss them; the agreed approach (the file used where it is, per
computer) was built in one session with the four stories, the tests and the documentation together, and the
maintainer committed it as `bb8676b`. The **Save Anyway** question for an unreadable file went beyond the agreed plan
and was kept.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names are as they were at the time; 012 later renamed one and changed how the tests load settings
- The documentation paths are today's; at the time they were under `specs/`
