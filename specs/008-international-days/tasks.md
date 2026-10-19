# Tasks: International Days

**Input**: Design documents from `specs/008-international-days/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic and data changes: the 8 new test cases
are in `Anniversary.Tests/CalendarRuleTests.cs`, and `Anniversary.Tests/CalendarTests.cs` checks the count and the
order. The menus are checked by hand with [quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. Everything went into one commit, `fb5312e` ("Adding international
calendar", 2026-10-10).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US3)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): logic and views in `Anniversary.Shared/`, tests in `Anniversary.Tests/`, calendars and
  their build script in `calendars/`, documentation in `README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and the UN data

- [X] T001 Start from the state 007 left: `Anniversary.slnx` building without warnings and 181 tests passing with `dotnet test --project Anniversary.Tests`
- [X] T002 [P] Read the UN list of international days (https://www.un.org/en/observances/list-days-weeks) from the page's markup, entry by entry (date, name, resolution), after a first quick parse paired some names with the wrong dates; measure it (239 days on 187 dates, up to 6 on one day, 33 names longer than 60 characters). The parsed data is the input for `calendars/Build-Calendars.ps1` and wasn't committed

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Let the calendar build script write a calendar that belongs to no region

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Change `Write-Calendar` in `calendars/Build-Calendars.ps1`: write the `locale` line only when a locale is given, widen its report's file-name column from 12 to 13 characters, and say in the script's description that "The US holiday and International Days calendars are written from the tables below, which give the source of every entry" (FR-006, FR-010)

**Checkpoint**: The script can write a holiday calendar without a `locale`; user stories can begin

---

## Phase 3: User Story 1 - See the international days of a day (Priority: P1) 🎯 MVP

**Goal**: International Days is a built-in calendar that shows the day's UN and fun days and is never the starting
calendar

**Independent Test**: Choose International Days and go to 10 October 2026, 7 August 2026 and 12 October 2026: the line
under the date shows "World Mental Health Day, World Migratory Bird Day", "International Beer Day" and nothing
(quickstart scenario 1)

### Tests for User Story 1

- [X] T004 [P] [US1] Add the theory `InternationalCalendarHasTheseDays` to `Anniversary.Tests/CalendarRuleTests.cs`: it loads the shipped calendar with `AnniversaryBook.Load("Anniversary.Calendar.International.json")`, expects no warnings, and checks `GetDay(...).CalendarNames` for 8 days: 14 March 2026 "Pi Day", 21 March 2026 "World Poetry Day, International Day of Forests", 9 May 2026 "World Migratory Bird Day", 7 August 2026 and 6 August 2027 "International Beer Day", 10 October 2026 "World Mental Health Day, World Migratory Bird Day", 24 October 2026 "United Nations Day" and 12 October 2026 "" (FR-002, FR-003, FR-004, FR-005)

### Implementation for User Story 1

- [X] T005 [US1] Agree on the content with the maintainer, as recorded in `specs/008-international-days/research.md`: about 50 well-known UN days plus the five fun days (his choice over all 239 days), then the proposed list of 56 UN days and 5 fun days, each date and resolution checked against the parsed UN list; International Day of Mathematics was dropped because the UN list doesn't include it; approved with "Looks good, go ahead" (FR-002, FR-003)
- [X] T006 [US1] Add the ordered table `$international` to `calendars/Build-Calendars.ps1`, generated from the parsed UN data so names, dates and resolutions are copied rather than retyped: 61 keys in month order, each with a comment giving its resolution (with the UN body where it isn't the General Assembly), "UN list" where the list gives none, or "tradition" for the fun days; World Migratory Bird Day as `05-Sat-2` and `10-Sat-2`, International Beer Day as `08-Fri-1`, World Poetry Day and International Day of Forests together on `03-21`; two UN names shortened (International Holocaust Remembrance Day, International Volunteer Day) (FR-002, FR-003, FR-004, FR-009)
- [X] T007 [US1] Write the calendar from `calendars/Build-Calendars.ps1` with `Write-Calendar 'International' 'International Days' '' … $international -Holidays`: no locale, the source "United Nations list of international days (https://www.un.org/en/observances/list-days-weeks, as of 10 October 2026), plus fun days kept by tradition" and the license "MIT, Copyright (c) David Pokluda (part of Anniversary; see LICENSE)", with a comment above the table naming the UN list and the date (FR-001, FR-006, FR-009)
- [X] T008 [US1] Run `calendars/Build-Calendars.ps1`: it writes the new `calendars/Anniversary.Calendar.International.json` (61 entries: 58 dates and 3 rules, no `locale`), and the other 11 calendar files come out unchanged; the existing `Anniversary.Calendar.*.json` wildcard in `Anniversary.Shared/Anniversary.Shared.csproj` embeds the new file with no project change (FR-001, FR-006, FR-010)
- [X] T009 [US1] Raise the discovery counts in `Anniversary.Tests/CalendarTests.cs`: `ShippedCalendarsAreDiscovered` expects 12 calendars and `UserCalendarIsAddedToShippedOnes` 13 (FR-001)
- [X] T010 [US1] Check scenario 1 of `specs/008-international-days/quickstart.md` by hand

**Checkpoint**: International Days can be chosen and shows its days

---

## Phase 4: User Story 2 - Find a calendar in the Calendar menu (Priority: P2)

**Goal**: The menu is called Calendar and lists the countries first, then a separator and the calendars without a
region

**Independent Test**: Open ⋯ ▸ Calendar: Austria (Österreich) … United States, a separator, International Days; the
same under View ▸ Calendar on macOS (quickstart scenario 2)

### Tests for User Story 2

- [X] T011 [P] [US2] Check the order in `Anniversary.Tests/CalendarTests.cs`: `ShippedCalendarsAreDiscovered` asserts that `Calendars` equals itself ordered by `string.IsNullOrEmpty(c.Locale)` and then by display name with `StringComparer.CurrentCulture`, and that the last calendar is "International Days" with no locale; `UserCalendarIsAddedToShippedOnes` asserts that the last two are "International Days" and "Test" (a user's file with a name but no locale) (FR-008)

### Implementation for User Story 2

- [X] T012 [US2] Sort the calendars in `AddActiveCalendarIfMissing` in `Anniversary.Shared/AnniversaryBook.cs`, which runs at the end of `RefreshCalendars()` and of each calendar load: `OrderBy(c => string.IsNullOrEmpty(c.Locale)).ThenBy(c => c.DisplayName, StringComparer.CurrentCulture)` instead of a sort by display name only; the `Calendars` summary now describes the two groups (FR-008)
- [X] T013 [P] [US2] Rename the menu in `Anniversary.Shared/Views/MainWindow.axaml`: the header of the ⋯ menu's `CalendarMenu` and of the macOS View menu's `NativeMenuItem` becomes "Calendar" (FR-007)
- [X] T014 [US2] In `Anniversary.Shared/Views/MainWindow.axaml.cs`, find the macOS submenu by its new header (`NativeItem(menuBar, "View", "Calendar")`), and in `UpdateCalendarMenus` add a `Separator` to the ⋯ submenu and a `NativeMenuItemSeparator` to the macOS submenu before a calendar without a locale that follows one with a locale, tracked with a plain `previousHasLocale` variable (a first version that stashed a flag in `Tag` was replaced as harder to read) (FR-007, FR-008)
- [X] T015 [US2] Run the real menus headless on the Mac (Austria … United States, the separator, International Days), then check scenario 2 of `specs/008-international-days/quickstart.md` by hand (docs/spec.md §12, criterion 11)

**Checkpoint**: International Days is easy to find, last, and the menu name fits every calendar

---

## Phase 5: User Story 3 - Check where each day comes from (Priority: P3)

**Goal**: The source of every day can be looked up and the calendar rebuilt exactly

**Independent Test**: Re-run the calendar build script: all 12 calendar files come out the same; a day looked up in
the script has the same date and resolution on the UN list (quickstart scenario 3)

### Implementation for User Story 3

- [X] T016 [US3] Document the calendar in `calendars/README.md`: a table row (`Anniversary.Calendar.International.json`, International Days, "58 dates + 3 rules", "Well-known days from the UN list of international days, plus five fun days", MIT (part of Anniversary)) and the section "The International Days calendar": 56 days chosen from the UN list (239 days in October 2026, too many for the small window), each with its UN resolution in the script or "UN list"; the UN's names, two of them shortened; every date checked against the UN list; the list's weeks and Vesak left out; the five fun days; no commercial "national day" lists, because they are copyrighted; no `locale`, so choose it in the menu (FR-011)
- [X] T017 [US3] Check scenario 3 of `specs/008-international-days/quickstart.md`: re-running `calendars/Build-Calendars.ps1` reproduces all 12 files, and the script gives the source of each day (FR-009, FR-010)

**Checkpoint**: All three user stories work

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and the final checks

- [X] T018 [P] Update `docs/spec.md` (then `specs/spec.md`): 5.1 (12 shipped calendars, International Days with no locale), 5.7 (the list's order, and "Holiday calendars (US and International Days)" with an International Days paragraph), 6 (`Calendars` is "discovered and sorted per 5.7"), 7.2 (the menu row "Calendar ▸ {calendars}" with the separator, and the macOS View menu), 7.10 (renamed "Calendar menu": the order, the separator, the note that the mock still shows *Nameday Calendar*, and that the user renamed the menu), 9 (`CalendarTests` and `CalendarRuleTests`), 11 (C15 "first called Nameday Calendar", new C21) and 12 (criterion 11 says **Calendar**)
- [X] T019 [P] Update `docs/plan.md` (then `specs/plan.md`): the Decisions rows "Name-day calendars" (⋯ ▸ Calendar ▸, first called Nameday Calendar) and "Holiday calendars" (renamed to **Calendar** by user decision, calendars without a locale last after a separator, International Days ships), work item 11 ("now **Calendar**"), the new work item 15 **international-days** ✅, "Holiday and observance calendars (done)" with item 3 ✅ (56 days chosen with the user; the full UN list of 239 days on 187 dates was too much for the small window; no commercial "national day" sites), and the maintainer's plan for what comes next (user README, packages, Start at Login, releases)
- [X] T020 [P] Update `docs/copilot/handover.md` (then `specs/copilot/handover.md`): 189 tests, the Calendars row (12 shipped, International Days with 56 UN days plus five fun days), the command list ("Calendar ▸ (countries, then International Days after a separator)"), the calendars paragraph (renamed from Nameday Calendar; calendars without a locale last), and the next steps in the maintainer's order
- [X] T021 [P] Update `README.md`: the macOS View menu (Calendar, Appearance), "12 calendars: the US holiday calendar, International Days and 10 name-day calendars", and in the calendars paragraph a sentence on International Days and **⋯ ▸ Calendar** (macOS **View ▸ Calendar**), "where International Days comes last"
- [X] T022 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: 189 tests pass, 8 of them new in `Anniversary.Tests/CalendarRuleTests.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–5)**: depend on Foundational; User Stories 2 and 3 also need the calendar file from User
  Story 1
- **Polish (Phase 6)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: the sort and the menu code need nothing from User Story 1, but its tests (T011) expect
  International Days to be the last calendar, so they need T008
- **User Story 3 (P3)**: documents and rebuilds the file from User Story 1 (T006–T008)

### Within Each User Story

- Tests first, then the script or `AnniversaryBook`, then the views, then the manual check from quickstart.md
- `MainWindow.axaml` and `MainWindow.axaml.cs` change together: the code finds the macOS submenu by the header the
  markup gives it

### Parallel Opportunities

- T002 next to T001
- T004 next to T005–T007; T011 and T013 next to T012
- T018–T021 together: four different documents

---

## Parallel Example: User Story 2

```bash
# The order tests and the menu markup can be written next to the sort:
Task: "Check the order in Anniversary.Tests/CalendarTests.cs"
Task: "Sort the calendars in AddActiveCalendarIfMissing in Anniversary.Shared/AnniversaryBook.cs"
Task: "Rename the menu to Calendar in Anniversary.Shared/Views/MainWindow.axaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (the sample days)

### Incremental Delivery

1. Setup and Foundational: the script can write a calendar without a region
2. User Story 1: the International Days calendar
3. User Story 2: the Calendar menu, its order and the separator
4. User Story 3: the calendars README and the rebuild check
5. Polish: documentation and the full test run

### What happened

On the evening of 2026-10-10 the maintainer asked what an international calendar day would be and for an example.
Asked where to start, he chose the International Days calendar before the packaging work; he then chose a selection of
about 50 well-known UN days over all 239 and approved the proposed list. The assistant built the calendar, the tests
and the documentation (with the menu still called Nameday Calendar), reporting 189 passing tests. The maintainer then
proposed the rename and asked whether International Days should come first or last; he chose both changes, with
International Days last after a separator, and committed everything as `fb5312e`.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names are as they were at the time; none of the tests named here changed later
- The documentation paths are today's; at the time they were under `specs/`
