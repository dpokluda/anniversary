# Tasks: Single Settings File

**Input**: Design documents from `specs/012-single-settings-file/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic, file format and settings changes: the
settings tests are in `Anniversary.Tests/UserSettingsTests.cs` and `Anniversary.Tests/LoadTests.cs`, and the other
test classes now load with settings objects instead of writing `appsettings.json`. The hand edit and the message are
checked by hand with [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. Everything went into one commit, `9c89279` ("Tweaking icon size and
consolidating config files", 2026-10-11), together with an icon change that belongs to 010 and isn't listed here.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US3)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): the model, logic and views in `Anniversary.Shared/`, tests in `Anniversary.Tests/`,
  documentation in `README.md`, `Anniversary.Desktop/README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and the investigation the maintainer asked for

- [X] T001 Start from the state of 2026-10-10 (011 and "Closing the 5.0 plan"): `Anniversary.slnx` building without warnings and 203 tests in `Anniversary.Tests` (200 pass and the 3 Windows-only registry tests skip elsewhere)
- [X] T002 Investigate which file each setting came from, as the maintainer asked: `UserSettings.Load` in `Anniversary.Shared/Model/UserSettings.cs` read the theme, calendar and data file from the app-written `usersettings.json`, and `LoadConfig` in `Anniversary.Shared/AnniversaryBook.cs` read `calendarFile`, `dataFile`, `numberOfDaysAhead` and `displayYearsOnBirthdays` from `appsettings.json` (next to the executable, then in the app-data folder, then built in); the menu choices won, so renaming `usersettings.json` to `appsettings.json` kept the data file but lost the calendar (the calendar key there is `calendarFile`, not `calendar`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: One settings object with the two v4 options, shared by the book, the main window and the tests

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Add `NumberOfDaysAhead` (`int`, default 7, "the number of days shown in the "Next N days" section") and `DisplayYearsOnBirthdays` (`bool`, default `true`, "whether birthdays and anniversaries show the number of years") to `UserSettings` in `Anniversary.Shared/Model/UserSettings.cs`; the class summary becomes "All the settings (spec 5.5) … people may also edit it, with the app closed" (FR-001, FR-002)
- [X] T004 Replace `Load(string? preferredCalendar, string? preferredDataFile, IReadOnlyList<string> folders)` with `Load(UserSettings settings, IReadOnlyList<string> folders)` in `Anniversary.Shared/AnniversaryBook.cs`: the private constructor takes the settings and keeps them as the new `Settings` property ("the main window shares the same object"), and the `params` overload loads `new UserSettings { Calendar = preferredCalendar }`; the class summary now says "Holds the settings, the name-day calendar and the people" (FR-001)
- [X] T005 Take the two options from `Settings` in `Anniversary.Shared/AnniversaryBook.cs`: `GetDay` looks `Settings.NumberOfDaysAhead` days ahead (still stopping at 31 December 9999) and `ShowYears` checks `Settings.DisplayYearsOnBirthdays` (FR-003)
- [X] T006 Pass one settings object at start-up in `Anniversary.Shared/App.axaml.cs` (`AnniversaryBook.Load(settings, [AppContext.BaseDirectory, appData])`, then the same `settings` to `MainWindow`), and in `Anniversary.Shared/Views/MainWindow.axaml.cs` caption the last section `Next {book.Settings.NumberOfDaysAhead} days` and document the `settings` parameter as "the same object the book was loaded with; saved when a menu choice changes" (FR-003, FR-006)
- [X] T007 [P] Add `Settings(calendar, dataFile, daysAhead, displayYears)` to `Anniversary.Tests/TestFiles.cs` (by default the test calendar and data file, 7 days and years shown) and make `TestFiles.Book` load with it instead of writing `appsettings.json`, so `DayTests` (years turned off, fewer days ahead) runs unchanged through the settings; the helper's summary now says it builds "the JSON files the tests load (calendar and people) and the settings that use them"
- [X] T008 Load with settings objects in the existing tests: `Anniversary.Tests/DataFileTests.cs` (`AnniversaryBook.Load(new UserSettings { DataFile = … }, [_appData.FolderPath])`) and `Anniversary.Tests/LoadTests.cs` (`TestFiles.Settings()` in `FirstFolderWinsOverSecond`, `FallsBackToSecondFolder` and `InvalidDataFileGivesWarningAndNoPeople`)

**Checkpoint**: The book, the main window and the tests share one `UserSettings` object with all five settings; user
stories can begin

---

## Phase 3: User Story 1 - Set the two v4 options in the settings file (Priority: P1) 🎯 MVP

**Goal**: `numberOfDaysAhead` and `displayYearsOnBirthdays`, set in `usersettings.json` with the app closed, apply at
the next start

**Independent Test**: Set 14 days and no years in `usersettings.json` with the app closed and start the app: "Next 14
days" and no years (quickstart scenario 1)

### Tests for User Story 1

- [X] T009 [P] [US1] In `Anniversary.Tests/UserSettingsTests.cs`, extend `MissingFileGivesDefaults` (System, no calendar or data file, 7 days, years shown, no `LoadError`) and `SavedValuesLoadAgain` (Dark, Slovak, a data file path, 14 days and no years come back after `Save` and `Load`), and add `CommentsAndTrailingCommasAreAllowed` (a `//` comment and a trailing comma; 14 days and no years are read, with no `LoadError`), which takes over from `ConfigAllowsCommentsAndTrailingCommas` in `LoadTests`

### Implementation for User Story 1

- [X] T010 [US1] Read hand edits in `Anniversary.Shared/Model/UserSettings.cs`: add `ReadCommentHandling = JsonCommentHandling.Skip` and `AllowTrailingCommas = true` to the serializer options, next to the case-insensitive camelCase names and the string enum converter (FR-004)
- [X] T011 [US1] Check scenario 1 of `specs/012-single-settings-file/quickstart.md` by hand (docs/spec.md §12, criterion 6)

**Checkpoint**: The two v4 options work from `usersettings.json`

---

## Phase 4: User Story 2 - Find every setting in one file (Priority: P2)

**Goal**: One file with all five settings, created at the first start and written whole on every menu choice;
`appsettings.json` is no longer read

**Independent Test**: Start with no app-data folder: `usersettings.json` appears with five keys, and an
`appsettings.json` next to it changes nothing (quickstart scenario 2)

### Tests for User Story 2

- [X] T012 [P] [US2] Add `SaveWritesEveryKey` to `Anniversary.Tests/UserSettingsTests.cs`: saving default settings writes `theme`, `calendar`, `dataFile`, `numberOfDaysAhead` and `displayYearsOnBirthdays`, with `"numberOfDaysAhead": 7`
- [X] T013 [P] [US2] Rework `Anniversary.Tests/LoadTests.cs` for loading without `appsettings.json`: add `DataFileNameInSettingsIsFoundInTheFolders` (`Family.Data.json` in the app-data folder is loaded and is the save target, with no warnings), `MissingDataFileIsReportedAndSavedToTheAppDataFolder` (`Data file not found: Missing.Data.json`, no people, the save target in the app-data folder) and `SwitchingToAMissingOrInvalidCalendarIsReported` (through `UseCalendar`: "Calendar file not found: …", then "Error loading calendar: …"); remove the five tests that relied on `appsettings.json` (`AbsolutePathInConfigIsUsed`, `InvalidCalendarFileGivesWarningAndEmptyCalendar`, `MissingNonDefaultFilesAreReported`, `InvalidConfigUsesDefaultsAndWarns` and `ConfigAllowsCommentsAndTrailingCommas`); `FallsBackToBuiltInDefaults` no longer checks `Config.CalendarFile`
- [X] T014 [P] [US2] Adapt `Anniversary.Tests/CalendarTests.cs` to calendars without a configuration file: `ConfiguredCalendarIsUsedWhenPreferredIsGone` becomes `ChosenCalendarThatIsGoneFallsBackToTheRegion` (`sk-SK` gives Slovak), `CalendarForTheRegionIsUsedWhenNoneIsChosenOrConfigured` becomes `CalendarForTheRegionIsUsedWhenNoneIsChosen`, `ChosenAndConfiguredCalendarsWinOverTheRegion` becomes `ChosenCalendarWinsOverTheRegion`, and `CalendarGivenByAbsolutePathIsListed` switches to the calendar with `UseCalendar(path)` and checks its name on 9 October 2026
- [X] T015 [P] [US2] In `Anniversary.Tests/DataFileTests.cs`, rename `ChosenDataFileWinsOverTheConfiguredOne` to `ChosenDataFileIsUsedInsteadOfTheDefaultOne` (the chosen file wins over an `Anniversary.Data.json` in the app-data folder) and give `ReloadClearsOldWarnings` its first warning from an unknown calendar key (`13-45`) instead of a missing configured calendar
- [X] T016 [US2] Remove `WriteConfig` from `Anniversary.Tests/TestFiles.cs` once no test writes `appsettings.json`

### Implementation for User Story 2

- [X] T017 [US2] Remove `Anniversary.Shared/Model/AppConfig.cs` and the built-in `Anniversary.Shared/Defaults/appsettings.json` (the `Defaults` folder, embedded as a whole, now holds only `Anniversary.Data.json`), and from `Anniversary.Shared/AnniversaryBook.cs` the `Config` property, `ConfigFileName` and `LoadConfig()`, so no `appsettings.json` is read anywhere (FR-007)
- [X] T018 [US2] Take the data file from the settings in `Anniversary.Shared/AnniversaryBook.cs`: the new private `DataFileName` (`Settings.DataFile` unless it is `null` or blank, else the new constant `DefaultDataFile`, `Anniversary.Data.json`) is what `Load` reads the people from and what `SavePathFor()` uses when no data file is in use (FR-008)
- [X] T019 [US2] Choose the calendar without a configuration step in `Load` in `Anniversary.Shared/AnniversaryBook.cs`: `Settings.Calendar` if it is among the discovered calendars, otherwise `CalendarForRegion(CultureInfo.CurrentCulture)`, which falls back to the US calendar; the `params` overload's documentation no longer mentions "the configured one" (FR-009)
- [X] T020 [US2] Create the file at the first start in `OnFrameworkInitializationCompleted` in `Anniversary.Shared/App.axaml.cs`: when `usersettings.json` doesn't exist after `UserSettings.Load`, call `settings.Save()` before applying the theme ("Every setting is then in the file, ready to edit") (FR-005)
- [X] T021 [US2] Document `DataFile` in `Anniversary.Shared/Model/UserSettings.cs` as "a full path chosen with Open or Save As, or a file name found in the lookup folders (spec 5.3); `null` for `Anniversary.Data.json`"; `Save()` still serializes the whole object, so each menu choice (`ChooseTheme`, `ChooseCalendar` and `UseDataFile` in `Anniversary.Shared/Views/MainWindow.axaml.cs`) writes all five keys (FR-006, FR-008)
- [X] T022 [US2] Check scenario 2 of `specs/012-single-settings-file/quickstart.md` by hand

**Checkpoint**: All settings are in one file, and `appsettings.json` is gone

---

## Phase 5: User Story 3 - Never lose a hand-edited file (Priority: P2)

**Goal**: A file the app can't read gives a warning and the defaults, and is never saved over

**Independent Test**: Put a typo into `usersettings.json`, start the app, change the theme and quit: the
"Configuration Error" message appeared, and the file is unchanged (quickstart scenario 3)

### Tests for User Story 3

- [X] T023 [P] [US3] In `Anniversary.Tests/UserSettingsTests.cs`, make `InvalidFileGivesDefaults` also expect a `LoadError`, and add `FileThatCouldNotBeReadIsNotSavedOver`: a file with `"numberOfDaysAhead": "14"` (a string) loads with a `LoadError` that names `numberOfDaysAhead` and without its `dataFile`, and after the theme is changed and saved, the file's text is unchanged
- [X] T024 [P] [US3] Add `SettingsThatCouldNotBeReadAreReported` to `Anniversary.Tests/LoadTests.cs`, which takes over from `InvalidConfigUsesDefaultsAndWarns`: a file with `"numberOfDaysAhead": "many"` gives one warning that starts with `Error loading configuration: {path}: ` and ends with a new line and `Using default settings.`, and the book's settings are the defaults (7 days and the System theme, although the file says Dark)

### Implementation for User Story 3

- [X] T025 [US3] Add `LoadError` (`[JsonIgnore]`, private setter) to `Anniversary.Shared/Model/UserSettings.cs` and set it in `Load`: a `JsonException`, `IOException` or `UnauthorizedAccessException` now keeps its message there (before, the "app-written" file silently started over with the defaults), and the defaults are returned with `FilePath` set (FR-010)
- [X] T026 [US3] Make `Save()` in `Anniversary.Shared/Model/UserSettings.cs` return without writing while `LoadError` is set ("The file couldn't be read, perhaps because of a typo made by hand; writing would lose what's in it."); other write failures stay ignored (FR-011)
- [X] T027 [US3] Report it in `Load` in `Anniversary.Shared/AnniversaryBook.cs`: when `settings.LoadError` is set, the first warning is `{ConfigurationWarningPrefix}: {settings.FilePath}: {settings.LoadError}`, a new line and `Using default settings.`; `ConfigurationWarningPrefix` ("Error loading configuration") is now documented as the prefix of warnings about the settings file, and `ShowWarnings` in `Anniversary.Shared/Views/MainWindow.axaml.cs` still shows such warnings titled "Configuration Error" after the window opens (FR-010)
- [X] T028 [US3] Check scenario 3 of `specs/012-single-settings-file/quickstart.md` by hand (docs/spec.md §12, criterion 6: a broken `usersettings.json` is left as it is)

**Checkpoint**: All three user stories work

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and the final checks

- [X] T029 [P] Update `docs/spec.md` (then `specs/spec.md`): 4 (no `AppConfig.cs`; `Defaults` with only `Anniversary.Data.json`; the `TestFiles.cs` line), 5.1 (settings are in `usersettings.json`; v4's `appsettings.json` is no longer read, and where its keys went; the bullet on the built-in `appsettings.json` removed), 5.2 (`UserSettings` replaces v4's `AppConfig`), 5.3 (the data file from the settings), 5.4 (the two rows for the settings file), 5.5 (renamed "Settings (`usersettings.json`)": one file, when the app writes it, editing with the app closed, the key table, `LoadError` and `Save()`), 5.6 (the target path), 5.7 (the active calendar in three steps), 6 (`Settings` and the new `Load`), 6.1 (`Settings.DisplayYearsOnBirthdays`, `Settings.NumberOfDaysAhead`), 7.2 ("N from the settings"), 8 (save the settings when the file is missing), 9 (`DayTests`, `LoadTests`, `UserSettingsTests`, `CalendarTests` and `DataFileTests`), 10 (moving from v4; the user guide covers `usersettings.json`), 11 (C5 and C9) and 12 (criterion 6)
- [X] T030 [P] Update `docs/plan.md` (then `specs/plan.md`): the new Decisions row "Settings" (one file, user decision of 11 October 2026; the two options file-only, the user's choice over menu items or a settings window; every key written and the file created at the first start; a file the app can't read gives a warning and is never saved over; why `appsettings.json` went) and the rows that named `calendarFile` or `appsettings.json` (Data files, Holiday calendars, Sharing the data file, Theme switching)
- [X] T031 [P] Update `docs/copilot/handover.md` (then `specs/copilot/handover.md`): 205 tests (202 pass and 3 skip outside Windows) in the status table and the test command, "all settings, spec 5.5" in the data-files paragraph, the decision "One settings file, `usersettings.json`", and "a menu choice wins" in the calendar decision
- [X] T032 [P] Update "Files and settings" in `Anniversary.Desktop/README.md`: `usersettings.json` holds all settings; the menus keep the theme, calendar and data file there; `numberOfDaysAhead` and `displayYearsOnBirthdays` are only in the file and are changed by quitting the app, editing the file and starting the app again, with a full example; after a typo the app says so, uses the default settings and leaves the file alone (FR-012)
- [X] T033 [P] Update `README.md`: the files table (no `appsettings.json` row; `usersettings.json` holds all settings and is written when a menu choice changes), a note that `usersettings.json` is only in the app-data folder and created at the first start and that v4's `appsettings.json` is no longer read, "Moving from v4" (open or copy the data file, without `appsettings.json`; set the two options in `usersettings.json` instead), and "Calendars" without the sentence about `calendarFile` (FR-012)
- [X] T034 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: 205 tests, of which 202 pass and 3 skip outside Windows; CI built the commit with warnings as errors and passed on Windows, macOS and Ubuntu

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–5)**: depend on Foundational; independent of each other
- **Polish (Phase 6)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after Foundational; independent of User Story 1 (every key is written whatever the reader
  accepts)
