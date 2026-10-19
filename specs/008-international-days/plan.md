# Implementation Plan: International Days

**Branch**: `008-international-days` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/008-international-days/spec.md`

## Summary

Add a twelfth built-in calendar, **International Days**, for people whose country has no name days: 56 well-known days
chosen with the maintainer from the United Nations list of international days, plus five fun days, International Beer
Day among them. Like the US calendar ([006](../006-holiday-calendar-rules/plan.md)), its entries are written in
`calendars/Build-Calendars.ps1`, each with its source, and generated into
`calendars/Anniversary.Calendar.International.json`, which the existing wildcard embeds in the app. Days that move use
006's rule keys (`08-Fri-1`, `05-Sat-2`, `10-Sat-2`). The file has no `locale`, so the OS-region default never picks
it. In the same change the menu **Nameday Calendar** becomes **Calendar** (⋯ menu and macOS View menu), and
`AnniversaryBook.Calendars` lists the calendars with a locale first and those without after them, with a separator
between the two groups in both menus.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`); PowerShell 7 for the calendar build script

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme (`MenuItem` and `Separator` in the ⋯ menu,
`NativeMenuItem` and `NativeMenuItemSeparator` in the macOS menu bar); `System.Text.Json` reads the calendar files. No
new packages.

**Storage**: JSON files. The new `calendars/Anniversary.Calendar.International.json` is embedded in the app by the
existing `Anniversary.Calendar.*.json` wildcard in `Anniversary.Shared/Anniversary.Shared.csproj` (no project change);
the chosen calendar is saved in `usersettings.json` as before ([003](../003-calendar-chooser/spec.md)).

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`dotnet test --project Anniversary.Tests`), classic style: 8 new
test cases in `Anniversary.Tests/CalendarRuleTests.cs` load the shipped calendar; `Anniversary.Tests/CalendarTests.cs`
checks the count and the order (189 tests in total). The menu name and the separator were checked in a headless run of
the real menus and are checked by hand with [quickstart.md](quickstart.md).

**Target Platform**: Windows, macOS and Linux desktops (x64 and arm64)

**Project Type**: desktop app

**Performance Goals**: no measurable cost: one more small embedded calendar (61 entries), and a list of about a dozen
calendars sorted whenever it is rebuilt (each time the ⋯ menu opens).

**Constraints**: one calendar shows at a time; the "Today is" line sits in v4's small window, so the selection stays
short; every day comes from the UN list or from tradition, never from commercial "national day" lists (copyrighted);
the calendar file format (spec 5.1) is unchanged; the file is generated, never edited by hand.

