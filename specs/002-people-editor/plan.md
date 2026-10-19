# Implementation Plan: People Editor

**Branch**: `002-people-editor` | **Date**: 2026-10-09 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-people-editor/spec.md`

## Summary

Let users add and change people in the app. **⋯ ▸ Edit People…** (macOS: **File ▸ Edit People…**) opens
`PeopleEditorWindow`, a modal, resizable window with an Avalonia `DataGrid` over a `DataGridCollectionView` of
`PersonRow`s, a copy of `AnniversaryBook.People`: Add and Remove, a filter by name, sorting by column (view only) and
cell errors through `INotifyDataErrorInfo`, with Save disabled while any row is invalid. An **Edit…** button in each row
opens `AnniversariesDialog`, which edits that person's anniversaries as `AnniversaryRow`s with unique dates. Save calls
`AnniversaryBook.SavePeople` with `SavePathFor()` (the file the people came from, or the app-data folder for the empty
built-in list); it writes the v4 format through `{target}.tmp`, then replaces `People` and rebuilds the day index, so
the main window refreshes without loading the file again. The one new package is `Avalonia.Controls.DataGrid`; the only
new types besides the two windows are the editor-only `PersonRow` and `AnniversaryRow`.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`)

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme (`CalendarDatePicker` cells, modal windows);
`Avalonia.Controls.DataGrid` 12.1.2, versioned separately from Avalonia (the grid, its Fluent theme, and
`DataGridCollectionView` for sorting and filtering); `System.Text.Json` (writing with
`JavaScriptEncoder.UnsafeRelaxedJsonEscaping`).

**Storage**: the people data file (JSON in the v4 format, `Anniversary.Data.json` by default), written back to the file
it was loaded from or created in the user's app-data folder, always through `{target}.tmp`. Nothing else is written;
`appsettings.json` never.

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`dotnet test --project Anniversary.Tests`), classic style with
real files in `TempFolder`s and real row objects; 30 test cases in `Anniversary.Tests/SaveTests.cs` (13) and
`Anniversary.Tests/EditorRowTests.cs` (17), 78 tests in phase 1 in total; the windows are checked by hand
([quickstart.md](quickstart.md)).

**Target Platform**: Windows (x64), macOS (arm64 and x64) and Linux (x64) desktops, the phase-1 targets; `win-arm64` and
`linux-arm64` came with [005](../005-ci-pipeline/spec.md).

**Project Type**: desktop app

**Performance Goals**: none recorded. The editor sorts and filters an in-memory copy of one family's list; Save writes
one small JSON file and rebuilds the day index, so the main window updates without loading the file again.

**Constraints**: keep v4's data format; never leave a half-written file; change nothing until Save; code-behind views
without MVVM, and no new types except the two editor rows; the app writes only `usersettings.json` and, through the
editor, the people file (spec C9).

