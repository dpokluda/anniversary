# Implementation Plan: Holiday Calendar Rules

**Branch**: `006-holiday-calendar-rules` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/006-holiday-calendar-rules/spec.md`

## Summary

Let calendar files hold days that move from year to year, ship a US holiday calendar, and choose the default calendar
by the OS region. When a calendar loads, `AnniversaryBook` splits its keys into `MM-dd` entries and rule entries
(`MM-Ddd-n`, `MM-Ddd-last` and `easter`, each with an optional offset of ±1–99 days, recognised by one case-insensitive
regular expression) and reports any other key in one warning. The names for a day are the `MM-dd` entry followed by
every rule entry whose date, worked out for the day's year and the years before and after, is that day; "Today is",
Next N days and Find all use them, and Easter comes from the anonymous Gregorian algorithm.
`calendars/Build-Calendars.ps1` validates rule keys, writes `Anniversary.Calendar.US.json` (11 federal holidays and
19 observances, each with its source) and adds Velký pátek and Velikonoční pondělí to the Czech calendar. When no
calendar is chosen or configured, `Load` takes the first calendar whose `locale` has the region of
`CultureInfo.CurrentCulture`, otherwise the US one; the built-in `appsettings.json` no longer names a calendar. The same
commit also carried a separate fix for the crash at the ends of the date range, a known issue that 005 had recorded in
the handover; it isn't part of this feature.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`)

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme (the existing calendar menu lists one more calendar;
no new UI); `System.Text.Json` for the calendar files; `System.Text.RegularExpressions` (the rule-key grammar) and
`System.Globalization` (`CultureInfo`, `RegionInfo`: the OS region) from the .NET base library. No new packages.
PowerShell 7 runs `calendars/Build-Calendars.ps1`.

**Storage**: JSON files. Calendar files (`Anniversary.Calendar.*.json`, embedded from `calendars/`, or a user's own
next to the executable or in the app-data folder) may use rule keys; `calendars/Anniversary.Calendar.US.json` is new;
the built-in `appsettings.json` loses `calendarFile`; `usersettings.json` is unchanged (its `calendar` came with 003).

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`dotnet test --project Anniversary.Tests`), classic style with
real files in a `TempFolder` and the embedded shipped calendars. `TestFiles.LoadInCulture` sets
`CultureInfo.CurrentCulture` for one load, so the region tests pass on any machine. 85 new test cases: 75 in
`Anniversary.Tests/CalendarRuleTests.cs` and 10 in `Anniversary.Tests/CalendarTests.cs`; 165 tests in total, including
the 2 of the separate fix. The default on a real OS and the menu are checked by hand ([quickstart.md](quickstart.md)).

**Target Platform**: Windows, macOS and Linux desktops (x64 and arm64)

**Project Type**: desktop app

**Performance Goals**: no stored dates and no caches. A rule's date is worked out when a day is shown or searched, for
three years per rule; Find still ends on 31 December of the following year.

