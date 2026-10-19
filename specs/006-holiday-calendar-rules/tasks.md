# Tasks: Holiday Calendar Rules

**Input**: Design documents from `specs/006-holiday-calendar-rules/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic, file format and settings changes: 75
test cases in the new `Anniversary.Tests/CalendarRuleTests.cs` and 10 new ones in `Anniversary.Tests/CalendarTests.cs`.
The default on a real OS, the menu and the messages are checked by hand with [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. Everything went into one commit, `5bb1afe` ("Adding calendar (namedays)
rules and US calendar", 2026-10-10), together with a separate fix for the ends of the date range (see T001).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): logic in `Anniversary.Shared/`, tests in `Anniversary.Tests/`, calendars and their build
  script in `calendars/`, documentation in `README.md`, `calendars/README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and the test class

- [X] T001 Start from the state 005 left (`1409624`: `Anniversary.slnx` building without warnings, 78 tests) plus the separate fix for the ends of the date range, a known issue recorded in that commit's handover, made just before in the same session and committed with this feature: `GetDay` and `Find` in `Anniversary.Shared/AnniversaryBook.cs` stop at 31 December 9999, Yesterday, Tomorrow and Find Next in `Anniversary.Shared/Views/MainWindow.axaml.cs` do nothing (or report "not found") at the ends, with `NextDaysStopOnLastDayOfYear9999` in `Anniversary.Tests/DayTests.cs` and `SearchStopsOnLastDayOfYear9999` in `Anniversary.Tests/FindTests.cs` (80 tests)
- [X] T002 [P] Create `Anniversary.Tests/CalendarRuleTests.cs`: a sealed `IDisposable` test class with the summary "Tests of rule keys for dates that move from year to year (spec 5.1, 6.3) and of the shipped holiday entries (5.7)" and one `TempFolder` deleted in `Dispose`; its tests write their calendars with `TestFiles.Book(_folder, calendar: …)`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Rule keys in calendar files: recognised, worked out for the years shown, and used by "Today is", Next N
days and Find

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Add the rule-key grammar to `Anniversary.Shared/AnniversaryBook.cs`: `RuleKeyPattern`, one case-insensitive, culture-invariant regular expression with the groups `month` (`01`–`12`), `weekday` (`mon`…`sun`), `nth` (`1`–`4` or `last`) and `offset` (`+` or `-`, then 1–99 without a leading zero), or `easter` with an optional offset; and `WeekdayNames`, indexed by `DayOfWeek` (FR-001, FR-002, FR-003)
- [X] T004 Split a calendar's entries when it loads, in `Anniversary.Shared/AnniversaryBook.cs`: `LoadCalendar` resets `_calendar` and the new `_calendarRules` (`List<(Match Rule, string Names)>`), and `AddCalendarEntries(fileName, entries)` puts each non-empty key that `IsValidNameday` accepts ("MM-dd" existing in a leap year) into `_calendar` and each key matching `RuleKeyPattern` into `_calendarRules`, in file order (FR-001, FR-015)
- [X] T005 Work out rule dates in `Anniversary.Shared/AnniversaryBook.cs`: `TryResolveRule(rule, year, out date)` takes `MM-Ddd-n` as the first weekday on or after the 1st plus `7 × (n − 1)` days, `MM-Ddd-last` as the last weekday on or before the month's last day and `easter` from `EasterSunday(year)` (the anonymous Gregorian algorithm of Meeus, Jones and Butcher), then adds the offset in day numbers; it returns `false` when the year or the result is outside `DateOnly`'s range (FR-001, FR-002, FR-007)
- [X] T006 Add `CalendarNamesOn(day)` to `Anniversary.Shared/AnniversaryBook.cs`: the day's non-empty `MM-dd` entry, then each rule entry whose date in the day's year, the year before or the year after is that day, in file order, joined with ", "; use it in `GetDay` for `CalendarNames` and for the names in Next N days, and in `Find` for matching (FR-004, FR-005)
- [X] T007 [P] Update the documentation comments: the class remarks and `Data` in `Anniversary.Shared/Model/CalendarData.cs` ("a calendar file (name days or holidays)"; keys in `MM-dd` format or a rule key such as `11-Thu-4`, `05-Mon-last` or `easter+1`, spec 5.1) and `CalendarNames` in `Anniversary.Shared/DayInfo.cs` ("names or holidays")
- [X] T008 [P] Accept rule keys in `calendars/Build-Calendars.ps1`: `Test-RuleKey` with the app's pattern; `Write-Calendar` throws for keys that are neither `MM-dd` nor rules, counts only `MM-dd` keys for the full-year check, sorts keys ordinally ("so the files are the same whichever OS runs the script") and prints "entries" instead of "days" (FR-001)
- [X] T009 [P] Add the rule tests to `Anniversary.Tests/CalendarRuleTests.cs`: `WeekdayRuleFallsOnItsDayOnly` (8 cases, among them 31 May 2027 in a May with five Mondays and 29 February 2028 for `02-Tue-last`; nothing a week before or after), `EasterFallsOnKnownDates` (12 years from 1 to 9999), `EasterIsASundayFrom22MarchTo25AprilInEveryYear` (every year from 1 to 9999), `OffsetMovesTheDateEvenIntoAnotherYear` (6 cases, among them `12-Mon-last+7` on 4 January 2027 and `01-Mon-1-7` on 29 December 2025), `DayEntryComesFirstThenRulesInFileOrder`, `RuleKeysIgnoreCase`, `NextDaysAcrossNewYearUseTheNextYearsRules`, `FindFindsARuleDateInTheFollowingYear` and `RulesAtTheEndsOfTheDateRangeDontFail` (FR-001 to FR-005, FR-007)

