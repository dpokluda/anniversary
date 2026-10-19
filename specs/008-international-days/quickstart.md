# Quickstart: International Days

How to check the feature as built on 2026-10-10. Expected behaviour: [spec.md](spec.md); the calendar's content:
[contracts/international-calendar-file.md](contracts/international-calendar-file.md); the menu:
[contracts/calendar-menu.md](contracts/calendar-menu.md); order and grouping: [data-model.md](data-model.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- For scenario 3: PowerShell 7 and internet access (the calendar build script downloads the pinned name-day sources).
- The app-data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux). For a "fresh install", move it aside first.

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 189 passing tests, 8 of them new
(`InternationalCalendarHasTheseDays` in `Anniversary.Tests/CalendarRuleTests.cs`).

## Scenario 1: International Days (User Story 1)

1. Choose **⋯ ▸ Calendar ▸ International Days** (macOS: **View ▸ Calendar ▸ International Days**). **Expected**: the
   line under the date shows the day's international days, or nothing; the item is checked; `usersettings.json` has
   `"calendar": "Anniversary.Calendar.International.json"`.
2. Click the date (or choose **⋯ ▸ Change Date…**) and go to each day below. **Expected**: the line under the date
   reads:

   | Day | Line under the date |
   |---|---|
   | 14 March 2026 | Pi Day |
   | 21 March 2026 | World Poetry Day, International Day of Forests |
   | 9 May 2026 | World Migratory Bird Day |
   | 7 August 2026 | International Beer Day |
   | 6 August 2027 | International Beer Day |
   | 10 October 2026 | World Mental Health Day, World Migratory Bird Day |
   | 24 October 2026 | United Nations Day |
   | 12 October 2026 | (empty) |

3. Quit and start the app again. **Expected**: International Days is still the calendar.
4. With the app-data folder moved aside, start the app with the OS region set to Czechia (on Linux, for example
   `LC_ALL=cs_CZ.UTF-8`), then to a region without its own calendar, such as the United States. **Expected**: Czech,
   then United States; never International Days.

## Scenario 2: the Calendar menu (User Story 2; docs/spec.md §12, criterion 11)

1. Open the **⋯** menu, or right-click the window. **Expected**: **Calendar ▸** between the separator after Data File
   and Theme ▸; no "Nameday Calendar".
2. Open **Calendar ▸**. **Expected**: Austria (Österreich), Bulgaria (България), Czechia (Česko), France, Hungary
   (Magyarország), Italy (Italia), Latvia (Latvija), Poland (Polska), Russia (Россия), Slovakia (Slovensko), United
   States, a separator, International Days; the active calendar is checked.
3. On macOS, open **View ▸ Calendar**. **Expected**: the same list with the same separator; **View** also has
   **Appearance ▸**.
4. Put `Anniversary.Calendar.Test.json` with the content `{ "name": "Test", "calendar": { "10-09": "Test" } }` into the
   app-data folder and open the menu again, without a restart. **Expected**: **Test** below the separator, after
   International Days. Choosing it shows "Test" on 9 October and survives a restart; removing the file falls back to
   the default without errors (criterion 11).

## Scenario 3: where each day comes from (User Story 3)

1. Run `pwsh -File calendars/Build-Calendars.ps1`. **Expected**: no error, and the report line
   `International  61 entries  International Days` among the twelve calendars.
2. Run `git status --short calendars`. **Expected**: no calendar `.json` file is listed; all 12 came out the same.
3. In the script, look up a UN day, for example `'03-22' = 'World Water Day' # A/RES/47/193`, and find it on the
   [UN list](https://www.un.org/en/observances/list-days-weeks). **Expected**: the same date and resolution. The fun
   days say `# tradition`.
4. Open `calendars/Anniversary.Calendar.International.json` and `calendars/README.md`. **Expected**: the file has
   `name`, `source` and `license` but no `locale`; the README lists International Days (58 dates + 3 rules) and has
   the section "The International Days calendar".

## Platform checks

- **Windows and Linux**: the ⋯ menu shows **Calendar ▸** with the countries, the separator and International Days.
- **macOS**: **View ▸ Calendar** and the ⋯ menu both show the separator before International Days.
- At the time, the tests ran on the Mac and a headless run of the real menus showed Austria … United States, then the
  separator, then International Days. No record says the menus were then checked by hand on Windows and Linux.
