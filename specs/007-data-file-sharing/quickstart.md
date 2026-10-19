# Quickstart: Data File Sharing

How to check the feature as built on 2026-10-10. Expected behaviour: [spec.md](spec.md); exact texts:
[contracts/data-file-menu.md](contracts/data-file-menu.md); states: [data-model.md](data-model.md); the setting:
[contracts/data-file-setting.md](contracts/data-file-setting.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- A data file to open, for example a v4 `Anniversary.Data.json` or one the app wrote.
- A folder that stands in for a cloud drive (any folder; a OneDrive, iCloud Drive or Dropbox folder for the real
  thing) and, for scenario 2 in full, a second computer with the same cloud drive.
- The app-data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux). For a "fresh install", move it aside first.

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 181 passing tests, 16 of them in
`Anniversary.Tests/DataFileTests.cs` (the list is in [tasks.md](tasks.md)).

## Scenario 1: a family copy (User Story 1; docs/spec.md §12, criterion 13)

1. Start with no app-data folder and put the data file in Documents.
2. Choose **⋯ ▸ Data File ▸ Open…** (Ctrl+O; macOS **File ▸ Open Data File…**, ⌘O) and pick the file.
   **Expected**: the open dialog lists *Anniversary data* and all files; the main window shows the file's people at
   once; `usersettings.json` now has `"dataFile"` with the file's full path.
3. Quit and start the app again. **Expected**: the same people.
4. Add a person in **Edit People…** and click **Save**. **Expected**: the person is in the file in Documents, and no
   `Anniversary.Data.json` appears in the app-data folder.
5. Choose Open… again and pick a calendar file, for example `calendars/Anniversary.Calendar.Czech.json`.
   **Expected**: "Could not open the data file: This isn't an Anniversary data file (it has no "data" list)." (title
   **Open Data File**); the people and the file in use don't change.

## Scenario 2: several computers (User Story 2; criterion 14)

1. On computer A choose **⋯ ▸ Data File ▸ Save As…** (Ctrl+Shift+S; macOS ⇧⌘S) and save into the cloud-drive folder.
   **Expected**: the dialog suggests the current file's name with `.json` and asks before replacing a file; the people
   are written there; Edit People… shows the new path in its footer; `usersettings.json` names the new file.
2. On computer B choose **Open…** and pick the same file. **Expected**: B shows the same people, and B's
   `usersettings.json` has B's own path to the file.
3. On A add a person and save. Once the cloud app has synced, click into B's window. **Expected**: the person appears
   on B without a restart. Without a second computer: edit the file in a text editor while the app runs, save it, and
   switch back to the app.
4. Quit, rename the data file and start the app. **Expected**: after the window opens, "Data file not found: {full
   path}" (title **Data Error**) and no people. Rename the file back, switch to another app and back. **Expected**:
   the people appear.
5. While the app runs, delete the file and switch away and back. **Expected**: the people shown stay.

## Scenario 3: questions before saving (User Story 3; criterion 14)

1. Open **Edit People…**, then change the data file elsewhere (another computer or a text editor). Click **Save**.
   **Expected**: a question titled **Edit People** that names the file, with **Overwrite** and **Cancel**. Press Esc:
   the editor stays open and nothing is saved. Click Save again and choose **Overwrite**: the file is written and the
   main window shows the editor's list.
2. Quit, break the file (for example delete its last `}`) and start the app. **Expected**: "Error loading data: …"
   (title **Data Error**). Open Edit People… and click Save. **Expected**: a question with **Save Anyway** and
   **Cancel**, which suggests cancelling if the file is on a cloud drive that hasn't finished syncing. Enter chooses
   Cancel.
3. Start with the data file missing (as in scenario 2, step 4), open Edit People… and click Save. **Expected**: the
   **Save Anyway** question before the file is created.
4. Save twice in a row with no change elsewhere. **Expected**: no question.

## Scenario 4: Show in Folder (User Story 4)

1. Choose **⋯ ▸ Data File ▸ Show in Folder** (macOS **File ▸ Show Data File in Finder**). **Expected**: Explorer,
   Finder or the Linux file manager opens the folder that holds the data file.
2. With no app-data folder and no file chosen, choose it again. **Expected**: "The data file's folder doesn't exist
   yet:" and the app-data folder's path (title **Data File**).

## Platform checks

- **Windows and Linux**: the ⋯ menu shows **Data File ▸ Open…** (Ctrl+O), **Save As…** (Ctrl+Shift+S) and **Show in
  Folder**; both shortcuts work in the main window; the native open and save dialogs behave as in scenarios 1 and 2.
- **macOS**: the **File** menu shows Edit People…, a separator, **Open Data File…** ⌘O, **Save Data File As…** ⇧⌘S and
  **Show Data File in Finder**; the ⋯ menu shows ⌘O and ⇧⌘S.
- At the time, a headless run of the real windows confirmed the menus and the questions. The native open and save
  dialogs and Show in Finder can't run headless and were left for this manual check (`dotnet run`, then ⌘O, ⇧⌘S and
  File ▸ Show Data File in Finder).
