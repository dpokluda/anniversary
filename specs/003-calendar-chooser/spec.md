# Feature Specification: Calendar Chooser

**Feature Branch**: `003-calendar-chooser`

**Created**: 2026-10-09

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "Also, should we include something like choose calendar (I am not sure if that's the right
name) to show available nameday (calendars). It would show available ones (we would have them as separate json files).
We obviously already have Czech one, but if we can generate similar one for other (at least european countries - do many
countries have a concept of a nameday) that would be nice." Follow-up: "Are you saying that we can't find a web page
that contains different namedays per country? Please look harder!"

**Later changes**: [006](../006-holiday-calendar-rules/spec.md) added rule keys for floating dates and a US holiday
calendar, and made the default follow the OS region, with US where no calendar matches (the built-in `appsettings.json`
no longer names Czech); [008](../008-international-days/spec.md) renamed the menu **Calendar** and replaced the order by
display name alone: the calendars with a locale first, then a separator and those without (International Days);
[012](../012-single-settings-file/spec.md) removed v4's `appsettings.json`, and with it `calendarFile`.

## Clarifications

### Session 2026-10-09

- Q: What should the command look like, and where do the calendars come from? → A: A **Nameday Calendar** submenu in the
  ⋯ and right-click menu (macOS: **View ▸ Nameday Calendar**) lists every calendar the app finds as radio items, with
  the active one checked. Each calendar is a separate `Anniversary.Calendar.*.json` file, as the maintainer suggested,
  found next to the executable, in the app-data folder and among the calendars built into the app, so a user can add one
  by dropping a file in.
- Q: Where is the choice remembered? → A: In the app's own `usersettings.json`, next to the theme; `appsettings.json` is
  never written. If the remembered calendar is gone, `calendarFile` from `appsettings.json` applies, and Czech when it
  names none.
- Q: Should the calendar follow the OS locale? → A: Not yet. The maintainer had noted that the "Nameday calendar would
  be per the locale - in our case we use Czech one only I think"; phase 1 keeps Czech, v4's calendar, as the default,
  records each calendar's locale, and leaves the automatic choice by OS locale as a non-goal.
- Q: Can calendars for other countries be generated, and from what? → A: Yes. After the maintainer asked to "look
  harder" (the assistant admitted it "was too quick to stop at licensing"), nine more were generated from openly
  licensed sources pinned to a version: radoone/nameday-mcp (MIT) for Austria, Bulgaria, France, Hungary, Italy, Poland,
  Russia and Slovakia, and the official list of the Latvian State Language Centre (CC0) for Latvia. Czech stays v4's
  calendar, its names unchanged. Name days are never typed in or made up by hand.
- Q: Which countries are left out, and why? → A: Croatia (the only open source has damaged entries), Greece (the open
  source covers only 176 days, and many Greek name days move with Easter), Sweden and Finland (copyrighted official
  lists), and Lithuania, Estonia, Denmark, Germany and Spain (no source with a clear license and verifiable quality
  yet). They can be added later the same way.
- Q: (reviewing the files later that night) Every calendar file contains the same license "Slovak Name Days MCP Server";
  is this on purpose? → A: Yes, for the eight calendars from radoone/nameday-mcp: its MIT license names that copyright
  holder, and the notice must be kept word for word (the full text is in `calendars/THIRD-PARTY-NOTICES.md`). Czech and
  Latvian have their own license lines. The assistant offered to add the repository's name to the field; the files kept
  the original text.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Choose the name-day calendar (Priority: P1)

The user opens **⋯ ▸ Nameday Calendar** (macOS: **View ▸ Nameday Calendar**) and picks another country's calendar
([mock](../../docs/screens/main-window-calendar-menu.light.preview.png)). The names under the date, in Next N days and
in Find change at once, the people stay as they are, and the app opens with the same calendar the next time.

**Why this priority**: It is what the maintainer asked for: a way to see the available calendars and choose one. Before
it, the app used only the Czech calendar, and another one could only be named by hand in `appsettings.json`.

