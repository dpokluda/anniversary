# Feature Specification: International Days

**Feature Branch**: `008-international-days`

**Created**: 2026-10-10

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "Another idea is that I'm hearing in radio today is the international beer day and stupid
things like that. Could something like that be used for countries without namedays? … international calendar day -
what would it be? Can you show me an example? … I am now considering to rename "Nameday calendar" to just "Calendar",
what do you think? Also should we put the international file either first or last (after separator)?"

## Clarifications

### Session 2026-10-10

- Q: Could days like International Beer Day be shown for countries without name days, and where would they come from?
  → A: Yes, as an "International Days" calendar built on the UN's official list of international days plus a few
  well-known fun days, compiled for the app. The popular "national day" sites are commercial and copyrighted, so they
  aren't used. The maintainer had this saved as a plan that morning, as the step after the rule keys and the US
  calendar ([006](../006-holiday-calendar-rules/spec.md)).
- Q: What would such a calendar show? (the maintainer asked for an example) → A: It is one more calendar in the menu,
  not tied to a country, so the OS region never picks it. Only one calendar shows at a time, so it is chosen instead of
  a country's calendar. On Saturday 10 October 2026 the line under the date would read "World Mental Health Day, World
  Migratory Bird Day".
- Q: Should it hold the whole UN list or a selection? → A: About 50 well-known UN days plus the fun days, with the list
  shown first; the maintainer chose this over all 239 days. The full list has 239 days on 187 dates, up to 6 on one day
  and 33 names longer than 60 characters: too much text for the small window.
- Q: Is the proposed list right? → A: Yes ("Looks good, go ahead"): 56 UN days under the UN's names, two of them
  shortened, plus Pi Day, Star Wars Day, World Emoji Day, Talk Like a Pirate Day and International Beer Day (the first
  Friday of August). International Day of Mathematics (14 March) was dropped because the UN list doesn't include it;
  Pi Day is on that date.
- Q: Should the menu **Nameday Calendar** be renamed **Calendar**? → A: Yes. Two of the twelve calendars (US and
  International Days) don't list name days, and "Calendar ▸" matches "Theme ▸" next to it. The main window's
  **Namedays** box keeps its name, because it shows the people's name days. (Until then the name had been kept on
  purpose, although the US calendar lists holidays.)
- Q: Should International Days come first or last in the menu? → A: Last, after a separator: most people look for their
  country, and the starting calendar is picked by country, so the countries come first, sorted by name. The groups
  follow whether a calendar has a region (locale) rather than singling out one file, so a calendar file a user adds
  without one also goes after the separator; the macOS View menu gets the same separator. The maintainer chose both
  changes ("Yes, both").

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See the international days of a day (Priority: P1)

Someone whose country has no name days chooses **⋯ ▸ Calendar ▸ International Days** (macOS: **View ▸ Calendar ▸
International Days**). The line under the date then shows the day's well-known international days, such as World Water
Day or the International Day of Peace, and a few fun ones, such as Pi Day or International Beer Day. Like any
calendar's names, they also appear in Next N days and are found by Find…, and the choice is remembered.

**Why this priority**: It is what the maintainer asked for: something to show where there are no name days. The menu
changes in User Story 2 only make sense once this calendar exists.

**Independent Test**: Choose International Days and change the date to 10 October 2026, 7 August 2026 and
12 October 2026: the line under the date shows "World Mental Health Day, World Migratory Bird Day", then "International
Beer Day", then nothing.

**Acceptance Scenarios**:

1. **Given** the app with its shipped calendars, **When** the user chooses ⋯ ▸ Calendar ▸ International Days, **Then**
   the main window shows the day's international days at once, and International Days is still the calendar after a
   restart.
2. **Given** International Days, **When** the day is 14 March 2026 or 24 October 2026, **Then** the line under the date
   reads "Pi Day" or "United Nations Day".
3. **Given** International Days, **When** the day is 21 March 2026, **Then** it reads "World Poetry Day, International
   Day of Forests" (two days on one date).
4. **Given** International Days, **When** the day is 7 August 2026 or 6 August 2027 (the first Friday of August),
   **Then** it reads "International Beer Day".
5. **Given** International Days, **When** the day is 9 May 2026 (the second Saturday of May), **Then** it reads "World
   Migratory Bird Day"; on 10 October 2026 (also the second Saturday of October) it reads "World Mental Health Day,
   World Migratory Bird Day".
