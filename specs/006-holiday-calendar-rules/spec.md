# Feature Specification: Holiday Calendar Rules

**Feature Branch**: `006-holiday-calendar-rules`

**Created**: 2026-10-10

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "Let's address installers later on. I would like to address US (generic/default)
calendar. For that we would need to support those calculated fields. Can we discuss that?" … "We discussed that for the
US calendar we would need something where we would calculate days (first monday after easter), etc. Do you remember?"

**Later changes**: [008](../008-international-days/spec.md) renamed the menu from **Nameday Calendar** to **Calendar**
(the maintainer's proposal, reversing the decision here to keep the name), put calendars without a locale last, after
a separator, and added International Days, which has no locale and so is never the default.
[012](../012-single-settings-file/spec.md) stopped reading v4's `appsettings.json` and removed the built-in one, so
the `calendarFile` step no longer exists: since then the calendar is the menu choice, then the OS region, then the
United States. Not a later change, but in the same commit: the separate fix for the crash at the ends of the date
range, which 005 had recorded in the handover as a known issue (see Assumptions).

## Clarifications

### Session 2026-10-10

- Q: For the US calendar, could public holidays be used, given that they aren't on the same day every year? → A: Not
  with fixed dates alone: a calendar file keyed every entry by an `MM-dd` date, so Martin Luther King Jr. Day,
  Presidents' Day, Memorial Day, Labor Day, Columbus Day, Thanksgiving and the days tied to Easter couldn't be stored.
  Calendar files get rule keys next to `MM-dd`, such as `11-Thu-4`, `05-Mon-last` or `easter+1`, whose dates the app
  works out for the year shown: first the rule keys, then a US calendar on top of them. The maintainer agreed ("I think
  it is a good plan") and the idea was saved in the plan that morning.
- Q: What do `11-Thu-4` and `05-Mon-last` mean, and where are they worked out? → A: A key reads month, weekday, which
  one: `11-Thu-4` is the fourth Thursday of November (Thanksgiving, 26 November 2026) and `05-Mon-last` the last Monday
  of May (Memorial Day, 25 May 2026). The n-th weekday counts from the 1st of the month; `last` counts back from the
  month's last day and is used instead of a fifth weekday, which not every month has. The app works the dates out each
  time a day is shown or searched; they aren't stored. A more readable form such as `11-4th-Thu` was offered; the keys
  stayed as they were.
- Q: With no calendar chosen or configured, the app now selects the US one; is that a good idea? → A: Only as the
  fallback. The default follows the OS region: the calendar whose locale has that region (only the region counts, so
  Czechia gives Czech with a Czech or an English language setting, and Slovakia gives Slovak), otherwise the US calendar
  (for example in the United States, the United Kingdom, Germany, or with no region). A calendar chosen in the menu, and
  then `calendarFile` in the user's own `appsettings.json` (a copied v4 file names Czech), still win; the built-in
  configuration no longer names a calendar. On Windows the region comes from the Regional format setting, not from
  "Country or region".
- Q: How are the US holiday dates checked, and what about a holiday that falls on a weekend? → A: The federal holidays
  of 2025–2027 are checked against OPM's federal holiday schedules. Holidays are shown on their legal date; the weekday
  off that OPM lists when one falls on a weekend (for example Friday 3 July 2026) isn't shown.
- Q: Should the **Nameday Calendar** menu get a new name now that it also offers a holiday calendar? → A: Not now: it
  keeps its name (user decision). [008](../008-international-days/spec.md) later renamed it **Calendar**.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See the US holidays, also those that move (Priority: P1)

Someone in the United States, a country without name days, uses the **United States** calendar from **⋯ ▸ Nameday
Calendar** (macOS: **View ▸ Nameday Calendar**). The "Today is" line shows the day's federal holiday or observance,
including those whose date changes every year, such as Thanksgiving (the fourth Thursday of November), Memorial Day
(the last Monday of May) and Easter. Next N days lists the coming ones, and Find finds them.

**Why this priority**: It is what the maintainer asked for. Countries without name days had no calendar of their own,
and most US federal holidays move from year to year, which the calendar files couldn't express.

**Independent Test**: Choose the United States calendar and go to 26 November 2026, 25 May 2026 and 5 April 2026: the
"Today is" line shows Thanksgiving, Memorial Day and Easter.

**Acceptance Scenarios**:

1. **Given** the United States calendar, **When** the user goes to 26 November 2026, **Then** the "Today is" line shows
   *Thanksgiving*, and on 27 November *Black Friday*.
2. **Given** the United States calendar, **When** the user goes to 5 April 2026, **Then** it shows *Easter*; 3 April
   shows *Good Friday* and 17 February *Mardi Gras*.
3. **Given** the United States calendar and 20 November 2026, **When** the main window shows Next 7 days, **Then** the
   names line lists Thanksgiving.
4. **Given** the United States calendar, **When** the user goes to a federal holiday of 2025, 2026 or 2027, **Then** it
   is on the date in OPM's schedules, for example Martin Luther King Jr. Day on 19 January 2026 and Memorial Day on
   25 May 2026 and 31 May 2027.
5. **Given** 4 July 2026, a Saturday, **When** the user looks at 3 and 4 July 2026, **Then** *Independence Day* is
   shown on 4 July and nothing on Friday 3 July, the observed day off.
6. **Given** the United States calendar and 27 November 2026, **When** the user searches for "Thanksgiving" from the
   current date, **Then** the app goes to 25 November 2027.

---

### User Story 2 - Start with the calendar for your region (Priority: P2)

On a fresh install, the app starts with the calendar that fits the computer's region: Czech in Czechia, Slovak in
Slovakia, Austrian in Austria, and so on. Where no shipped calendar fits, for example in the United States, the United
Kingdom or Germany, it starts with the United States calendar. A calendar chosen in the menu, or named as
`calendarFile` in the user's own `appsettings.json`, still wins.

**Why this priority**: Every new user who hasn't chosen a calendar gets the default. Until then it was always Czech
(as in v4), and the first version of this feature made it the US one, which the maintainer questioned.

**Independent Test**: On a fresh install, start the app with the OS region set to Czechia: the Czech calendar is
checked in the menu. With the region set to the United States or Germany: the United States calendar.

**Acceptance Scenarios**:

1. **Given** a fresh install and the OS region Czechia, with Czech or English as the language, **When** the app starts,
   **Then** the Czech calendar is active, and 6 April 2026 shows "Vendula, Velikonoční pondělí".
2. **Given** a fresh install and the OS region Slovakia or Austria, **When** the app starts, **Then** the Slovak or the
   Austrian calendar is active.
3. **Given** a fresh install and the OS region United States, United Kingdom or Germany, or a setting without a region,
   **When** the app starts, **Then** the United States calendar is active.
4. **Given** a calendar chosen in the menu, **When** the app starts with any OS region, **Then** the chosen calendar is
   used.
5. **Given** no calendar chosen in the menu and an `appsettings.json` whose `calendarFile` names a calendar (for
   example v4's file copied over, which names Czech), **When** the app starts, **Then** that calendar is used, whatever
   the OS region.
6. **Given** the chosen calendar's file was removed, **When** the app starts, **Then** it uses the default as above,
   without errors.

---

### User Story 3 - See the Czech holidays that move with Easter (Priority: P3)

With the Czech calendar, Velký pátek (Good Friday) and Velikonoční pondělí (Easter Monday), the two Czech public
holidays whose dates follow Easter, appear after the day's names, for example "Vendula, Velikonoční pondělí" on 6 April
2026.

**Why this priority**: The v4 calendar already shows several public holidays on their fixed dates, such as Svátek práce
on 1 May, but had no way to hold the two that move with Easter. The same rule keys fill that gap.

**Independent Test**: Choose Czechia (Česko) and go to 3 and 6 April 2026.

**Acceptance Scenarios**:

1. **Given** the Czech calendar, **When** the user goes to 3 April 2026, **Then** the "Today is" line shows "Richard,
   Velký pátek".
2. **Given** the Czech calendar, **When** the user goes to 6 April 2026, **Then** it shows "Vendula, Velikonoční
   pondělí".
3. **Given** the Czech calendar and a day in 2027, **When** the user searches the calendar for "Velikonoční pondělí",
   **Then** the app goes to 29 March 2027.
4. **Given** the Czech calendar, **When** any other day is shown, **Then** the names are the same as in v4.

---

### User Story 4 - Write days that move in your own calendar file (Priority: P3)

Someone who adds their own `Anniversary.Calendar.{Name}.json` (in the app-data folder or next to the executable) can
list, next to `MM-dd` dates, days that move from year to year: the n-th weekday of a month (`11-Thu-4`), the last
weekday of a month (`05-Mon-last`) and Easter (`easter`), each optionally moved by up to 99 days (`easter+1`,
`11-Thu-4+1`). Keys the app doesn't understand are reported in one message, and the rest of the file still works.

**Why this priority**: The shipped calendars use the same keys. Documenting them lets users write calendars for their
own holidays, and the message points out typos: until then, an entry with such a key was silently never shown.

**Independent Test**: Put a calendar file with rule keys and one misspelt key into the app-data folder and choose it
in the menu: one message names the misspelt key, and the rule entries show on the right days.

**Acceptance Scenarios**:

1. **Given** a calendar with `11-Thu-4`, **When** the user goes to 26 November 2026, **Then** the entry is shown, and
   not on the Thursdays a week before or after.
2. **Given** a calendar with `05-Mon-last` and `02-Tue-last`, **When** the user goes to 31 May 2027 (a May with five
   Mondays) and 29 February 2028, **Then** the entries are shown there.
3. **Given** a calendar with `12-Mon-last+7` or `01-Mon-1-7`, **When** the user goes to 4 January 2027 or 29 December
   2025, **Then** the entry is shown: an offset can move a date into the next or the previous year.
4. **Given** a calendar with `01-Mon-1` and 30 December 2026, **When** the main window shows Next 7 days, **Then** the
   entry is listed (the first Monday of 2027 is 4 January).
5. **Given** keys in any letter case, such as `11-THU-4`, `05-mon-LAST` or `Easter`, **When** the calendar loads,
   **Then** they work and nothing is reported.
6. **Given** a calendar with the keys `13-01`, `11-Thx-4`, `easter+`, `11-Thu-5`, `easter+100` and `1-5` among valid
   ones, **When** it loads, at start-up or when chosen in the menu, **Then** one message (title "Data Error") says
   "Unknown dates in {file}: 13-01, 11-Thx-4, easter+, 11-Thu-5, easter+100 and 1 more", and the valid entries still
   show.
7. **Given** a day with an `MM-dd` entry and two rule entries, **When** it is shown, **Then** the `MM-dd` entry comes
   first, then the rule entries in the order of the file, separated by ", ".

### Edge Cases

- **A fifth weekday**: `11-Thu-5` isn't accepted, because not every month has five Thursdays; `last` always exists.
- **New Year**: an offset can move a date into the previous or next year (`12-Mon-last+7`, `01-Mon-1-7`); Next N days
  across New Year uses the next year's dates; Find, which searches to 31 December of the following year, finds a rule
  date in that year.
- **Several entries on one day**: the `MM-dd` entry first, then the rule entries in file order, joined with ", ".
- **Ends of the date range**: a rule date before 1 January 0001 or after 31 December 9999 (for example `12-Sun-last+7`
  in 9999) is skipped, and nothing fails.
- **Holidays on a weekend**: shown on their legal date only; observed days off aren't listed.
- **No region**: a language setting without a country, or none at all, gives the United States calendar.
- **Two calendars for one region**: the first by display name is used.
- **Windows Regional format**: the region comes from the Regional format, not from "Country or region", so a Czech user
  with an English (United States) format gets the United States calendar and chooses Czech once.
- **Moving from v4**: a copied v4 `appsettings.json` names Czech, so the Czech calendar stays whatever the region.
- **Orthodox Easter**: not supported; `easter` is Western Easter.
- **Older calendar files**: files with only `MM-dd` keys, including v4's, load and show the same names as before.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Besides `MM-dd` dates, a calendar file MUST accept keys for days that move from year to year:
  `MM-Ddd-n`, the n-th (1–4) weekday `Ddd` (`Mon`, `Tue`, `Wed`, `Thu`, `Fri`, `Sat` or `Sun`) of month `MM`, for
  example `11-Thu-4`; `MM-Ddd-last`, the last such weekday of the month, for example `05-Mon-last`; and `easter`, Easter
  Sunday (Western, Gregorian calendar).
- **FR-002**: Each of these keys MUST accept an offset of `+n` or `-n` days, n from 1 to 99, for example `easter+1` or
  `11-Thu-4+1`; an offset MAY move the date into the previous or the next year.
- **FR-003**: These keys MUST be case-insensitive.
- **FR-004**: The app MUST work out each such entry's date for the years it shows, so that the "Today is" line, the
  names in Next N days (also across New Year) and Find (also in the following year, and when searching the calendar
  only) include it.
- **FR-005**: When several calendar entries fall on one day, the app MUST show the `MM-dd` entry first, then each rule
  entry that falls on the day in the order of the file, separated by ", ".
- **FR-006**: A key that is neither a valid `MM-dd` date nor a valid rule MUST be ignored and reported, once each time
  the calendar loads, in one warning: "Unknown dates in {file}: {key}, {key}", listing at most five keys, followed by
  "and {n} more" when there are more. The calendar's other entries MUST still work.
- **FR-007**: A rule whose date in a year would fall outside the dates the app can show (before 1 January 0001 or after
  31 December 9999) MUST be skipped for that year, without an error.
- **FR-008**: The app MUST ship a **United States** calendar (locale `en-US`) with the 11 federal holidays of
  5 U.S.C. 6103(a) and 19 well-known observances, 30 entries in all, with its sources and license recorded in the file.
- **FR-009**: Each US entry MUST come from the law or from long tradition, with the source of each entry documented
  with the calendar. Holidays MUST be listed on their legal date; the weekday off observed when one falls on a weekend
  isn't listed.
- **FR-010**: The Czech calendar MUST show Velký pátek on Good Friday and Velikonoční pondělí on Easter Monday, the
  Czech public holidays that move with Easter (Act No. 245/2000 Coll., § 2), and keep all its v4 entries unchanged.
- **FR-011**: When no calendar was chosen in the menu (or the chosen one is no longer available) and the user's
  `appsettings.json` names no `calendarFile`, the app MUST use the calendar whose locale has the OS region. Only the
  region counts, so Czechia gives Czech with any language. If several calendars have the region, the first by display
  name MUST be used.
- **FR-012**: When no calendar has the OS region, or the OS setting has no region, the app MUST use the United States
  calendar.
- **FR-013**: A calendar chosen in the menu MUST still come first and `calendarFile` from the user's own
  `appsettings.json` second, so a copied v4 file keeps Czech. The built-in configuration MUST NOT name a calendar any
  more, so a fresh install follows the OS region.
- **FR-014**: The **Nameday Calendar** menu (⋯ and right-click; on macOS also **View ▸ Nameday Calendar**) MUST list
  the United States calendar among the shipped calendars, sorted by display name, and keep its name.
- **FR-015**: Calendar files without rule keys, including v4's, MUST load and show the same names as before. People's
  namedays, birthdays and anniversaries keep their date formats.

### Key Entities

- **Calendar file**: a name-day or holiday calendar (`Anniversary.Calendar.{Name}.json`) with a display name, a locale,
  its source and license, and entries keyed by an `MM-dd` date or by a rule.
- **Rule entry**: a day that moves from year to year: a month, a weekday and which one (first to fourth, or last), or
  Easter, plus an optional shift of up to 99 days. Its date is worked out for each year shown.
- **Locale region**: the country part of a calendar's locale (`CZ` in `cs-CZ`), compared with the OS region to choose
  the default calendar.
- **United States calendar**: 30 entries: 11 federal holidays, 6 observances designated by law (Mother's Day,
  Father's Day, Flag Day, Patriot Day and the start and end of daylight saving time) and 13 traditional dates.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With the United States calendar, all 33 federal holiday dates of 2025–2027 match OPM's published
  schedules (on their legal dates), and the floating observances fall on their 2026 dates, for example Mardi Gras on
  17 February, Mother's Day on 10 May and Black Friday on 27 November.
- **SC-002**: Easter is right on 12 known dates from the year 1 to 9999 and is a Sunday between 22 March and 25 April
  in every year from 1 to 9999.
- **SC-003**: Acceptance criterion 12 in [docs/spec.md](../../docs/spec.md) §12: on a fresh install with the OS region
  set to the United States (or to a region without its own calendar), the US calendar is active: Thanksgiving on
  26 November 2026, Black Friday on 27 November, Easter on 5 April 2026, and Next 7 days on 20 November 2026 lists
  Thanksgiving. With the region set to Czechia, or with Czech chosen, 6 April 2026 shows "Vendula, Velikonoční
  pondělí".
- **SC-004**: On a fresh install the active calendar fits the OS region for every region tested (Czechia with Czech or
  English, Slovakia, Austria) and is the United States calendar for the United States, the United Kingdom, Germany and
  settings without a region; a calendar chosen in the menu or named in `appsettings.json` wins over the region.
- **SC-005**: A calendar file with mistakes never stops the app: its valid entries still show and its unknown keys are
  listed in a single message; no day from 1 January 0001 to 31 December 9999 fails because of a rule.
- **SC-006**: Running the calendar build script again reproduces the committed calendar files exactly, whichever OS
  runs it (checked on macOS at the time).
- **SC-007**: 85 new automated test cases cover the rule keys, the shipped holiday entries and the default calendar,
  and all 165 tests pass with a build free of warnings.

## Assumptions

- Builds on [003](../003-calendar-chooser/spec.md) (calendar files found next to the executable, in the app-data
  folder and built in; the Nameday Calendar menu and the remembered choice; the `locale`, `source` and `license`
  metadata, `locale` then reserved for choosing a calendar by the OS later; the calendar build script) and on
  [001](../001-cross-platform-rewrite/spec.md) (the "Today is" line, Next N days, Find, and load warnings shown after
  the main window opens).
- Holidays are defined by law or by long tradition, so there is no list to download; each US entry is written with its
  source. Name days still come only from pinned, openly licensed sources (003).
- Rule keys are for calendar files only; a person's nameday stays an `MM-dd` date.
- The OS region is the one the OS gives the app: the Regional format on Windows, Language & Region on macOS, and
  `LANG`/`LC_ALL` on Linux.
- Not part of this feature, and recorded as further ideas: observed days off, Orthodox Easter, and showing a holiday
  calendar together with a name-day calendar. The International Days calendar from the same plan came next
  ([008](../008-international-days/spec.md)). The macOS app bundle, planned next until then, and the installers were
  put off by the maintainer ("Let's address installers later on").
- The same commit also carried a separate bug fix, made just before in the same session for a known issue that 005
  had recorded in the handover: the app no longer crashes at the ends of the date range. Next N days and Find stop at
  31 December 9999, Yesterday does nothing on 1 January 0001 and Tomorrow on 31 December 9999, and Find Next on
  31 December 9999 reports "not found". It isn't part of this feature.