**Independent Test**: Choose **Slovakia (Slovensko)** and go to 9 October: the date shows "Dionýz" instead of
"Štefan/Sára", and after a restart Slovakia is still checked.

**Acceptance Scenarios**:

1. **Given** a fresh install, **When** the user opens ⋯ ▸ Nameday Calendar, **Then** it lists the available calendars by
   display name as radio items, with **Czechia (Česko)** checked.
2. **Given** the Czech calendar on 9 October, **When** the user chooses **Hungary (Magyarország)**, **Then** the date
   shows "Dénes, János" at once, Next N days and Find use the Hungarian names, and the people's namedays, birthdays and
   anniversaries stay as they were.
3. **Given** a calendar was chosen, **When** the app starts again, **Then** that calendar is active and checked.
4. **Given** the chosen calendar's file is no longer available, **When** the app starts, **Then** it uses the calendar
   named by `calendarFile` in `appsettings.json` (Czech by default) and shows no warning.
5. **Given** macOS, **When** the user opens **View ▸ Nameday Calendar** in the menu bar, **Then** it lists the same
   calendars, and choosing one works the same way.
6. **Given** the user chooses a calendar that can't be found or read, **When** the app switches to it, **Then** it shows
   "Calendar file not found: {name}" or "Error loading calendar: {reason}" (title "Data Error") and no calendar names,
   and problems from earlier switches aren't shown again.

---

### User Story 2 - Name days for more countries (Priority: P2)

Besides v4's Czech calendar, the app ships calendars for Austria, Bulgaria, France, Hungary, Italy, Latvia, Poland,
Russia and Slovakia. Their names come from openly licensed sources, converted without typing anything in by hand. Each
file says where its names come from and under what license, and the license notices come with the app.

**Why this priority**: The maintainer asked for calendars of other, at least European, countries and pushed for real
sources ("Please look harder!"). The chooser (User Story 1) works with any calendar file, but it is only useful with
more than one.

**Independent Test**: The menu lists ten calendars, well-known name days match, the published app has
`THIRD-PARTY-NOTICES.md` next to the executable, and generating the calendars again changes no file.

**Acceptance Scenarios**:

1. **Given** a fresh install, **When** the user opens the menu, **Then** it lists ten calendars: Austria (Österreich),
   Bulgaria (България), Czechia (Česko), France, Hungary (Magyarország), Italy (Italia), Latvia (Latvija), Poland
   (Polska), Russia (Россия) and Slovakia (Slovensko).
2. **Given** a shipped calendar, **When** the user goes to a well-known name day, **Then** it matches: for example the
   Slovak calendar shows "Cyril, Metod" on 5 July, the Hungarian one includes István on 20 August, and the Polish one
   includes Adam and Ewa on 24 December.
3. **Given** any shipped calendar file, **When** a user opens it, **Then** it names its source and its license.
4. **Given** a published copy of the app, **When** the user looks next to the executable, **Then**
   `THIRD-PARTY-NOTICES.md` is there, with the MIT notice of radoone/nameday-mcp and the CC0 dedication of the Latvian
   list.
5. **Given** the shipped Czech calendar, **When** it is compared with v4's, **Then** its names are identical.
6. **Given** the shipped calendars, **When** the maintainer generates them again, **Then** the files come out identical;
   a source with an invalid day, an empty entry or fewer than 360 days stops the generation with an error.

---

### User Story 3 - Add or replace a calendar with your own file (Priority: P3)

A user copies a file named `Anniversary.Calendar.{Name}.json` into the app-data folder, or next to the executable. It is
offered the next time the menu opens, with no restart. A file with the name of a shipped calendar replaces that
calendar, and a calendar file in v4's format, without the new details, still works.

**Why this priority**: Because calendars are files, as the maintainer suggested, anyone can add a country that isn't
shipped or correct a shipped one; but most users only choose among the shipped calendars.

**Independent Test**: Acceptance criterion 11 of [docs/spec.md](../../docs/spec.md) §12: copy a test calendar file into
the app-data folder, choose it, restart, then remove it.

**Acceptance Scenarios**:

