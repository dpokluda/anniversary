# Quickstart: Holiday Calendar Rules

How to check the feature as built on 2026-10-10. Expected behaviour: [spec.md](spec.md); the keys:
[contracts/calendar-rule-keys.md](contracts/calendar-rule-keys.md); the default:
[contracts/default-calendar.md](contracts/default-calendar.md); the shipped entries:
[contracts/holiday-calendars.md](contracts/holiday-calendars.md); states: [data-model.md](data-model.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- A way to change the OS region: the Regional format on Windows, Language & Region on macOS; on macOS and Linux,
  starting the app with `LC_ALL` set (for example `LC_ALL=cs_CZ.UTF-8`) is enough.
- A "fresh install" for scenario 2: move the app-data folder aside (`%APPDATA%\Anniversary` on Windows,
  `~/Library/Application Support/Anniversary` on macOS, `~/.config/Anniversary` on Linux), so that no
  `usersettings.json` names a calendar and no `appsettings.json` names a `calendarFile`.
- For the optional script check: PowerShell 7 (`pwsh`) and internet access (the script downloads the pinned name-day
  sources).

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 165 passing tests, among them the 75 in
`Anniversary.Tests/CalendarRuleTests.cs`, 10 new ones in `Anniversary.Tests/CalendarTests.cs` and the 2 of the separate
date-range fix (the list is in [tasks.md](tasks.md)).

Optional: in `calendars/`, run `pwsh -File Build-Calendars.ps1`. **Expected**: one line per calendar, `US` with
30 entries and `Czech` with 368, and `git status` shows no changed calendar files.

## Scenario 1: the US calendar (User Story 1; docs/spec.md §12, criterion 12)

1. Open **⋯ ▸ Nameday Calendar** (macOS: **View ▸ Nameday Calendar**). **Expected**: 11 calendars, **United States**
   last. Choose it.
2. Choose **Change Date…** and go to 26 November 2026. **Expected**: the "Today is" line shows *Thanksgiving*; on
   27 November, *Black Friday*.
3. Go to 5 April 2026, 3 April, 17 February, 25 May and 19 January 2026. **Expected**: *Easter*, *Good Friday*,
   *Mardi Gras*, *Memorial Day* and *Martin Luther King Jr. Day*.
4. Go to 20 November 2026. **Expected**: the names line of Next 7 days lists Thanksgiving.
5. Go to 4 July 2026, then 3 July. **Expected**: *Independence Day* on Saturday 4 July; nothing on Friday 3 July.
6. On 27 November 2026, choose **Find…**, enter "Thanksgiving" and check **From current date** and **Search calendar
   only**. **Expected**: the app goes to 25 November 2027.

## Scenario 2: the default for the region (User Story 2; criterion 12)

1. With a fresh install, start the app with the region set to Czechia (on macOS or Linux:
   `LC_ALL=cs_CZ.UTF-8 dotnet run --project Anniversary.Desktop`). **Expected**: **Czechia (Česko)** is checked in the
   calendar menu, and 6 April 2026 shows "Vendula, Velikonoční pondělí". No message appears.
2. Start again with the region set to the United States, then to Germany or the United Kingdom. **Expected**: **United
   States** is checked each time.
3. Choose **Slovakia (Slovensko)**, quit, and start with any region. **Expected**: Slovakia stays checked.
4. Move the app-data folder aside again, then put an `appsettings.json` with
   `{ "calendarFile": "Anniversary.Calendar.Czech.json" }` into a new app-data folder, as a copied v4 file would be.
   Start with the region set to the United States. **Expected**: Czech. Remove the file afterwards.
5. On Windows only: set the Regional format to English (United States) with Czechia as "Country or region".
   **Expected**: United States, because the Regional format decides; choosing Czechia (Česko) once is remembered.

## Scenario 3: the Czech Easter holidays (User Story 3; criterion 12)

1. Choose **Czechia (Česko)** and go to 3 April 2026, then 6 April. **Expected**: "Richard, Velký pátek", then
   "Vendula, Velikonoční pondělí".
2. Go to any day in 2027, choose **Find…**, enter "Velikonoční pondělí", check **Search calendar only** and leave
   **Whole words only** and **From current date** unchecked. **Expected**: 29 March 2027.
3. Go to 9 October 2026. **Expected**: "Štefan/Sára", as in v4 (criterion 3).

## Scenario 4: your own calendar file (User Story 4)

1. Save this as `Anniversary.Calendar.Test.json` in the app-data folder:

   ```json
   { "name": "Test", "calendar": { "07-04": "Independence Day", "11-THU-4": "Thanksgiving",
     "05-mon-LAST": "Memorial Day", "easter+1": "Easter Monday", "12-Mon-last+7": "Late",
     "01-Mon-1": "First Monday", "11-Thu-5": "Typo" } }
   ```

2. Open **⋯ ▸ Nameday Calendar** and choose **Test**, which appears without a restart. **Expected**: a message titled
   **Data Error**: "Unknown dates in Anniversary.Calendar.Test.json: 11-Thu-5".
3. Go to 26 November 2026, 25 May 2026, 6 April 2026 and 4 July 2026. **Expected**: Thanksgiving, Memorial Day,
   Easter Monday and Independence Day; nothing on 19 November or 3 December 2026.
4. Go to 4 January 2027. **Expected**: "Late, First Monday": `12-Mon-last+7` from 2026 and `01-Mon-1` from 2027 fall
   on the same day and are joined in file order.
5. Go to 30 December 2026. **Expected**: Next 7 days lists Late and First Monday.
6. Quit and start again. **Expected**: Test is still chosen, and the same message appears after the window opens.
7. Delete the file and start again. **Expected**: no error; the calendar is the default from scenario 2.

## Platform checks

- **Windows and Linux**: **⋯ ▸ Nameday Calendar** lists United States; the default follows the Regional format
  (Windows) or `LANG`/`LC_ALL` (Linux).
- **macOS**: **View ▸ Nameday Calendar** and the ⋯ menu list United States; the default follows Language & Region.
- At the time, a headless run of the real window confirmed Thanksgiving, the calendar menu and the switch to Czech. On
  the Mac the work was done on (region `en-US`), the app opened on the United States calendar, and with
  `LC_ALL=cs_CZ.UTF-8` on Czech, showing "Vendula, Velikonoční pondělí" on 6 April 2026. No record says the region
  default was then checked on Windows or Linux.