**Constraints**: v4 calendar files and `MM-dd` keys keep working unchanged; loading never throws (unknown keys give one
warning, and dates outside `DateOnly`'s range are skipped); holiday entries come from the law or long tradition and are
written in the build script with their sources, while name days still come only from pinned sources; the generated
files are the same whichever OS runs the script; people's dates keep their formats.

**Scale/Scope**: 11 shipped calendars (US new). US: 30 entries, 16 `MM-dd` dates and 14 rules. Czech: 366 `MM-dd`
entries and 2 rules. Three rule forms plus offsets. No new public member on `AnniversaryBook`; `AppConfig.CalendarFile`
becomes nullable.

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | No new type, project or package. The rule keys, the date calculation and the region choice are private members of `AnniversaryBook`, which still shows no UI and doesn't read the clock: dates come from the day passed in. It reads `CultureInfo.CurrentCulture` for the region, as it already did for sorting the calendars, and tests set the culture. The rule entries are kept as `(Match Rule, string Names)` tuples, not a new type. |
| II. Established Coding Style and Complete XML Documentation | PASS | The changed public documentation (`Load`'s `preferredCalendar`, `AppConfig.CalendarFile`, `CalendarData` and its `Data`, `DayInfo.CalendarNames`) names the spec sections; `CalendarRuleTests` has a `<summary>` naming spec 5.1, 6.3 and 5.7, `TestFiles.LoadInCulture` is documented, and the test names are sentences. Short comments explain the rule pattern, the three-year check and the Easter algorithm; v4 style. |
| III. Classic Tests on Real Objects | PASS | `CalendarRuleTests` loads real `AnniversaryBook`s from calendar files written to a `TempFolder` and from the embedded shipped calendars; the region tests set the real current culture with a small helper (`LoadInCulture`), with no mocks. docs/spec.md §9 lists the new tests, and §12 got acceptance criterion 12 for the manual check ([quickstart.md](quickstart.md)). |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | One implementation in `Anniversary.Shared` for all three OSes, with no platform code; the region comes from the culture .NET reads from each OS. At the time, the default was checked on the Mac the work was done on (region `en-US`: US) and with `LC_ALL=cs_CZ.UTF-8` (the real main window opened on Czech), and a headless run of the real window confirmed Thanksgiving, the calendar menu and the switch to Czech. No record says the region default was then checked on Windows or Linux. |
| V. Safe, Private and Compatible User Data | PASS | The v4 calendar format stays readable: rule keys are optional, `MM-dd` is unchanged and the Czech calendar keeps its v4 values. Loading never throws: an unknown key becomes a warning, and a rule date outside the range is skipped. The app writes nothing new. The default calendar changed from Czech (v4) to the OS region; a copied v4 `appsettings.json` still selects Czech, and the change is recorded as C19. |
| VI. Verifiable Calendar Data | PASS | This feature is the origin of the rule that holidays and observance days are written in the script, each with its source: every US entry carries its U.S. Code section or "tradition", the Czech additions cite zákon č. 245/2000 Sb., § 2, and both files carry `source` and `license`. `calendars/README.md` lists the sources and the OPM check; name days still come only from pinned sources; the script reproduced the committed files exactly on macOS. |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer asked for the US calendar and questioned US as everyone's default; the menu name was kept (user decision). docs/spec.md (1, 5.1, 5.2, 5.4, 5.7, 6, 6.1–6.3, 7.10, 9, C19, criteria 3, 11 and 12), docs/plan.md, the handover, `README.md` and `calendars/README.md` changed in the same commit. The user guide didn't exist yet (009). |

**Post-design re-check**: PASS. The design adds only private members to `AnniversaryBook`, one nullable property, one
test helper and one test class; no package, no platform code, and the calendar format only grows. The manual-check gap
under Principle IV remains as noted.

## Project Structure

### Documentation (this feature)

```text
specs/006-holiday-calendar-rules/
├── spec.md                      # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md          # spec quality checklist
├── plan.md                      # this file
├── research.md                  # Phase 0
├── data-model.md                # Phase 1
├── quickstart.md                # Phase 1
├── contracts/                   # Phase 1
│   ├── calendar-rule-keys.md    # rule-key grammar, names on a day, unknown-key warning
│   ├── default-calendar.md      # which calendar is active: menu choice, calendarFile, OS region, US
│   └── holiday-calendars.md     # the shipped US entries, the Czech additions, the build script
└── tasks.md                     # /speckit-tasks
```

### Source Code (repository root)

```text
Anniversary.Shared/
├── AnniversaryBook.cs              # changed: rule keys (RuleKeyPattern, AddCalendarEntries, CalendarNamesOn,
│                                   #   TryResolveRule, EasterSunday), the default by region (CalendarForRegion,
│                                   #   RegionOf, FallbackCalendarFile); GetDay and Find use CalendarNamesOn
├── DayInfo.cs                      # changed: CalendarNames documented as names or holidays
├── Defaults/
│   └── appsettings.json            # changed: no calendarFile (historical; removed by 012)
├── Model/
│   ├── AppConfig.cs                # changed: CalendarFile is string?, null by default (historical; removed by 012)
│   └── CalendarData.cs             # changed: documentation of rule keys
└── Views/
    └── MainWindow.axaml.cs         # changed by the separate date-range fix only
Anniversary.Tests/
├── CalendarRuleTests.cs            # new: 75 test cases
├── CalendarTests.cs                # changed: 11 shipped calendars, 10 new region test cases, adjusted tests
├── DayTests.cs                     # changed by the separate date-range fix only (1 test)
├── FindTests.cs                    # changed by the separate date-range fix only (1 test)
├── LoadTests.cs                    # changed: the built-in defaults name no calendar; en-US gives US
└── TestFiles.cs                    # changed: LoadInCulture
calendars/
├── Anniversary.Calendar.US.json    # new: 16 dates and 14 rules
├── Anniversary.Calendar.Czech.json # changed: easter-2, easter+1 and the source
├── Build-Calendars.ps1             # changed: rule keys, -Holidays, ordinal order, the US table, the Czech holidays
└── README.md                       # changed: title, US and Czech rows, "The US holiday calendar", Greece
README.md                           # changed: "Your data", "Moving from v4", "Calendars", projects table
docs/spec.md                        # changed (then specs/spec.md)
docs/plan.md                        # changed (then specs/plan.md)
docs/copilot/handover.md            # changed (then specs/copilot/handover.md)
```

**Structure Decision**: The existing three-project layout is kept. All logic is in `AnniversaryBook` in
`Anniversary.Shared`, because working out dates and reading the region need no platform code; the views are unchanged
by this feature, and `Anniversary.Desktop` is untouched. The new calendar file is embedded by the existing
`Anniversary.Calendar.*.json` item in `Anniversary.Shared/Anniversary.Shared.csproj`, so no project file changed. The
only new files are the calendar and the test class.

## Complexity Tracking

No violations.
