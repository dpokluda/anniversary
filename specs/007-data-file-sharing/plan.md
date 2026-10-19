# Implementation Plan: Data File Sharing

**Branch**: `007-data-file-sharing` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/007-data-file-sharing/spec.md`

## Summary

Let a family member open a data file they were sent, and let one person use one data file from several computers
through a cloud drive. A new **Data File** submenu (⋯ and right-click; the macOS File menu) has Open…, Save As… and
Show in Folder. The chosen file is used where it is and its full path is remembered per computer in
`usersettings.json` (`UserSettings.DataFile`), which `AnniversaryBook.Load` prefers over `dataFile` from
`appsettings.json`. `AnniversaryBook` keeps track of the file in use, whether it failed to load, and its last-write time
and size, so the main window reloads the people when the file changed elsewhere and the people editor asks before
overwriting such changes or a file that couldn't be loaded. The native open and save dialogs and opening the folder
come from Avalonia itself; `MessageDialog` gets a two-button `Confirm`.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`)

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme, including its `StorageProvider` (native open and save
dialogs) and `Launcher` (opening a folder in the file manager); `Avalonia.Controls.DataGrid` 12.1.2 for the existing
people editor; `System.Text.Json` (`JsonDocument` checks a file before Open… uses it). No new packages.

**Storage**: JSON files. The people data file can be anywhere the user picks (Documents, a cloud-drive folder, …);
`usersettings.json` in the app-data folder gets the key `dataFile`; the read-only `appsettings.json` still names the
data file when nothing was chosen (`AppConfig.DataFile`).

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`dotnet test --project Anniversary.Tests`), classic style with
real files in `TempFolder`s; 16 new test cases in `Anniversary.Tests/DataFileTests.cs` (181 tests in total); the
native dialogs, menus and questions are checked by hand ([quickstart.md](quickstart.md)).

**Target Platform**: Windows, macOS and Linux desktops (x64 and arm64)

**Project Type**: desktop app

**Performance Goals**: no background work. The change check reads the data file's last-write time and size once each
time the main window is activated or the editor opens; a reload reads one small JSON file.

**Constraints**: use the chosen file where it is (no copy, no Import, no merge); never overwrite changes made
elsewhere, or a file that couldn't be loaded, without asking; work with any cloud drive that syncs a local folder
(OneDrive, iCloud Drive, Dropbox, …); the app writes only `usersettings.json` and the data file; the v4 data format
stays unchanged.

**Scale/Scope**: one data file per computer with one family's people; three commands, two shortcuts, two questions in
the editor; one new `Load` overload and four new members on `AnniversaryBook` (`DataFileLoadFailed`, `OpenDataFile`,
`HasDataFileChanged`, `ReloadPeopleIfChanged`).

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | No new type, project or package. Choosing, opening, change tracking and reloading are in `AnniversaryBook`, which still shows no UI and doesn't read the clock (it compares the file's own time stamp). The views stay code-behind; `MessageDialog` gets one more method. |
| II. Established Coding Style and Complete XML Documentation | PASS | The new public members (`Load` overload, `DataFileLoadFailed`, `OpenDataFile`, `HasDataFileChanged`, `ReloadPeopleIfChanged`, `UserSettings.DataFile`, `MessageDialog.Confirm`) have XML docs; `DataFileTests` has a `<summary>` naming spec 5.3–5.6 and 7.11, and its test names are sentences; v4 style. |
| III. Classic Tests on Real Objects | PASS | `DataFileTests` loads real `AnniversaryBook`s from real files in two `TempFolder`s (app-data and a stand-in cloud folder); "another computer" is simulated by rewriting the file and moving its last-write time. No mocks. docs/spec.md §9 lists the tests; the native dialogs, menus and questions are manual checks (§12 criteria 13 and 14, [quickstart.md](quickstart.md)). |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | One implementation in `Anniversary.Shared` for all three OSes, with no platform code; macOS conventions (File menu, ⌘O and ⇧⌘S, "Finder") and Ctrl+O / Ctrl+Shift+S elsewhere. At the time, a headless run of the real windows on macOS confirmed the menus and the questions; the native dialogs and Show in Finder can't run headless and were left for a manual check with `dotnet run`. No record says they were then checked on each OS. |
| V. Safe, Private and Compatible User Data | PASS with justification | This feature is the origin of the rules "the editor asks before overwriting a data file that changed elsewhere or could not be loaded" and "a chosen data file is used where it is, never copied". The format is unchanged; Save As… uses the existing write to `{target}.tmp`; the app writes only `usersettings.json` and the data file. Loading at start-up and reloading never throw (warnings). Only the explicit Open… reports a bad file by throwing to the window, which shows a message and changes nothing, as Save already does (002). |
| VI. Verifiable Calendar Data | N/A | No calendar data changes. |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer chose "where it is" (docs/plan.md, Decisions); the **Save Anyway** question went beyond the agreed plan, was reported in the session and kept. docs/spec.md (5.3–5.6, 6, 7.2, 7.6, 7.8, new 7.11, 8, 9, C20, criteria 13 and 14), docs/plan.md, the handover and `README.md` changed in the same commit. The user guide didn't exist yet (009). |

**Post-design re-check**: PASS. The design only adds members to existing types (`AnniversaryBook`, `UserSettings`,
`MessageDialog`, the two windows), adds no package and keeps all file logic UI-free; the manual-check gap under
Principle IV remains as noted.

## Project Structure

### Documentation (this feature)

```text
specs/007-data-file-sharing/
├── spec.md                   # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md       # spec quality checklist
├── plan.md                   # this file
├── research.md               # Phase 0
├── data-model.md             # Phase 1
├── quickstart.md             # Phase 1
├── contracts/                # Phase 1
│   ├── data-file-menu.md     # Data File commands, dialogs, questions and messages
│   ├── data-file-api.md      # AnniversaryBook data-file API (and MessageDialog.Confirm)
│   └── data-file-setting.md  # dataFile in usersettings.json
└── tasks.md                  # /speckit-tasks
```

### Source Code (repository root)

```text
Anniversary.Shared/
├── AnniversaryBook.cs              # changed: Load overload with the chosen data file, DataFileLoadFailed,
│                                   #   OpenDataFile, HasDataFileChanged, ReloadPeopleIfChanged, the file stamp
├── App.axaml.cs                    # changed: passes settings.DataFile to AnniversaryBook.Load
├── Model/
│   └── UserSettings.cs             # changed: DataFile
└── Views/
    ├── MainWindow.axaml            # changed: Data File submenu; macOS File menu items
    ├── MainWindow.axaml.cs         # changed: Open…, Save As…, Show in Folder, shortcuts, reload on activation
    ├── MessageDialog.cs            # changed: Confirm (two buttons, Cancel is the default)
    └── PeopleEditorWindow.axaml.cs # changed: Overwrite / Save Anyway questions before saving
Anniversary.Tests/
└── DataFileTests.cs                # new: 16 test cases
README.md                           # changed: macOS menu bar, files table, "Sharing your data", moving from v4
docs/spec.md                        # changed (then specs/spec.md)
docs/plan.md                        # changed (then specs/plan.md)
docs/copilot/handover.md            # changed (then specs/copilot/handover.md)
```

**Structure Decision**: The existing three-project layout is kept. Everything lives in `Anniversary.Shared` (logic in
`AnniversaryBook`, UI in the existing views) because the dialogs, the file manager and the file checks need no
platform code; `Anniversary.Desktop` is unchanged. The only new file is the test class.

## Complexity Tracking

No violations.
