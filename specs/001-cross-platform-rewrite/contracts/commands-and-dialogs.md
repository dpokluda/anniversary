# Contract: commands, shortcuts, dialogs and messages

The command surface as built on 2026-10-09 (`Anniversary.Shared/Views/MainWindow.axaml` and its code-behind, the
dialogs in `Anniversary.Shared/Views/`, the app menu in `Anniversary.Shared/App.axaml`). The window itself is in
[main-window.md](main-window.md). System spec: [docs/spec.md](../../../docs/spec.md) §7.1–7.7. Mocks:
[commands menu](../../../docs/screens/main-window-menu.light.preview.png),
[macOS](../../../docs/screens/main-window-macos.light.preview.png),
[Change Date](../../../docs/screens/change-date-dialog.light.preview.png),
[with the calendar open](../../../docs/screens/change-date-dialog-calendar.light.preview.png),
[Find](../../../docs/screens/find-dialog.light.preview.png),
[About](../../../docs/screens/about-dialog.light.preview.png).

## Commands menu

One menu, defined once as a window resource. It is the **⋯** button's flyout (opening below the button, aligned to its
right edge) and the window's context menu (right-click anywhere in the window).

| Item | Action | Shortcut shown |
|---|---|---|
| **Today** | shows today | none |
| **Change Date…** | opens [Change Date](#change-date) | none |
| *separator* | | |
| **Find…** | opens [Find](#find) | Ctrl+F (macOS: ⌘F) |
| **Find Next** | [Find Next](#find) | F3 (macOS: ⌘G) |
| *separator* | | |
| **Theme ▸** System / Light / Dark | radio items; see [Theme](#theme) | none |
| *separator* | | |
| **About Anniversary…** | opens [About](#about) | none |

At the move to GitHub the menu also held 002's **Edit People…** (after Find Next, with its own separator) and 003's
**Nameday Calendar ▸** (before Theme ▸). Menu items and separators share a 10 px inset (item padding 10,4,10,4,
separator padding 10,3,10,3), so the lines start where the text starts; the submenu arrow is 5 × 9 instead of
Fluent's 8 × 16.

## Keyboard (main window)

| Keys | Action |
|---|---|
| Ctrl+F (Windows, Linux), ⌘F (macOS) | Find… |
| F3 (all), ⌘G (macOS) | Find Next |

The key handler treats Ctrl and ⌘ alike, so Ctrl+G also runs Find Next on Windows and Linux.

## macOS menu bar

Windows and Linux ignore it; there, everything is in the ⋯ menu.

| Menu | Items |
|---|---|
| app menu (**Anniversary**) | **About Anniversary**; Quit (⌘Q) and the rest come from macOS |
| **View** | **Appearance ▸** System / Light / Dark (radio items, as Theme ▸) |
| **Go** | **Yesterday**, **Tomorrow**, **Today**, **Change Date…** |
| **Find** | **Find…** (⌘F), **Find Next** (⌘G) |

002 added **File ▸ Edit People…** and 003 **View ▸ Nameday Calendar**. Native menu items have no routed `Click` event
and no generated fields, so the main window finds each one by its headers (for example "Go", "Yesterday") and wires
its `Click` in code.

## Change Date

Opened by **Change Date…** (⋯ menu, macOS Go menu) and by clicking the date.

- Title **Change Date**; sized to its content, not resizable, not in the taskbar, centred on the main window, modal.
- A section **Current Date** (at least 240 wide) with a date picker in the long date format, today highlighted, set to
  the shown day. **OK** (accent, Enter) and **Cancel** (Esc) on the right.
- OK shows the picked day; if the picker was cleared, the shown day stays. Cancel changes nothing.
- The dialog keeps about v4's size; the calendar opens as a popup below its edge, at the app's font size, with 28 × 26
  day cells and smaller arrows (about 200 × 210 instead of Fluent's 300 × 320).

## Find

The **Find** dialog: same window settings as Change Date.

- **Name:** and a text box (placeholder "e.g. Petr, Novák…"), then a section **Options** with **From current date**,
  **Whole words only** and **Search calendar only**; **OK** (accent, Enter) and **Cancel** (Esc) on the right.
- It opens with the last values and the name selected.
- **Find…**: on OK the values are kept; an empty name gives the message below; otherwise the search starts at the
  shown day (From current date) or at 1 January of the shown year.
- **Find Next**: with no name kept, it opens Find…; otherwise it searches from the day after the shown day.
- The search runs to 31 December of the following year (rules in
  [anniversarybook-api.md](anniversarybook-api.md#findname-from-wholewordsonly-calendaronly)); a match becomes the
  shown day, otherwise a message says so.

## About

- Title **About Anniversary**; same window settings; about 400 wide.
- The app icon (40 × 40, scaled down from the 256 px image), **Anniversary Application** (bold), **Version 5.0**
  (`Version {Major}.{Minor}` from the assembly version) and **OK** (accent, Enter and Esc) at the top right; below,
  "This program displays names, birthdates, and anniversaries for a specified day." and a muted copyright line.
- Opened by ⋯ ▸ About Anniversary… and, on macOS, by About Anniversary in the app menu.

## Messages

`MessageDialog.Show(owner, title, message)`: a small modal window built in code, with the owner's icon, sized to its
content (240 to 400 wide), not resizable, not in the taskbar, centred on the main window, with the wrapped text and an
**OK** button (accent, Enter and Esc) on the right.

| When | Title | Text |
|---|---|---|
| the configuration couldn't be read (after the window opens) | Configuration Error | `Error loading configuration: {message}`, a line break, `Using default settings.` |
| a calendar or people file problem (after the window opens) | Data Error | `Calendar file not found: {name}`, `Error loading calendar: {message}`, `Data file not found: {name}` or `Error loading data: {message}` |
| Find with an empty name | Find | `Please enter a name to search for.` |
| Find found nothing | Find | `'{name}' not found.` |

## Theme

| Choice | Theme variant | Behaviour |
|---|---|---|
| **System** (default) | `ThemeVariant.Default` | follows the OS light or dark setting live, including scheduled changes (macOS Auto, Windows' app mode and PowerToys Light Switch, the GNOME or KDE colour scheme); no OS-specific code |
| **Light** | `ThemeVariant.Light` | always light |
| **Dark** | `ThemeVariant.Dark` | always dark |

- Choosing an item sets the application's theme variant, so every open window, dialog and menu changes at once, and
  the brushes switch with it; then the choice is saved to `usersettings.json`
  ([settings-files.md](settings-files.md)).
- The radio marks in Theme ▸ and in macOS's View ▸ Appearance always show the current choice.
- At start-up the saved theme is applied before the main window is created.
