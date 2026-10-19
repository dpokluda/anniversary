# Implementation Plan: Cross-Platform Rewrite

**Branch**: `001-cross-platform-rewrite` | **Date**: 2026-10-09 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-cross-platform-rewrite/spec.md`

## Summary

Rewrite the WinForms v4 app as v5, a desktop app for Windows, macOS and Linux that looks and behaves like v4 except
where docs/spec.md §11 says otherwise. Avalonia 12 on .NET 10 was chosen over .NET MAUI, Uno Platform, Eto.Forms and
toolkits outside .NET, so v4's model and logic carry over. Three projects: `Anniversary.Shared` holds the code-behind
views and the one logic class `AnniversaryBook` (loading with the lookup next to the executable → app-data folder →
built-in defaults and load warnings, `GetDay`, `Find`); `Anniversary.Desktop` is the `Anniversary` executable; and
`Anniversary.Tests` tests the logic with real files. v4's system-menu commands move to one ⋯ and right-click menu (and
the macOS menu bar), the theme follows the OS or a choice kept in the app-written `usersettings.json`, and each target
is published as one self-contained file.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`), with nullable reference types and implicit usings

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme (`Avalonia` and `Avalonia.Themes.Fluent` in the shared
library, `Avalonia.Desktop` in the executable), compiled bindings by default; `System.Text.Json`. No other packages
(`Avalonia.Controls.DataGrid` came with 002).

**Storage**: JSON files, each found next to the executable, then in the per-user app-data folder, then among the
embedded defaults: `appsettings.json` (read only), `Anniversary.Calendar.Czech.json` and `Anniversary.Data.json`; plus
the app-written `usersettings.json` (the theme) in the app-data folder.

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`global.json`; `dotnet test --project Anniversary.Tests`),
classic style with real files in `TempFolder`s; 38 test cases in `DayTests`, `FindTests`, `LoadTests` and
`UserSettingsTests`; the windows, menus, themes and platforms are checked by hand ([quickstart.md](quickstart.md)).

**Target Platform**: Windows (x64), macOS (Apple Silicon and Intel) and Linux (x64) desktops: `win-x64`, `osx-arm64`,
`osx-x64` and `linux-x64`

**Project Type**: desktop app

**Performance Goals**: no background work. The files are small and read once at start-up; a day needs one index
lookup plus one per day in "Next N days"; Find checks at most about two years, day by day.

**Constraints**: behave like v4 unless docs/spec.md §11 says otherwise; v4's file formats unchanged; no personal data in
the binaries; loading never throws or shows UI; the app writes only `usersettings.json`; no MVVM, DI container or
mocking library, and only the listed packages; a native look on each OS; one self-contained executable per target.
Not in phase 1: installers and MSIX, a macOS `.app` bundle, signing and notarization, trimming and AOT, translated UI
texts.

