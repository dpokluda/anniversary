# Quickstart: Cross-Platform Rewrite

How to check the feature as built on 2026-10-09 (first committed as `67a4092`). Expected behaviour: [spec.md](spec.md);
exact texts and layout: [contracts/main-window.md](contracts/main-window.md) and
[contracts/commands-and-dialogs.md](contracts/commands-and-dialogs.md); file formats:
[contracts/people-data-file.md](contracts/people-data-file.md), [contracts/calendar-file.md](contracts/calendar-file.md)
and [contracts/settings-files.md](contracts/settings-files.md); load results: [data-model.md](data-model.md).

Later features changed parts of this (see the spec's Later changes). With today's code, the two options that were in
`appsettings.json` are set in `usersettings.json` instead (012), and the default calendar depends on the OS region
(006).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- For real data, v4's `Anniversary.Data.json` (optionally v4's `appsettings.json`).
- The app-data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux). For a fresh start, move it aside.
- For the platform checks, a Windows PC, a Mac and a Linux desktop (the maintainer used Fedora).

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 78 passing tests, 38 of them this feature's (`DayTests` 11,
`FindTests` 13, `LoadTests` 9, `UserSettingsTests` 5; the others are 002's and 003's). The list is in
[tasks.md](tasks.md); later features added and changed tests, so today's counts differ.

## Scenario 1: the day at a glance (User Story 1; docs/spec.md §12, criteria 2, 3 and 6)

1. Start the app with no app-data folder. **Expected**: the window shows today, the Czech calendar's names for the
   day, empty Namedays, the hint "No people yet. Use ⋯ ▸ Edit People… to add some." and "Next 7 days" with calendar
   names only; keyboard focus is on Quit.
2. Copy v4's `Anniversary.Data.json` into the app-data folder, restart, and go to 9 October 2026 (Change Date, scenario
   3). **Expected**: "Štefan/Sára" under the date, empty Namedays and Birthdays and Anniversaries, and Next 7 days with
   a people line and then "Marina, Andrej, Marcel, Renáta, Agáta, Tereza, Havel".
3. Go to a day with a birthday and an anniversary from the v4 file. **Expected**: "{Name} ({age})" and "{Name} -
   {description} ({years})"; a birth year of `9999` shows just the name.
4. Put an `appsettings.json` with `"numberOfDaysAhead": 14` and `"displayYearsOnBirthdays": false` into the app-data
   folder and restart. **Expected**: "Next 14 days", and no years.
5. Break the data file (for example delete its last `}`) and restart. **Expected**: the window opens, then a message
   titled **Data Error** says "Error loading data: …"; no people. Do the same with `appsettings.json`. **Expected**:
   **Configuration Error**, "Error loading configuration: …" and "Using default settings."; Next 7 days again.
6. Make `appsettings.json` valid again with `"calendarFile": "Missing.json"` and restart. **Expected**: "Calendar
   file not found: Missing.json" and no calendar names.
7. Select text in the three boxes and copy it. **Expected**: it can be selected and copied.

## Scenario 2: resizing (User Story 1; criterion 5)

1. Drag the window wider and taller, and maximise it. **Expected**: every section stretches to the width, the date and
   names stay centred, ⋯ stays at the top-right, only "Next 7 days" gets taller, Yesterday and Tomorrow stay at the
   bottom-left and Quit at the bottom-right.
2. Shrink it to the minimum (280 × 420). **Expected**: nothing is cut off; long texts wrap, and a box with too much
   text scrolls on its own.
3. Restart. **Expected**: the window opens centred at its start-up size again.

## Scenario 3: browsing (User Story 2; criterion 4)

1. Click **Tomorrow** and **Yesterday**. **Expected**: the shown day moves one day forward and back.
2. Click the date (hand cursor, tooltip "Click to change the date"). **Expected**: **Change Date** opens on the shown
   day, about v4's size; its calendar drops down below the dialog. Pick a day and press **OK**: it is shown. Open it
   again and press Esc: nothing changes. Clear the picker and press OK: the shown day stays.