**Scale/Scope**: one new calendar file (58 dates and 3 rules), 12 shipped calendars; one menu renamed in two places;
one sort changed; one separator in two menus; 8 new test cases and two updated ones; documentation in five files.

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | No new type, project or package. The order is one LINQ sort in `AnniversaryBook` (`AddActiveCalendarIfMissing`); the separator is a local `previousHasLocale` flag in the code-behind's `UpdateCalendarMenus`, which replaced a first version that stashed a flag in `Tag` because that was harder to read. |
| II. Established Coding Style and Complete XML Documentation | PASS | The `Calendars` summary describes the new order; the two inline comments name the spec sections (5.7, 7.10). The new test is a sentence (`InternationalCalendarHasTheseDays`) in a class whose summary already covers "the shipped holiday entries (5.7)". v4 style. |
| III. Classic Tests on Real Objects | PASS with justification | `InternationalCalendarHasTheseDays` loads the real shipped calendar through `AnniversaryBook.Load` and checks 8 days; `CalendarTests` checks the order of the shipped calendars and of a user's file in a `TempFolder`. No mocks; docs/spec.md §9 lists both. The menus themselves can't be unit-tested: §12 criterion 11 got the new name, but no criterion names the separator; it was confirmed in a headless run of the real menus and is a manual check in [quickstart.md](quickstart.md). |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | Shared code only. The ⋯ menu gets a `Separator` and the macOS View menu a `NativeMenuItemSeparator`, so each OS shows its own kind. At the time, a headless run of the real menus on the Mac showed Austria … United States, the separator, then International Days; no record says the rename and the separator were then checked on Windows and Linux. |
| V. Safe, Private and Compatible User Data | PASS | No user data changes and nothing new is written. The calendar format is unchanged: `locale` was already optional (003), so a file without it is valid, and v4-style files still load (they join the second group). The calendar is a shipped file with no personal data. |
| VI. Verifiable Calendar Data | PASS | Generated by `calendars/Build-Calendars.ps1`; re-running it wrote the new file and left the other 11 unchanged. Every UN day is copied from the parsed UN list with its resolution (or "UN list") in the script, the fun days say "tradition", and the file carries `source` and `license`. `calendars/README.md` lists the selection, the date check and what was left out (weeks, Vesak). Commercial "national day" sites were ruled out as copyrighted when the feature was first planned; the constitution's rule against them goes back to that. `calendars/THIRD-PARTY-NOTICES.md` is unchanged: the calendar's license is the app's own (MIT, part of Anniversary). |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer chose the size of the selection, approved the list, and chose the rename and the order ([research.md](research.md); docs/plan.md, Decisions: "renamed … (user decision)"). docs/spec.md (5.1, 5.7, 6, 7.2, 7.10, 9, C15, new C21, criterion 11), docs/plan.md, the handover, `README.md` and `calendars/README.md` changed in the same commit. The user guide didn't exist yet (009). |

**Post-design re-check**: PASS. The design adds data (one generated file) and changes one sort, the menu's name and
one menu-building loop; no new type or package; the manual-check gap under Principles III and IV remains as noted.

## Project Structure

### Documentation (this feature)

```text
specs/008-international-days/
├── spec.md                              # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md                  # spec quality checklist
├── plan.md                              # this file
├── research.md                          # Phase 0
├── data-model.md                        # Phase 1
├── quickstart.md                        # Phase 1
├── contracts/                           # Phase 1
│   ├── international-calendar-file.md   # the calendar's metadata, entries, sources and generation
│   └── calendar-menu.md                 # Calendar menu name, order and separator; AnniversaryBook.Calendars
└── tasks.md                             # /speckit-tasks
```

### Source Code (repository root)

```text
Anniversary.Shared/
├── AnniversaryBook.cs                       # changed: Calendars sorted with a locale first, then without
└── Views/
    ├── MainWindow.axaml                     # changed: "Calendar" in the ⋯ menu and in the macOS View menu
    └── MainWindow.axaml.cs                  # changed: finds View ▸ Calendar; separator before calendars
                                             #   without a locale
Anniversary.Tests/
├── CalendarRuleTests.cs                     # changed: InternationalCalendarHasTheseDays (8 cases)
└── CalendarTests.cs                         # changed: 12 shipped calendars, the order, International Days last
calendars/
├── Anniversary.Calendar.International.json  # new: generated, 58 dates + 3 rules, no locale
├── Build-Calendars.ps1                      # changed: the International Days table; locale only when given
└── README.md                                # changed: table row, "The International Days calendar"
README.md                                    # changed: 12 calendars, International Days, ⋯ ▸ Calendar
docs/spec.md                                 # changed (then specs/spec.md)
docs/plan.md                                 # changed (then specs/plan.md)
docs/copilot/handover.md                     # changed (then specs/copilot/handover.md)
```

**Structure Decision**: The existing three-project layout is kept. The calendar is data: a generated file in
`calendars/`, embedded by the existing wildcard, so neither project file changes. The order lives in `AnniversaryBook`
(UI-free, testable) and the separator in the main window's code-behind, which already builds both calendar menus;
`Anniversary.Desktop` is unchanged.

## Complexity Tracking

No violations.
