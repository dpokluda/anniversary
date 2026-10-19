# Tasks: Cross-Platform Rewrite

**Input**: Design documents from `specs/001-cross-platform-rewrite/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic, file format and settings changes: this
feature's 38 test cases are in `Anniversary.Tests/DayTests.cs`, `Anniversary.Tests/FindTests.cs`,
`Anniversary.Tests/LoadTests.cs` and `Anniversary.Tests/UserSettingsTests.cs`. The windows, menus, themes and platforms
are checked by hand with [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. The work was done on 2026-10-09 in the old `pokluda` repository, together
with 002, 003 and 004, and reached this repository without history in its first commit, `67a4092` ("Moving to GitHub
from a private VSO repository", 2026-10-10). Test names are those at `67a4092`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US6)
- Descriptions name the exact files and, in brackets, the work items of docs/plan.md, the deliberate changes from v4
  (C…) and the requirements they serve

## Path Conventions

- Desktop app (see plan.md): views and logic in `Anniversary.Shared/`, the executable in `Anniversary.Desktop/`, tests
  in `Anniversary.Tests/`, documentation in `README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The spec, plan and mocks first, then the solution and its three projects (work items 1 and 2)

- [X] T001 Write the system spec, the plan and the preview mocks with the maintainer before any code (work item 1): `docs/spec.md` (then `specs/spec.md`: goals, technology, coding guidelines, solution layout, data, logic, UI, start-up, tests, publishing, the deliberate changes from v4, the acceptance criteria and the implementation order), `docs/plan.md` (then `specs/plan.md`: the framework table, the decisions confirmed with the maintainer, the coding guidelines and the work breakdown) and `docs/screens/` (then `specs/screens`: approximate HTML mocks from `docs/screens/mockup.html`, exported as PNGs with a "PREVIEW MOCK" banner) (FR-001)
- [X] T002 Create `Anniversary.slnx` with the three projects and `Directory.Build.props` (`net10.0`, nullable, implicit usings, the latest C#, version 5.0.0, `AvaloniaVersion` 12.1.4, compiled bindings by default) (work item 2)
- [X] T003 [P] Create `global.json`, which opts `dotnet test` into Microsoft.Testing.Platform, as xUnit v3 needs on the .NET 10 SDK
- [X] T004 [P] Create `Anniversary.Shared/Anniversary.Shared.csproj`: assembly `Anniversary.Shared` with the root namespace `Anniversary`, an XML documentation file, `Avalonia` and `Avalonia.Themes.Fluent`, `Assets/**` as Avalonia resources, `Defaults/*.json` embedded under their plain file names (`LogicalName="%(Filename)%(Extension)"`) and internals visible to the tests (FR-012)
- [X] T005 [P] Create `Anniversary.Desktop/Anniversary.Desktop.csproj` (WinExe with the assembly name `Anniversary`, so the executable stays `Anniversary.exe` as in v4; the app icon; an XML documentation file; `Avalonia.Desktop`; a reference to the shared project) and `Anniversary.Desktop/app.manifest` from the Avalonia template (FR-031)
- [X] T006 [P] Create `Anniversary.Tests/Anniversary.Tests.csproj`: a test executable with `xunit.v3`, `xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk`, a global `using Xunit` and a reference to the shared project; no mocking library

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: v4's model, the logic class with loading and warnings, the built-in defaults, the app shell and the icon
(work items 3, 4, 5 and 8)

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 [P] Port v4's model with XML docs: `Anniversary.Shared/Model/Person.cs` (`Name`, `DateOnly? Birthdate`, `Namedate` as `MM-dd`, `Dictionary<DateOnly, string>? Anniversaries`), `Anniversary.Shared/Model/AnniversaryData.cs` (v4's `Model.Anniversary`, renamed; `People` as `data`) and `Anniversary.Shared/Model/CalendarData.cs` (v4's `Model.Calendar`, renamed; `Data` as `calendar`) (work item 3; C8; FR-011)
- [X] T008 [P] Create `Anniversary.Shared/Model/AppConfig.cs` (historical; removed by 012) with `CalendarFile` "Anniversary.Calendar.Czech.json", `DataFile` "Anniversary.Data.json", `DisplayYearsOnBirthdays` true and `NumberOfDaysAhead` 7: the real file names instead of v4's `calendar.txt` and `data.txt` (C5; FR-011)
- [X] T009 [P] Create the `DayInfo` record in `Anniversary.Shared/DayInfo.cs` (`CalendarNames`, `Namedays`, `Birthdays`, `NextDaysPeople`, `NextDaysNames`) with XML docs (work item 3)
- [X] T010 [P] Add the built-in defaults: `Anniversary.Shared/Defaults/appsettings.json` (v4's configuration; historical, removed by 012), `Anniversary.Shared/Defaults/Anniversary.Data.json` (`{ "data": [] }`, the maintainer's decision for no personal data) and v4's Czech calendar embedded under its file name `Anniversary.Calendar.Czech.json` (today `calendars/Anniversary.Calendar.Czech.json`, which 003 generated with the same values plus metadata) (work items 3 and 8; C13; FR-013)
- [X] T011 Create `AnniversaryBook` in `Anniversary.Shared/AnniversaryBook.cs`: `Load(…, params string[] folders)` keeps the non-blank folders (the last one is the app-data folder) and loads `appsettings.json`, then `Config.CalendarFile`, then `Config.DataFile`, each found on its own (an absolute path as is; else the first folder that has the file; else the embedded resource with that name); JSON is read with case-insensitive names, comments and trailing commas; `JsonException`, `IOException`, `UnauthorizedAccessException` and `NotSupportedException` become `Warnings` ("Error loading configuration: …" with "Using default settings.", "Calendar file not found: …", "Error loading calendar: …", "Data file not found: …", "Error loading data: …") with the defaults, an empty calendar or no people; `Config`, `People` and the `MM-dd` index hold each person once per key; the class never reads the clock or shows UI, and in this feature it writes no file (work item 3; C2, C3, C6; FR-011, FR-012, FR-015)
- [X] T012 [P] Add the test helpers `Anniversary.Tests/TempFolder.cs` (a unique folder under the temp path with `Write` and `PathOf`, deleted on dispose) and `Anniversary.Tests/TestFiles.cs` (writes `appsettings.json`, a calendar and a people file into a folder and loads a book from it) (work item 4)
- [X] T013 Add `Anniversary.Tests/LoadTests.cs` (9 cases, spec 5.3 and 5.4): `FirstFolderWinsOverSecond`, `FallsBackToSecondFolder`, `FallsBackToBuiltInDefaults` (the Czech calendar with "Štefan/Sára" on 9 October, no people, no warnings), `AbsolutePathInConfigIsUsed`, `InvalidDataFileGivesWarningAndNoPeople`, `InvalidCalendarFileGivesWarningAndEmptyCalendar`, `MissingNonDefaultFilesAreReported`, `InvalidConfigUsesDefaultsAndWarns` and `ConfigAllowsCommentsAndTrailingCommas` (work item 4; FR-011, FR-012, FR-013, FR-015)
- [X] T014 Create `Anniversary.Shared/App.axaml`: the application's name (`Name="Anniversary"`), `RequestedThemeVariant="Default"`, `<FluentTheme DensityStyle="Compact" />`, the light and dark brushes `SectionBackground`, `SectionBorder`, `ValueBackground`, `ValueBorder`, `SectionHeader` and `SeparatorBrush` in `ThemeDictionaries`, the `AppFontSize` font size on windows, menus, tooltips, inputs and buttons, subtle separators, and the styles `Border.section`, `TextBlock.caption`, `TextBlock.muted`, `Border.value`, `SelectableTextBlock.names` (with `strong` and `hint`), `Button.cmd` and `Button.subtle` that replace v4's group boxes (work item 5; FR-010)
- [X] T015 Create `Anniversary.Shared/App.axaml.cs`: `Initialize` sets the platform's font size as resources (`AppFontSize` 12 on Windows, Segoe UI 9 pt as in v4, and 13 on macOS and Linux; `AppNamesFontSize` one more and `AppDateFontSize` four more, mirroring v4's 10 pt and 12 pt; the macOS-only values came with 004); `AppDataFolder` (ApplicationData on Windows and Linux, LocalApplicationData on macOS, plus `Anniversary`); `OnFrameworkInitializationCompleted` loads the book from `AppContext.BaseDirectory` and the app-data folder and opens `MainWindow` on `DateOnly.FromDateTime(DateTime.Today)`; the single-view (mobile) lifetime isn't handled in phase 1 (work item 5; FR-010, FR-012)
- [X] T016 [P] Create `Anniversary.Desktop/Program.cs`: `[STAThread] Main` runs `BuildAvaloniaApp().StartWithClassicDesktopLifetime(args)`, and the public static `BuildAvaloniaApp()` (`UsePlatformDetect`, `LogToTrace`) also serves the XAML previewer
- [X] T017 [P] Draw the app icon with `icon/Build-Icon.ps1` (PowerShell 7 with System.Drawing, so Windows only): v4's tear-off calendar (yellow binding, red "24", blue lines, stacked pages) as vector shapes, with fewer details at small sizes and crisp 16 px pixels, written to `Anniversary.Shared/Assets/App.ico` (16–256 px), `Anniversary.Shared/Assets/App.png` (256 px), `icon/App.icns` and `icon/App-1024.png`, and described in `icon/README.md` (work item 8; C17; FR-032)