**Checkpoint**: Any calendar file can use rule keys; user stories can begin

---

## Phase 3: User Story 1 - See the US holidays, also those that move (Priority: P1) 🎯 MVP

**Goal**: A United States calendar with the federal holidays and well-known observances ships with the app

**Independent Test**: Choose the United States calendar: 26 November 2026 shows Thanksgiving, 25 May 2026 Memorial Day
and 5 April 2026 Easter (quickstart scenario 1)

### Tests for User Story 1

- [X] T010 [P] [US1] Add `UsCalendarHasTheFederalHolidays` (the 11 federal holidays of 2025–2027 on the dates in OPM's schedules, fixed-date holidays on their legal date: 33 cases) and `UsCalendarHasTheFloatingObservances` (Mardi Gras, Daylight Saving Time begins, Good Friday, Easter, Mother's Day, Father's Day, Daylight Saving Time ends and Black Friday in 2026) to `Anniversary.Tests/CalendarRuleTests.cs`, both loading the shipped file with `AnniversaryBook.Load("Anniversary.Calendar.US.json")` (FR-008, FR-009)
- [X] T011 [P] [US1] Update `ShippedCalendarsAreDiscovered` (11 calendars, among them `Anniversary.Calendar.US.json` as "United States") and `UserCalendarIsAddedToShippedOnes` (12) in `Anniversary.Tests/CalendarTests.cs` (FR-014)

### Implementation for User Story 1

- [X] T012 [US1] Add the US table to `calendars/Build-Calendars.ps1`: 30 entries in an ordered table, each with its source in a comment (5 U.S.C. 6103(a), 36 U.S.C. 109, 110, 117 or 144, 15 U.S.C. 260a(a), or tradition), written by `Write-Calendar 'US' 'United States' 'en-US'` with the `source` "5 U.S.C. 6103(a) (federal holidays); 36 U.S.C. 109, 110, 117, 144 (observances); 15 U.S.C. 260a (daylight saving time); traditional dates", the MIT license line and the new `-Holidays` switch, which skips the full-year check (FR-008, FR-009)
- [X] T013 [US1] Generate `calendars/Anniversary.Calendar.US.json` with the script: 16 `MM-dd` dates and 14 rules. The existing `Anniversary.Calendar.*.json` item in `Anniversary.Shared/Anniversary.Shared.csproj` embeds it, so it appears in ⋯ ▸ Nameday Calendar (macOS View ▸ Nameday Calendar) after Slovakia (Slovensko) (FR-008, FR-014)
- [X] T014 [US1] Check scenario 1 of `specs/006-holiday-calendar-rules/quickstart.md` by hand (docs/spec.md §12, criterion 12)

**Checkpoint**: The US calendar can be chosen and shows its holidays, also those that move

---

## Phase 4: User Story 2 - Start with the calendar for your region (Priority: P2)

**Goal**: Without a menu choice or a configured `calendarFile`, the calendar for the OS region is active, otherwise the
US one

**Independent Test**: On a fresh install, start with the region Czechia (Czech calendar) and with the United States or
Germany (United States calendar) (quickstart scenario 2)

### Tests for User Story 2

- [X] T015 [P] [US2] Add `LoadInCulture(culture, preferredCalendar, folders)` to `Anniversary.Tests/TestFiles.cs`: it sets `CultureInfo.CurrentCulture` for one `AnniversaryBook.Load` and restores it, "the way the app runs with the OS culture (spec 5.7)", so the tests pass on any machine
- [X] T016 [P] [US2] Add `CalendarForTheRegionIsUsedWhenNoneIsChosenOrConfigured` (`cs-CZ` and `en-CZ` → Czech, `sk-SK` → Slovak, `de-AT` → Austrian; `en-US`, `en-GB`, `de-DE`, the neutral `cs` and the invariant culture → US; no warnings) and `ChosenAndConfiguredCalendarsWinOverTheRegion` (in `cs-CZ`, a configured Slovak and a chosen Polish calendar win) to `Anniversary.Tests/CalendarTests.cs`; make `ExeFolderWinsOverAppDataWhichWinsOverEmbedded` choose the Czech calendar explicitly and `ConfiguredCalendarIsUsedWhenPreferredIsGone` write an `appsettings.json` naming Slovak (FR-011, FR-012, FR-013)
- [X] T017 [P] [US2] Update `FallsBackToBuiltInDefaults` (in `en-US`: `Config.CalendarFile` is `null`, the US calendar is active, and 4 July 2026 shows Independence Day) and `InvalidConfigUsesDefaultsAndWarns` (in `en-US`: Independence Day) in `Anniversary.Tests/LoadTests.cs` (FR-012, FR-013)

### Implementation for User Story 2

- [X] T018 [US2] Make `CalendarFile` in `Anniversary.Shared/Model/AppConfig.cs` (historical; removed by 012) a `string?` without a default ("or `null` to choose it by the OS region (spec 5.7)"), and remove `calendarFile` from `Anniversary.Shared/Defaults/appsettings.json` (historical; removed by 012) (FR-013)
- [X] T019 [US2] Choose the default in `Load` in `Anniversary.Shared/AnniversaryBook.cs`: the preferred calendar if it is among `Calendars`, else `Config.CalendarFile` if it isn't blank, else `CalendarForRegion(CultureInfo.CurrentCulture)`, which returns the first calendar (by display name) whose `locale` has the culture's region, or `FallbackCalendarFile` (`Anniversary.Calendar.US.json`); `RegionOf(cultureName)` gives `RegionInfo.TwoLetterISORegionName`, or `null` for a neutral or invariant culture or an unknown name; `Load`'s documentation says so (FR-011, FR-012, FR-013)
- [X] T020 [US2] Check scenario 2 of `specs/006-holiday-calendar-rules/quickstart.md` by hand (docs/spec.md §12, criterion 12)

**Checkpoint**: A fresh install starts with the calendar for its region, or the US one

---

## Phase 5: User Story 3 - See the Czech holidays that move with Easter (Priority: P3)

**Goal**: The Czech calendar shows Velký pátek and Velikonoční pondělí

**Independent Test**: Choose Czechia (Česko): 3 April 2026 shows "Richard, Velký pátek" and 6 April "Vendula,
Velikonoční pondělí" (quickstart scenario 3)

### Tests for User Story 3

- [X] T021 [P] [US3] Add `CzechCalendarHasTheEasterHolidays` to `Anniversary.Tests/CalendarRuleTests.cs`: no warnings, "Richard, Velký pátek" on 3 April 2026, "Vendula, Velikonoční pondělí" on 6 April 2026, and Find (calendar only) from 1 January 2027 lands on 29 March 2027 (FR-010)

### Implementation for User Story 3

- [X] T022 [US3] Add `easter-2` Velký pátek and `easter+1` Velikonoční pondělí (zákon č. 245/2000 Sb., § 2) to the Czech calendar in `calendars/Build-Calendars.ps1` and name the law in its `source`; the file is its own source, so running the script again changes nothing (FR-010)
- [X] T023 [US3] Regenerate `calendars/Anniversary.Calendar.Czech.json`: the 366 v4 entries unchanged, the two rules after `12-31` (ordinal order) and the new `source` (FR-010, FR-015)
- [X] T024 [US3] Check scenario 3 of `specs/006-holiday-calendar-rules/quickstart.md` by hand (docs/spec.md §12, criteria 3 and 12)

**Checkpoint**: The Czech calendar shows its Easter holidays

---

## Phase 6: User Story 4 - Write days that move in your own calendar file (Priority: P3)

**Goal**: Users can write rule keys in their own calendar files, and keys the app doesn't understand are reported

**Independent Test**: Choose a calendar file with rule keys and one misspelt key: one message names the key, and the
rule entries show on the right days (quickstart scenario 4)

### Tests for User Story 4

- [X] T025 [P] [US4] Add `UnknownKeysAreReportedOnceAndTheOtherEntriesWork` to `Anniversary.Tests/CalendarRuleTests.cs`: `13-01`, `11-Thx-4`, `easter+`, `11-Thu-5`, `easter+100` and `1-5` give the single warning "Unknown dates in Anniversary.Calendar.Test.json: 13-01, 11-Thx-4, easter+, 11-Thu-5, easter+100 and 1 more", and Independence Day and Thanksgiving still show (FR-006)

### Implementation for User Story 4

- [X] T026 [US4] Report unknown keys in `AddCalendarEntries` in `Anniversary.Shared/AnniversaryBook.cs`: keys that are neither `MM-dd` nor rules are collected and reported once per load as "Unknown dates in {fileName}: {keys}", at most `MaxReportedKeys` (5) of them, then " and {n} more"; the main window shows the warning after it opens or after the calendar is chosen, titled "Data Error" (FR-006)
- [X] T027 [US4] Check scenario 4 of `specs/006-holiday-calendar-rules/quickstart.md` by hand

**Checkpoint**: All four user stories work

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and the final checks

- [X] T028 [P] Update `docs/spec.md` (then `specs/spec.md`): 1 (choosing the calendar by the OS locale is no longer a non-goal), 5.1 (the `calendarFile` default, rule keys in calendar files, `locale` choosing the default, the new "Rule keys (floating dates)" table, 11 shipped calendars and the built-in `appsettings.json` without `calendarFile`), 5.2 (`CalendarData` keys), 5.4 (the "Unknown dates" warning), 5.7 (renamed "Calendars (choosing and adding)": the four steps and "Holiday calendars"), 6 (the `Load` rule and the rule entries in file order), 6.1 and 6.2 (calendar names from 6.3), the new 6.3 "Calendar names for a day (rule keys)", 7.10 (11 calendars; the menu keeps its name, user decision), 9 (`LoadTests`, `CalendarTests` and the new `CalendarRuleTests`), 11 (C19) and 12 (criteria 3 and 11 adjusted, new criterion 12)
- [X] T029 [P] Update `docs/plan.md` (then `specs/plan.md`): the Decisions row "Holiday calendars", work item 13 **calendar-rules** ✅, the next steps (International Days next; the macOS app bundle and the installers later, user decision), the section renamed "Holiday and observance calendars (in progress)" with calendar-rules ✅ and us-holidays ✅, and the further ideas (observed days off, Orthodox Easter, overlay calendars such as Czech name days with US holidays)
- [X] T030 [P] Update `docs/copilot/handover.md` (then `specs/copilot/handover.md`): 165 tests, the "Calendars" status row, the main window showing "the calendar's names or holidays", the decision bullet on calendars (the OS region default with US otherwise, the menu name, holiday entries written in the script with their sources, names joined with ", ", no observed days off or Orthodox Easter yet), `calendars/Build-Calendars.ps1` running on macOS, the next steps, and the lesson on clicking buttons and macOS menu items in the headless harness
- [X] T031 [P] Update `README.md`: "Your data" (11 calendars; `usersettings.json` holds "the chosen theme and calendar"; `Anniversary.Calendar.*.json` are "extra or replacement calendars"), "Moving from v4" (choose Czechia (Česko) once if the OS region isn't Czechia; a copied v4 `appsettings.json` already selects it), the section renamed "Calendars" (the default by region, the United States calendar, `calendarFile`, the rule keys with examples) and the projects table
- [X] T032 [P] Update `calendars/README.md`: the title "Calendars", the validation (`MM-dd` or rule keys, a full year for name days only, ordinal order), the US row ("16 dates + 14 rules") and the Czech row ("366 + 2 rules"), `locale` making a calendar the default for its region, the new section "The US holiday calendar" (federal holidays, observances designated by law, traditional dates, the OPM check, legal dates), and Greece's reason (Orthodox Easter)
- [X] T033 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: 165 tests pass, 85 of them new for this feature in `Anniversary.Tests/CalendarRuleTests.cs` and `Anniversary.Tests/CalendarTests.cs`
- [X] T034 Run `pwsh -File Build-Calendars.ps1` in `calendars/` on macOS (PowerShell 7.6 from Homebrew): it reproduces the committed calendar files exactly
- [X] T035 Check the data behind `Anniversary.Tests/CalendarRuleTests.cs` against its sources: the federal holidays against OPM's schedules for 2025–2027, the U.S. Code sections and the Czech law online, and the Easter dates against an independent implementation
- [X] T036 Run the real main window headless (Thanksgiving, the calendar menu and the switch to Czech) and start it with `LC_ALL=cs_CZ.UTF-8` (Czech, "Vendula, Velikonoční pondělí" on 6 April 2026); leave the region on Windows and Linux to the manual checks in `specs/006-holiday-calendar-rules/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–6)**: depend on Foundational
- **Polish (Phase 7)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after Foundational and User Story 1, whose US calendar is the fallback (T013)
- **User Story 3 (P3)**: after Foundational; independent of the other stories
- **User Story 4 (P3)**: after Foundational; `AddCalendarEntries` (T004) gets the warning

### Within Each User Story

- Tests first, then `AnniversaryBook` or the build script, then the generated calendar files, then the manual check
  from quickstart.md
- T016 and T017 load through `TestFiles.LoadInCulture` (T015)
- `calendars/Build-Calendars.ps1` and the calendar files it writes change together: the files are generated, never
  edited by hand

### Parallel Opportunities

- T002 next to T001
- T007, T008 and T009 next to T003–T006
- In each story, the test tasks (T010 and T011, T015–T017, T021, T025) next to the implementation tasks
- The stories share `Anniversary.Tests/CalendarRuleTests.cs`, and User Stories 1 and 3 also
  `calendars/Build-Calendars.ps1`, so they are best done one after another
- T028–T032 together: five different documents

---

## Parallel Example: User Story 1

```bash
# The tests and the calendar table can be written at the same time:
Task: "Add the US calendar tests to Anniversary.Tests/CalendarRuleTests.cs"
Task: "Update the calendar counts in Anniversary.Tests/CalendarTests.cs"
Task: "Add the US table to calendars/Build-Calendars.ps1"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (the US calendar)

### Incremental Delivery

1. Setup and Foundational: rule keys in any calendar file
2. User Story 1: the US calendar
3. User Story 2: the default by region
4. User Story 3: the Czech Easter holidays
5. User Story 4: the warning for unknown keys
6. Polish: documentation, the full test run, the script run and the checks against the sources

### What happened

On the morning of 2026-10-10 the maintainer asked whether US public holidays could be used although they aren't on
the same day every year; the assistant proposed rule keys, the maintainer agreed, and the idea was saved in the plan.
That afternoon, after the date-range fix, the maintainer put the installers off and asked for the US calendar. The
assistant built the rule keys, the US calendar and the Czech Easter holidays in one go (155 tests), with the US calendar
as the default. The maintainer asked what `11-Thu-4` and `05-Mon-last` mean and got the explanation (the keys stayed
as they were). Then the maintainer asked whether selecting the US calendar by default was a good idea, and the default
became the OS region's calendar with US as the fallback (165 tests). The maintainer committed everything, with the
date-range fix, as `5bb1afe`.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names are as they were at the time; 012 renamed `ConfiguredCalendarIsUsedWhenPreferredIsGone`,
  `CalendarForTheRegionIsUsedWhenNoneIsChosenOrConfigured` and `ChosenAndConfiguredCalendarsWinOverTheRegion` and
  replaced the `appsettings.json` tests, and 008 added `InternationalCalendarHasTheseDays` to `CalendarRuleTests`
- The documentation paths are today's; at the time they were under `specs/`
