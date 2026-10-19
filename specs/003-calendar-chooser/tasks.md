# Tasks: Calendar Chooser

**Input**: Design documents from `specs/003-calendar-chooser/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic, file format and settings changes: the 10
test cases are in `Anniversary.Tests/CalendarTests.cs`. The generator checks its own output, and the menus are checked
by hand with [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. The work was done on 2026-10-09 in the old `pokluda` repository, which has
no history; it is first visible in `67a4092` ("Moving to GitHub from a private VSO repository", 2026-10-10).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US3)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): views and logic in `Anniversary.Shared/`, the executable's project in
  `Anniversary.Desktop/`, tests in `Anniversary.Tests/`, the calendars and their generator in `calendars/`,
  documentation in `README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and the test class

- [X] T001 Start from the phase-1 app of [001](../001-cross-platform-rewrite/spec.md) and [002](../002-people-editor/spec.md): v4's Czech calendar as the only one, chosen with `calendarFile` in `appsettings.json` ("Phase 1 ships only the Czech name-day calendar; another calendar can be selected with `calendarFile`", Data files row of `docs/plan.md`, then `specs/plan.md`), the ⋯ menu with Edit People… and Theme, and `usersettings.json` holding the theme
- [X] T002 [P] Create `Anniversary.Tests/CalendarTests.cs`: a sealed `IDisposable` test class with the summary "Tests of discovering and switching name-day calendars (spec 5.7).", two `TempFolder`s (`_exe` and `_appData`, standing for the executable's folder and the app-data folder) deleted in `Dispose`; its calendar files are written with `TestFiles.Calendar(days, name)` in `Anniversary.Tests/TestFiles.cs`, whose optional `name` adds the display name

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Calendar files with metadata, the generator's core with the Czech calendar, and the list of available
calendars

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 [P] Add the optional metadata to `CalendarData` in `Anniversary.Shared/Model/CalendarData.cs`: `Name`, `Locale` (e.g. "cs-CZ"), `Source` and `License`, all `string?` with XML docs, next to the v4 days in `[JsonPropertyName("calendar")] Data`; files without them still load (FR-008)
- [X] T004 [P] Add `Anniversary.Shared/CalendarInfo.cs`: `public sealed record CalendarInfo(string FileName, string DisplayName, string? Locale)`, documented as "Describes an available name-day calendar." (FR-001, FR-002)
- [X] T005 Create `calendars/Build-Calendars.ps1` (PowerShell 7, `#Requires -Version 7.0`, stops at the first error) with the parameters `-OutputFolder` and `-CzechSource`, `Test-DayKey` (an `MM-dd` key that is a real date in 2000) and `Write-Calendar`, which rejects invalid keys, empty values and fewer than 360 days, writes `name`, `locale`, `source`, `license` and the sorted `calendar` as UTF-8 without a byte-order mark, prints one line per calendar and checks that the file parses back with the same number of days; its Czech step writes `calendars/Anniversary.Calendar.Czech.json` from v4's Czech calendar, the names unchanged and the metadata "Czechia (Česko)", `cs-CZ`, the v4 source and a license added (FR-008, FR-011, FR-012)
- [X] T006 Embed the calendars in `Anniversary.Shared/Anniversary.Shared.csproj`: `..\calendars\Anniversary.Calendar.*.json` as `EmbeddedResource` with `LogicalName="%(Filename)%(Extension)"` (shown under `Calendars\` in the project), so a built-in calendar is found by its plain file name (spec 5.3 step 3) (FR-002)
- [X] T007 Discover the calendars in `Anniversary.Shared/AnniversaryBook.cs`: using the non-blank folders that `Load` keeps (`_folders`, which `UseCalendar` also needs later), `RefreshCalendars()` lists the files matching `Anniversary.Calendar.*.json` in each existing folder in order, then the embedded resources with such names, keeping the first of each file name (compared case-insensitively) and reading only its `name` and `locale` (`TryReadCalendarInfo`: a file that can't be read isn't offered; `DisplayNameFromFile` gives the part between `Anniversary.Calendar.` and `.json`); `Calendars`, sorted by display name with the current culture; and `CalendarFile`, set by `LoadCalendar(fileName)`; files are only read, never written (FR-002, FR-003, FR-007, FR-008, FR-009)

**Checkpoint**: The book lists the available calendars and knows the active one; user stories can begin

---

## Phase 3: User Story 1 - Choose the name-day calendar (Priority: P1) 🎯 MVP

**Goal**: ⋯ ▸ Nameday Calendar (macOS View ▸ Nameday Calendar) switches the calendar at once and remembers the choice

**Independent Test**: Choose Slovakia (Slovensko): 9 October shows "Dionýz", and after a restart Slovakia is still
checked (quickstart scenario 1)

### Tests for User Story 1

- [X] T008 [P] [US1] Add `PreferredCalendarIsUsedWhenAvailable` (Slovak: `CalendarFile` and "Dionýz" on 9 October 2026), `ConfiguredCalendarIsUsedWhenPreferredIsGone` (a removed calendar gives Czech and no warnings), `UseCalendarSwitchesCalendarButKeepsPeople` (Hungarian: "Dénes, János" on 9 October, `Find` of "István" as a whole word in the calendar only gives 20 August 2026, and Jan's nameday on 24 June stays), `UseCalendarReplacesOldWarnings` (a missing calendar, then Czech: no warnings) and `UserSettingsRemembersCalendar` (save and load keep `Calendar`) to `Anniversary.Tests/CalendarTests.cs`

### Implementation for User Story 1

- [X] T009 [P] [US1] Add `string? Calendar` to `UserSettings` in `Anniversary.Shared/Model/UserSettings.cs` ("the file name of the chosen name-day calendar, or `null` if none was chosen yet"), saved as `calendar` in `usersettings.json` next to `theme` (FR-005)
- [X] T010 [US1] Apply the remembered choice in `Load(string? preferredCalendar, params string[] folders)` in `Anniversary.Shared/AnniversaryBook.cs`: after the configuration, discover the calendars, then load `preferredCalendar` if a discovered calendar has that file name, otherwise `Config.CalendarFile` (default `Anniversary.Calendar.Czech.json`), without a warning about the remembered one (FR-005, FR-006)
- [X] T011 [US1] Add `UseCalendar(string fileName)` to `Anniversary.Shared/AnniversaryBook.cs`: clear `Warnings`, then load the calendar with the usual lookup (an absolute path, the folders in order, the embedded calendars) and warnings (`Calendar file not found: {name}`, `Error loading calendar: {message}`, an empty calendar); the people aren't touched (FR-004, FR-009)
- [X] T012 [US1] Pass the remembered calendar at start-up in `Anniversary.Shared/App.axaml.cs`: `AnniversaryBook.Load(settings.Calendar, AppContext.BaseDirectory, appData)` (FR-005)
- [X] T013 [P] [US1] Add the `CalendarMenu` item **Nameday Calendar** to the commands menu after Edit People… and a separator, before Theme, and **Nameday Calendar** with an empty `NativeMenu` above Appearance in the macOS **View** menu, in `Anniversary.Shared/Views/MainWindow.axaml` (FR-001)
- [X] T014 [US1] Fill and wire the lists in `Anniversary.Shared/Views/MainWindow.axaml.cs`: `_nativeCalendarMenu`, found by its header like the other native items; `UpdateCalendarMenus()`, run at start-up, fills both menus from `Calendars` with radio items (`GroupName` "Calendar" in the ⋯ menu), checking the one whose file name equals `CalendarFile` (case-insensitively); `ChooseCalendar(fileName)` calls `UseCalendar`, saves `UserSettings.Calendar` (the only file written), rebuilds the lists, refreshes the window and shows the new warnings (title "Data Error") (FR-001, FR-004, FR-005, FR-007, FR-009)
- [X] T015 [US1] Check scenario 1 of `specs/003-calendar-chooser/quickstart.md` by hand

**Checkpoint**: The user can choose among the available calendars, and the choice is remembered

---

## Phase 4: User Story 2 - Name days for more countries (Priority: P2)

**Goal**: Nine more countries' calendars from openly licensed sources, with their sources, licenses and notices

**Independent Test**: The menu lists ten calendars, the spot dates match, the published app has `THIRD-PARTY-NOTICES.md`
next to the executable, and a second run of the generator changes no file (quickstart scenario 2)

### Tests for User Story 2

- [X] T016 [P] [US2] Add `ShippedCalendarsAreDiscovered` to `Anniversary.Tests/CalendarTests.cs`: with no folders there are 10 calendars, Czech is shown as "Czechia (Česko)", Latvian has the locale `lv-LV`, and the list is sorted by display name

### Implementation for User Story 2

- [X] T017 [US2] Add the radoone/nameday-mcp step to `calendars/Build-Calendars.ps1`: the pinned commit `5d3f301f0796b2fca8048db8c092e9bd09504dfe` and eight calendars (`at`, `bg`, `fr`, `hu`, `it`, `pl`, `ru`, `sk` → Austrian, Bulgarian, French, Hungarian, Italian, Polish, Russian, Slovak) with their display names and locales; each day's names trimmed, empty ones dropped and the rest joined with ", ", a day without names left out; `source` the pinned file's URL and `license` "MIT, Copyright (c) 2024 Slovak Name Days MCP Server (see THIRD-PARTY-NOTICES.md)" (FR-011, FR-012)
- [X] T018 [US2] Add the Latvian step to `calendars/Build-Calendars.ps1`: the CSV of the State Language Centre's traditional list from data.gov.lv, `datums` (`dd.MM.`) turned into `MM-dd` and `vardadienas` trimmed, rows without names skipped; display name "Latvia (Latvija)", locale `lv-LV`, license `CC0-1.0` (FR-011, FR-012)
- [X] T019 [US2] Run `pwsh -File calendars/Build-Calendars.ps1`: it writes the ten `calendars/Anniversary.Calendar.*.json` files (Czech 366 days, Austrian 366, Bulgarian 366, French 365, Hungarian 364, Italian 365, Latvian 366, Polish 366, Russian 366, Slovak 362); a second run gives identical files, and the Czech names are identical to v4's (FR-011, FR-012)
- [X] T020 [P] [US2] Write `calendars/THIRD-PARTY-NOTICES.md`: the MIT license of radoone/nameday-mcp word for word ("Copyright (c) 2024 Slovak Name Days MCP Server"), with the files that use it, the pinned commit and the conversion; and the CC0 1.0 dedication of the Latvian list (FR-014)
- [X] T021 [P] [US2] Write `calendars/README.md`: "Generated. Don't edit by hand.", the command that re-creates the files, what the script validates, a table of the ten files (display name, days, source, license), how the sources were checked (radoone's Czech file agrees with v4 on 359 of 366 days; spot checks on well-known dates; native letters intact) and the countries not included, with the reason (FR-013)
- [X] T022 [US2] Ship the notices in `Anniversary.Desktop/Anniversary.Desktop.csproj`: `..\calendars\THIRD-PARTY-NOTICES.md` linked as `THIRD-PARTY-NOTICES.md` with `CopyToOutputDirectory` and `CopyToPublishDirectory` set to `PreserveNewest` and `ExcludeFromSingleFile`, so it lies next to the executable after a build and after a single-file publish (FR-014)
- [X] T023 [US2] Check scenario 2 of `specs/003-calendar-chooser/quickstart.md` by hand

**Checkpoint**: Ten calendars ship, each with its source and license

---

## Phase 5: User Story 3 - Add or replace a calendar with your own file (Priority: P3)

**Goal**: A user's `Anniversary.Calendar.{Name}.json` is offered as soon as the menu opens again, and replaces a shipped
calendar of the same name

**Independent Test**: Copy a test calendar file into the app-data folder, choose it, restart, remove it (quickstart
scenario 3; docs/spec.md §12, criterion 11)

### Tests for User Story 3

- [X] T024 [P] [US3] Add `UserCalendarIsAddedToShippedOnes` (a "Test" calendar in the app-data folder makes 11), `ExeFolderWinsOverAppDataWhichWinsOverEmbedded` (two `Anniversary.Calendar.Czech.json` copies: "Exe" on 9 October, one Czech entry, named "Exe Czech"), `DisplayNameComesFromFileNameWhenMetadataIsMissing` (`Anniversary.Calendar.Klingon.json` without `name` is "Klingon") and `CalendarGivenByAbsolutePathIsListed` (a `calendarFile` path is listed with its `name`) to `Anniversary.Tests/CalendarTests.cs`

### Implementation for User Story 3

- [X] T025 [US3] List the active calendar even when discovery doesn't find it, in `Anniversary.Shared/AnniversaryBook.cs`: `AddActiveCalendarIfMissing(data)`, called by `RefreshCalendars()` and `LoadCalendar`, adds `CalendarFile` (for example a full path from `calendarFile`) with its `name` or a name from the file, then sorts the list (FR-010)
- [X] T026 [US3] Rebuild the lists when the menu opens: `Opening="OnCommandsMenuOpening"` on the `CommandsMenu` flyout in `Anniversary.Shared/Views/MainWindow.axaml`, and `OnCommandsMenuOpening` in `Anniversary.Shared/Views/MainWindow.axaml.cs`, which calls `RefreshCalendars()` and `UpdateCalendarMenus()` ("Picks up calendar files added while the app runs (spec 7.10).") (FR-003)
- [X] T027 [US3] Check scenario 3 of `specs/003-calendar-chooser/quickstart.md` by hand (docs/spec.md §12, criterion 11)

**Checkpoint**: All three user stories work

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, the mocks and the final checks

- [X] T028 [P] Update `docs/spec.md` (then `specs/spec.md`): 1 (the non-goal "automatic choice of name-day calendar by OS locale"), 4 (`calendars/`, `CalendarInfo.cs`, the linked calendars), 5.1 (the optional metadata and the ten built-in calendars, Czech staying the default), 5.2 (`CalendarData` with the metadata), 5.3 (`Load` with the preferred calendar), 5.5 ("User settings (theme and calendar)", `Calendar`), the new 5.7 "Name-day calendars (choosing and adding)", 6 ("Calendar support" and `CalendarInfo`), 7.2 (the menu row and the macOS View menu), the new 7.10 "Nameday Calendar menu", 8 (`Load(settings.Calendar, …)`), 9 (`CalendarTests`), 10 (the notices copied on publish), 11 (C15), 12 (criterion 11) and 13 (step 9)
- [X] T029 [P] Update `docs/plan.md` (then `specs/plan.md`): the Decisions row "Name-day calendars" (the menu, where the files are found, the choice in `usersettings.json`, the ten calendars and their sources, the generator, the countries left out), work item 11 **calendar-chooser**, and the follow-up "one task per additional name-day calendar (license checked, every date verified against its source)"
- [X] T030 [P] Update the mocks in `docs/screens/mockup.html` (then under `specs/screens`): **Nameday Calendar ▸** in the commands menu (`docs/screens/main-window-menu.light.preview.png`, `docs/screens/main-window-menu.dark.preview.png`) and in the macOS View menu (`docs/screens/main-window-macos.light.preview.png`), and a new view with the submenu open, listing the ten calendars with Czechia checked (`docs/screens/main-window-calendar-menu.light.preview.png`)
- [X] T031 [P] Update `README.md` (as it stood at the move to GitHub): the publish note that `THIRD-PARTY-NOTICES.md` must stay next to the executable, "Your data" (the Czech calendar as the default and 9 more to choose from), the files table (`usersettings.json` holds the chosen theme and name-day calendar; `Anniversary.Calendar.*.json` are extra or replacement calendars the app never writes), the new "Name-day calendars" section, **View** (Nameday Calendar, Appearance) in the macOS checks, and `calendars/` in the projects table
- [X] T032 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: the 10 tests in `Anniversary.Tests/CalendarTests.cs` pass with the rest (78 tests at the move to GitHub)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–5)**: depend on Foundational; User Story 1's tests use the Slovak and Hungarian calendars of
  User Story 2 (T019)
- **Polish (Phase 6)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; the menu works with the Czech calendar alone, but its tests (T008) need the
  shipped Slovak and Hungarian calendars (T019)
- **User Story 2 (P2)**: after Foundational: it extends the generator (T005), and the embedding (T006) ships its files
- **User Story 3 (P3)**: after Foundational; the rebuild on opening (T026) uses `UpdateCalendarMenus()` from User Story
  1 (T014)

### Within Each User Story

- Tests first, then `AnniversaryBook` (or the generator), then the views, then the manual check from quickstart.md
- `MainWindow.axaml` and `MainWindow.axaml.cs` change together: the markup names the `CalendarMenu` item and the
  `Opening` handler, and the code fills the lists

### Parallel Opportunities

- T002 next to T001
- T003 and T004 next to each other and next to the generator (T005)
- In User Story 1: T008, T009 and T013 next to the `AnniversaryBook` tasks
- In User Story 2: T016, T020 and T021 next to the generator tasks
- T028–T031 together: four different documents

---

## Parallel Example: User Story 1

```bash
# The tests, the setting and the menu markup can be written next to the AnniversaryBook changes:
Task: "Add the choice tests to Anniversary.Tests/CalendarTests.cs"
Task: "Add Calendar to UserSettings in Anniversary.Shared/Model/UserSettings.cs"
Task: "Add Nameday Calendar to Anniversary.Shared/Views/MainWindow.axaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (choosing and remembering a calendar; it uses the shipped Slovak and
   Hungarian calendars, which existed before the app code, see What happened)

### Incremental Delivery

1. Setup and Foundational: the calendars are discovered and listed in `Calendars`
2. User Story 1: the menu, switching and the remembered choice
3. User Story 2: nine more countries, the notices and the calendars' README
4. User Story 3: the user's own files, picked up while the app runs
5. Polish: documentation, the mocks and the full test run

### What happened

The maintainer asked for the chooser on 2026-10-09, while the phase-1 spec was being written. The assistant specified it
(spec 5.1, 5.7, 7.10, the tests and criterion 11) and added the submenu to the mocks. After "Please look harder!" it
found the sources, wrote `calendars/Build-Calendars.ps1`, generated and validated the ten calendars (a second run gave
identical files, and the Czech names were identical to v4's) and wrote `calendars/README.md` and
`calendars/THIRD-PARTY-NOTICES.md`, all before any app code existed. The maintainer merged those spec changes; the app
code (discovery, the menu, the remembered choice and `CalendarTests`) was built later that evening, with the spec's
implementation order putting it last (step 9), after the core app and the people editor. Later that night the maintainer
asked about the license line in the calendar files, and it stayed as it was. Everything moved to GitHub, without
history, in `67a4092` on 2026-10-10.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names and expectations are as they were at the move to GitHub: 006 changed the counts and some set-ups and added
  the region tests, 008 the expected order, and 012 renamed `ConfiguredCalendarIsUsedWhenPreferredIsGone` (now
  `ChosenCalendarThatIsGoneFallsBackToTheRegion`)
- The calendar files and the script are described as committed in `67a4092`; at that move the script's Czech input
  became the repository's own file and the Czech `license` was reworded for the MIT License
- The documentation paths are today's; at the time they were under `specs/`
