# Feature Specification: People Editor

**Feature Branch**: `002-people-editor`

**Created**: 2026-10-09

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "Would it be possible to have some kind of simple editor in place? I am envisioning a
dialog with grid view (each row is one person and columns are Name, nameday, birthday and anniversaries. Anniversaries
would be some kind of serialized value but would contain an edit button which would display another dialog (key/value)
with anniversary date and value is the anniversary name? Is that possible or too complicated? If possible, can I see a
possible mock of such UI? (With respect to default dataset. I would think it should be empty. …)" After seeing the
mocks: "Yes, let's do it. I like it a lot."

**Later changes**: [007](../007-data-file-sharing/spec.md) made Save write into the data file in use (also one chosen
with Data File ▸ Open… or Save As…), made the app load a file changed elsewhere again before the editor opens, and made
the editor ask first (**Overwrite** or **Save Anyway**) when the file changed elsewhere or couldn't be loaded;
[012](../012-single-settings-file/spec.md) took the name of the file that Save creates in the app-data folder from
`usersettings.json` instead of `appsettings.json`.

## Clarifications

### Session 2026-10-09

- Q: Is a simple people editor possible, or too complicated, and what should it look like? → A: Possible. A dialog with
  a grid, one row per person, with the columns Name, Nameday, Birthday and Anniversaries; the Anniversaries cell
  summarizes them and has an **Edit…** button that opens a second dialog with each anniversary's date and name. The
  maintainer described this design, saw mocks of both windows and approved it: "Yes, let's do it. I like it a lot."
- Q: Where does Save write when there is no data file yet, given that the built-in people list stays empty (the
  maintainer's decision in the same message)? → A: Into the user's app-data folder, as `Anniversary.Data.json` (or the
  file name configured as `dataFile` in `appsettings.json`), creating the folder if needed. People loaded from a file
  are saved back into that file.
- Q: Does sorting the list in the editor change the order of the people in the file? → A: No. Sorting only changes the
  view; the file keeps the list order, which is also the order of the main window's lists.
- Q: How is a nameday entered, given that it has no year? → A: Picked from a calendar like the birthday, keeping only
  the month and day, shown in the user's regional month-and-day format so it matches the Birthday column; 29 February
  is allowed. (The mock shows typed `MM-dd` text instead.)
- Q: Should Remove or Cancel ask for confirmation? → A: No. The editor works on a copy and nothing changes until Save,
  so Cancel undoes everything, removals included.
- Q: Should Enter save the editor? → A: No. Save isn't the default button, because Enter finishes editing a cell in the
  grid.
- Q: Can a person have two anniversaries on the same date? → A: No. The dates identify the anniversaries in the data
  file, so OK stays disabled until every date is unique.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add and change people in the app (Priority: P1)

On a fresh install the people list is empty, and the main window says "No people yet. Use ⋯ ▸ Edit People… to add
some." The user chooses **⋯ ▸ Edit People…** (also on right-click; macOS: **File ▸ Edit People…**), adds people with a
name, a nameday and a birthday, changes or removes others, and clicks **Save**. The data file is written, and the main
window shows the people at once and at every later start.

**Why this priority**: It is what the maintainer asked for, and with an empty built-in list it is the way to get people
into the app; the other way is copying a v4 data file into the app-data folder or next to the executable.

**Independent Test**: On a computer without Anniversary data, add one person with a name, a nameday and a birthday and
save: `Anniversary.Data.json` appears in the app-data folder, and the main window shows the person on their days at once
and again after a restart.

**Acceptance Scenarios**:

1. **Given** a fresh install with no data file, so the main window shows the hint, **When** the user chooses ⋯ ▸ Edit
   People… (macOS: **File ▸ Edit People…**), **Then** a window titled "Edit People" opens with an empty list, and its
   footer shows "0 people" and the path of `Anniversary.Data.json` in the app-data folder.
2. **Given** the editor, **When** the user clicks **+ Add**, **Then** an empty person is added at the end of the list,
   selected, with the name ready for typing.
3. **Given** a new person with a name, a nameday and a birthday, **When** the user clicks **Save**, **Then** the editor
   closes, `Anniversary.Data.json` is written to the app-data folder, and the main window shows the person on their
   days at once, without the hint.
4. **Given** people saved this way, **When** the app is started again, **Then** it shows the same people.
5. **Given** people loaded from an existing data file (for example v4's file copied into the app-data folder), **When**
   the user changes a person and saves, **Then** the change is written into that same file and the other people keep
   their order.
6. **Given** one or more selected people, **When** the user clicks **− Remove**, **Then** they leave the list without a
   question; Cancel still brings them back.

---

### User Story 2 - Edit a person's anniversaries (Priority: P2)

Each row's Anniversaries cell shows that person's anniversaries in one line, for example "seznámení (9/1/2004), svatba
(6/21/2008)" in US English, with an **Edit…** button. It opens a small dialog where the user adds, changes and removes
anniversaries, each a date and a description. OK puts the result back into the editor, and Save in the editor writes
it.

**Why this priority**: Anniversaries are the third kind of event the main window shows, and acceptance criterion 10
asks for a person with two of them; but a person can be added and saved without any.

**Independent Test**: Give a person two anniversaries in the dialog, click OK and Save: the row shows both by date, and
the main window shows them on their days. A date used twice disables OK.

**Acceptance Scenarios**:

1. **Given** a person in the editor, **When** the user clicks **Edit…** in the Anniversaries column, **Then** a dialog
   titled "Anniversaries – {Name}" lists that person's anniversaries by date, with the columns **Date** and
   **Anniversary**.
2. **Given** the dialog, **When** the user adds two anniversaries with a date and a description each and clicks **OK**,
   **Then** the person's Anniversaries cell shows both as "{description} ({date})", ordered by date and separated by
   ", ".
3. **Given** the dialog, **When** an anniversary lacks a date or a description, or two have the same date, **Then** OK
   is disabled, the cells are marked, and the dialog says "Every anniversary needs a date and a name, and each date can
   be used only once."
4. **Given** changes in the dialog, **When** the user clicks **Cancel** or presses Esc, **Then** the person's
   anniversaries stay as they were.
5. **Given** anniversaries changed with OK, **When** the user saves in the editor, **Then** the data file holds them
   under the person, ordered by date, and the main window shows them on their days ("{Name} - {description}
   ({years})").
6. **Given** a person without anniversaries, **When** the editor shows the row, **Then** the cell shows a muted "none".

---

### User Story 3 - Catch mistakes and discard unwanted changes (Priority: P2)

The editor marks a blank name or an invalid nameday and won't save until it is fixed. Cancel closes the editor and
throws away every change since it opened. If the file can't be written, the editor says so and keeps the changes, and
the file stays as it was.

**Why this priority**: The data file holds a family's list. Acceptance criterion 10 requires that invalid entries
disable Save and OK and that Cancel discards all changes; a failed save must not lose the edits or damage the file.

**Independent Test**: Blank a name: Save is disabled. Make changes and click Cancel: the main window and the file are
unchanged. Save into a folder that can't be written: a message appears and the editor stays open.

**Acceptance Scenarios**:

1. **Given** the editor, **When** a person's name is blank, **Then** the Name cell is marked as an error and **Save**
   is disabled until a name is entered.
2. **Given** a data file with a nameday that isn't a valid month and day (for example `1-5`), **When** the editor
   opens, **Then** that Nameday cell is empty and marked as an error, and Save stays disabled until a valid nameday is
   picked.
3. **Given** changes in the editor, **When** the user clicks **Cancel** or presses Esc, **Then** the editor closes
   without a question, and neither the main window nor the data file changes.
4. **Given** the user is editing a cell, **When** they press Enter, **Then** the edit is committed and the editor stays
   open.
5. **Given** a data file that can't be written (for example in a read-only folder next to the executable), **When** the
   user clicks **Save**, **Then** the editor says "Could not save the data file: {reason}" (title "Edit People") and
   stays open with the changes, and the file stays as it was.

---

### User Story 4 - Find people in a long list (Priority: P3)

A **Filter by name** box at the top right shows only the people whose name contains the typed text. Clicking a column
header sorts the list by that column. Neither changes the order in which the people are saved.

**Why this priority**: It helps once the list is long; the editor works without it.

**Independent Test**: Type part of a name into the filter and click column headers: the list narrows and sorts. Save:
the file keeps its order.

**Acceptance Scenarios**:

1. **Given** many people, **When** the user types part of a name into "Filter by name", **Then** only the people whose
   name contains it are shown, ignoring case.
2. **Given** the list, **When** the user clicks a column header, **Then** the list is sorted by that column; when the
   editor opens, it is sorted by name.
3. **Given** a sorted or filtered list, **When** the user saves, **Then** the file keeps the people in their earlier
   order, with new people at the end.
4. **Given** a filter, **When** the user clicks **+ Add**, **Then** the filter is cleared so the new person is visible.

### Edge Cases

- **No data file yet** (the built-in empty list): the first Save creates `Anniversary.Data.json` (and the folder) in the
  app-data folder; from then on the app loads that file.
- **Data file next to the executable**: Save writes it there. If that folder can't be written, the save fails with the
  message and the editor stays open.
- **Data file configured with a full path** (`dataFile` in `appsettings.json`): Save writes that path, even when the
  file was missing at start-up.
- **Configured data file missing or unreadable at start-up**: after the start-up warning the editor starts with no
  people, and Save writes where it would for the built-in list (the app-data folder, or the full path) without asking,
  which replaces an unreadable file in that place. ([007](../007-data-file-sharing/spec.md) later made the editor ask
  first.)
- **29 February** is a valid nameday.
- **The same date twice** among one person's anniversaries isn't allowed.
- **Spaces**: names and namedays are trimmed when saved, and anniversary descriptions when the Anniversaries dialog's
  OK is clicked; a blank nameday and an empty list of anniversaries are left out of the file.
- **Order**: Add puts new people at the end; sorting and filtering only change the view, so the file keeps its order,
  which is the order of the main window's lists.
- **Removing by mistake**: Remove doesn't ask; Cancel brings everything back.
- **Two dialogs, one Save**: OK in the Anniversaries dialog changes only the editor's copy; Cancel in the editor
  discards those changes too.
- **Failed save**: the people are first written to a temporary file that then replaces the data file, so a failure
  leaves the old file as it was.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The commands menu (the ⋯ button and right-click) MUST have **Edit People…**, after Find Next and a
  separator; on macOS the menu bar's **File** menu MUST have **Edit People…** too.
- **FR-002**: Edit People… MUST open a window titled "Edit People" that stays in front of the main window until it is
  closed and can be resized (the list takes the extra space), with one row per person and the columns **Name**,
  **Nameday**, **Birthday** and **Anniversaries**, each edited in place.
- **FR-003**: The editor MUST work on a copy of the people: nothing in the main window or in the data file changes
  until Save.
- **FR-004**: **Add** MUST append an empty person at the end of the list, select it and start editing its name;
  **Remove** MUST remove the selected people (one or several) without asking.
- **FR-005**: A name MUST be required: blank (after trimming spaces) is an error.
- **FR-006**: A nameday MUST be optional and picked as a month and day only, shown in the user's regional month-and-day
  format (the short date format without the year); 29 February MUST be allowed. A nameday in the data that isn't a
  valid month and day MUST be marked as an error.
- **FR-007**: A birthday MUST be optional and picked as a date, shown in the user's short date format.
- **FR-008**: The Anniversaries cell MUST show the person's anniversaries as "{description} ({date})", ordered by date
  and separated by ", ", or a muted "none", and MUST have an **Edit…** button that opens the Anniversaries dialog for
  that person.
- **FR-009**: The Anniversaries dialog MUST be titled "Anniversaries – {Name}" and list the person's anniversaries by
  date with the columns **Date** and **Anniversary**, with Add and Remove. Every anniversary MUST have a date and a
  description, and no date may be used twice; while that isn't so, OK MUST be disabled and the dialog MUST say "Every
  anniversary needs a date and a name, and each date can be used only once."
- **FR-010**: OK in the Anniversaries dialog MUST change only that person in the editor's copy (its Anniversaries cell
  updates), and Cancel MUST discard the dialog's changes; only Save in the editor writes them.
- **FR-011**: Cells with errors MUST be marked, and **Save** MUST be disabled while any person has an error.
- **FR-012**: A filter box ("Filter by name") MUST show only the people whose name contains the typed text, ignoring
  case, and clicking a column header MUST sort the list, which starts sorted by name. Sorting and filtering MUST NOT
  change the order in which the people are saved, and Add MUST clear the filter so the new person is visible.
- **FR-013**: The footer MUST show the number of people and the full path of the file that Save writes.
- **FR-014**: **Save** MUST write the people, close the editor and update the main window. **Cancel** and Esc MUST close
  the editor without saving and without asking. Enter MUST NOT save; it finishes editing a cell.
- **FR-015**: Save MUST write into the file the people were loaded from. When they didn't come from a file (the built-in
  empty list, or a configured file that was missing or couldn't be read), it MUST write the configured data file name
  (`Anniversary.Data.json` unless `appsettings.json` names another) into the user's app-data folder, creating the folder
  if needed; a data file configured with a full path MUST be written in place. Save MUST NOT write any other file.
- **FR-016**: The saved file MUST use the existing (v4) data file format, so it loads again unchanged: people in list
  order, names and namedays trimmed, empty optional values left out, anniversaries ordered by date, dates as
  `yyyy-MM-dd` and namedays as `MM-dd`, and letters such as Czech ones written as they are.
- **FR-017**: Saving MUST NOT leave a half-written data file. If the file can't be written, the editor MUST say "Could
  not save the data file: {reason}" (title "Edit People") and stay open with the changes.
- **FR-018**: After Save, the main window MUST show the saved people at once (for the displayed day, the next days and
  Find) without reading the file again.

### Key Entities

- **Person**: a name (required), an optional nameday (a month and day), an optional birthday and optional anniversaries;
  one row in the editor.
- **Anniversary**: a date and a description, for example "svatba" (wedding); a person's anniversary dates are unique.
- **People data file**: the people in the v4 format, by default `Anniversary.Data.json`, in the order the main window
  lists them; the editor writes it back to the file it came from, or creates it in the app-data folder.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On a fresh install with no data file, a user adds a person with a nameday, a birthday and two
  anniversaries, and after Save the main window shows them at once and again after a restart; on Windows the file is
  `%APPDATA%\Anniversary\Anniversary.Data.json` (acceptance criterion 10 in [docs/spec.md](../../docs/spec.md) §12).
- **SC-002**: Blank names, invalid namedays and duplicate anniversary dates disable Save or OK in every case
  (criterion 10).
- **SC-003**: Cancel discards all changes: afterwards the main window and the data file are exactly as before
  (criterion 10).
- **SC-004**: A file saved by the editor loads again with the same people in the same order, including names with Czech
  letters, namedays, birthdays and anniversaries, and keeps the documented data file format.
- **SC-005**: No failed save leaves a half-written data file or loses the edits in the open editor.
- **SC-006**: 30 automated test cases cover saving and the editor's rows (13 for saving, 17 for the rows); with the rest
  of phase 1, all 78 tests passed and the build had no warnings.

## Assumptions

- Builds on [001](../001-cross-platform-rewrite/spec.md): the main window and its lists, the ⋯ commands menu (also on
  right-click) and the macOS menu bar, the message dialog, the lookup of the data file (next to the executable, then the
  app-data folder, then the built-in list) with its start-up warnings, and the empty built-in people list with the hint
  "No people yet. Use ⋯ ▸ Edit People… to add some." The maintainer asked for the empty list in the same message as
  this editor.
- The data file keeps the v4 format; the editor adds nothing to it. The app never writes `appsettings.json`; besides its
  own `usersettings.json` it writes only the people data file, and only from the editor.
- One person edits the list on one computer: the editor doesn't notice changes made to the data file elsewhere and
  replaces the file without asking. Noticing such changes, asking first and saving to another file (Save As…) came with
  [007](../007-data-file-sharing/spec.md).
- The mocks ([editor](../../docs/screens/people-editor.light.preview.png),
  [editor, dark](../../docs/screens/people-editor.dark.preview.png),
  [anniversaries](../../docs/screens/anniversaries-editor.light.preview.png)) are approximate; where they differ from
  this spec, the spec wins (for example, the nameday is picked as a month and day, not typed as `MM-dd`).
- The user guide shipped with every download came later ([009](../009-user-guide/spec.md)); the repository README
  describes the data file and the editor.