**Checkpoint**: A book loads from the folders with warnings and built-in defaults, and the app starts with the shared
styles; user stories can begin

---

## Phase 3: User Story 1 - See the day at a glance (Priority: P1) 🎯 MVP

**Goal**: The main window shows the day like v4 (calendar names, Namedays, Birthdays and Anniversaries, Next N days),
with v4's size, resizing and look

**Independent Test**: With v4's `Anniversary.Data.json` in the app-data folder the window shows today; without it, the
hint (quickstart scenarios 1 and 2)

### Tests for User Story 1

- [X] T018 [P] [US1] Add `Anniversary.Tests/DayTests.cs` (11 cases, spec 6.1): `CalendarNamesAreShownForTheDay`, `NamedayIsListed`, `BirthdayShowsYears` ("Eva (44)"), `BirthdayWithoutYearsWhenConfigTurnsItOff`, `AnniversaryLineHasNameDescriptionAndYears` ("Jan - svatba (18)"), `PersonWithSeveralEventsOnOneDayIsListedOnceInEachList`, `NextDaysRespectsTheConfiguredNumberOfDays`, `NextDaysAreInDateOrderWithoutDuplicatesAndCalendarEntriesAreSplit`, `UnknownYearShowsNoYears` (a 9999 birth year and a future anniversary), `TwentyNinthOfFebruaryMatchesOnlyInLeapYears` and `DayWithoutDataIsEmpty` (work item 4; FR-003, FR-004, FR-005, FR-006, FR-007)

