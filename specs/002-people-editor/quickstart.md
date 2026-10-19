# Quickstart: People Editor

How to check the feature as built on 2026-10-09. Expected behaviour: [spec.md](spec.md); exact texts:
[contracts/editor-ui.md](contracts/editor-ui.md); the written file:
[contracts/people-file-write.md](contracts/people-file-write.md); rows and rules: [data-model.md](data-model.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- The app-data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux). For a "fresh install", move it aside first.
- For scenario 4, a data file with several people (for example the one scenario 1 writes, with a few more people).

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 78 passing tests (all of phase 1), 30 of them in
`Anniversary.Tests/SaveTests.cs` and `Anniversary.Tests/EditorRowTests.cs` (the list is in [tasks.md](tasks.md)).

## Scenario 1: add people on a fresh install (User Story 1; docs/spec.md §12, criterion 10)

1. Start with no app-data folder. **Expected**: the Birthdays and Anniversaries box says "No people yet. Use ⋯ ▸ Edit
   People… to add some."
2. Choose **⋯ ▸ Edit People…** (or right-click; macOS: **File ▸ Edit People…**). **Expected**: a window titled **Edit
   People** with an empty list; the footer says "0 people · " and the full path of `Anniversary.Data.json` in the
   app-data folder.
3. Click **+ Add**. **Expected**: a new row at the end, selected, with its Name cell ready for typing; the footer counts
   it.
4. Type a name, pick a nameday (only the month and day show, in your regional format, for example `6/24` in US English)
   and a birthday, and click **Save**. **Expected**: the editor closes; `Anniversary.Data.json` is in the app-data
   folder, in the format of [contracts/people-file-write.md](contracts/people-file-write.md); the hint is gone, and the
   person shows on their days at once (use Change Date… to go there).
5. Quit and start the app again. **Expected**: the same people.
6. Open the editor, change a name, select another row and click **− Remove**, then **Save**. **Expected**: no question;
   the file has the new name and no longer has the removed person; the others keep their order.

## Scenario 2: anniversaries (User Story 2; criterion 10)

1. In the editor, click **Edit…** in a person's Anniversaries cell. **Expected**: a dialog titled **Anniversaries –
   {Name}**, which can't be resized.
2. Add two anniversaries with a date and a description each, and click **OK**. **Expected**: the cell shows both as
   "{description} ({date})", ordered by date and separated by ", ".
3. Open the dialog again and give the second anniversary the date of the first. **Expected**: the date cell is marked,
   OK is disabled, and the dialog says "Every anniversary needs a date and a name, and each date can be used only once."
   Clear a description instead. **Expected**: the same. Click **Cancel**. **Expected**: the cell is unchanged.
4. Click **Save** in the editor. **Expected**: the file has the anniversaries under the person, as
   `"yyyy-MM-dd": "description"` ordered by date, and on those days the main window shows "{Name} - {description}
   ({years})".
5. Look at a person without anniversaries. **Expected**: a muted *none*.

## Scenario 3: mistakes, Cancel and a failed save (User Story 3; criterion 10)

1. Clear a person's name. **Expected**: the cell is marked as an error and Save is disabled; typing a name enables it
   again.
2. Quit, change a `namedate` in the data file to `"1-5"` and start again; open the editor. **Expected**: that Nameday
   cell is empty and marked, and Save is disabled until a nameday is picked.
3. Add, change and remove people, then click **Cancel** (or press Esc). **Expected**: no question; the main window and
   the data file are unchanged, and the editor shows the old list when opened again.
4. While editing a cell, press Enter. **Expected**: the edit is committed and the editor stays open.
5. Make the folder that holds the data file read-only for your user (on macOS or Linux: `chmod u-w` on the folder),
   change something and click **Save**. **Expected**: "Could not save the data file: …" (title **Edit People**); the
   editor stays open with the changes, and the file is as before. Restore the folder (`chmod u+w`).

## Scenario 4: filter and sort (User Story 4)

1. With several people, type part of a name into **Filter by name**. **Expected**: only the matching people show, in any
   letter case; the footer still counts everyone.
2. Click the Name, Nameday, Birthday and Anniversaries headers. **Expected**: the list sorts by that column; when the
   editor opens it is sorted by Name.
3. With a filter typed, click **+ Add**. **Expected**: the filter is cleared and the new row shows.
4. Sort by another column and click **Save**. **Expected**: the file keeps the earlier order with the new person at the
   end, and the main window lists people in that order.

## Platform checks

- **Windows**: phase 1 was built, tested and run live on Windows (the handover at the move to GitHub); scenarios 1–4.
- **macOS**: **File ▸ Edit People…** in the menu bar opens the editor, and Save writes to
  `~/Library/Application Support/Anniversary`. At the move to GitHub these were still on the handover's "Still open on
  macOS" list.
- **Linux**: the ⋯ menu only (no menu bar); "Edit People saves there, and the main window updates" (item 5 of the
  handover's Linux check list, for `~/.config/Anniversary`).
- **Theme**: with Theme set to Dark, the editor and the Anniversaries dialog are dark too (compare with the
  [dark mock](../../docs/screens/people-editor.dark.preview.png)).
- At the time, the maintainer reported that the app "works fine in both MacOS and Fedora Linux" (session 2026-10-10);
  there is no record of each editor check.
