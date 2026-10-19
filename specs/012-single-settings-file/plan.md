# Implementation Plan: Single Settings File

**Branch**: `012-single-settings-file` | **Date**: 2026-10-11 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/012-single-settings-file/spec.md`

## Summary

Put all settings into the app's own `usersettings.json` and stop reading v4's `appsettings.json`. `UserSettings` gets
v4's two options, `NumberOfDaysAhead` (7) and `DisplayYearsOnBirthdays` (`true`), reads comments and trailing commas,
and keeps the reason in `LoadError` when the file can't be read; while it is set, `Save()` writes nothing.
`AnniversaryBook.Load` now takes the `UserSettings` object instead of a calendar and a data file, keeps it as
`Settings` (replacing `Config`), reports `LoadError` as the existing "Error loading configuration" warning, now naming
the file, takes the data file from `Settings.DataFile` (or `Anniversary.Data.json`) and chooses the calendar without
the `calendarFile` step. `App` saves the settings at start-up when the file is missing, so every key is there to edit.
`Anniversary.Shared/Model/AppConfig.cs` and the built-in `Anniversary.Shared/Defaults/appsettings.json` are removed,
and the tests load with settings objects instead of writing `appsettings.json`. The maintainer's choices (file-only
options, one file) come from the session of 2026-10-11.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`)

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme (no markup changes); `System.Text.Json`
(`JsonSerializer` with case-insensitive camelCase names, `JsonCommentHandling.Skip`, trailing commas and
`JsonStringEnumConverter`). No new packages.

**Storage**: JSON files. `usersettings.json` in the app-data folder holds all five settings; `appsettings.json` is no
longer read, and its built-in copy is gone. The people and calendar formats are unchanged.

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`dotnet test --project Anniversary.Tests`), classic style with
real files in `TempFolder`s. `UserSettingsTests` grows from 5 to 8 test cases and `LoadTests` goes from 9 to 8 (4 new,
5 removed), so the suite goes from 203 to 205 test cases; the tests build settings with `TestFiles.Settings(…)` instead
of writing `appsettings.json` with `TestFiles.WriteConfig(…)`. The hand edit and the message are checked by hand
([quickstart.md](quickstart.md)).

**Target Platform**: Windows, macOS and Linux desktops (x64 and arm64)

**Project Type**: desktop app

**Performance Goals**: none beyond reading one small JSON file once at start-up (one file fewer than before) and
writing it at the first start and when a menu choice changes.

