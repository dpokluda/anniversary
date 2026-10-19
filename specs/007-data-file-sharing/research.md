# Research: Data File Sharing

Decisions made while planning and building the feature on 2026-10-10. Sources: the session of 2026-10-10 (the
maintainer's two use cases and the assistant's report), the Decisions table in [docs/plan.md](../../docs/plan.md),
the decisions in [docs/copilot/handover.md](../../docs/copilot/handover.md), [docs/spec.md](../../docs/spec.md) 5.3–5.6,
7.8 and 7.11, and `git diff 5bb1afe bb8676b`. The session excerpt has only the request and the final report, so
alternatives are listed only where a source names them.

## Use the chosen file where it is

- **Decision**: Open… and Save As… make the chosen file the data file in use. It is read and written in place and
  never copied into the app-data folder.
- **Rationale**: One mechanism covers both use cases: a family member opens the file you sent, and each of your
  computers opens the same file on a cloud drive, so a change saved on one is the change all of them see.
- **Alternatives considered**: asking, or a separate Import command. The maintainer chose "where it is" over both
  (user decision).
- **Source**: docs/plan.md, Decisions, "Sharing the data file"; the handover ("no copy, no Import, no merge").

## Remember the choice per computer in `usersettings.json`

- **Decision**: `UserSettings.DataFile` holds the full path of the chosen file. Open… and Save As… save it once they
  succeed; at start-up the app passes it to `AnniversaryBook.Load`, where it wins over `dataFile` from
  `appsettings.json`.
- **Rationale**: The same shared file can have a different path on each computer (for example on Windows and on macOS),
  so the path belongs to the computer. `usersettings.json` is the file the app writes for choices made in its menus
  (theme, calendar), while `appsettings.json` is never written by the app (spec C9).
- **Alternatives considered**: none recorded beyond the existing hand-edited `dataFile` in `appsettings.json` (a file
  name or a full path), which keeps working when nothing was chosen; the app never writes that file (spec C9).
- **Source**: docs/plan.md, Decisions; spec 5.3 and 5.5; session 2026-10-10.

## Notice changes made elsewhere by last-write time and size

- **Decision**: After loading or saving, `AnniversaryBook` remembers the file's last-write time (UTC) and size.
  `HasDataFileChanged()` is true when the file exists and either value differs. The main window checks when it is
  activated, and before Edit People… opens; `ReloadPeopleIfChanged()` then loads the people again and reports problems
  as at start-up, replacing the earlier warnings.
- **Rationale**: The cloud app syncs the file in the background, so the change shows the next time the user comes back
  to the window. A file that appears after being missing counts as a change, so it is picked up. A file that is gone
  doesn't, because a cloud app may remove it for a moment while it replaces it. The app's own saves renew the stamp,
  so they never look like changes made elsewhere.
- **Alternatives considered**: none recorded.
- **Source**: spec 5.6 and 7.11; the handover; `DataFileTests` (`DeletedFileIsNotAChange`, `SavingIsNotAChange`).

## No merging

- **Decision**: The app doesn't merge. If two computers edit the file while offline, the cloud app may keep a
  "conflicted copy", which the user resolves; inside the app, the editor asks before overwriting changes made
  elsewhere.
- **Rationale**: The cloud app may keep a conflicted copy for the user to resolve, and the editor's question prevents
  a silent loss.
- **Alternatives considered**: merging edits from two computers; not done.
- **Source**: spec 7.11; docs/plan.md ("No merging."); the handover.

## Keep a missing chosen file as the data file

- **Decision**: A chosen file (or one configured with a full path) that is missing stays the data file: the start-up
  warning names it, there are no people, the editor saves there after asking, and the file is loaded as soon as it
  appears.
- **Rationale**: On a new computer, or before the cloud app has synced, the file may simply not be there yet; the
  link must survive. The code says: "A chosen or absolute file stays the data file, so it's saved there and picked up
  when it appears."
- **Alternatives considered**: none recorded. Before this feature, a data file that wasn't found left no file in use.
- **Source**: docs/plan.md, Decisions; spec 5.4; `AnniversaryBook.LoadPeople`.

## Ask before saving over a file that couldn't be loaded

- **Decision**: When the data file couldn't be loaded (missing or unreadable), Save in the editor asks **Save Anyway**
  or **Cancel**; when it changed elsewhere, it asks **Overwrite** or **Cancel**. Cancel is the default button in both
  (`MessageDialog.Confirm`).
- **Rationale**: Without the question, a file that couldn't be read, for example one only partly synced, would be
  replaced by the editor's empty list. The session's report marks only the question for a file that can't be read as
  "Beyond the plan": the assistant added it and reported it, and it was kept.
- **Alternatives considered**: saving without asking, as before this feature.
- **Source**: session 2026-10-10; spec 7.6 and 7.8.

## Refuse files that aren't data files

- **Decision**: Before using a file, `OpenDataFile` checks that it is a JSON object with a `data` list. Otherwise it
  throws a `JsonException` ("This isn't an Anniversary data file (it has no "data" list).") and nothing changes.
- **Rationale**: In the code's words: "A calendar or any other JSON file would otherwise open as an empty list, and
  saving would replace it."
- **Alternatives considered**: reading the file the way the start-up load does, which would open such a file as an
  empty list.
- **Source**: `AnniversaryBook.OpenDataFile`; session 2026-10-10 ("Open… refuses JSON that isn't a data file, such as
  a calendar"); `DataFileTests`.

## Menu placement and shortcuts

- **Decision**: A **Data File** submenu below Edit People… in the ⋯ and right-click menu; on macOS, **Open Data
  File…** (⌘O), **Save Data File As…** (⇧⌘S) and **Show Data File in Finder** in the File menu; Ctrl+O and
  Ctrl+Shift+S on Windows and Linux.
- **Rationale**: The app's commands live in the ⋯ menu, which is also the right-click menu, and on macOS in the menu
  bar with ⌘ shortcuts (spec C1, C7). The assistant also added Ctrl+O and Ctrl+Shift+S on Windows and Linux.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; spec 7.2.

## Native dialogs and the file manager through Avalonia

- **Decision**: Open… and Save As… use Avalonia's `StorageProvider` (`OpenFilePickerAsync`, `SaveFilePickerAsync`,
  then `TryGetLocalPath()`), and Show in Folder uses `Launcher.LaunchDirectoryInfoAsync`.
- **Rationale**: Each OS gets its own dialogs and file manager from shared code, with no platform code and no package
  beyond those the spec allows (spec 2).
- **Alternatives considered**: none recorded.
- **Source**: spec 7.11; `MainWindow.axaml.cs`.

## Opening a data file by double-clicking: deferred

- **Decision**: Not part of this feature; it waits for the installers.
- **Rationale**: Opening a data file by double-clicking it needs installers and a file extension of its own.
- **Alternatives considered**: none; it was recorded as a later step. After the installers (010), the assistant
  listed it again as optional, and the maintainer didn't take it up (session 2026-10-10).
- **Source**: docs/plan.md, Decisions; spec 7.11.

## Verification approach

- **Decision**: Tests with real files for the logic (`DataFileTests`, 16 cases), a headless run of the real windows
  for the menus and the questions (nothing of it was committed), and manual `dotnet run` checks for what can't run
  headless: the native open and save dialogs and Show in Finder (⌘O, ⇧⌘S, File ▸ Show Data File in Finder).
- **Rationale**: The native dialogs and Show in Finder can't run headless; everything else could be checked by the
  tests or the headless run.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; the handover's lessons (headless renders).