### Implementation for User Story 1

- [X] T019 [US1] Add `GetDay(DateOnly day)` to `Anniversary.Shared/AnniversaryBook.cs`: the calendar text for the day's `MM-dd` key; Namedays and then Birthdays from the indexed people in data-file order (the birthday, then the anniversaries in dictionary order); the years only when `DisplayYearsOnBirthdays` is on and the year isn't after the shown day's (C16); for the next `NumberOfDaysAhead` days the people (each once) and the calendar names split at `,`, trimmed, empty ones skipped, each once (C4; FR-003, FR-004, FR-005, FR-006, FR-007)
- [X] T020 [P] [US1] Create `MessageDialog.Show(owner, title, message)` in `Anniversary.Shared/Views/MessageDialog.cs`: a modal window built in code with the owner's icon, the wrapped text (240 to 400 wide, margin 16) and an **OK** button (accent, default and cancel) on the right; sized to its content, not resizable, not in the taskbar, centred on the owner (work item 6; FR-027)
- [X] T021 [US1] Lay out `Anniversary.Shared/Views/MainWindow.axaml`: title "Anniversary", `App.ico`, width 312 and a start-up height of 468, minimum 280 × 420, centred on the screen; a grid with rows `Auto,Auto,Auto,*,Auto`, row spacing 6 and margin 8; the "Today is" section (caption, the bold date, a separator, the semibold italic calendar names, all wrapping); Namedays and Birthdays and Anniversaries with value boxes of fixed height 70 holding scrolling, selectable `names strong` text; Next N days filling the rest (`names`); Yesterday and Tomorrow on the left and Quit (accent) on the right, column spacing 6 (work item 5; C10; FR-002, FR-008, FR-009)
- [X] T022 [US1] Write the main window's code in `Anniversary.Shared/Views/MainWindow.axaml.cs`: the constructor `(AnniversaryBook book, DateOnly day, UserSettings settings)` and a parameterless one for the XAML previewer; `Refresh()` (the date as `"D"` in the current culture, the calendar names, the lists joined by line breaks, the hint "No people yet. Use ⋯ ▸ Edit People… to add some." with the `hint` class when there are no people, and the two Next N days lines joined with ", " without empty ones); the caption "Next {N} days"; on `Opened`, focus on Quit and each warning in a `MessageDialog`, titled "Configuration Error" when it starts with `ConfigurationWarningPrefix` and "Data Error" otherwise; Quit closes the window (work item 5; C6; FR-002, FR-006, FR-014, FR-015)
- [X] T023 [US1] Check scenarios 1 and 2 of `specs/001-cross-platform-rewrite/quickstart.md` by hand (docs/spec.md §12, criteria 2, 3, 5 and 6)

