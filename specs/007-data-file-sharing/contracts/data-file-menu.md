# Contract: Data File commands, dialogs and messages

The user interface this feature added, as built on 2026-10-10 (`Anniversary.Shared/Views/MainWindow.axaml`,
`MainWindow.axaml.cs`, `PeopleEditorWindow.axaml.cs`). Behaviour: [spec.md](../spec.md); states:
[data-model.md](../data-model.md); system spec: [docs/spec.md](../../../docs/spec.md) 7.2, 7.6, 7.8 and 7.11.

## Menu items

| Menu | Item | Shortcut (gesture text shown) | Action |
|---|---|---|---|
| ⋯ button and right-click menu, below **Edit People…** | **Data File ▸ Open…** | Ctrl+O (macOS: ⌘O) | [Open…](#open) |
| | **Data File ▸ Save As…** | Ctrl+Shift+S (macOS: ⇧⌘S) | [Save As…](#save-as) |
| | **Data File ▸ Show in Folder** | none | [Show in Folder](#show-in-folder) |
| macOS menu bar ▸ **File**, after **Edit People…** and a separator | **Open Data File…** | ⌘O | as Open… |
| | **Save Data File As…** | ⇧⌘S | as Save As… |
| | **Show Data File in Finder** | none | as Show in Folder |

The ⋯ menu then reads: Today, Change Date…, ─, Find…, Find Next, ─, Edit People…, **Data File ▸**, ─, Nameday
Calendar ▸, Theme ▸, ─, About Anniversary…. In the main window, Ctrl+O (⌘O) runs Open… and Ctrl+Shift+S (⇧⌘S) runs
Save As….

## Open…

1. The OS open dialog: title **Open Data File**; file types **Anniversary data** (`*.json`; macOS `public.json`;
   `application/json`) and all files; one file.
2. Closing the dialog without a file, or picking an item that has no local path, does nothing.
3. The file is read and checked ([data-file-api.md](data-file-api.md#opendatafilepath)). On success the people are
   replaced, the file becomes the data file in use, its path is saved as `dataFile` in `usersettings.json`
   ([data-file-setting.md](data-file-setting.md)), and the main window is refreshed.
4. On failure: a message with **OK**, title **Open Data File**, text `Could not open the data file: {message}`; nothing
   changes. For JSON without a `data` list, `{message}` is `This isn't an Anniversary data file (it has no "data"
   list).`

## Save As…

1. The OS save dialog: title **Save Data File As**; suggested name: the file name of the current save target
   (`Anniversary.Data.json` unless another file is in use); default extension `json`; file type **Anniversary data**;
   the OS asks before replacing a file.
2. Closing the dialog without a file does nothing.
3. The current people are written to the chosen path in the data file format (to `{path}.tmp`, which then replaces the
   file). On success that file becomes the data file in use, its path is saved as `dataFile`, and the main window is
   refreshed.
4. If writing fails (`IOException`, `UnauthorizedAccessException`): a message, title **Save Data File As**, text
   `Could not save the data file: {message}`; the data file in use stays the same.

## Show in Folder

- Opens the folder of the save target (the data file in use, or where the editor would create it) in the file
  manager: Explorer, Finder or the Linux file manager.
- If that folder doesn't exist: a message, title **Data File**, text `The data file's folder doesn't exist yet:`,
  a line break, and the folder.

## Changes made elsewhere

When the main window is activated, and before Edit People… opens, the app loads the people again if the data file was
changed elsewhere ([data-model.md](../data-model.md#rules)). After such a reload it refreshes the main window and shows
each warning in its own message.

## Warnings (at start-up and after a reload)

| Situation | Title | Text |
|---|---|---|
| The data file isn't found | **Data Error** | `Data file not found: {name}` (the full path for a chosen file) |
| The data file can't be read | **Data Error** | `Error loading data: {message}` |

## Questions in Edit People… (on Save)

Both questions have the title **Edit People**, the message, and two buttons on the right: the confirm button, then
**Cancel**. Cancel is the accent button and both the default and the cancel button, so Enter and Esc choose it. Cancel
keeps the editor open without saving. `{path}` is the editor's save target.

| When | Confirm button | Message |
|---|---|---|
| The data file was changed elsewhere after it was loaded (checked first) | **Overwrite** | `The data file was changed elsewhere, for example on another computer, after it was loaded:` / `{path}` / (empty line) / `Saving replaces those changes with yours.` |
| Otherwise, the data file couldn't be loaded (missing or unreadable) | **Save Anyway** | `The data file couldn't be loaded, so its people aren't in this list:` / `{path}` / (empty line) / `If it's on a cloud drive that hasn't finished syncing, cancel and try again later.` |

(`/` marks a line break.) After the confirm button, or when neither applies, saving goes on as before (002): a failure
shows `Could not save the data file: {message}` (title **Edit People**) and the editor stays open.
