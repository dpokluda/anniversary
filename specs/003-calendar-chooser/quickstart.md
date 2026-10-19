# Quickstart: Calendar Chooser

How to check the feature as built on 2026-10-09. Expected behaviour: [spec.md](spec.md); the menu and its messages:
[contracts/calendar-menu.md](contracts/calendar-menu.md); the file format and the shipped calendars:
[contracts/calendar-file.md](contracts/calendar-file.md); the generator:
[contracts/build-calendars.md](contracts/build-calendars.md); states: [data-model.md](data-model.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository. To see the feature exactly as it was, check out `67a4092` (the move to
  GitHub); later features changed the default calendar, the menu name and the order (see the spec's Later changes).
- For scenario 2, step 4: PowerShell 7 (`pwsh`) and internet access.
- The app-data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux). For a "fresh install", move it aside first.

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the move to GitHub (`67a4092`): a build without warnings and 78 passing tests, 10 of them in
`Anniversary.Tests/CalendarTests.cs` (the list is in [tasks.md](tasks.md)).

## Scenario 1: choose a calendar (User Story 1)

1. Start with no app-data folder. Open **⋯ ▸ Nameday Calendar** (or right-click the window). **Expected**: ten radio
   items sorted by display name (as in the [mock](../../docs/screens/main-window-calendar-menu.light.preview.png)), with
   **Czechia (Česko)** checked; Change Date… to 9 October 2026 shows "Štefan/Sára" under the date.
2. Choose **Slovakia (Slovensko)**. **Expected**: the date shows "Dionýz" at once, the check moves, the people's
   namedays, birthdays and anniversaries stay, and `usersettings.json` now has
   `"calendar": "Anniversary.Calendar.Slovak.json"`.
3. Choose **Hungary (Magyarország)**. **Expected**: 9 October shows "Dénes, János". Then **Find…** "István" with *Whole
   words only* and *Search calendar only*, without *From current date*. **Expected**: 20 August 2026.
4. Quit and start the app again. **Expected**: Hungary is checked and its names are shown.
5. Quit, set `"calendar"` in `usersettings.json` to `"Anniversary.Calendar.Removed.json"` and start. **Expected**: the
   Czech calendar (or the one `calendarFile` names) and no warning.

## Scenario 2: the shipped calendars (User Story 2)

1. Open the menu. **Expected**: Austria (Österreich), Bulgaria (България), Czechia (Česko), France, Hungary
   (Magyarország), Italy (Italia), Latvia (Latvija), Poland (Polska), Russia (Россия), Slovakia (Slovensko).
2. Spot-check well-known name days, as `calendars/README.md` lists them: Slovakia on 5 July shows "Cyril, Metod",
   Hungary on 20 August includes István, Poland on 24 December includes Adam and Ewa, France on 6 December shows
   Nicolas. **Expected**: all match. Open any file in `calendars/`: it has `name`, `locale`, `source` and `license`.
3. Publish one target, for example:

   ```bash
   dotnet publish Anniversary.Desktop -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true \
     -p:IncludeNativeLibrariesForSelfExtract=true -o publish/osx-arm64
   ```

   **Expected**: `THIRD-PARTY-NOTICES.md` lies next to the executable, with the radoone/nameday-mcp MIT license and the
   Latvian CC0 dedication.
4. Run `pwsh -File calendars/Build-Calendars.ps1` (on `67a4092`). **Expected**: one line per calendar (for example
   `Slovak       362 days  Slovakia (Slovensko)`) and no change to any file in `calendars/` (`git status` is clean).
   Today's script also writes the calendars of [006](../006-holiday-calendar-rules/spec.md) and
   [008](../008-international-days/spec.md) and reproduces today's twelve files.

## Scenario 3: your own calendar file (User Story 3; docs/spec.md §12, criterion 11)

1. While the app runs, put `Anniversary.Calendar.Test.json` with
   `{ "name": "Test", "calendar": { "10-09": "Test name" } }` into the app-data folder, then open the ⋯ menu again.
   **Expected**: **Test** is listed among the calendars.
2. Choose it. **Expected**: 9 October shows "Test name" at once. Restart. **Expected**: Test is still checked.
3. Quit, delete the file and start. **Expected**: the Czech calendar (`calendarFile`), without errors.
4. Put `Anniversary.Calendar.Klingon.json` with only `{ "calendar": { "01-01": "Worf" } }` into the app-data folder.
   **Expected**: the menu lists **Klingon**.
5. Put an `Anniversary.Calendar.Czech.json` with `"name": "My Czech"` into the app-data folder, and another with
   `"name": "Exe Czech"` next to the executable. **Expected**: the menu has one Czech entry, **Exe Czech**; choosing
   it (or restarting with Czech active) shows the names of the file next to the executable.
6. Put a file with invalid JSON, `Anniversary.Calendar.Broken.json`, into the app-data folder. **Expected**: it isn't
   listed.

## Platform checks

- **Windows and Linux**: the ⋯ menu and the right-click menu show **Nameday Calendar ▸** between Edit People… (after a
  separator) and Theme ▸; the radio check follows the choice.
- **macOS**: the menu bar's **View** menu shows **Nameday Calendar ▸** above Appearance with the same list; choosing
  there works as in the ⋯ menu, and the check follows. The list is built at start-up and rebuilt when the ⋯ menu opens
  or a calendar is chosen.
- At the move to GitHub the handover still listed the macOS menu bar, including View ▸ Nameday Calendar, among the items
  not yet checked on a Mac, and the Linux check list asked whether the Nameday Calendar submenu opens.