**Scale/Scope**: one family's people list and one calendar of up to 366 days; one main window, three dialogs (Change
Date, Find, About) and the message window; one commands menu with six items; four test classes.

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS with justification | Three projects: `Anniversary.Shared` (the views and all logic), `Anniversary.Desktop` (the `Anniversary` executable) and `Anniversary.Tests`. Views use code-behind; no MVVM, DI container or service interface. All non-UI logic is in `AnniversaryBook`, which takes the day as a `DateOnly` and collects load problems in `Warnings`. Packages: `Avalonia`, `Avalonia.Themes.Fluent`, `Avalonia.Desktop` and the three test packages. The views and v4's ported models are the baseline; the new types `DayInfo`, `MessageDialog` and `UserSettings`/`AppTheme` are justified under Complexity Tracking. |
| II. Established Coding Style and Complete XML Documentation | PASS | v4's style (Allman braces, braces on every `if`, `_camelCase` fields, `On…` handlers, file-scoped namespaces). Every type and public member has XML docs, and `Anniversary.Shared` and `Anniversary.Desktop` generate documentation files, so a missing comment is a build warning (acceptance criterion 1 allows none). Test classes have a `<summary>` naming the spec section ("Tests of `AnniversaryBook.GetDay` (spec 6.1)") and test names are sentences. |
| III. Classic Tests on Real Objects | PASS | `DayTests`, `FindTests`, `LoadTests` and `UserSettingsTests` (38 cases) use real `AnniversaryBook` and `UserSettings` objects with real JSON files in `TempFolder`s, written by `TestFiles`. No mocks; the day is passed in, so no clock fake is needed. The windows, menus, resizing and themes are manual checks (docs/spec.md §12 criteria 2–7 and 9, [quickstart.md](quickstart.md)). |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | One implementation in `Anniversary.Shared` for all three OSes: Fluent `DensityStyle="Compact"`, the platform's font size set in `App.Initialize` (12 px on Windows, 13 px on macOS and Linux), v4's window size, the OS light or dark setting by default, ⌘ shortcuts and the `NativeMenu` menu bar on macOS. `OperatingSystem.Is…()` appears only for those look values, the shortcut texts and the app-data folder. Built, tested and run on Windows; the first macOS run the same evening worked but looked heavier than on Windows, which [004](../004-macos-native-look/spec.md) fixed with macOS-only values; Linux was published but not run until the maintainer checked macOS and Fedora on 2026-10-10, without code changes. `osx-x64` hasn't been run on an Intel Mac (unverified). |
| V. Safe, Private and Compatible User Data | PASS | The built-in people list is empty (user decision). v4's data, calendar and configuration files load unchanged, and `9999` keeps meaning "year unknown" (the years are hidden, C16). Loading never throws or shows UI; problems are shown after the window opens (C6). The app writes only `usersettings.json` and never `appsettings.json` (C9); writing the people file came with 002. |
| VI. Verifiable Calendar Data | PASS with justification | The only calendar is v4's Czech calendar, built in with its values unchanged: the maintainer's own data, not typed in or made up. Generating it with `calendars/Build-Calendars.ps1`, its `source` and `license` metadata and the third-party notices came with [003](../003-calendar-chooser/spec.md), which confirmed the values are identical to v4. |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer asked for the framework recommendation first and confirmed the decisions (docs/plan.md, "Decisions (all confirmed with the user)"); the one choice made without them, keeping the theme in `usersettings.json`, is recorded as such. The spec, plan and mocks came before the code (work item 1); every deliberate difference from v4 is in docs/spec.md §11 (C1–C13, C16, C17); `README.md` covers building, running and publishing; a handover was written for the macOS session. |

**Post-design re-check**: PASS. The design keeps one logic class, three projects and the allowed packages, and the
three new types are justified below. The open verification points were Linux (checked by the maintainer the next day)
and `osx-x64` (still unverified).

## Project Structure

### Documentation (this feature)

```text
specs/001-cross-platform-rewrite/
├── spec.md                       # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md           # spec quality checklist
├── plan.md                       # this file
├── research.md                   # Phase 0
├── data-model.md                 # Phase 1
├── quickstart.md                 # Phase 1
├── contracts/                    # Phase 1
│   ├── anniversarybook-api.md    # Load, GetDay, Find, Warnings and DayInfo
│   ├── people-data-file.md       # Anniversary.Data.json, as read
│   ├── calendar-file.md          # v4's calendar with MM-dd keys
│   ├── settings-files.md         # appsettings.json (read only) and usersettings.json (the theme)
│   ├── main-window.md            # layout, texts, sizes and resizing
│   └── commands-and-dialogs.md   # commands menu, macOS menu bar, shortcuts, dialogs and messages
└── tasks.md                      # /speckit-tasks
```

### Source Code (repository root)