3. Choose **⋯ ▸ Change Date…**, then **⋯ ▸ Today**. **Expected**: the same dialog; Today shows today.

## Scenario 4: Find (User Story 3; criterion 4)

1. Press Ctrl+F (macOS ⌘F) and search for "sára" with no options while a day in 2026 is shown. **Expected**: 9 October
   2026, if no earlier day matches.
2. Press F3 (macOS ⌘G). **Expected**: the next matching day, searched from the day after the shown one.
3. Search with **Whole words only** for part of a name. **Expected**: no match where the text is only part of a word.
4. Search for a person's name with **Search calendar only**. **Expected**: people are ignored.
5. Search for a name that doesn't exist. **Expected**: "'{name}' not found." (title **Find**). Search with an empty
   name. **Expected**: "Please enter a name to search for."
6. Reopen Find. **Expected**: the last name and options, with the name selected.

## Scenario 5: commands on each OS (User Story 4; criteria 2 and 4)

1. Click **⋯**. **Expected**: the menu opens below it, aligned right: Today, Change Date…, Find… Ctrl+F, Find Next
   F3, Theme ▸, About Anniversary…, with separator lines that start where the text starts.
2. Right-click anywhere in the window. **Expected**: the same menu.
3. Choose **About Anniversary…**. **Expected**: the icon, "Anniversary Application", "Version 5.0", the description
   and the copyright line; Enter or Esc closes it.
4. On macOS: the menu bar has **Go** (Yesterday, Tomorrow, Today, Change Date…), **Find** (Find… ⌘F, Find Next ⌘G),
   **View ▸ Appearance** and **About Anniversary** in the app menu; each works, and ⌘Q quits.
5. Press **Quit**. **Expected**: the app closes.

## Scenario 6: theme (User Story 5; criterion 7)

1. Choose **⋯ ▸ Theme ▸ Dark** with a dialog or menu open. **Expected**: everything turns dark at once; Dark is
   checked. Restart. **Expected**: the app opens dark, and `usersettings.json` in the app-data folder holds
   `"theme": "Dark"`.
2. Choose **System** and switch the OS between light and dark (Windows: Settings ▸ Personalization ▸ Colors ▸ app
   mode, or PowerToys Light Switch; macOS: Appearance, including Auto; Linux: the GNOME or KDE colour scheme).
   **Expected**: the app follows while it runs.
3. On macOS, use **View ▸ Appearance**. **Expected**: the same, and the check marks in both menus agree.
4. Break `usersettings.json` and restart. **Expected**: System, without a message.

## Scenario 7: publishing (User Story 6; criterion 9)

```bash
dotnet publish Anniversary.Desktop -c Release -r win-x64 --self-contained -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64
```

1. Publish for `win-x64`, `osx-arm64`, `osx-x64` and `linux-x64` (all can be built on Windows). **Expected**: each
   folder holds one executable, `Anniversary.exe` or `Anniversary`, without `.pdb` or `.xml` files (at the time 003's
   `THIRD-PARTY-NOTICES.md` sat next to it).
2. Copy a folder to a computer with that OS and no .NET; on macOS and Linux run `chmod +x Anniversary` (and, for a
   download on macOS, remove the quarantine flag as the README said at the time). **Expected**: the app starts, with
   its icon on the Windows executable and the windows.
3. Follow `README.md`. **Expected**: it covers building, testing, running, publishing, where the files are found and
   moving from v4.

## Platform checks

- **Windows**: v4's size and native font sizes, light and dark (checked live during the build).
- **macOS**: the menu bar, ⌘F and ⌘G, the theme with Auto, and data in `~/Library/Application Support/Anniversary`.
  The first run on 2026-10-09 worked but looked heavier than Windows; the macOS-only look values are
  [004](../004-macos-native-look/spec.md)'s.
- **Linux**: no menu bar, so everything goes through ⋯; the theme follows GNOME's style; data in
  `~/.config/Anniversary`. The maintainer checked macOS and Fedora on 2026-10-10 with no code changes. `osx-x64` hasn't
  been run on an Intel Mac.
