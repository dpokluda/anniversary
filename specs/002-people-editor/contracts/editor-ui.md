# Contract: Edit People… and the editor windows

The user interface this feature added, as built on 2026-10-09 (`Anniversary.Shared/Views/MainWindow.axaml`,
`PeopleEditorWindow.axaml`, `PeopleEditorWindow.axaml.cs`, `AnniversariesDialog.axaml` and
`AnniversariesDialog.axaml.cs` in the same folder). Behaviour: [spec.md](../spec.md); rows and rules:
[data-model.md](../data-model.md); system spec: [docs/spec.md](../../../docs/spec.md) 7.2, 7.8 and 7.9; mocks
(approximate): [editor](../../../docs/screens/people-editor.light.preview.png),
[editor, dark](../../../docs/screens/people-editor.dark.preview.png),
[anniversaries](../../../docs/screens/anniversaries-editor.light.preview.png).

## Menu items

| Menu | Item | Shortcut | Action |
|---|---|---|---|
| ⋯ button and right-click menu, after **Find Next** and a separator | **Edit People…** | none | opens the editor |
| macOS menu bar ▸ **File** | **Edit People…** | none | the same |

The ⋯ menu then reads: Today, Change Date…, ─, Find…, Find Next, ─, **Edit People…**, ─, Nameday Calendar ▸, Theme ▸,
─, About Anniversary…. Native menu items have no routed click event, so the main window finds the macOS item by its
header and wires it in code.

The editor is modal over the main window. When it closes after a successful Save, the main window refreshes; after
Cancel nothing changes.

## Edit People window

| Property | Value |
|---|---|
| Title | **Edit People**, with the app icon |
| Size | 700 × 400 at first, at least 520 × 280; resizable, and the grid takes the extra space |
| Position | centred on the main window; modal; not shown in the taskbar |
| Layout | margin 12: the toolbar at the top, the footer at the bottom, the grid in between |

### Toolbar

- Left: **+ Add** and **− Remove** (the texts are `+  Add` and `−  Remove`, with a minus sign).
- Right: a text box, 180 wide, with the placeholder *Filter by name*.

### Columns

| Column | Width | Cell | Sorted by | Error |
|---|---|---|---|---|
| **Name** | `2*` | text, edited in place | the name | "Name is required." when blank |
| **Nameday** | 105 | date picker showing only the month and day in `PersonRow.MonthDayFormat` (for example `M/d` or `d. M.`), which is also its placeholder | the `MM-dd` text | "Use MM-dd, e.g. 06-24." when invalid |
| **Birthday** | 125 | date picker, short date in the current culture | the date | none (optional) |
| **Anniversaries** | `3*` | the summary `"{description} ({date})"` (short date in the current culture), ordered by date, joined by `", "` and cut with an ellipsis, or a muted italic *none*; an **Edit…** button on the right | the summary | none |

The grid starts sorted by Name, ascending; clicking a header sorts by that column. Columns can be resized, and several
rows can be selected (extended selection). Horizontal grid lines; rows are at least 28 high. Cells with errors get the
Fluent theme's error style.

### Footer

- Left, muted, cut with an ellipsis: `{count} people · {path}`, where `{count}` counts every person in the list (also
  those a filter hides) and `{path}` is the save target ([people-file-write.md](people-file-write.md#where)).
- Right: **Save** (accent) and **Cancel**.

### Commands and keys

| Command | Behaviour |
|---|---|
| **+ Add** | commits the current edit, clears the filter, appends an empty person at the end of the list, selects it, scrolls to it and starts editing its name |
| **− Remove** | removes all selected people; no question |
| Filter text | shows only the people whose name contains the trimmed text (current culture, ignoring case); an empty text shows everyone. A cell edit is committed first, and the view refreshes only when the text really changed |
| **Edit…** | opens the [Anniversaries dialog](#anniversaries-dialog) for that row; OK replaces the row's anniversaries |
| **Save** | disabled while any row has an error. Commits the current edit, checks again, writes the people in list order ([people-file-write.md](people-file-write.md)) and closes; for a write error, see [Messages](#messages) |
| **Cancel**, Esc | closes without saving and without asking |
| Enter | commits a cell edit; Save isn't the default button |

### Messages

| When | Title | Text | Then |
|---|---|---|---|
| The file can't be written (`IOException`, `UnauthorizedAccessException`) | **Edit People** | `Could not save the data file: {message}` | **OK**; the editor stays open with the changes |

## Anniversaries dialog

| Property | Value |
|---|---|
| Title | **Anniversaries – {Name}**, or **Anniversaries** when the name is blank; with the app icon |
| Size | 400 wide; the height fits the content; not resizable |
| Position | centred on the editor; modal; not shown in the taskbar |
| Layout | margin 12, spacing 8: toolbar, grid (160 high), note, buttons |

The toolbar has **+ Add** (appends an empty row and selects it) and **− Remove** (removes the selected rows). The grid
can't be sorted, allows extended selection, and lists the rows by date when the dialog opens:

| Column | Width | Cell | Errors |
|---|---|---|---|
| **Date** | 140 | date picker, short date in the current culture | "Date is required.", "This date is used twice." |
| **Anniversary** | the rest | text | "Anniversary is required." |

- The note (muted, wrapping) is shown while anything is invalid: `Every anniversary needs a date and a name, and each
  date can be used only once.`
- Buttons on the right: **OK** (accent, the default button; disabled while anything is invalid) and **Cancel** (Esc).
- **OK** commits the current edit, checks again and returns the anniversaries, with trimmed descriptions, to the editor,
  which updates the row's summary. **Cancel** changes nothing. Neither writes the file; only Save in the editor does.
