# Research: Single Settings File

Decisions made while the feature was discussed and built on 2026-10-11. Sources: the session of 2026-10-11 (the
maintainer's request, the two questions about the files and the assistant's answers), the Decisions table in
[docs/plan.md](../../docs/plan.md) ("Settings"), the decisions in
[docs/copilot/handover.md](../../docs/copilot/handover.md), [docs/spec.md](../../docs/spec.md) 5.1, 5.4, 5.5 and C9,
and `git show 9c89279` compared with the state at `26063da`.
The session excerpt ends before the maintainer's final decision and the implementation report; the decision itself is
recorded in docs/plan.md and the handover, so alternatives are listed only where a source names them.

## Keep the two v4 options file-only

- **Decision**: `numberOfDaysAhead` and `displayYearsOnBirthdays` are set only by editing the settings file; there is
  no menu item or settings window for them.
- **Rationale**: The maintainer suggested it ("It could be just the config .json for now") and chose it over menu
  items or a settings window. The options already worked from a JSON file: the assistant's first answer was that
  "nothing needs building", because v5 still read them from `appsettings.json`.
- **Alternatives considered**: a settings screen (the maintainer's "or should it be a screen?") and menu items; not
  chosen (user decision).
- **Source**: session 2026-10-11; docs/plan.md, Decisions, "Settings"; the handover.

## One settings file instead of two

- **Decision**: All settings go into `usersettings.json` in the app-data folder, the file the app already wrote for
  the menu choices; v4's `appsettings.json` is no longer read. `AppConfig` and the built-in `appsettings.json` are
  removed.
- **Rationale**: Two files with overlapping keys confused even the author (user decision, 11 October 2026). Since 001
  the app wrote its own `usersettings.json` for the menu choices (the theme from 001, the calendar from 003, the data
  file from 007) and read v4's `appsettings.json` (`calendarFile`, `dataFile`, `displayYearsOnBirthdays`,
  `numberOfDaysAhead`), which it never wrote. Pointed to `appsettings.json` in the app-data folder, the maintainer
  found `usersettings.json` there and added the options to it; that file ignored them and would have dropped them at
  its next save. He then renamed it to `appsettings.json`: the people still loaded, because `dataFile` is a key in
  both files, but the Czech calendar was gone, because the calendar key there is `calendarFile`, not `calendar`, so
  the app fell back to the calendar for the Mac's region (`en_US`), the US one.
