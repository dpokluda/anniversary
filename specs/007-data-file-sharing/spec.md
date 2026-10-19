# Feature Specification: Data File Sharing

**Feature Branch**: `007-data-file-sharing`

**Created**: 2026-10-10

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "Now, I have two use cases: - you tell a family member to download/install the program
and you give them your family data file; they start the file and they need to be able to "open" the data file and keep
using it - second a person could have multiple computers and they want to have "shared" data file, that's when they
would probably put it on a cloud drive and reference the file from the data file; this way you update on one machine
and it is visible/available on all. What would be a good way to achieve these? Let's discuss this."

**Later changes**: [012](../012-single-settings-file/spec.md) made `usersettings.json` the only settings file: v4's
`appsettings.json` is no longer read, so its `dataFile` no longer applies, and a file name typed by hand into `dataFile`
in `usersettings.json` also works.

## Clarifications

### Session 2026-10-10

- Q: Should Data File ▸ Open… copy the chosen file into the app's folder or use it where it is? → A: Use it where it
  is; it is never copied. The maintainer chose this over asking or a separate Import command.
- Q: Where is the chosen file remembered, given that each computer may reach the shared file by its own path? → A: Per
  computer, in that computer's `usersettings.json`; the choice wins over `dataFile` in `appsettings.json`.
- Q: How does a change saved on another computer reach this one, and what if both computers change the file? → A: The
  app loads the file again when its main window is activated (and before Edit People… opens) if the file's last-change
  time or size changed, and the editor asks before overwriting changes made elsewhere. The app doesn't merge: if two
  computers edit the file while offline, the cloud app may keep a "conflicted copy", which the user resolves.
- Q: What if the chosen file is missing at start-up, for example because it isn't synced yet? → A: The start-up warning
  names it; the app keeps the link, loads the file once it appears, and the editor asks before creating a new file
  there.
- Q: What should Save in the editor do when the data file exists but couldn't be read? → A: Ask first (**Save Anyway**
  or Cancel) instead of silently replacing it with an empty list. The assistant added this beyond the agreed plan and
  reported it; it was kept.
- Q: Should a data file also open by double-clicking it? → A: Not in this feature: that needs installers and a file
  extension of its own, so it waits for the installers.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Open a data file someone sent (Priority: P1)

A family member installs the app and gets your data file, for example by email. They save it somewhere permanent, such
as Documents, and choose **⋯ ▸ Data File ▸ Open…** (macOS: **File ▸ Open Data File…**). The app shows the people from
that file at once, uses the file where it is, saves their edits into it and opens it again at every start.

**Why this priority**: It is the first use case the maintainer named and the smallest slice that lets someone else use
your list. Without it, a data file had to be copied into the app-data folder (or next to the executable), or named in
`appsettings.json`, by hand.

**Independent Test**: On a computer without Anniversary data, open a data file from another folder with Open…: its
people appear at once and again after a restart, and a person added in Edit People… is saved into that same file.

**Acceptance Scenarios**:

1. **Given** a fresh install and a data file someone sent, saved in Documents, **When** the user chooses ⋯ ▸ Data File
   ▸ Open… (Ctrl+O; macOS **File ▸ Open Data File…**, ⌘O) and picks it, **Then** the main window shows its people at
   once.
2. **Given** a file opened this way, **When** the app is started again, **Then** it uses the same file.
3. **Given** a file opened this way, **When** the user adds a person in Edit People… and saves, **Then** the person is
   written into that file and no copy appears in the app-data folder.
4. **Given** the user picks a file that can't be read or isn't a people data file (for example a calendar file),
   **When** Open… reads it, **Then** the app says "Could not open the data file: {reason}" (title "Open Data File") and
   the people and the data file in use stay as they were.
5. **Given** a v4 `Anniversary.Data.json`, **When** the user opens it with Open…, **Then** its people appear, because
   the format is unchanged.

---

### User Story 2 - Use one data file from several computers (Priority: P2)

One person keeps the data file on a cloud drive (OneDrive, iCloud Drive, Dropbox, …) and uses it from several computers.
On the first computer they choose **⋯ ▸ Data File ▸ Save As…** and save into the cloud-drive folder; on each other one
they choose **Open…** and pick that file. A change saved on one computer appears on the others the next time their app
window becomes active, once the cloud app has synced it. Each computer remembers its own path to the file.

**Why this priority**: It is the maintainer's second use case. It builds on "use the file where it is" and adds moving
the file to a shared folder and noticing changes made elsewhere.

**Independent Test**: Save As… into a cloud-drive folder, then change that file elsewhere (on another computer, or in
a text editor) and switch back to the app: the main window shows the change without a restart.

**Acceptance Scenarios**:

1. **Given** the people in the current data file, **When** the user chooses Save As… (Ctrl+Shift+S; macOS **File ▸ Save
   Data File As…**, ⇧⌘S) and saves into a cloud-drive folder, **Then** the people are written there and the app uses
   that file from then on, also after a restart.