**Constraints**: never lose a hand edit (no write while the file can't be read); loading never throws, and problems
are shown after the window opens; the app writes only `usersettings.json`, the data file and the Start at Login entry;
the data file stays per computer (007); the v4 people and calendar formats stay unchanged.

**Scale/Scope**: one file per user and computer with five keys; one type and one built-in file removed, no type
added; on `AnniversaryBook`, a new `Load` overload and `Settings` replace the old three-parameter `Load`, `Config` and
`ConfigFileName`; `UserSettings` gets three properties.

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | The feature removes a type (`AppConfig`) and a built-in file, and adds no type, project or package. Reading and writing the file stay in `UserSettings.Load` and `Save`, where spec 5.5 put them in 001; turning a load problem into a warning stays in `AnniversaryBook`, as it was for `appsettings.json`. `AnniversaryBook` still shows no UI and doesn't read the clock. |
| II. Established Coding Style and Complete XML Documentation | PASS | The new members (`NumberOfDaysAhead`, `DisplayYearsOnBirthdays`, `LoadError`, `Settings`, the new `Load` overload, `TestFiles.Settings`) have XML docs; changed summaries say "settings" instead of "configuration"; test names are sentences; one inline comment explains why `Save()` returns early. |
| III. Classic Tests on Real Objects | PASS | `UserSettingsTests` reads and writes real files in a `TempFolder` (a file with a typo stays exactly as it was); `LoadTests` loads real `AnniversaryBook`s with settings objects and real files. No mocks. docs/spec.md §9 lists the tests; the hand edit and the message are acceptance criterion 6 in §12 and in [quickstart.md](quickstart.md). |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | No platform code: the same file in each OS's app-data folder (spec 5.3) and the existing message dialog. CI built the commit with warnings as errors and ran the tests on Windows, macOS and Ubuntu, all passing. The session excerpt ends before the implementation report, so there is no record of the manual check (criterion 6) on each OS. |
| V. Safe, Private and Compatible User Data | PASS with justification | This feature is the origin of the rule "a settings file the app could not read is never saved over". Loading still never throws: an unreadable file gives the defaults and a warning after the window opens. The app still writes only `usersettings.json`, the data file and the Start at Login entry. No longer reading v4's `appsettings.json` is a deliberate break (user decision, spec C9); the v4 people and calendar formats, which the principle protects, are unchanged, and the README tells v4 users where the two options go. |
| VI. Verifiable Calendar Data | N/A | No calendar data changes. |
| VII. Decided, Specified and Documented Changes | PASS | Both choices are the maintainer's (docs/plan.md, Decisions, "Settings": "user decision, 11 October 2026"; the handover). In the same commit: docs/spec.md (4, 5.1–5.7, 6, 6.1, 7.2, 8, 9, 10, C5, C9 and criterion 6), docs/plan.md, the handover, `README.md` and the user guide `Anniversary.Desktop/README.md`. |

**Post-design re-check**: PASS. The design moves two properties from a removed type into an existing one and adds a
load state (`LoadError`) instead of a type; all file logic stays UI-free, and the manual-check gap under Principle IV
remains as noted.

## Project Structure

### Documentation (this feature)

```text
specs/012-single-settings-file/
├── spec.md                   # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md       # spec quality checklist
├── plan.md                   # this file
├── research.md               # Phase 0
├── data-model.md             # Phase 1
├── quickstart.md             # Phase 1
├── contracts/                # Phase 1
│   ├── usersettings-file.md  # the settings file: keys, defaults, reading, writing, errors
│   └── settings-api.md       # UserSettings and the AnniversaryBook settings API
└── tasks.md                  # /speckit-tasks
```

### Source Code (repository root)

```text
Anniversary.Shared/
├── AnniversaryBook.cs              # changed: Load(UserSettings, folders), Settings instead of Config, the settings
│                                   #   warning with the file's path, the data file from the settings; no LoadConfig,
│                                   #   no ConfigFileName, no calendarFile step
├── App.axaml.cs                    # changed: saves the settings when the file is missing; Load(settings, folders)
├── Defaults/
│   └── appsettings.json            # removed: the built-in v4 configuration
├── Model/
│   ├── AppConfig.cs                # removed
│   └── UserSettings.cs             # changed: NumberOfDaysAhead, DisplayYearsOnBirthdays, LoadError; comments and
│                                   #   trailing commas; Save() writes nothing while LoadError is set
└── Views/
    └── MainWindow.axaml.cs         # changed: "Next {N} days" from book.Settings
Anniversary.Tests/
├── CalendarTests.cs                # changed: no configured calendar; three tests renamed, one reworked
├── DataFileTests.cs                # changed: loads with UserSettings; one test renamed
├── LoadTests.cs                    # changed: 4 new tests, 5 removed
├── TestFiles.cs                    # changed: Settings(…) instead of WriteConfig(…)
└── UserSettingsTests.cs            # changed: 3 new tests, 3 extended
Anniversary.Desktop/README.md       # changed: "Files and settings"
README.md                           # changed: files table, "Moving from v4", "Calendars"
docs/spec.md                        # changed (then specs/spec.md)
docs/plan.md                        # changed (then specs/plan.md)
docs/copilot/handover.md            # changed (then specs/copilot/handover.md)
```

**Structure Decision**: The three-project layout is kept, and `Anniversary.Desktop` is unchanged apart from its user
guide. The settings model stays in `Anniversary.Shared/Model/UserSettings.cs`, the logic in `AnniversaryBook` and the
start-up in `App`; the feature removes two files and adds none. The same commit (`9c89279`) also changed
`icon/Build-Icon.ps1` and `icon/README.md` (the taller icon page, a follow-up to 010), which aren't part of this
feature.

## Complexity Tracking

No violations.
