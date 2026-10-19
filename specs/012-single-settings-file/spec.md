# Feature Specification: Single Settings File

**Feature Branch**: `012-single-settings-file`

**Created**: 2026-10-11

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "Also previously we had some config options: - how many days ahead to show namedays -
show number of years for birthdays. Can we add these to the program? It could be just the config .json for now (or
should it be a screen?)", then "Is it this file? I see the following on my MacOS `…/Anniversary/usersettings.json`. Is
that the file or wrong file?" and "Ok, so I renamed the file and now my calendar is gone (it doesn't show the Czech) but
it still loads the right data (my people) file. Does this mean that the app is actually using both files? Can we
investigate which config .json is actually used?"

## Clarifications

### Session 2026-10-11

- Q: Should the two v4 options get menu items or a settings window, or is a settings file enough? → A: A file is
  enough: both options stay file-only. The maintainer chose that over menu items or a settings window.
- Q: Which file holds them: the app's own `usersettings.json`, or a separate, hand-written `appsettings.json` as in v4?
  → A: One file, `usersettings.json`, for all settings; v4's `appsettings.json` is no longer read (user decision). Two
  files with overlapping keys (`dataFile` in both, the calendar as `calendar` in one and `calendarFile` in the other)
  confused even the author: after `usersettings.json` was renamed to `appsettings.json`, the people still loaded but
  the Czech calendar was gone.
- Q: How do people find the keys to edit, when the app writes the file only after a menu choice? → A: The app writes
  every key, and creates the file at the first start, so every setting is in the file, ready to edit.
- Q: When may the file be edited by hand? → A: With the app closed: the app reads it at start-up, and saving a menu
  choice writes the values it read.
- Q: What if a hand edit leaves the file unreadable, for example after a typo? → A: The app says so when it starts,
  uses the default settings and never saves over the file, so nothing in it, such as the data file's path, is lost.
- Q: Where do the keys of v4's `appsettings.json` go? → A: `numberOfDaysAhead` and `displayYearsOnBirthdays` go into
  `usersettings.json` as they are, `dataFile` is the data file there, and `calendarFile` is the calendar chosen in the
  app.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Set the two v4 options in the settings file (Priority: P1)

v4 had two options: how many days the "Next 7 days" section covers, and whether birthdays and anniversaries show the
number of years. The user quits the app, sets `numberOfDaysAhead` and `displayYearsOnBirthdays` in `usersettings.json`
with a text editor, and starts the app again: the main window looks that many days ahead and shows or hides the years.

**Why this priority**: It is what the maintainer asked for. The options existed in v5, but only in a second,
hand-made `appsettings.json`, and the maintainer looked for them in `usersettings.json`; now they are there.

**Independent Test**: With the app closed, set `numberOfDaysAhead` to 14 and `displayYearsOnBirthdays` to `false` in
`usersettings.json` and start the app: the last section is called "Next 14 days" and birthdays show no years.

**Acceptance Scenarios**:

1. **Given** the app is closed and `usersettings.json` has `"numberOfDaysAhead": 14` and
   `"displayYearsOnBirthdays": false`, **When** the app starts, **Then** the last section is titled "Next 14 days" and
   covers the 14 days after the shown day, and birthdays and anniversaries are listed without the number of years.
2. **Given** a `usersettings.json` without these two keys, **When** the app starts, **Then** it looks 7 days ahead and
   shows the years, as v4 did by default.
3. **Given** the user wrote a key in another letter case, added a `//` comment or left a comma after the last value,
   **When** the app starts, **Then** it still reads the values.
4. **Given** values set by hand, **When** the user then changes the theme or the calendar in the menus, **Then** the
   file still holds the values set by hand.

---

### User Story 2 - Find every setting in one file (Priority: P2)

All settings are in one file, `usersettings.json` in the app-data folder: the theme, calendar and data file the user
chooses in the menus, and the two options set by hand. The app creates the file with every setting at the first start
and writes every setting whenever a menu choice changes. v4's `appsettings.json` is no longer read, so no second file
has keys that overlap with this one.

**Why this priority**: Two files with overlapping keys confused even the author: the options put into the app's file
were ignored, and renaming that file lost the calendar choice. With one file and every key in it, there is no question
which file to edit. The options themselves (User Story 1) work without this story.

**Independent Test**: Start the app with no app-data folder: `usersettings.json` appears with all five settings. Put an
`appsettings.json` with `"numberOfDaysAhead": 3` next to it and start the app again: it still shows "Next 7 days".

**Acceptance Scenarios**:

1. **Given** no app-data folder (a first start), **When** the app starts, **Then** `usersettings.json` is created
   there with all five settings at their defaults: the theme `System`, no calendar and no data file chosen, 7 days and
   years shown.
2. **Given** the user chooses a theme, a calendar, or a data file with Data File ▸ Open… or Save As…, **When** the
   choice is saved, **Then** `usersettings.json` holds the new value and every other setting.
3. **Given** v4's `appsettings.json` (or one made for an earlier v5) next to the executable or in the app-data folder,
   **When** the app starts, **Then** nothing in it applies: neither its two options nor its calendar or data file.
4. **Given** `dataFile` set by hand to a file name such as `Family.Data.json`, **When** the app starts, **Then** the
   app looks for that file next to the executable and then in the app-data folder, as it does for
   `Anniversary.Data.json`.
5. **Given** no `dataFile` (missing, `null` or empty), **When** the app starts, **Then** the people come from
   `Anniversary.Data.json`, found as before.

---

### User Story 3 - Never lose a hand-edited file (Priority: P2)

Editing a file by hand invites typos. When the app can't read `usersettings.json`, it says so after its window opens,
starts with the default settings and leaves the file exactly as it is, so nothing in it (such as the path of a data
file on a cloud drive) is lost. Once the user has fixed the file and starts the app again, everything is back.

**Why this priority**: Until now only the app wrote the file: when it couldn't read it, it silently started over with
the defaults and replaced the file at its next save. Now that people edit the file, that would throw their settings
away.

**Independent Test**: With the app closed, put a typo into `usersettings.json` (for example `"numberOfDaysAhead":
"14"`, a number in quotes), start the app, change the theme and quit: a "Configuration Error" message appeared, and the
file is unchanged.

**Acceptance Scenarios**:

1. **Given** a `usersettings.json` that isn't valid JSON, has a value of the wrong type, or can't be opened, **When**
   the app starts, **Then** after the main window opens a message titled "Configuration Error" says "Error loading
   configuration: {path}: {reason}" and, on a new line, "Using default settings.", and the app runs with the default
   settings.
2. **Given** that situation, **When** the user changes the theme, the calendar or the data file in the menus, **Then**
   the choice applies until the app closes, and the file isn't written.
3. **Given** the user fixes the file with the app closed, **When** the app starts again, **Then** no message appears,
   the file's values apply, and menu choices are saved again.
4. **Given** a value of the wrong type, **When** the message appears, **Then** its reason names the key, for example
   `numberOfDaysAhead`.

### Edge Cases

- **First start**: there is no file yet; the app creates it with the defaults before its window opens. If it can't be
  written (for example in a read-only folder), the app runs with the defaults, and menu choices last until it closes.
- **A file from before this feature** (with only the theme, calendar and data file): the two options take their
  defaults, and the app adds their keys the next time it saves a menu choice.
- **Editing while the app runs**: the app reads the file only at start-up, and a menu choice made afterwards writes the
  values it read, replacing the edit. So the file is edited with the app closed.
- **Comments** in a hand-edited file are allowed, but the app's next save writes the file in its own layout, without
  them.
- **Unknown keys** (for example v4's `calendarFile` typed into `usersettings.json`) are ignored and aren't kept when the
  app saves.
- **v4's `appsettings.json`** left next to the executable or in the app-data folder is ignored without a message;
  people moving from v4 copy its two options into `usersettings.json` by hand.
- **A calendar that isn't available** (removed, or misspelt in the file): the calendar for the OS region is shown,
  otherwise the US one, without a message.
- **A data file name typed by hand that isn't found**: the warning "Data file not found: {name}" and no people; saving
  in the editor creates the file in the app-data folder.
- **A file that can't be read** applies none of its values, including the data file's path: the people come from the
  default data file until the file is fixed, and the path stays in the file.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST keep all its settings in one file, `usersettings.json` in the app-data folder of the user
  account (one per user and computer): the theme, the calendar, the data file, how many days the "Next N days" section
  covers (`numberOfDaysAhead`) and whether birthdays and anniversaries show the years (`displayYearsOnBirthdays`).
- **FR-002**: `numberOfDaysAhead` (a whole number, default 7) and `displayYearsOnBirthdays` (`true` or `false`, default
  `true`) MUST be set by editing the file; this feature adds no menu item or window for them.
- **FR-003**: At start-up the main window MUST use them: the last section is titled "Next {N} days" and covers the N
  days after the shown day, and when `displayYearsOnBirthdays` is `false`, birthdays and anniversaries are listed
  without the number of years.
- **FR-004**: The app MUST read the file at start-up, accepting key names in any letter case, comments and trailing
  commas; a missing key MUST take its default (the theme `System`, no calendar and no data file chosen, 7 days, years
  shown).
- **FR-005**: If the file doesn't exist at start-up, the app MUST create it with every setting at its default.
- **FR-006**: When a choice changes in the menus (the theme, the calendar, or a data file chosen with Data File ▸
  Open… or Save As…), the app MUST write every setting to the file, keeping the other values it read at start-up,
  including those set by hand.
- **FR-007**: The app MUST NOT read `appsettings.json` any more, neither v4's file next to the executable or in the
  app-data folder nor a built-in one; none of its keys apply.
- **FR-008**: The data file MUST be the one named by `dataFile`: a full path, as Open… and Save As… write it, is used
  as is; a file name typed by hand is looked for next to the executable, then in the app-data folder, then among the
  built-in files. When `dataFile` isn't set (missing, `null` or blank), the data file MUST be `Anniversary.Data.json`,
  found the same way.
- **FR-009**: The calendar MUST be the one named by `calendar` if that calendar is available, otherwise the calendar
  for the OS region, otherwise the US one; no other file can name the calendar.
- **FR-010**: If the file can't be read (invalid JSON, a value of the wrong type, or the file can't be opened), the app
  MUST start anyway with the default for every setting and, after its window opens, show "Error loading
  configuration: {path}: {reason}" and, on a new line, "Using default settings." in a message titled "Configuration
  Error".
- **FR-011**: While the file couldn't be read, the app MUST NOT write it: menu choices apply until the app closes, and
  the file stays as the user left it.
- **FR-012**: The user guide shipped with the app MUST explain where the file is, which settings the menus keep there,
  that the two options are only in the file and how to change them (quit, edit, start again, with an example), and
  what happens when the app can't read the file; the repository README MUST list `usersettings.json` as the only
  settings file and tell people moving from v4 to set the two options there instead of in `appsettings.json`.

### Key Entities

- **Settings file** (`usersettings.json`): one per user account and computer, in the app-data folder. It holds five
  settings; the app writes all of them at the first start and whenever a menu choice changes, and people may edit it
  with the app closed.
- **Setting**: a named value with a default: three are chosen in the menus (theme, calendar, data file) and two are
  set only by hand (`numberOfDaysAhead`, `displayYearsOnBirthdays`).
- **v4's configuration file** (`appsettings.json`): v4's hand-written file with `calendarFile`, `dataFile`,
  `displayYearsOnBirthdays` and `numberOfDaysAhead`, which v5 read (but never wrote) until this feature; now ignored.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With the app closed, setting `numberOfDaysAhead` to 14 and `displayYearsOnBirthdays` to `false` in
  `usersettings.json` shows "Next 14 days" and no years after the next start (acceptance criterion 6 in
  [docs/spec.md](../../docs/spec.md) §12).
- **SC-002**: After the first start, all five settings are in one file, each with exactly one key, and no other file
  changes a setting (before: `dataFile` was in two files, and the calendar was `calendar` in one and `calendarFile` in
  the other).
- **SC-003**: A `usersettings.json` the app can't read is left exactly as it was, whatever menu choices the user makes,
  and the problem is reported at every start until it is fixed (criterion 6).
- **SC-004**: Values set by hand survive every later menu choice: after the theme, the calendar and the data file have
  been changed in the app, the two options still have the values the user set.
- **SC-005**: Seven new automated tests cover the settings file and loading with it, five tests of the removed
  configuration file are gone, and all 205 tests pass with a build free of warnings on Windows, macOS and Linux (on
  macOS and Linux, 202 pass and the 3 Windows-only tests are skipped).

## Assumptions

- Builds on [001](../001-cross-platform-rewrite/spec.md) (the app-written `usersettings.json` with the theme, v4's
  `appsettings.json` read but never written, and load warnings shown after the window opens, titled "Configuration
  Error" for settings problems), [003](../003-calendar-chooser/spec.md) (the calendar choice remembered),
  [006](../006-holiday-calendar-rules/spec.md) (the default calendar by OS region),
  [007](../007-data-file-sharing/spec.md) (the data file chosen with Open… or Save As…, remembered per computer) and
  [009](../009-user-guide/spec.md) (the user guide shipped with every download).
- The two options stay file-only, as the maintainer suggested "for now"; menu items or a settings window are out of
  scope.
- The settings stay per user and computer, so a shared data file can have its own path on each computer (007); the
  settings file itself isn't shared.
- Nothing is moved from v4 automatically: the README tells people moving from v4 to open their data file with Data
  File ▸ Open… (or copy it into the app-data folder), choose the calendar once, and set the two options in
  `usersettings.json`.
- The app reads the settings only at start-up and doesn't watch the file while it runs.
- The taller icon page, committed together with this feature, belongs to the icon work of
  [010](../010-install-packages/spec.md), not to this feature.