**Scale/Scope**: one family's people list; two windows (the editor and the Anniversaries dialog), two editor-only row
classes, one menu item in two menus, and the editing members of `AnniversaryBook` (`People`, `DataFilePath`,
`SavePathFor`, `SavePeople`, `IsValidNameday`, with `AppDataFolder`).

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS with justification | Two new types, the editor-only `PersonRow` and `AnniversaryRow` (see Complexity Tracking); the windows use code-behind, without MVVM. Saving, the save target and the nameday rule live in `AnniversaryBook`, which shows no UI and doesn't read the clock; the cell rules live in the rows, where `PersonRow.NamedayDate` uses the current year only to give the date picker a date (the stored nameday has no year). The one new package, `Avalonia.Controls.DataGrid`, is on the allowed list. |
| II. Established Coding Style and Complete XML Documentation | PASS | The new types and all their public members have XML docs (`SavePeople` documents its two exceptions); `SaveTests` and `EditorRowTests` have a `<summary>` naming spec 5.6 and 7.8–7.9, and their test names are sentences; v4 style. |
| III. Classic Tests on Real Objects | PASS | `SaveTests` loads real `AnniversaryBook`s from real files in two `TempFolder`s (the executable's folder and app-data) and reads the written file itself; `EditorRowTests` uses real rows. No mocks. docs/spec.md §9 lists both; the windows are manual checks (§12 criterion 10, [quickstart.md](quickstart.md)). |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | One implementation in `Anniversary.Shared` with no platform code; Edit People… is also in the macOS File menu; the grid uses the Fluent theme and the app's font size. Phase 1 was built, tested and run live on Windows. At the move to GitHub, the macOS menu-bar item and saving on Linux were still on the handover's check lists; the maintainer later reported that the app works on macOS and Fedora (session 2026-10-10), with no record of each editor check. |
| V. Safe, Private and Compatible User Data | PASS with justification | The format stays v4's (round-trip tested); the file is written to `{target}.tmp`, which then replaces the target; no people are built in; the app writes only `usersettings.json` and, through the editor, the data file; the editor works on a copy. The rule that the editor asks before overwriting a data file that changed elsewhere or couldn't be loaded came later, with [007](../007-data-file-sharing/plan.md): here Save replaced its target without asking, an unreadable file in that place included. |
| VI. Verifiable Calendar Data | N/A | No calendar data changes; a nameday is a person's own date. |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer asked for the editor, saw the mocks and approved it before any code: the session lists the spec changes made then (7.8, 7.9, the save logic, the tests, the menu entries, C9, the acceptance criteria, the implementation order and the file layout), and docs/plan.md records the decision ("People editor") and work item 10. C14 records the change from v4; `README.md` and the handover describe the editor. The user guide didn't exist yet (009). |

**Post-design re-check**: PASS. The design adds only the two row classes justified below, two windows, the editing
members on `AnniversaryBook` and one allowed package; all file logic stays UI-free. The gap noted under Principle V was
closed by 007.

## Project Structure

### Documentation (this feature)

```text
specs/002-people-editor/
├── spec.md                    # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md        # spec quality checklist
├── plan.md                    # this file
├── research.md                # Phase 0
├── data-model.md              # Phase 1
├── quickstart.md              # Phase 1
├── contracts/                 # Phase 1
│   ├── editor-ui.md           # Edit People…, the editor and the Anniversaries dialog: texts, keys, messages
│   ├── people-file-write.md   # the people data file as the editor writes it
│   └── editing-api.md         # AnniversaryBook editing API
└── tasks.md                   # /speckit-tasks
```

### Source Code (repository root)

Phase 1 reached this repository in one commit (`67a4092`), so "new" marks files that exist only for this feature and
"changed" marks this feature's part of a file shared with [001](../001-cross-platform-rewrite/plan.md).

```text
Directory.Build.props                   # changed: AvaloniaDataGridVersion 12.1.2
Anniversary.Shared/
├── Anniversary.Shared.csproj           # changed: Avalonia.Controls.DataGrid package
├── AnniversaryBook.cs                  # changed: People, DataFilePath, SavePathFor, SavePeople, IsValidNameday
├── App.axaml                           # changed: DataGrid Fluent theme, grid font size, row height, date picker cells
└── Views/
    ├── MainWindow.axaml                # changed: Edit People… in the commands menu and the macOS File menu
    ├── MainWindow.axaml.cs             # changed: EditPeople(), refresh after Save, macOS item wired by header
    ├── PeopleEditorWindow.axaml        # new: toolbar, grid, footer
    ├── PeopleEditorWindow.axaml.cs     # new: copy, Add/Remove, filter, sort, validation, Save/Cancel
    ├── AnniversariesDialog.axaml       # new: toolbar, grid, error note, OK/Cancel
    ├── AnniversariesDialog.axaml.cs    # new: copy, Add/Remove, validation, OK/Cancel
    ├── PersonRow.cs                    # new: editable person row with validation
    └── AnniversaryRow.cs               # new: editable anniversary row with validation
Anniversary.Tests/
├── SaveTests.cs                        # new: 13 test cases
└── EditorRowTests.cs                   # new: 17 test cases
README.md                               # changed: the people file is edited with ⋯ ▸ Edit People…
docs/spec.md                            # changed (then specs/spec.md)
docs/plan.md                            # changed (then specs/plan.md)
docs/screens/                           # new: people-editor (light, dark) and anniversaries-editor mocks
                                        #   (then specs/screens/)
```

**Structure Decision**: The three-project layout is kept. The editor windows and the two row classes live in
`Anniversary.Shared/Views/` (the rows are editor-only types, next to the views that use them), saving lives in
`AnniversaryBook`, and `Anniversary.Desktop` is unchanged, because the editor needs no platform code.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| New type `PersonRow` (editor-only, `Anniversary.Shared/Views/PersonRow.cs`) | Two-way editing in the grid needs change notification, cell errors need `INotifyDataErrorInfo`, and the date pickers need `DateTime?` (spec 7.8) | Editing `Person` directly: it has no change notification or error reporting, and it stores `DateOnly?` and `MM-dd` text |
| New type `AnniversaryRow` (editor-only, `Anniversary.Shared/Views/AnniversaryRow.cs`) | The same role in the Anniversaries dialog; the duplicate-date check runs over the whole list (spec 7.8, 7.9) | Editing the person's `Dictionary<DateOnly, string>` directly: no change notification or cell errors, and the date picker needs `DateTime?` |