1. **Given** the app is running, **When** the user copies `Anniversary.Calendar.Test.json` with `"name": "Test"` into
   the app-data folder and opens the menu again, **Then** **Test** is listed among the calendars.
2. **Given** Test is listed, **When** the user chooses it, **Then** the main window shows its names at once, and the
   choice survives a restart.
3. **Given** the Test file is removed, **When** the app starts, **Then** it falls back to `calendarFile` (Czech by
   default) without errors.
4. **Given** a calendar file in v4's format, without a name, called `Anniversary.Calendar.Klingon.json`, **When** the
   menu opens, **Then** it is listed as **Klingon**.
5. **Given** a file named like a shipped calendar (for example `Anniversary.Calendar.Czech.json`) next to the executable
   or in the app-data folder, **When** the menu opens or that calendar is used, **Then** the user's file replaces the
   shipped one and is listed once; a file next to the executable wins over one in the app-data folder.
6. **Given** `calendarFile` in `appsettings.json` names a calendar by its full path, **When** the app starts, **Then**
   that calendar is used and listed, checked, in the menu.

### Edge Cases

- **Remembered calendar gone** (its file was removed or renamed): the next start uses `calendarFile` (Czech by default)
  without a warning.
- **A calendar file that can't be read** (for example invalid JSON) isn't offered in the menu. If it is the active
  calendar, the warning "Error loading calendar: {reason}" appears and no calendar names are shown.
- **Not found when used**: a calendar that isn't there when the app loads it gives "Calendar file not found: {name}" and
  no calendar names until another one is chosen.
- **Repeated warnings**: after a switch only the problems of that switch are shown, not earlier ones.
- **No details in the file**: a v4 calendar file loads, and its menu name comes from its file name.
- **The same file name in several places**: listed once; the file next to the executable wins over the one in the
  app-data folder, which wins over the built-in calendar.
- **Days without names**: days missing from a calendar (for example 29 February in some) show no names.
- **A file added while the app runs** is offered the next time the ⋯ menu opens.
- **Other file names** aren't offered, except a calendar that `calendarFile` names by its full path.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The commands menu (the ⋯ button and right-click) MUST have a **Nameday Calendar** submenu, after Edit
  People… and a separator and before Theme, that lists the available calendars by display name as radio items with the
  active one checked. On macOS the menu bar's **View** menu MUST have the same **Nameday Calendar** list, above
  Appearance.
- **FR-002**: The available calendars MUST be the files named `Anniversary.Calendar.*.json` next to the executable and
  in the user's app-data folder, plus the calendars built into the app. Files with the same name MUST be listed once:
  the one next to the executable wins over the one in the app-data folder, which wins over the built-in one.
- **FR-003**: The list MUST be sorted by display name and MUST be rebuilt each time the menu opens, so a calendar file
  added while the app runs is offered without a restart.
- **FR-004**: Choosing a calendar MUST make it active at once: the names under the date, in Next N days and in Find MUST
  come from it, and the people MUST stay as they are.
- **FR-005**: The choice MUST be remembered for the user in `usersettings.json` and used at every later start while that
  calendar is available.
- **FR-006**: When no calendar was chosen, or the chosen one is no longer available, the app MUST use the calendar named
  by `calendarFile` in `appsettings.json`, or the Czech calendar when none is named, so a v4 user sees the same calendar
  as before. A chosen calendar that is gone MUST NOT cause a warning.
- **FR-007**: The app MUST NOT write calendar files or `appsettings.json`.
- **FR-008**: A calendar file MAY carry a display name, a locale (culture code), a source and a license next to its
  days. All four MUST be optional, so v4 calendar files still load; without a display name, the menu MUST show the part
  of the file name between `Anniversary.Calendar.` and `.json`.
- **FR-009**: If a calendar can't be found or read when it is loaded, the app MUST show "Calendar file not found:
  {name}" or "Error loading calendar: {reason}" (title "Data Error") and continue with no calendar names; after a
  switch, only the problems of that switch MUST be shown. A calendar file that can't be read MUST NOT be offered in the
  menu.