- **Alternatives considered**: keeping both files, the design since 001, when the choice of an app-written
  `usersettings.json` next to the never-written `appsettings.json` was made autonomously while the user was away (the
  plan's "Theme switching" row at the time). In the session, the assistant explained how to split the settings
  between the two files again and offered to say more plainly in the user guide that `appsettings.json` is a separate
  file and `usersettings.json` shouldn't be edited. The maintainer chose one file instead.
- **Source**: session 2026-10-11; docs/plan.md, Decisions, "Settings"; spec 5.1, 5.5 and C9.

## Write every key, and create the file at the first start

- **Decision**: `Save()` writes all five keys, including those at their defaults (`null` for a calendar or data file
  not chosen), and at start-up the app saves the settings when `usersettings.json` doesn't exist.
- **Rationale**: So every setting is in the file, ready to edit by hand with the app closed (spec 5.5, docs/plan.md).
  Before, the file appeared only after the first menu choice, and the two options weren't in it at all.
- **Alternatives considered**: none recorded.
- **Source**: docs/plan.md, Decisions, "Settings"; spec 5.5 and 8; `UserSettingsTests.SaveWritesEveryKey`.

## Edit the file with the app closed

- **Decision**: The app reads the file once, at start-up. The user guide says: quit the app, edit the file in a text
  editor, and start the app again.
- **Rationale**: The values read at start-up stay in memory, and every menu choice writes them back with the new
  choice ("saving a menu choice writes the values it read", spec 5.5), so an edit made while the app runs would be
  replaced at the next menu choice.
- **Alternatives considered**: none recorded.
- **Source**: spec 5.5; `Anniversary.Desktop/README.md`, "Files and settings".

## Never save over a file that couldn't be read

- **Decision**: When the file can't be read (a `JsonException`, `IOException` or `UnauthorizedAccessException`),
  `UserSettings.Load` returns the defaults and keeps the reason in `LoadError`; `Save()` writes nothing while
  `LoadError` is set; `AnniversaryBook.Load` reports it as `Error loading configuration: {path}: {reason}` and
  `Using default settings.`, which the main window shows titled "Configuration Error" after it opens.
- **Rationale**: In the code's words: "The file couldn't be read, perhaps because of a typo made by hand; writing would
  lose what's in it." Nothing in the file, such as the data file's path, is lost, and menu choices apply to the
  current session only (spec 5.5); the handover: "so hand edits aren't lost".
- **Alternatives considered**: the behaviour until then, when only the app wrote the file: a file it couldn't read
  silently gave the defaults ("The file is app-written; if it's unusable, start over with defaults.") and was replaced
  at the next menu choice, which would throw hand edits away.
- **Source**: `UserSettings.cs` before and after the commit; spec 5.4 and 5.5; docs/plan.md; the handover.

## Read the file as the hand-edited `appsettings.json` was read

- **Decision**: `usersettings.json` is read with case-insensitive names, as before, and now also with comments and
  trailing commas allowed.
- **Rationale**: These were the reading rules of `appsettings.json` (spec 5.1 until this feature), the hand-edited file
  whose place `usersettings.json` takes; its test `ConfigAllowsCommentsAndTrailingCommas` became
  `CommentsAndTrailingCommasAreAllowed` in `UserSettingsTests`.
- **Alternatives considered**: none recorded.
- **Source**: spec 5.1 (as of `26063da`) and 5.5; `UserSettings.cs`; the tests.

## The data file and the calendar without `appsettings.json`

- **Decision**: The data file is `dataFile` from the settings, or `Anniversary.Data.json` when it isn't set; a full
  path is used as is, and a file name is looked up as before (spec 5.3), so a name typed by hand works too. The
  calendar is `calendar` from the settings if it is available, otherwise the calendar for the OS region, otherwise the
  US one: the `calendarFile` step between them is gone. v4's `dataFile` thus becomes `dataFile` in `usersettings.json`
  and its `calendarFile` the calendar chosen in the app (spec 5.1).
- **Rationale**: With one file, each setting has one key. Looking up a file name was already how the configured
  `dataFile` worked; it now applies to `dataFile` in `usersettings.json`, which until then was documented only as the
  full path chosen with Open… or Save As….
- **Alternatives considered**: none recorded. A calendar outside the lookup folders, which `calendarFile` could name by
  its full path, can no longer be chosen at start-up; `AnniversaryBook.UseCalendar` still accepts a full path, as the
  reworked test `CalendarGivenByAbsolutePathIsListed` shows.
- **Source**: spec 5.1, 5.3 and 5.7; `AnniversaryBook.Load` and `DataFileName`; `LoadTests` and `CalendarTests`.

## One settings object for the book and the main window

- **Decision**: `AnniversaryBook.Load(UserSettings, folders)` keeps the object as `Settings`, and `App` passes the same
  object to the main window, which changes and saves it when a menu choice changes. `Load(string? preferredCalendar,
  params string[] folders)` stays and loads default settings with that calendar, for the tests and the XAML previewer.
- **Rationale**: The book reads the calendar, the data file and the two options from it, and the main window writes the
  theme, the calendar and the data file into it, so the values read at start-up go into every save.
- **Alternatives considered**: none recorded; it replaces `Load(preferredCalendar, preferredDataFile, folders)` and
  `Config`.
- **Source**: spec 6 and 8; `AnniversaryBook.cs`, `App.axaml.cs`, `MainWindow.axaml.cs`.

## Moving from v4

- **Decision**: No import: v4's `appsettings.json` is ignored without a message. The README tells people moving from
  v4 to open their `Anniversary.Data.json` with Data File ▸ Open… (or copy it into the app-data folder), to choose
  Czechia (Česko) in the calendar menu once if their OS region isn't Czechia, and to set `numberOfDaysAhead` and
  `displayYearsOnBirthdays` in `usersettings.json` if they changed them in v4's `appsettings.json`.
- **Rationale**: Each v4 key has its place (spec 5.1), and the data file and the calendar are chosen in the app anyway.
- **Alternatives considered**: none recorded.
- **Source**: `README.md`, "Moving from v4"; spec 5.1 and 10.

## Verification approach

- **Decision**: Tests with real files (`UserSettingsTests`, 8 cases; `LoadTests`, 8 cases; 205 in total), CI on
  Windows, macOS and Ubuntu, and acceptance criterion 6 as the manual check (the hand edit, and a broken file left as
  it is).
- **Rationale**: Everything except the main window's caption and the message dialog can be checked with real files.
- **Alternatives considered**: none recorded. How the change was checked by hand at the time isn't recorded (the
  session excerpt ends before the implementation report); the CI run for the commit passed.
- **Source**: docs/spec.md §9 and §12; the handover (205 tests, of which 202 pass and 3 skip outside Windows); the
  CI run for `9c89279`.
