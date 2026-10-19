# Quickstart: Single Settings File

How to check the feature as built on 2026-10-11. Expected behaviour: [spec.md](spec.md); the file and its keys:
[contracts/usersettings-file.md](contracts/usersettings-file.md); load states: [data-model.md](data-model.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- A text editor.
- The app-data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux). For a first start, move it aside (and back afterwards).
- A data file with a few birthdays, so that hiding the years shows (any v4 `Anniversary.Data.json` will do).

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 205 tests, of which 202 pass and the 3 Windows-only registry tests
skip on macOS and Linux (all 205 run on Windows). CI passed for the commit on Windows, macOS and Ubuntu. The settings
tests are listed in [tasks.md](tasks.md).

## Scenario 1: the two options by hand (User Story 1; docs/spec.md §12, criterion 6)

1. Quit the app. In `usersettings.json` in the app-data folder, set `"numberOfDaysAhead": 14` and
   `"displayYearsOnBirthdays": false`.
2. Start the app. **Expected**: the last section is titled "Next 14 days" and covers the next 14 days; birthdays and
   anniversaries are listed without the number of years.
3. Quit, add a line with a `//` comment and leave a comma after the last value; start the app again. **Expected**: the
   same, and no message.
4. Choose **⋯ ▸ Theme ▸ Dark** and another calendar in **⋯ ▸ Calendar** (macOS: **View ▸ Appearance ▸ Dark** and
   **View ▸ Calendar**). **Expected**: `usersettings.json` has the new theme and calendar and still
   `"numberOfDaysAhead": 14` and `"displayYearsOnBirthdays": false`, rewritten in the app's layout (the comment is
   gone).
5. Quit, remove the two keys and start the app. **Expected**: "Next 7 days" and the years, as in v4.

## Scenario 2: one file (User Story 2)

1. Quit the app, move the app-data folder aside and start the app. **Expected**: the folder and `usersettings.json`
   are created at once, with all five keys as in [the contract](contracts/usersettings-file.md#keys); the calendar is
   the one for the OS region.
2. Choose a theme, a calendar, and a data file with **⋯ ▸ Data File ▸ Open…** (macOS: **File ▸ Open Data File…**).
   **Expected**: after each choice the file holds the new value and all the other keys.
3. Quit and put an `appsettings.json` with
   `{ "numberOfDaysAhead": 3, "calendarFile": "Anniversary.Calendar.Polish.json" }` into the app-data folder; start the
   app. **Expected**: no message, the number of days from `usersettings.json` and the calendar chosen in the app:
   `appsettings.json` is ignored.
4. Quit, set `"dataFile": "Family.Data.json"` and put a data file with that name into the app-data folder; start the
   app. **Expected**: its people are shown.
5. Quit, set `"dataFile": "Missing.Data.json"` (no such file) and start the app. **Expected**: after the window opens,
   "Data file not found: Missing.Data.json" (title **Data Error**) and no people.

## Scenario 3: a typo (User Story 3; criterion 6)

1. Quit the app, copy `usersettings.json` somewhere else, and change `"numberOfDaysAhead": 14` to
   `"numberOfDaysAhead": "14"` (or set any number in quotes).
2. Start the app. **Expected**: after the window opens, a message titled **Configuration Error**: "Error loading
   configuration: {path of usersettings.json}: …" with a reason that names `numberOfDaysAhead`, then "Using default
   settings."; the app shows "Next 7 days", the years, the System theme, the calendar for the OS region and the people
   from the default data file.
3. Choose another theme. **Expected**: it changes at once. Quit. **Expected**: `usersettings.json` is exactly as in
   step 1 (compare it with the copy).
4. Fix the typo and start the app. **Expected**: no message; the theme, calendar, data file and both options from the
   file apply, and menu choices are saved again.
5. Repeat with the last `}` deleted. **Expected**: the same message, with the JSON error as the reason.

## Platform checks

- **Windows, macOS and Linux**: the file is in the app-data folder listed above; scenarios 1–3 behave the same.
- **macOS**: the theme and the calendar are in the **View** menu (Appearance, Calendar); the Data File commands are in
  the **File** menu.
- At the time, CI built the commit with warnings as errors and ran the tests on Windows, macOS and Ubuntu. The session
  excerpt ends before the implementation report, so there is no record of these manual checks at the time.