6. **Given** International Days, **When** the day is 12 October 2026, **Then** the line under the date is empty.
7. **Given** a fresh install in any OS region, **When** the app starts for the first time, **Then** it starts with the
   calendar for that region or with the US calendar, never with International Days.

---

### User Story 2 - Find a calendar in the Calendar menu (Priority: P2)

The menu that lists the calendars is now called **Calendar** instead of **Nameday Calendar**, because the US and
International Days calendars don't list name days. It shows the countries first, in alphabetical order, then a
separator, then the calendars that don't belong to a country: International Days, and any calendar a user adds without
a region. The macOS **View** menu shows the same.

**Why this priority**: With twelve calendars, two of them without name days, the old name was misleading, and sorted by
name alone International Days would sit among the countries. Most people look for their own country.

**Independent Test**: Open ⋯ ▸ Calendar: Austria (Österreich) … United States, a separator, then International Days;
on macOS, View ▸ Calendar shows the same list.

**Acceptance Scenarios**:

1. **Given** the app, **When** the user opens the ⋯ menu (or right-clicks the window), **Then** it has **Calendar ▸**
   where it had **Nameday Calendar ▸**.
2. **Given** macOS, **When** the user opens the **View** menu, **Then** it has **Calendar ▸** and **Appearance ▸**.
3. **Given** the 12 shipped calendars, **When** the user opens Calendar ▸, **Then** the 11 country calendars come first
   in alphabetical order (Austria (Österreich) … United States), then a separator, then International Days, with the
   active calendar checked.
4. **Given** a calendar file with a name but no region in the app-data folder (for example "Test"), **When** the menu
   opens, **Then** it is listed after the separator, sorted by name with International Days ("International Days",
   then "Test").

---

### User Story 3 - Check where each day comes from (Priority: P3)

The maintainer, or anyone reviewing the calendar, can see where every day comes from and rebuild the calendar file
exactly: the calendar build script lists each UN day with its UN resolution (or "UN list" where the list gives none)
and marks the fun days as tradition, the file names its source and license, and the calendars README explains how the
days were chosen and what was left out.

**Why this priority**: The calendar's entries are what users read first, and the app's calendars come only from
sources that can be checked and used openly. It matters for keeping the calendar right more than for daily use.

**Independent Test**: Re-run the calendar build script: all 12 calendar files come out byte for byte the same. Pick a
day in the script and find the same date and resolution on the UN list.

**Acceptance Scenarios**:

1. **Given** the repository, **When** the maintainer re-runs the calendar build script, **Then**
   `Anniversary.Calendar.International.json` and the other 11 calendar files are reproduced exactly.
2. **Given** the script, **When** the maintainer looks up a UN day, **Then** its UN resolution, or "UN list", is next
   to it; the fun days say "tradition".
3. **Given** the calendar file, **When** someone opens it, **Then** it names its source (the UN list as of 10 October
   2026, plus fun days kept by tradition) and its license, and it has no region.
4. **Given** the calendars README, **When** someone reads it, **Then** it lists International Days with its size,
   source and license, and explains how the 56 days were chosen from the UN list and what was left out.

### Edge Cases

- **Two days on one date** (21 March): both are shown, joined with ", ".
- **A fixed day and a moving day on the same date** (10 October 2026): the fixed one comes first: "World Mental Health
  Day, World Migratory Bird Day".
- **A day twice a year**: World Migratory Bird Day falls on the second Saturday of May and again of October.
- **A day that moves**: International Beer Day is on 7 August 2026 and 6 August 2027.
- **A date without an international day** (most of the year, for example 12 October 2026): no calendar text.
- **A user's calendar without a region**, including a v4-style file without any metadata: listed after the separator
  with International Days, sorted by name.
- **First start in any region**: International Days is never the starting calendar; it is chosen in the menu.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST ship an **International Days** calendar (`Anniversary.Calendar.International.json`) as a
  built-in calendar, so that 12 calendars are offered: the US holiday calendar, International Days and the 10 name-day
  calendars.
- **FR-002**: International Days MUST list 56 well-known days chosen from the United Nations list of international
  days, under the UN's names, each on its date as checked against that list (as of 10 October 2026); two long names
  MAY be shortened (International Holocaust Remembrance Day, International Volunteer Day).
- **FR-003**: International Days MUST also list five fun days kept by tradition: Pi Day (14 March), Star Wars Day
  (4 May), World Emoji Day (17 July), Talk Like a Pirate Day (19 September) and International Beer Day (the first
  Friday of August).