```text
Anniversary.slnx                        # new: the three projects
Directory.Build.props                   # new: net10.0, nullable, implicit usings, version 5.0.0, Avalonia version
global.json                             # new: dotnet test runs on Microsoft.Testing.Platform
README.md                               # new: build, test, run, publish, the data files, moving from v4
Anniversary.Shared/
├── Anniversary.Shared.csproj           # new: Avalonia + Fluent; Assets; Defaults embedded by file name
├── App.axaml                           # new: Fluent Compact, theme brushes, styles, macOS app menu (About)
├── App.axaml.cs                        # new: font sizes, app-data folder, theme, start-up, template fixes
├── AnniversaryBook.cs                  # new: Load, GetDay, Find, Config, Warnings
├── DayInfo.cs                          # new: what the window shows for one day
├── Assets/
│   ├── App.ico                         # new: window and exe icon (16–256 px)
│   └── App.png                         # new: About icon (256 px)
├── Defaults/
│   ├── appsettings.json                # new: v4's configuration (removed by 012)
│   └── Anniversary.Data.json           # new: the empty people list
├── Model/
│   ├── AppConfig.cs                    # new: v4's configuration (removed by 012)
│   ├── AnniversaryData.cs              # new: v4's Model.Anniversary, renamed
│   ├── CalendarData.cs                 # new: v4's Model.Calendar, renamed
│   ├── Person.cs                       # new: ported from v4
│   └── UserSettings.cs                 # new: AppTheme and usersettings.json
└── Views/
    ├── MainWindow.axaml                # new: layout, commands menu, macOS menu bar
    ├── MainWindow.axaml.cs             # new: day, Find, theme, warnings, shortcuts
    ├── ChangeDateDialog.axaml          # new
    ├── ChangeDateDialog.axaml.cs       # new
    ├── FindDialog.axaml                # new
    ├── FindDialog.axaml.cs             # new
    ├── AboutDialog.axaml               # new
    ├── AboutDialog.axaml.cs            # new
    └── MessageDialog.cs                # new: replaces MessageBox.Show
Anniversary.Desktop/
├── Anniversary.Desktop.csproj          # new: the Anniversary executable, icon, Release publish settings
├── Program.cs                          # new: entry point
└── app.manifest                        # new: from the Avalonia template
Anniversary.Tests/
├── Anniversary.Tests.csproj            # new: xUnit v3
├── DayTests.cs                         # new: 11 cases
├── FindTests.cs                        # new: 13 cases
├── LoadTests.cs                        # new: 9 cases
├── UserSettingsTests.cs                # new: 5 cases
├── TempFolder.cs                       # new: test helper
└── TestFiles.cs                        # new: test helper
calendars/
└── Anniversary.Calendar.Czech.json     # v4's Czech calendar, built in; 003 generated this file with the same
                                        #   values plus metadata
icon/
├── Build-Icon.ps1                      # new: draws the icon (Windows, System.Drawing)
├── README.md                           # new
├── App.icns                            # new: macOS sizes up to 1024 px
└── App-1024.png                        # new: master image
docs/spec.md                            # new (then specs/spec.md)
docs/plan.md                            # new (then specs/plan.md)
docs/screens/                           # new: preview mocks and mockup.html (then specs/screens/)
```

**Structure Decision**: The three-project layout from the spec: everything that isn't platform start-up lives in
`Anniversary.Shared` (views in `Views/`, v4's ported models in `Model/`, logic in `AnniversaryBook`), so later iOS and
Android heads can sit next to `Anniversary.Desktop` without restructuring. The shared library is called
`Anniversary.Shared` so that the executable can stay `Anniversary.exe`, as in v4, without an assembly name clash. The
files 002 and 003 added at the same time (the editor views, `CalendarInfo.cs`, the other calendars and their script)
are listed in their own plans.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New type `DayInfo` (record, `Anniversary.Shared/DayInfo.cs`) | `GetDay` returns everything the main window shows for one day (the calendar names, Namedays, Birthdays and the two Next N days lists) as one value; the window only joins the lists, and `DayTests` assert on it | No alternative was recorded; the record was part of the `AnniversaryBook` surface the spec set out before implementation (docs/spec.md §6) |
| New type `MessageDialog` (static class, `Anniversary.Shared/Views/MessageDialog.cs`) | v4 showed its messages with WinForms' `MessageBox.Show`, which isn't available outside Windows; `Show` builds a small modal window in code for the load warnings and Find's messages | A package for message boxes isn't allowed (docs/spec.md §2: no other packages); the class is "a minimal cross-platform replacement for `MessageBox.Show`" |
| New types `UserSettings` and `AppTheme` (`Anniversary.Shared/Model/UserSettings.cs`) | The theme choice must survive a restart (C12), and the app never writes the user-edited `appsettings.json` (C9) | Writing the choice into `appsettings.json` was rejected so that the user-edited file stays untouched (C9; v4's unused `SaveConfiguration` was dropped); the separate app-written file was the assistant's choice while the maintainer was away (docs/plan.md) |
