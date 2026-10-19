# Quickstart: User Guide

How to check the feature as built on 2026-10-10. Expected behaviour: [spec.md](spec.md); the guide's sections and
texts: [contracts/user-guide-outline.md](contracts/user-guide-outline.md); where the guide goes:
[contracts/publish-folder.md](contracts/publish-folder.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- For the checks on each OS: a Windows, a macOS and a Linux computer, and the downloads of a CI run that passed
  (downloading them needs a GitHub sign-in), or folders published as below.
- A plain text editor.
- The app-data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux).

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: the same 189 passing tests as before the feature; no code changed.

## Scenario 1: the guide is in every download (User Story 1; FR-001)

1. Publish for the OS you're on, for example on an Apple Silicon Mac, and compare the copy with the source:

   ```bash
   dotnet publish Anniversary.Desktop -c Release -r osx-arm64 --self-contained \
     -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/osx-arm64
   ls publish/osx-arm64
   cmp Anniversary.Desktop/README.md publish/osx-arm64/README.md
   ```

   **Expected**: the folder holds `Anniversary`, `README.md` and `THIRD-PARTY-NOTICES.md`, and `cmp` reports no
   difference.
2. Download the six archives of a CI run that passed and list them. **Expected**: each `Anniversary-{rid}.zip` or
   `Anniversary-{rid}.tar.gz` holds a `{rid}` folder with the executable, `README.md` and `THIRD-PARTY-NOTICES.md`.

## Scenario 2: starting the app with the guide (User Story 1; FR-005)

On each OS, with a fresh download and no other help, follow only "Starting it":

1. **Windows**: double-click `Anniversary.exe`. **Expected**: if Windows says "Windows protected your PC", **More
   info** and **Run anyway** start the app.
2. **macOS**: pick `osx-arm64` (Apple Silicon) or `osx-x64` (Intel), double-click `Anniversary`, close the warning,
   click **Open Anyway** in **System Settings ▸ Privacy & Security** and double-click it again. **Expected**: the app
   starts, with a Terminal window next to it. Or, in Terminal in that folder, run
   `xattr -d com.apple.quarantine Anniversary`, then `./Anniversary`.
3. **Linux**: in a terminal in the extracted folder, `./Anniversary`. **Expected**: the app starts on a desktop
   distribution without installing anything else.

## Scenario 3: reading it as plain text (User Story 1; FR-003)

Open the downloaded `README.md` in a plain text editor. **Expected**: the headings, lists and the JSON example read
naturally; there are no tables, images or links to decode.

## Scenario 4: the guide matches the app (User Stories 2 and 3; FR-004, FR-006–FR-008, FR-012)

Run the app and read the opening paragraph, "Using it", "Your people" and "Calendars" next to it:

1. Open the **⋯** menu, then right-click the window. **Expected**: both show Today, Change Date…, Find…, Find Next,
   Edit People…, Data File ▸ (Open…, Save As…, Show in Folder), Calendar ▸, Theme ▸ and About Anniversary…: every
   command the guide lists.
2. Try the shortcuts. **Expected**: Ctrl+F, F3, Ctrl+O and Ctrl+Shift+S on Windows and Linux, and ⌘F, ⌘G, ⌘O and ⇧⌘S
   on macOS, do what the guide says.
3. On macOS, open the menu bar. **Expected**: **File ▸ Open Data File…**, **View ▸ Calendar** and **View ▸
   Appearance** exist.
4. Open **Calendar ▸**. **Expected**: the ten name-day countries the guide lists, United States and International
   Days; with no calendar chosen, the app started with the one for the OS region (Czech in Czechia, the United States
   where no calendar matches).
5. Follow "Your people": add a person in **Edit People…** and click **Save**; **Data File ▸ Show in Folder** opens the
   folder with the data file; on a second computer or user account, **Data File ▸ Open…** on a copy of that file shows
   the same people. **Expected**: each step works as the guide says (the behaviour itself is checked by
   [007's quickstart](../007-data-file-sharing/quickstart.md)).

## Scenario 5: files, settings and removing (User Story 4; FR-009–FR-011)

1. With the app closed, create `appsettings.json` in the app-data folder with the guide's example,
   `{ "numberOfDaysAhead": 14, "displayYearsOnBirthdays": false }`, and start the app. **Expected**: the caption reads
   "Next 14 days", and birthdays and anniversaries show no years. Delete the file afterwards.
2. Delete the extracted app folder. **Expected**: the app-data folder, and whatever the app wrote there (for example
   `Anniversary.Data.json` after a Save in the editor), is still there.
3. Read "License". **Expected**: the MIT License, Copyright (c) David Pokluda, `THIRD-PARTY-NOTICES.md` next to the
   app, and https://github.com/dpokluda/anniversary.

## Scenario 6: the developer README (FR-002)

Open `README.md` at the repository root. **Expected**: the first paragraph says that this README is for developers and
links to `Anniversary.Desktop/README.md`, "which comes with every download"; "Publish" and "Downloading a build" list
`README.md` next to the executable; the Projects table names the user guide under `Anniversary.Desktop`.

## At the time

- A local macOS build was checked as in scenario 1, step 1, and the 189 tests passed.
- The maintainer reviewed the guide in the editor and approved it ("Looks good."). No record says scenarios 2, 4 and 5
  were then run on each OS; the macOS first-start steps were reworded by
  [010](../010-install-packages/spec.md) after the maintainer had tried the packaged app.