- **FR-004**: Days that move from year to year MUST fall on their day in every year: International Beer Day on the first
  Friday of August, and World Migratory Bird Day on the second Saturday of May and on the second Saturday of October.
- **FR-005**: A date with several international days MUST show all of them, joined with ", "; a date without one MUST
  show no calendar text.
- **FR-006**: International Days MUST NOT belong to a country or region, so the app MUST never pick it as the starting
  calendar for an OS region; users choose it in the Calendar menu, and the choice is remembered like any other.
- **FR-007**: The menu **Nameday Calendar** MUST be renamed **Calendar**, in the ⋯ (and right-click) menu and in the
  macOS **View** menu.
- **FR-008**: The Calendar menu MUST list the calendars that belong to a country or region first, sorted by display
  name, then a separator, then the calendars that don't (International Days and any calendar file without a region),
  also sorted by display name. The separator MUST appear only between these two groups, and the macOS View ▸ Calendar
  menu MUST show the same order and separator.
- **FR-009**: Every UN day MUST have its source recorded (its UN resolution, or "UN list" where the list gives none),
  and every fun day MUST be marked as kept by tradition. The calendar file MUST name its source (the UN list, with the
  date it was checked, plus fun days kept by tradition) and its license.
- **FR-010**: The calendar MUST be generated by the calendar build script together with the other shipped calendars and
  MUST NOT be edited by hand; re-running the script MUST reproduce every shipped calendar file exactly.
- **FR-011**: The calendars' documentation MUST describe International Days: its size, source and license, how the days
  were chosen and checked, and what was left out (the UN list's weeks, and Vesak, a full-moon day). Commercial "national
  day" lists MUST NOT be used, because they are copyrighted.

### Key Entities

- **International Days calendar**: a built-in calendar file, `Anniversary.Calendar.International.json`, shown as
  "International Days", with no region. It holds 61 entries, 58 on fixed dates and 3 that move, for the 56 UN days and
  the 5 fun days, and records its source and license.
- **Calendar region**: the optional region (locale) a calendar belongs to. It picks the starting calendar for an OS
  region ([006](../006-holiday-calendar-rules/spec.md)) and, with this feature, also decides whether a calendar is
  listed with the countries or after the separator.
- **Calendar menu**: the list of available calendars, countries first, then the others.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With International Days chosen, the eight sample days show exactly: 14 March 2026 "Pi Day"; 21 March 2026
  "World Poetry Day, International Day of Forests"; 9 May 2026 "World Migratory Bird Day"; 7 August 2026 and 6 August
  2027 "International Beer Day"; 10 October 2026 "World Mental Health Day, World Migratory Bird Day"; 24 October 2026
  "United Nations Day"; 12 October 2026 nothing; and loading the calendar reports no problem.
- **SC-002**: The calendar holds exactly 61 days: the 56 UN days, each checked against the UN list as of 10 October
  2026, and the 5 fun days.
- **SC-003**: On a fresh install the starting calendar is never International Days, in any region, and the menu offers
  12 calendars.
- **SC-004**: In the ⋯ menu and in the macOS View menu, the Calendar menu lists the 11 country calendars
  alphabetically, then one separator, then International Days; neither menu says "Nameday Calendar" any more.
- **SC-005**: Re-running the calendar build script reproduces all 12 shipped calendar files exactly; adding
  International Days left the other 11 unchanged.
- **SC-006**: 8 new automated test cases cover the sample days, the calendar discovery tests check the order, and all
  189 tests pass with a build free of warnings.

## Assumptions

- Builds on [003](../003-calendar-chooser/spec.md) (calendars found as files, chosen in a menu and remembered, with an
  optional name and region) and [006](../006-holiday-calendar-rules/spec.md) (days that move from year to year,
  holiday calendars written with a source for every entry, the starting calendar picked by OS region).
- Only one calendar shows at a time, so International Days is chosen instead of a country's calendar. Showing a holiday
  calendar together with a name-day calendar ("overlay calendars") was an idea that wasn't agreed.
- The selection reflects the UN list as of 10 October 2026.
- The [mock of the calendar menu](../../docs/screens/main-window-calendar-menu.light.preview.png) still shows "Nameday
  Calendar"; the system spec notes this instead of redrawing it.
- The user guide shipped with every download came later ([009](../009-user-guide/spec.md)); it describes the Calendar
  menu and International Days.
