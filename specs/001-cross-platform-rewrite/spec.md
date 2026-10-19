# Feature Specification: Cross-Platform Rewrite

**Feature Branch**: `001-cross-platform-rewrite`

**Created**: 2026-10-09

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "I have an existing simple application (dialog like) implemented using .NET 10 and WinForms
… I would like to reimplement the application using some kind of cross-platform library, targeting Windows, MacOS and
if possible also Linux and iOS (Android might be nice as well but not necessary …). The priority order would be
Windows, MacOS, Linux, iOS and finally Android. … First, recommend the right framework/UI library for the job."

**Later changes**: [005](../005-ci-pipeline/spec.md) added CI and the `win-arm64` and `linux-arm64` builds;
[006](../006-holiday-calendar-rules/spec.md) made the default calendar follow the OS region, and a bug fix committed
with it stops browsing and Find safely at the ends of the date range; [007](../007-data-file-sharing/spec.md) lets the
data file be chosen in the app; [009](../009-user-guide/spec.md) put a user guide next to the executable;
[010](../010-install-packages/spec.md) added install packages and put the macOS icon on a white rounded square;
[012](../012-single-settings-file/spec.md) replaced `appsettings.json` with a single `usersettings.json`.

## Clarifications

### Session 2026-10-09

- Q: Which cross-platform UI library should the rewrite use? → A: Avalonia 12 on .NET 10. It keeps C#, so v4's model
  and logic carry over, and it is strongest on the desktop, the first priority. .NET MAUI (no Linux, mobile-first),
  Uno Platform (viable, but heavier for a tiny app), Eto.Forms (no mobile) and Flutter, Tauri or Compose Multiplatform
  (they leave C#) were weighed and not chosen. Phase 1 is the desktop; iOS and Android come later.
- Q: Is JetBrains Rider enough, or is Visual Studio needed? → A: Rider with the AvaloniaRider plugin is enough; Visual
  Studio would only help with v4's WinForms designer and with .NET MAUI tooling, and neither is used.
- Q: Which rules does the code follow? → A: The maintainer's: honour v4's coding style; no unnecessary complexity or
  data structures, for a concise code base that is easy to navigate, understand and maintain; documentation on every
  class, struct and interface and on every public member; classic tests with hand-written fakes instead of a mocking
  library. They are now part of the constitution.
- Q: Does the Change Date dialog have to be as tall as the first mocks? → A: No. It keeps about v4's size, and its
  calendar drops down below the dialog's edge.
- Q: Is a "⋯" button next to Quit an established pattern? → A: The ⋯ button moved to the top-right of the "Today is"
  section, where Windows 11, GNOME and Android apps put their "more" menu. The same menu opens on right-click, and
  macOS gets the commands in its menu bar.
- Q: How must the window behave when it's resized? → A: Like v4's anchors: everything stretches to the width, only
  "Next N days" grows taller, Namedays and Birthdays keep their height, Yesterday and Tomorrow stay at the bottom-left
  and Quit at the bottom-right.
- Q: Can the user switch between light, dark and the system setting, with "system" also following changes during the
  day (macOS, or Windows with PowerToys)? → A: Yes: Theme ▸ System / Light / Dark (macOS: View ▸ Appearance), where
  System follows the OS live, including scheduled changes. The choice is kept in `usersettings.json`, a file only the
  app writes, so `appsettings.json` stays untouched; the assistant made that choice while the maintainer was away.
- Q: What should the built-in people data be? → A: Empty (user decision), instead of the made-up sample people the
  spec first had, so no personal data is compiled into the app. The user's own v4 data file is copied into the
  app-data folder or next to the executable.

### Session 2026-10-10

- Q: Is a ⋯ menu at the top-right common, when hamburger menus (☰) used to be at the top-left? → A: Yes, so it stays.
  A ☰ at the top-left usually opens a navigation drawer for switching pages, while a "more" button at the top-right
  holds commands (Windows 11, browsers, GNOME, Android, macOS and iOS toolbars), and this app has only commands. A
  Linux-only ☰ would be a one-line change, which the assistant didn't think worth making.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See the day at a glance (Priority: P1)

The app starts on today, just like v4: "Today is" with the date and the calendar's names for the day, then Namedays,
Birthdays and Anniversaries, and "Next 7 days", with Yesterday, Tomorrow and Quit at the bottom. The people come from
the user's own v4 data file, copied into the app-data folder or next to the executable; the day's names come from v4's
Czech calendar, which is built in. The window looks and resizes like v4 on Windows, macOS and Linux.

**Why this priority**: This window is what v4 is for ("what's today"); every other command moves around it. It is also
where the user's v4 data must show what v4 showed.

**Independent Test**: Copy v4's `Anniversary.Data.json` into the app-data folder and start the app: the window shows
today's date, the day's calendar names and the people with an event today and in the next 7 days. Without the file,
the people boxes are empty and a hint says how to add people.

**Acceptance Scenarios**:

1. **Given** v4's data file in the app-data folder and 9 October 2026 shown, **When** the user looks at the window,
   **Then** "Today is" shows the date and "Štefan/Sára", Namedays and Birthdays and Anniversaries are empty, and "Next
   7 days" shows a line of people and then "Marina, Andrej, Marcel, Renáta, Agáta, Tereza, Havel".
2. **Given** a person whose birthday or anniversary is on the shown day, **When** the window shows it, **Then** the
   line reads "{Name} ({age})" or "{Name} - {description} ({years})"; with `displayYearsOnBirthdays` set to `false` in
   `appsettings.json`, just "{Name}" or "{Name} - {description}".
3. **Given** a birth year of `9999` (v4's "year unknown"), **When** that birthday is shown, **Then** only the name
   appears, without a negative number.
4. **Given** no data file anywhere, **When** the app starts, **Then** the people boxes are empty and Birthdays and
   Anniversaries shows the hint "No people yet. Use ⋯ ▸ Edit People… to add some."
5. **Given** a data file that isn't valid JSON, **When** the app starts, **Then** the window opens first, a message
   titled "Data Error" says "Error loading data: …", and the app keeps running with no people.
6. **Given** `"numberOfDaysAhead": 14` in an `appsettings.json` in the app-data folder, **When** the app starts,
   **Then** the last section is called "Next 14 days" and covers 14 days.
7. **Given** the main window, **When** the user makes it wider and taller, **Then** every section stretches to the
   width, only "Next 7 days" gets taller, Yesterday and Tomorrow stay at the bottom-left and Quit at the bottom-right.

---

### User Story 2 - Browse to another day (Priority: P2)

Yesterday and Tomorrow step one day back or forward, Today returns to today, and Change Date… (or a click on the date)
opens a small dialog to pick any date. The window then shows that day's names, birthdays and next days.

**Why this priority**: v4 has the same commands; looking a few days back or ahead is the next thing after reading
today.

**Independent Test**: Click Tomorrow, then Yesterday, then the date; pick a date in Change Date and confirm: the window
shows each day; Today brings it back.

**Acceptance Scenarios**:

1. **Given** today is shown, **When** the user clicks Tomorrow, **Then** the next day is shown, and Yesterday goes back
   one day.
2. **Given** another day is shown, **When** the user chooses ⋯ ▸ Today (macOS: Go ▸ Today), **Then** today is shown.
3. **Given** the main window, **When** the user clicks the date (hand cursor, tooltip "Click to change the date") or
   chooses Change Date…, **Then** the "Change Date" dialog opens on the shown day; OK shows the picked day, and Cancel
   or Esc keeps the shown day.
4. **Given** the Change Date dialog, **When** the user opens its calendar, **Then** the calendar drops down below the
   dialog, which keeps about v4's size.

---

### User Story 3 - Find a name (Priority: P2)

Find… asks for a name and three options and jumps to the first day whose calendar names or people match; Find Next
continues from the following day.

**Why this priority**: v4 has Find and Find Next with the same options; they answer when a name's name day or a
person's event comes up.

**Independent Test**: With "Štefan/Sára" on 9 October, open Find (from the commands menu or with Ctrl+F) and search
for "sára" while a day in 2026 is shown: the window shows 9 October 2026, and Find Next shows the next match.

**Acceptance Scenarios**:

1. **Given** "Štefan/Sára" on 9 October and no earlier match, **When** the user finds "sÁRa" with no options while a
   day in 2026 is shown, **Then** 9 October 2026 is shown (the letter case doesn't matter).
2. **Given** "Whole words only", **When** the user finds "Bob", **Then** a day with "Bobby" doesn't match, while "Bob"
   separated by a space, comma, semicolon, tab, slash or hyphen does.
3. **Given** "Search calendar only", **When** the user finds a person's name, **Then** people are ignored.
4. **Given** an earlier search, **When** the user chooses Find Next (or presses F3), **Then** the search continues from
   the day after the shown day; with no earlier search, Find Next opens Find….
5. **Given** a name that matches nothing up to 31 December of the following year, **When** the search ends, **Then** a
   message titled "Find" says "'{name}' not found." and the shown day stays; an empty name gives "Please enter a name
   to search for."

---

### User Story 4 - Reach every command the way each OS expects (Priority: P3)

v4 kept Today, Change Date, Find and About in the Windows system menu. Now one commands menu opens from a ⋯ button at
the top-right of "Today is" and from a right-click anywhere in the window; macOS also has the commands in its menu bar,
and the shortcuts follow each platform.

**Why this priority**: The system menu isn't portable, so these commands need one place that works on every OS. The
story ranks after the stories whose commands it holds.

**Independent Test**: On Windows or Linux, open the ⋯ menu and the right-click menu and use each item, and press
Ctrl+F and F3; on macOS, use the menu bar's Go and Find menus, ⌘F and ⌘G, and About Anniversary in the app menu.

**Acceptance Scenarios**:

1. **Given** the main window, **When** the user clicks ⋯ (tooltip "More commands"), **Then** the commands menu opens
   below it, aligned to its right edge: Today, Change Date…, Find… (Ctrl+F), Find Next (F3), Theme ▸ and About
   Anniversary…, in groups separated by lines.
2. **Given** the main window, **When** the user right-clicks anywhere in it, **Then** the same menu opens.
3. **Given** Windows or Linux, **When** the user presses Ctrl+F or F3, **Then** Find… opens or Find Next runs; on macOS
   ⌘F and ⌘G do the same, and the menu shows ⌘F and ⌘G.
4. **Given** macOS, **When** the user opens the menu bar, **Then** Go has Yesterday, Tomorrow, Today and Change Date…,
   Find has Find… (⌘F) and Find Next (⌘G), View has Appearance, and the app menu has About Anniversary; ⌘Q quits.
5. **Given** any OS, **When** the user chooses About Anniversary…, **Then** a dialog shows the app icon, "Anniversary
   Application", "Version 5.0", "This program displays names, birthdates, and anniversaries for a specified day." and
   the copyright line; OK, Enter or Esc closes it.

---

### User Story 5 - Choose light, dark or the system theme (Priority: P3)

⋯ ▸ Theme ▸ System / Light / Dark (macOS: View ▸ Appearance) switches the whole app at once. System, the default,
follows the OS light or dark setting while the app runs, including changes on a schedule. The choice is remembered.

**Why this priority**: A user request beyond v4; without it the app already follows the OS setting.

**Independent Test**: Choose Dark: every open window and menu turns dark at once, and the app starts dark after a
restart. Choose System and switch the OS between light and dark while the app runs: the app follows.

**Acceptance Scenarios**:

1. **Given** System, **When** the OS switches between light and dark (by hand or on a schedule: macOS Auto, Windows'
   app mode or PowerToys Light Switch, the GNOME or KDE colour scheme), **Then** the app re-themes without a restart.
2. **Given** any theme, **When** the user chooses Light or Dark, **Then** all open windows, dialogs and menus switch at
   once and the menu checks the choice.
3. **Given** Dark was chosen, **When** the app starts again, **Then** it opens dark straight away.
4. **Given** a missing or broken `usersettings.json`, **When** the app starts, **Then** it uses System without a
   message.

---

### User Story 6 - Get a ready-to-run copy for each OS (Priority: P3)

Each desktop platform gets one self-contained executable, named Anniversary as in v4, that runs without installing
anything else: Windows (x64), macOS (Apple Silicon and Intel) and Linux (x64). It carries the refreshed app icon, and
the repository README explains how to build, run and publish it and how to move from v4.

**Why this priority**: The rewrite exists to run on each OS, and a single file is the simplest way to get it there;
installers weren't part of phase 1.

**Independent Test**: Publish for each target, copy the result to a computer with that OS and no developer tools,
make it executable on macOS and Linux, and start it.

**Acceptance Scenarios**:

1. **Given** the Windows build, **When** it is copied to a PC and started, **Then** `Anniversary.exe` opens the app,
   with the app icon on the file and the window.
2. **Given** the macOS or Linux build, **When** it is marked executable and started, **Then** the app opens without
   installing anything else.
3. **Given** the repository README, **When** a developer follows it, **Then** they can build, test, run and publish
   the app, and they know where it looks for its files and how to bring the v4 data along.

### Edge Cases

- **No people at all** (a fresh install, or no data file anywhere): the hint instead of an empty Birthdays and
  Anniversaries box; no people in Namedays or Next N days.
- **Year unknown** (`9999`) or a date after the shown day's year: the name without the years.
- **Several events on one day**: the person appears once in each list.
- **29 February**: shows only in leap years; in other years those birthdays, name days and anniversaries fall on no day.
- **Calendar texts with commas or slashes** ("Štefan/Sára", "Adam a Eva/Štědrý den"): shown as they are under the
  date; Next N days splits them only at commas, trims the pieces, skips empty ones and lists each name once.
- **Missing or broken files**: one message per problem after the window opens; the app goes on with an empty
  calendar, no people or v4's default configuration. A missing configuration file isn't a problem, because v4's
  configuration is built in.
- **A configured full path that doesn't exist**: reported as not found; the folders aren't searched instead.
- **Long text**: wraps to the box; a box whose text doesn't fit scrolls without moving the others, and a long date
  wraps at the minimum width instead of being cut off.
- **Find with an empty name, or no match** by 31 December of the following year: a message, and the shown day stays.
- **Find Next before any Find**: opens Find….
- **Change Date with the picker cleared**: OK keeps the shown day.
- **Missing or broken `usersettings.json`**: the System theme, without a message; if the theme can't be saved, the
  choice still applies until the app closes.
- **The ends of the date range** (1 January 0001 and 31 December 9999) weren't covered. A review on 2026-10-10 found
  that showing the last days of 9999, searching in 9999 or stepping past either end crashed the app (reachable only by
  picking such a date in Change Date); the later bug fix is under Later changes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST run on Windows, macOS and Linux with the same features, and MUST behave like v4 unless this
  specification says otherwise; every deliberate difference MUST be listed in the system spec
  ([docs/spec.md](../../docs/spec.md) §11).
- **FR-002**: At start-up the main window MUST show today: a "Today is" section with the date in the user's long date
  format and, under a line, the calendar's names for the day; then "Namedays", "Birthdays and Anniversaries" and "Next
  N days"; and Yesterday and Tomorrow at the bottom-left and Quit at the bottom-right. Keyboard focus MUST start on
  Quit, and Quit MUST close the app.
- **FR-003**: "Namedays" MUST list, one per line and in data-file order, the people whose name day is the shown day.
- **FR-004**: "Birthdays and Anniversaries" MUST list, one per line and in data-file order, "{Name} ({age})" for each
  birthday and then "{Name} - {description} ({years})" for each anniversary on the shown day. The years MUST be left
  out when `displayYearsOnBirthdays` is `false`, and whenever the date's year is after the shown day's year, so v4's
  `9999` ("year unknown") never shows a negative number.
- **FR-005**: A person MUST appear at most once in each list for a day, even when several of their events fall on it.
- **FR-006**: "Next N days" MUST take N from `numberOfDaysAhead` (default 7) and show it in its caption. Its first
  line MUST list the people with any event (name day, birthday or anniversary) in the N days after the shown day, each
  once, in date order and then data-file order; its second line MUST list the calendar's names for those days, split
  at commas, trimmed and each once, in date order. Each line is joined with ", ", and an empty line is left out.
- **FR-007**: Name days, birthdays and anniversaries on 29 February MUST show only in leap years.
- **FR-008**: The text in the Namedays, Birthdays and Anniversaries and Next N days boxes MUST be selectable, so it can
  be copied as in v4.
- **FR-009**: The main window MUST open centred at v4's size (312 × 468 at 100 % display scaling), MUST be resizable
  down to 280 × 420, and MUST NOT remember its size. When it is resized, every section MUST stretch to the width and
  only "Next N days" MUST change its height, with Yesterday and Tomorrow at the bottom-left and Quit at the
  bottom-right. Text MUST wrap to its box, and a box with more text than fits MUST scroll on its own.
- **FR-010**: The app MUST blend with each OS and keep v4's compact layout: the platform's standard UI font size, the
  names and the date slightly larger as in v4, and the OS light or dark setting by default.
- **FR-011**: The app MUST read v4's people file, calendar file and configuration file (`appsettings.json`) unchanged,
  with property names in any letter case, and with comments and trailing commas allowed.
- **FR-012**: Each file MUST be looked for on its own: a full path in the configuration is used as it is; otherwise the
  first copy found next to the executable, then in the user's app-data folder (`%APPDATA%\Anniversary` on Windows,
  `~/Library/Application Support/Anniversary` on macOS, `~/.config/Anniversary` on Linux), then the built-in default.
- **FR-013**: The built-in defaults MUST be v4's configuration, v4's Czech name-day calendar and an empty people list;
  no personal data is built into the app.
- **FR-014**: When there are no people at all, the Birthdays and Anniversaries box MUST show the muted hint "No people
  yet. Use ⋯ ▸ Edit People… to add some." instead of staying empty.
- **FR-015**: Loading MUST NOT stop the app or show anything before the main window is open. After it opens, each
  problem MUST be shown in its own message: "Calendar file not found: {name}", "Error loading calendar: {reason}",
  "Data file not found: {name}" or "Error loading data: {reason}" (title "Data Error"), or "Error loading
  configuration: {reason}" followed by "Using default settings." (title "Configuration Error"). The app then continues
  with an empty calendar, no people or v4's default configuration.
- **FR-016**: The app MUST write only its own `usersettings.json`; it MUST NOT write `appsettings.json`.
- **FR-017**: Yesterday and Tomorrow MUST show the previous or next day, and Today MUST show today again.
- **FR-018**: Change Date… and a click on the date MUST open a "Change Date" dialog with a date picker set to the shown
  day: OK shows the picked day (or keeps the shown day if the picker was cleared), and Cancel or Esc changes nothing.
  The dialog MUST keep about v4's size, with the calendar dropping down below it.
- **FR-019**: Find… MUST open a "Find" dialog with a Name box and the options "From current date", "Whole words only"
  and "Search calendar only", filled with the last values and with the name selected.
- **FR-020**: Find MUST start at the shown day ("From current date") or at 1 January of the shown year, go day by day
  up to 31 December of the following year, and show the first day whose calendar names or (unless "Search calendar
  only") a person's name contain the text, in any letter case. With "Whole words only" the text MUST equal a whole
  word, where words are separated by spaces, commas, semicolons, tabs, slashes and hyphens.
- **FR-021**: Find Next MUST search again with the last values, from the day after the shown day; without an earlier
  name it MUST open Find… instead.
- **FR-022**: An empty name MUST give "Please enter a name to search for." and no match "'{name}' not found.", both
  titled "Find"; the shown day stays.
- **FR-023**: One commands menu MUST open from a ⋯ button at the top-right of the "Today is" section (tooltip "More
  commands") and from a right-click anywhere in the window, with Today and Change Date…; Find… and Find Next; Theme ▸;
  and About Anniversary…, in groups separated by lines.
- **FR-024**: Ctrl+F MUST open Find… and F3 MUST run Find Next; on macOS ⌘F and ⌘G MUST do the same. The commands menu
  MUST show these shortcuts next to Find… and Find Next.
- **FR-025**: On macOS the menu bar MUST also offer Go (Yesterday, Tomorrow, Today, Change Date…), Find (Find… ⌘F,
  Find Next ⌘G), View ▸ Appearance and, in the app menu, About Anniversary; Quit (⌘Q) comes from macOS. Windows and
  Linux have no menu bar, so everything is in the ⋯ menu.
- **FR-026**: About Anniversary… MUST show the app icon, "Anniversary Application", the version ("Version 5.0"), "This
  program displays names, birthdates, and anniversaries for a specified day." and the copyright line, with OK.
- **FR-027**: Messages MUST appear in a small window over the main window, with a title, the text and an OK button that
  Enter and Esc also press.
- **FR-028**: Theme ▸ System / Light / Dark (macOS also View ▸ Appearance) MUST switch all open windows, dialogs and
  menus at once, and both menus MUST check the current choice.
- **FR-029**: System MUST follow the OS light or dark setting while the app runs, including scheduled changes (macOS
  Auto, Windows' app mode including PowerToys Light Switch, the GNOME or KDE colour scheme), without a restart.
- **FR-030**: The theme MUST be remembered per user in `usersettings.json` in the app-data folder and applied at the
  next start before the window appears. System is the default; a missing or unreadable file gives System without a
  message, and if the file can't be saved the choice still applies until the app closes.
- **FR-031**: Each desktop target, Windows (x64), macOS (Apple Silicon and Intel) and Linux (x64), MUST get one
  self-contained executable named `Anniversary` (`Anniversary.exe` on Windows, as in v4) that starts without
  installing anything else.
- **FR-032**: The app MUST have its own icon, v4's tear-off calendar redrawn in full colour so it is sharp at every
  size, on the executable, the windows and the About dialog.
- **FR-033**: The repository README MUST explain how to build, test, run and publish the app, where it looks for its
  files, and how to bring v4's data along.

### Key Entities

- **Person**: a name, plus an optional name day (month and day), an optional birth date (year `9999` means unknown)
  and optional anniversaries (a date and a description each). The order in the file is the order on screen.
- **People data file** (`Anniversary.Data.json`): the list of people in v4's format; the built-in one is empty.
- **Calendar** (`Anniversary.Calendar.Czech.json`): the text for each day of the year, keyed by month and day, often
  several names; v4's Czech calendar is built in.
- **Configuration** (`appsettings.json`): v4's four options (the calendar file, the data file, whether to show years,
  the number of days ahead); edited by hand and never written by the app.
- **User settings** (`usersettings.json`): the chosen theme, in the app-data folder; written only by the app.
- **Shown day**: the date the main window shows; it starts as today and changes with Yesterday, Tomorrow, Today,
  Change Date and Find.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With v4's data file in the app-data folder, 9 October 2026 shows "Štefan/Sára", empty Namedays and
  Birthdays and Anniversaries, and the two Next 7 days lines of the mocks; without the file, the people boxes are empty
  and the hint shows (acceptance criterion 3 in [docs/spec.md](../../docs/spec.md) §12).
- **SC-002**: The same app, with the same commands and data, runs on Windows, macOS and Linux, and publishing succeeds
  for all four targets, each a single file that starts without installing anything else (criteria 2 and 9).
- **SC-003**: Resizing matches v4 in every direction: only "Next N days" changes height, and nothing is cut off at the
  minimum size (criterion 5).
- **SC-004**: Yesterday, Tomorrow, Today, Change Date (from the menu and by clicking the date), Find, Find Next, About
  and Quit behave as specified, and Ctrl+F and F3 (⌘F and ⌘G on macOS) work (criterion 4).
- **SC-005**: Light and Dark apply at once and survive a restart; with System, changing the OS setting while the app
  runs re-themes it without a restart, and the check mark always shows the current choice (criterion 7).
- **SC-006**: No missing or broken file stops the app: each problem gives its specified message after the window
  opens, and files placed next to the executable or in the app-data folder override the built-in ones (criterion 6).
- **SC-007**: The published app contains no personal data: on a fresh install it shows no people.
- **SC-008**: 38 automated tests cover the day, Find, loading and theme-setting rules; at the move to GitHub all 78
  tests (with those of 002 and 003) passed and the build had no warnings (criterion 1).

## Assumptions

- Phase 1 is the desktop (Windows, macOS and Linux); iOS (phase 2) and Android (phase 3, optional) come later and must
  fit in without restructuring.
- Recorded separately, although they came the same evening: [002](../002-people-editor/spec.md) (Edit People… and
  saving the people file; the empty-list hint points to it) and [003](../003-calendar-chooser/spec.md) (choosing a
  calendar and the other nine name-day calendars), both specified and built together with this feature, and
  [004](../004-macos-native-look/spec.md) (the macOS-specific fonts, line heights, margins and corners), made that
  night after the first macOS run.
- Out of scope for phase 1: installers and MSIX, a macOS `.app` bundle, signing and notarization, trimming and AOT,
  translated UI texts, and choosing the calendar by the OS locale. Later features added packages
  ([010](../010-install-packages/spec.md)) and the default calendar by region
  ([006](../006-holiday-calendar-rules/spec.md)).
- v4 isn't part of this repository; its behaviour and coding style are the reference, and its data files are copied
  by hand into the app-data folder or next to the executable.
- `numberOfDaysAhead`, `displayYearsOnBirthdays`, `calendarFile` and `dataFile` stay in a hand-edited
  `appsettings.json`, as in v4; the app only reads it.
- The mocks in [docs/screens/](../../docs/screens/mockup.html) are approximate HTML drawings, not real renders; the
  sizes in the system spec win.
- CI came later ([005](../005-ci-pipeline/spec.md)); at the time, builds and tests ran on the maintainer's computers.