- **FR-010**: The active calendar MUST always be listed, so the menu shows a checked item, also when `calendarFile`
  names it by a full path.
- **FR-011**: The app MUST ship ten name-day calendars, each with a display name, a locale, a source and a license:
  Czech (v4's names, unchanged), Austrian, Bulgarian, French, Hungarian, Italian, Latvian, Polish, Russian and Slovak.
- **FR-012**: Shipped name days MUST come from v4's Czech calendar or from openly licensed sources pinned to a version,
  and MUST be generated reproducibly, never typed in or made up by hand. Generation MUST reject invalid day keys, empty
  entries and calendars of fewer than 360 days, and two runs MUST give identical files.
- **FR-013**: The calendars' documentation MUST list each shipped calendar's source, license and number of days, the
  checks made on the sources, and the countries left out with the reason.
- **FR-014**: The license notices for the bundled calendar data MUST ship next to the app's executable.

### Key Entities

- **Calendar file**: one calendar, named `Anniversary.Calendar.{Name}.json`: the names for each day of the year, keyed
  by month and day as in v4, plus an optional display name, locale, source and license.
- **Available calendar**: what the menu lists for a calendar file: its file name, display name and locale. The same file
  name found in several places is one available calendar.
- **Active calendar**: the calendar whose names the main window and Find use: the remembered one, otherwise the one
  named by `calendarFile`, otherwise Czech.
- **Remembered calendar**: the file name of the chosen calendar, kept in the user's `usersettings.json`; none until the
  user chooses one.
- **Calendar source**: where a shipped calendar's names come from (v4, or an openly licensed source pinned to a version)
  and its license, with the notices that ship next to the executable.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Copying a second calendar file into the app-data folder makes it appear in Nameday Calendar; choosing it
  changes the calendar names in the main window immediately; the choice survives a restart; and removing the file falls
  back to `calendarFile`/Czech without errors (acceptance criterion 11 in [docs/spec.md](../../docs/spec.md) §12, as it
  then read).
- **SC-002**: Switching calendars takes one menu choice, needs no restart, and never changes the people shown.
- **SC-003**: Ten name-day calendars ship instead of v4's one; every shipped file names its source and license, and
  every published copy of the app has the license notices next to the executable.
- **SC-004**: Generating the calendars again gives identical files, and the Czech calendar's names are identical to
  v4's.
- **SC-005**: A user who never chooses a calendar sees v4's Czech calendar as before: 9 October shows "Štefan/Sára".
- **SC-006**: 10 automated tests cover discovery, precedence, display names, the remembered choice and switching; they
  pass with the rest of the suite (78 tests at the move to GitHub) and a build free of warnings.

## Assumptions

- Builds on [001](../001-cross-platform-rewrite/spec.md): the file lookup (next to the executable, then the app-data
  folder, then the built-in files), load warnings shown after the main window opens, the app-written `usersettings.json`
  (until then holding only the theme), the ⋯ menu with its right-click twin and the macOS menu bar, and v4's calendar
  format and Czech calendar. The new submenu sits below [002](../002-people-editor/spec.md)'s Edit People….
- Calendar days are fixed month-and-day dates, as in v4. Days that move every year, such as Easter, can't be expressed
  yet ([006](../006-holiday-calendar-rules/spec.md) added that), which is also why Greek name days were left out.
- The calendar isn't chosen automatically from the OS locale: that was a phase-1 non-goal, and each calendar's locale is
  only recorded, "reserved for automatic choice by OS locale later" (spec 5.1 then);
  [006](../006-holiday-calendar-rules/spec.md) later used it to choose the default by the OS region.
- Only name-day calendars ship; holiday and observance calendars came later
  ([006](../006-holiday-calendar-rules/spec.md), [008](../008-international-days/spec.md)).
- Users add calendars by copying files; there is no import command, and the app never edits a calendar.
- Generating the calendars is a step for the maintainer that needs internet access; building the app doesn't need it,
  because the generated files are kept in the repository.
- The sources, the checks and the left-out countries are described in [calendars/README.md](../../calendars/README.md).