2. **Given** the shared file opened on two computers, **When** a person is added and saved on one and the cloud app has
   synced the file, **Then** the person appears on the other computer the next time its window is activated.
3. **Given** the file was changed elsewhere, **When** the user opens Edit People…, **Then** the editor starts from the
   changed file.
4. **Given** the remembered file is missing at start-up (for example not synced yet), **When** the app starts, **Then**
   it warns "Data file not found: {path}", shows no people, and loads the file as soon as it appears, the next time the
   window is activated.
5. **Given** the file disappears for a moment while the app runs (as when a cloud app replaces it), **When** the window
   is activated, **Then** the people shown stay as they are.

---

### User Story 3 - Save without losing changes made elsewhere (Priority: P2)

When the user saves in Edit People…, the editor asks first if the data file was changed elsewhere since it was loaded,
or if it couldn't be loaded at all (missing or unreadable). So a save never silently wipes out changes made on another
computer, or people the app couldn't read.

**Why this priority**: Sharing one file makes both situations likely, and a silent overwrite would lose a family's
data.

**Independent Test**: Open Edit People…, change the data file elsewhere, then click Save: the editor asks before
overwriting. Start with a missing or broken data file and click Save in the editor: it asks before saving.

**Acceptance Scenarios**:

1. **Given** the editor is open and the data file was changed elsewhere after it was loaded, **When** the user clicks
   Save, **Then** the editor asks (title "Edit People", buttons **Overwrite** and **Cancel**) and names the file;
   Overwrite saves, Cancel keeps the editor open without saving.
2. **Given** the data file was missing at start-up, **When** the user saves in the editor, **Then** it asks (**Save
   Anyway** or **Cancel**) before creating the file.
3. **Given** the data file couldn't be read, **When** the user saves in the editor, **Then** it asks (**Save Anyway**
   or **Cancel**) and suggests cancelling and trying later if the file is on a cloud drive that hasn't finished
   syncing.
4. **Given** either question, **When** the user presses Enter or Esc, **Then** Cancel is chosen.
5. **Given** the app itself saved the file last, **When** the user saves again, **Then** no question appears.

---

### User Story 4 - Find the data file (Priority: P3)

**⋯ ▸ Data File ▸ Show in Folder** (macOS: **File ▸ Show Data File in Finder**) opens the folder that holds the data
file in the file manager, for example to attach the file to an email for family or to back it up.

**Why this priority**: It helps with sending the file (User Story 1), but the file can also be found by hand.

**Independent Test**: Choose Show in Folder: the file manager opens the data file's folder; if the folder doesn't exist
yet, a message says so.

**Acceptance Scenarios**:

1. **Given** a data file in use, **When** the user chooses Show in Folder, **Then** the file manager (Explorer, Finder
   or the Linux file manager) opens the folder that holds it.
2. **Given** the people come from the built-in empty list and the app-data folder doesn't exist yet, **When** the user
   chooses Show in Folder, **Then** a message (title "Data File") says that the data file's folder doesn't exist yet and
   names it.

### Edge Cases

- **Not a data file**: a calendar file, another kind of file, or a file that can't be read is refused by Open…; the
  people and the data file in use don't change.
- **Missing at start-up** (not synced yet, or the drive isn't there): a warning names the file, no people are shown,
  the link is kept, and the file is loaded as soon as it appears; Save in the editor asks before creating it.
- **Unreadable** (broken, or only partly synced): the warning "Error loading data: {reason}" and no people; Save in the
  editor asks before replacing the file.
- **Gone for a moment** (a cloud app may remove the file while it replaces it): this isn't a change; the people shown
  stay.
- **The app's own saves** (Save in the editor, Save As…) don't count as changes made elsewhere.
- **Offline edits on two computers**: no merging; the cloud app may keep a "conflicted copy", which the user resolves.
- **Repeated warnings**: after a reload only the problems found by that reload are shown, not the ones shown earlier.
- **Save As… into a folder that can't be written**: "Could not save the data file: {reason}"; nothing changes.
- **Cancelled dialogs**: closing the open or save dialog without choosing a file changes nothing.
- **No file chosen on this computer**: the configured or default data file is used as before.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The commands menu (the ⋯ button and right-click) MUST have a **Data File** submenu below Edit People…
  with **Open…**, **Save As…** and **Show in Folder**. On macOS the menu bar's **File** menu MUST also have **Open Data
  File…**, **Save Data File As…** and **Show Data File in Finder**, after Edit People… and a separator.
- **FR-002**: Open… and Save As… MUST have the shortcuts Ctrl+O and Ctrl+Shift+S on Windows and Linux, and ⌘O and ⇧⌘S
  on macOS, shown next to the menu items.
- **FR-003**: Open… MUST let the user pick a file in the operating system's open dialog (offering *Anniversary data*
  `.json` files and all files) and MUST use the picked file where it is, never copying it; the main window MUST show
  its people at once.