- **User Story 3 (P2)**: after Foundational; independent of the other stories (its test replaces one that User Story 2
  removes, T013)

### Within Each User Story

- Tests first, then `UserSettings`, then `AnniversaryBook` and `App`, then the manual check from quickstart.md
- T016 waits for T013–T015: `WriteConfig` goes once no test calls it
- T017 waits for T013: the tests of `appsettings.json` go before the file does

### Parallel Opportunities

- T007 next to T005 and T006
- The test tasks of a story next to each other: T012–T015; T023 and T024
- T029–T033 together: five different documents

---

## Parallel Example: User Story 2

```bash
# The test changes are in four different files and can be made together:
Task: "Add SaveWritesEveryKey to Anniversary.Tests/UserSettingsTests.cs"
Task: "Rework Anniversary.Tests/LoadTests.cs for loading without appsettings.json"
Task: "Adapt Anniversary.Tests/CalendarTests.cs to calendars without a configuration file"
Task: "Rename the chosen-data-file test in Anniversary.Tests/DataFileTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (14 days and no years)

### Incremental Delivery

1. Setup and Foundational: one settings object with all five settings
2. User Story 1: the two options set by hand
3. User Story 2: one file; `appsettings.json` gone
4. User Story 3: a file with a typo is reported and never saved over
5. Polish: documentation and the full test run

### What happened

The maintainer asked for v4's two options ("It could be just the config .json for now"). The assistant first answered
that nothing needed building, because v5 still read them from `appsettings.json`; the maintainer added them to
`usersettings.json` instead, then renamed that file to `appsettings.json`, lost the Czech calendar, and asked which
file was actually used. After the explanation he decided on one file (user decision, 11 October 2026). The change, its
tests and the documentation were committed as `9c89279`, together with the taller icon page (a follow-up to 010), and
CI passed.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names are as they were at the time; no later feature changed them
- The documentation paths are today's; at the time they were under `specs/`