**Checkpoint**: The app shows the day like v4, with the user's v4 data

---

## Phase 4: User Story 2 - Browse to another day (Priority: P2)

**Goal**: Yesterday, Tomorrow, Today and Change Date move the shown day

**Independent Test**: Tomorrow, Yesterday, a click on the date and Change Date show each day, and Today brings it back
(quickstart scenario 3)

### Implementation for User Story 2

- [X] T024 [P] [US2] Create the Change Date dialog: `Anniversary.Shared/Views/ChangeDateDialog.axaml` (title "Change Date", sized to its content, not resizable, not in the taskbar, centred on the owner; a "Current Date" section at least 240 wide with a `CalendarDatePicker` in the long format, today highlighted; OK as accent and default, Cancel as cancel) and `Anniversary.Shared/Views/ChangeDateDialog.axaml.cs` (`SelectedDate` converts between `DateOnly` and the picker's `DateTime?` and keeps the initial date when the picker is cleared; OK and Cancel close with `true` and `false`) (work item 6; FR-018)
- [X] T025 [US2] Scale the calendar drop-down down: `CalendarFontSize` and `CalendarDayButtonFontSize` at the app's font size in `Anniversary.Shared/App.axaml.cs`, 28 × 26 day and month cells without gaps and arrows at 60 % in `Anniversary.Shared/App.axaml`, and a `TemplateApplied` class handler for `CalendarItem` that removes the template's minimum sizes and shortens its header rows, so the calendar drops down below the compact dialog (FR-018)
- [X] T026 [US2] Add the day commands to `Anniversary.Shared/Views/MainWindow.axaml.cs`: Yesterday and Tomorrow (one day back or forward), Today (`DateOnly.FromDateTime(DateTime.Today)`) and `ChangeDate()`, which the date's `PointerPressed` also opens (hand cursor and tooltip "Click to change the date" in `Anniversary.Shared/Views/MainWindow.axaml`) (FR-017, FR-018)
- [X] T027 [US2] Check scenario 3 of `specs/001-cross-platform-rewrite/quickstart.md` by hand (criterion 4)

**Checkpoint**: Any day can be shown

---

## Phase 5: User Story 3 - Find a name (Priority: P2)

**Goal**: Find… and Find Next jump to the next day whose calendar names or people match

**Independent Test**: Find "sára" shows 9 October, Find Next continues, and the messages appear for no match and for an
empty name (quickstart scenario 4)

### Tests for User Story 3

- [X] T028 [P] [US3] Add `Anniversary.Tests/FindTests.cs` (13 cases, spec 6.2): `SubstringMatchIgnoresCase`, `WholeWordsSplitOnEveryDelimiter` (space, comma, semicolon, tab, slash and hyphen), `WholeWordsDoNotMatchPartOfAWord`, `CalendarOnlyIgnoresPeople`, `StartDayIsIncluded`, `SearchEndsOnLastDayOfFollowingYear` (with 29 February) and `BlankNameFindsNothing` (empty and blank) (work item 4; FR-020)

### Implementation for User Story 3

- [X] T029 [US3] Add `Find(string name, DateOnly from, bool wholeWordsOnly, bool calendarOnly)` to `Anniversary.Shared/AnniversaryBook.cs`: `null` for a blank name; day by day from `from` to 31 December of `from.Year + 1`; the calendar text, then (unless `calendarOnly`) the names of the people indexed that day; case-insensitive ordinal matching, by `Contains` or by whole words split at `' '`, `','`, `';'`, `'\t'`, `'/'` and `'-'` (FR-020)
- [X] T030 [P] [US3] Create the Find dialog: `Anniversary.Shared/Views/FindDialog.axaml` ("Name:" and a text box with the placeholder "e.g. Petr, Novák…", an "Options" section with "From current date", "Whole words only" and "Search calendar only", OK and Cancel on the right) and `Anniversary.Shared/Views/FindDialog.axaml.cs` (`FindName`, `FromCurrentDate`, `WholeWordsOnly`, `SearchCalendarOnly`; on opening, the name gets the focus and is selected) (work item 6; FR-019)
- [X] T031 [US3] Add Find to `Anniversary.Shared/Views/MainWindow.axaml.cs`: the fields `_findName`, `_findFromCurrentDate`, `_findWholeWords` and `_findCalendarOnly`, kept between searches as in v4; `Find()` fills the dialog with them, keeps the new values on OK and searches from the shown day or from 1 January of its year; `FindNext()` opens Find… when there is no name, else searches from the next day; `PerformFind()` shows the found day, or "'{name}' not found.", or for an empty name "Please enter a name to search for." (title "Find") (FR-019, FR-021, FR-022)
- [X] T032 [US3] Check scenario 4 of `specs/001-cross-platform-rewrite/quickstart.md` by hand (criterion 4)

**Checkpoint**: Names can be found as in v4

---

## Phase 6: User Story 4 - Reach every command the way each OS expects (Priority: P3)

**Goal**: One ⋯ and right-click menu, the platforms' shortcuts, the macOS menu bar and About (work items 6 and 7)

**Independent Test**: Every item of the ⋯ and right-click menu works, Ctrl+F and F3 (⌘F and ⌘G) work, and on macOS the
menu bar does the same (quickstart scenario 5)

### Implementation for User Story 4

- [X] T033 [US4] Define the commands menu once, as the `CommandsMenu` `MenuFlyout` resource in `Anniversary.Shared/Views/MainWindow.axaml` (placement `BottomEdgeAlignedRight`): Today, Change Date…, a separator, Find… (`FindMenuItem`), Find Next (`FindNextMenuItem`), a separator, Theme ▸, a separator and About Anniversary…; and add the ⋯ button at the top-right of "Today is" (`Button.subtle` with the three-dot `MoreIcon` geometry at 12 px, tooltip "More commands") with the menu as its flyout (work item 7; C1; FR-023)
- [X] T034 [US4] Wire the menu in `Anniversary.Shared/Views/MainWindow.axaml.cs`: the same menu as the window's `ContextFlyout`; the gesture texts Ctrl+F and F3, or ⌘F and ⌘G on macOS; `OnKeyDown` runs Find… for Ctrl or ⌘ + F and Find Next for F3 or Ctrl or ⌘ + G; the handlers `OnToday`, `OnChangeDate`, `OnFind`, `OnFindNext` and `OnAbout` (work item 7; C7; FR-023, FR-024)
- [X] T035 [P] [US4] Align the menus: in `Anniversary.Shared/App.axaml` the item padding `10,4,10,4`, the separator padding `10,3,10,3` and the chevron margin `12,0,0,0`, so separators start where the text starts, and in `Anniversary.Shared/App.axaml.cs` a `TemplateApplied` class handler for `MenuItem` that shrinks the submenu arrow from 8 × 16 to 5 × 9 (FR-010, FR-023)
- [X] T036 [US4] Add the macOS menu bar as a `NativeMenu` on the window in `Anniversary.Shared/Views/MainWindow.axaml` (Go: Yesterday, Tomorrow, Today, Change Date…; Find: Find… with `Meta+F` and Find Next with `Meta+G`; View: Appearance ▸) and wire it in `Anniversary.Shared/Views/MainWindow.axaml.cs`, finding each `NativeMenuItem` by its headers with the `NativeItem` helper, because native items have no routed `Click` and no generated fields (work item 7; FR-025)
- [X] T037 [P] [US4] Create the About dialog: `Anniversary.Shared/Views/AboutDialog.axaml` (title "About Anniversary", 400 wide: the 40 × 40 `App.png`, "Anniversary Application" in bold, the version, OK as accent, default and cancel at the top right, the description "This program displays names, birthdates, and anniversaries for a specified day." and a muted copyright line) and `Anniversary.Shared/Views/AboutDialog.axaml.cs` ("Version {Major}.{Minor}" from the assembly version and the copyright text) (work item 6; FR-026)
- [X] T038 [US4] Add "About Anniversary" to the macOS application menu in `Anniversary.Shared/App.axaml` (a `NativeMenu` on the application), with `OnAbout` in `Anniversary.Shared/App.axaml.cs` opening the About dialog over the main window (work item 7; FR-025, FR-026)
- [X] T039 [US4] Check scenario 5 of `specs/001-cross-platform-rewrite/quickstart.md` by hand on Windows, macOS and Linux (criteria 2 and 4)

**Checkpoint**: Every command is reachable on every OS

---

## Phase 7: User Story 5 - Choose light, dark or the system theme (Priority: P3)

**Goal**: Theme ▸ System / Light / Dark, following the OS live and remembered in `usersettings.json`

**Independent Test**: Dark applies at once and survives a restart, and System follows the OS while the app runs
(quickstart scenario 6)

### Tests for User Story 5

- [X] T040 [P] [US5] Add `Anniversary.Tests/UserSettingsTests.cs` (5 cases, spec 5.5): `MissingFileGivesDefaults` (System and the file path), `SavedValuesLoadAgain` (Dark), `InvalidFileGivesDefaults`, `ThemeIsReadCaseInsensitively` ("light") and `SaveCreatesMissingFolder`; the calendar checks in the first two are 003's (work item 4; FR-030)

### Implementation for User Story 5

- [X] T041 [P] [US5] Create `Anniversary.Shared/Model/UserSettings.cs`: the `AppTheme` enum (`System`, `Light`, `Dark`) and `UserSettings` with `Theme` (default `System`) and the `[JsonIgnore]` `FilePath`; `Load(filePath)` returns the defaults for a missing file, invalid JSON or an I/O or access error, without warnings; `Save()` creates the folder and writes camelCase, indented JSON with the theme as a string, ignoring I/O and access errors; `appsettings.json` stays untouched (work item 3; C9, C12; FR-016, FR-030)
- [X] T042 [US5] Apply the theme in `Anniversary.Shared/App.axaml.cs`: `ApplyTheme(AppTheme)` sets `RequestedThemeVariant` to `Default`, `Light` or `Dark`; at start-up `usersettings.json` is loaded from the app-data folder and its theme applied before the main window is created, and the settings are passed to `MainWindow` (FR-029, FR-030)
- [X] T043 [US5] Add Theme ▸ System / Light / Dark as radio items (`ToggleType="Radio"`, group "Theme") to the commands menu and View ▸ Appearance with radio `NativeMenuItem`s to the macOS menu bar in `Anniversary.Shared/Views/MainWindow.axaml`, and `ChooseTheme` (apply, save, update the checks) and `UpdateThemeChecks` for both menus in `Anniversary.Shared/Views/MainWindow.axaml.cs` (work item 7; C12; FR-028)
- [X] T044 [US5] Check scenario 6 of `specs/001-cross-platform-rewrite/quickstart.md` by hand, including a change of the OS setting while the app runs (criterion 7; FR-029)

**Checkpoint**: The theme can be chosen and follows the OS

---

## Phase 8: User Story 6 - Get a ready-to-run copy for each OS (Priority: P3)

**Goal**: One self-contained executable per target, and a README that explains it (work item 9)

**Independent Test**: Publish the four targets and start each on its OS without .NET (quickstart scenario 7)

### Implementation for User Story 6

- [X] T045 [US6] Add the Release publish settings to `Anniversary.Desktop/Anniversary.Desktop.csproj`: no symbols (`DebugType` none) and no XML documentation in Release, `.pdb` and `.xml` files removed from the publish list, and compression inside the single file (work item 9; FR-031)
- [X] T046 [P] [US6] Write `README.md`: building, testing and running (with `global.json` and Rider's AvaloniaRider preview), the `dotnet publish` commands for `win-x64`, `osx-arm64`, `osx-x64` and `linux-x64` (self-contained, single file, native libraries included), `chmod +x` and running on macOS, where the files are found (next to the executable, the app-data folders, the built-in defaults), which files the app writes, moving from v4 (copy `Anniversary.Data.json` and, if changed, `appsettings.json` into the app-data folder; `9999` still means "year unknown") and the projects (work item 9; FR-033)
- [X] T047 [US6] Check scenario 7 of `specs/001-cross-platform-rewrite/quickstart.md` (criterion 9); the record shows the Linux builds published from the Mac on the morning of 2026-10-10 and the app checked by the maintainer on Fedora that day; CI has published every target since 005 (FR-031, FR-032)

**Checkpoint**: All six user stories work

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and the final checks

- [X] T048 [P] Record what the build found in `docs/spec.md` (then `specs/spec.md`): the Avalonia 12 findings (macOS menu items found by their headers, §7.2; `TemplateApplied` handlers for the calendar drop-down and the submenu arrow, §7.1; `global.json`, §4), the years hidden for `9999`, found while testing with the real v4 data (C16), the tests in §9 (`DayTests`, `FindTests`, `LoadTests`, `UserSettingsTests`), the deliberate changes from v4 in §11 (C1–C13, C16, C17) and acceptance criteria 1–9 in §12 (FR-001)
- [X] T049 [P] Write the handover for the macOS session, `handover-macos.md`, with a check list for the Mac; the macOS session later turned it into `specs/copilot/handover-linux.md` (now `docs/copilot/handover.md`)
- [X] T050 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: 78 tests passed, 38 of them this feature's
- [X] T051 Run the app live on Windows (v4's size and native font sizes, light and dark); the maintainer ran it on macOS the same evening (it worked; the heavier look led to 004) and checked macOS and Fedora on 2026-10-10 without code changes; `osx-x64` hasn't been run on an Intel Mac (`specs/001-cross-platform-rewrite/quickstart.md`, platform checks)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies; T001 (the spec, plan and mocks) came first
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–8)**: depend on Foundational; User Story 1's main window (T021, T022) and message window
  (T020) are used by every later story
- **Polish (Phase 9)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after User Story 1 (the buttons and the date are in its window); Today and Change Date… are
  also in User Story 4's menus
- **User Story 3 (P2)**: after User Story 1; Find… and Find Next are reached through User Story 4's menu and shortcuts
- **User Story 4 (P3)**: after User Story 1; it holds the commands of User Stories 2, 3 and 5
- **User Story 5 (P3)**: after Foundational and User Story 1; its menu items sit in User Story 4's menus
- **User Story 6 (P3)**: after User Story 1 (something to publish); independent of the other stories

### Within Each User Story

- Tests first, then `AnniversaryBook`, then the dialogs and the views, then the manual check from quickstart.md
- `MainWindow.axaml` and `MainWindow.axaml.cs` change together: the markup names the handlers, and the code uses the
  generated fields

### Parallel Opportunities

- T003–T006 next to each other after T002
- T007–T010, T012, T016 and T017 next to each other; T011 needs T007 and T008
- In each story, the test task and the dialog task next to the `AnniversaryBook` and window tasks (T018 and T020; T024;
  T028 and T030; T035 and T037; T040 and T041)
- T046 next to T045; T048 and T049 together

---

## Parallel Example: User Story 1

```bash
# The tests and the message window can be written next to the day logic:
Task: "Add DayTests to Anniversary.Tests/DayTests.cs"
Task: "Add GetDay(DateOnly day) to Anniversary.Shared/AnniversaryBook.cs"
Task: "Create MessageDialog.Show in Anniversary.Shared/Views/MessageDialog.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenarios 1 and 2 (the day at a glance with v4's data, and resizing)

### Incremental Delivery

1. Setup and Foundational: the spec, the projects, loading with warnings and the built-in defaults
2. User Story 1: the main window like v4 (MVP)
3. User Story 2: Yesterday, Tomorrow, Today and Change Date
4. User Story 3: Find and Find Next
5. User Story 4: the ⋯ and right-click menu, the shortcuts, the macOS menu bar and About
6. User Story 5: the theme
7. User Story 6: the published executables and the README
8. Polish: the spec's findings, the handover, the test run and the platform checks

### What happened

The maintainer asked for a framework recommendation on 2026-10-09 (around 17:00 local time). In the next hour and a half
the assistant wrote the plan, the HTML mocks and the spec, answering the maintainer's questions (see the spec's
Clarifications); the same evening the spec also got the people editor (002) and the calendar choice (003). After the
maintainer merged the spec changes (about 18:40), the app was built on the PC that evening, covering this feature's work
items 2–9 and 002's and 003's items 10 and 11, with 78 tests in all and a handover for the Mac. The maintainer ran it on
macOS that night, which started 004. The code reached this repository on 2026-10-10 as `67a4092`, together with the
switch to the MIT License and the made-up name in the mocks, which aren't features.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names are as they were at `67a4092`; the commit of 006 added `NextDaysStopOnLastDayOfYear9999` and
  `SearchStopsOnLastDayOfYear9999` (the date-range bug fix), and 012 reworked `LoadTests` and `UserSettingsTests` for
  the single settings file
- The same files also hold what 002 and 003 added at the same time (Edit People…, Nameday Calendar ▸, `CalendarInfo`,
  the editing and calendar members of `AnniversaryBook`); those tasks are in their own folders
- docs/plan.md's work items 2–9 were ticked ✅, and its window size corrected to 312 × 468, by the docs update of
  2026-10-10 (005); the spec's button spacing was corrected from 8 to the code's 6 at the same time
- The documentation paths are today's; at the time they were under `specs/`