- **FR-004**: Open… MUST refuse a file that can't be read or isn't a people data file (for example a calendar file)
  with the message "Could not open the data file: {reason}" (title "Open Data File"), and MUST then leave the people
  and the data file in use unchanged.
- **FR-005**: Save As… MUST let the user pick a location in the operating system's save dialog (suggesting the current
  data file's name as a `.json` file, with the OS asking before it replaces a file), write the current people there in
  the data file format, and use that file from then on. If writing fails, it MUST say "Could not save the data file:
  {reason}" (title "Save Data File As") and change nothing.
- **FR-006**: A file chosen with Open… or Save As… MUST be remembered for this computer only, in its
  `usersettings.json`, and used at every later start, ahead of `dataFile` in `appsettings.json`.
- **FR-007**: On a computer where no file was chosen, the app MUST keep using the configured or default data file as
  before.
- **FR-008**: Save in Edit People… MUST write into the data file in use, including a file opened with Open… or saved
  with Save As….
- **FR-009**: When the main window becomes active, and before Edit People… opens, the app MUST load the data file again
  if it was changed elsewhere since the app loaded or saved it (its last-change time or size differs), update the main
  window, and show the problems found by that load.
- **FR-010**: The app's own saves MUST NOT count as changes made elsewhere, and a data file that has disappeared MUST
  NOT count as a change: the people shown stay.
- **FR-011**: If the chosen data file (or one configured with a full path) is missing at start-up, the app MUST show
  the warning "Data file not found: {path}" after its window opens, start with no people, keep that file as the data
  file (saving goes there) and load it as soon as it appears.
- **FR-012**: If the data file exists but can't be read, the app MUST show the warning "Error loading data: {reason}",
  start with no people and keep that file as the data file.
- **FR-013**: When the user saves in Edit People… and the data file was changed elsewhere since it was loaded, the
  editor MUST ask first (title "Edit People", buttons **Overwrite** and **Cancel**, naming the file); Cancel MUST keep
  the editor open without saving.
- **FR-014**: When the user saves in Edit People… and the data file couldn't be loaded (missing or unreadable), the
  editor MUST ask first (buttons **Save Anyway** and **Cancel**, naming the file and suggesting to try again later if a
  cloud drive hasn't finished syncing); Cancel MUST keep the editor open without saving.
- **FR-015**: In these questions **Cancel** MUST be the default button, so Enter and Esc choose it.
- **FR-016**: Show in Folder MUST open the folder that holds the data file (where Save writes it) in the file manager;
  if that folder doesn't exist yet, it MUST say so instead (title "Data File").

### Key Entities

- **Data file**: the people list in the v4 format, by default `Anniversary.Data.json`. With this feature it can be any
  file the user opens or saves as, wherever it is (Documents, a cloud-drive folder, …), and it can be shared by family
  members or by one person's computers.
- **Remembered data file**: the full path of the file chosen with Open… or Save As…, kept in each computer's
  `usersettings.json`. When there is none, the configured or default data file is used.
- **Data file status** (while the app runs): which file is in use, whether it could not be loaded (missing or
  unreadable), and its last-change time and size when the app last loaded or saved it, which tell whether it was
  changed elsewhere.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On a fresh install, a family member sees the people from a file they were sent after one Open…, still
  sees them after a restart, and their edits are saved into that file (acceptance criterion 13 in
  [docs/spec.md](../../docs/spec.md) §12).
- **SC-002**: With one file on a cloud drive used by two computers, a person added on one appears on the other the
  next time its window is activated after the cloud app has synced, with no restart and no manual reload (criterion 14).
- **SC-003**: No save silently replaces a data file that changed elsewhere or couldn't be loaded: the editor asks first
  in every such case, and Cancel is the default (criterion 14).
- **SC-004**: Opening a file that isn't a people data file, such as a calendar file, is refused every time and never
  changes the people shown or the remembered file.
- **SC-005**: A computer on which no data file was chosen behaves exactly as before; the maintainer's own setup kept
  working unchanged.
- **SC-006**: 16 new automated tests cover choosing, sharing and watching the data file, and all 181 tests pass with a
  build free of warnings.

## Assumptions

- Builds on [001](../001-cross-platform-rewrite/spec.md) (file lookup, load warnings shown after the window opens, the
  app-written `usersettings.json`) and [002](../002-people-editor/spec.md) (Edit People… and the safe save of the
  people file).
- Syncing is the job of the user's cloud-drive app, which keeps a local folder in sync; the app only reads and writes a
  file in that folder.
- The app doesn't merge; conflicting edits made offline are resolved by the user in the cloud app.
- Opening a data file by double-clicking it is out of scope: it needs installers and a file extension of its own.
- When no file is chosen in the app, `dataFile` in `appsettings.json` (a file name or a full path) keeps working; the
  maintainer's own setup, which named no data file in the app, stayed unchanged.
- Sharing is documented in the repository README; the user guide shipped with every download came later
  ([009](../009-user-guide/spec.md)).
