# Contract: main window

The main window as built on 2026-10-09 (`Anniversary.Shared/Views/MainWindow.axaml`,
`Anniversary.Shared/Views/MainWindow.axaml.cs`, and the styles in `Anniversary.Shared/App.axaml`). The commands are in
[commands-and-dialogs.md](commands-and-dialogs.md). System spec: [docs/spec.md](../../../docs/spec.md) §7.1 and §7.2.
Mocks (approximate, drawn larger than the app):
[light](../../../docs/screens/main-window.light.preview.png),
[dark](../../../docs/screens/main-window.dark.preview.png),
[resized](../../../docs/screens/main-window-resized.light.preview.png).

## Window

| Property | Value |
|---|---|
| Title | **Anniversary** |
| Icon | `Anniversary.Shared/Assets/App.ico` |
| Start-up size | 312 × 468 device-independent pixels: v4's 464 × 701 client size, which v4 gave in physical pixels at 150 % scaling |
| Minimum size | 280 × 420 |
| Position | centred on the screen |
| Size remembered between runs | no, as in v4 |
| Margin around the content | 8 |

The start-up height and the margin are the resources `AppWindowHeight` and `AppWindowMargin`, which
[004](../../004-macos-native-look/spec.md) set to 492 and 12 on macOS.

## Layout

One column; rows `Auto, Auto, Auto, *, Auto` with a spacing of 6:

| Row | Content |
|---|---|
| 0 | Section **Today is**: the caption on the left and the **⋯** button on the right (a subtle button with a 12 px three-dot icon, tooltip *More commands*). Below it the **date** (the date font size, bold, centred, wraps; hand cursor, tooltip *Click to change the date*), a separator line, and the **calendar names** (the names font size, semibold, italic, centred, wraps). |
| 1 | Section **Namedays**: a value box with a fixed height of 70 that scrolls, holding selectable text (semibold italic, centred, wraps), one name per line. |
| 2 | Section **Birthdays and Anniversaries**: the same, one entry per line. With no people at all it shows the hint instead (muted, regular, not italic). |
| 3 | Section **Next {N} days**: the same, but the value box fills the remaining height, and the text is regular italic: line 1 the people, line 2 the calendar names, each joined with ", "; an empty line is left out. |
| 4 | **Yesterday** and **Tomorrow** on the left, **Quit** (accent colour) on the right; buttons at least 75 wide; a column spacing of 6. |

The code's button spacing is 6; the spec said 8 until the docs update of 2026-10-10 matched it to the code.

## Texts

| Element | Text |
|---|---|
| Date | the shown day in the current culture's long date format (`"D"`), e.g. "Friday, October 9, 2026" in English (US) |
| Calendar names | the calendar text for the day, e.g. "Štefan/Sára"; empty if there is none |
| Next N days caption | `Next {N} days`, N from `numberOfDaysAhead` |
| Hint | `No people yet. Use ⋯ ▸ Edit People… to add some.` (the editor came with [002](../../002-people-editor/spec.md)) |

## Look

- Fluent theme with `DensityStyle="Compact"`; the app's theme variant starts as `Default` (the OS light or dark
  setting) and is then replaced by the saved choice ([commands-and-dialogs.md](commands-and-dialogs.md#theme)).
- Font sizes, set in `App.Initialize` and applied to windows, menus, tooltips, inputs, buttons and the calendar:
  `AppFontSize` 12 on Windows (Segoe UI 9 pt, as in v4) and 13 on macOS and Linux; `AppNamesFontSize` one more;
  `AppDateFontSize` four more. The system font is used; none is bundled.
- Style classes: `section` (background `SectionBackground`, 1 px `SectionBorder`, corner radius 4, padding 8,4,8,8);
  `caption` (`SectionHeader` colour, bottom margin 4); `value` (background `ValueBackground`, 1 px `ValueBorder`,
  corner radius 3, padding 6,4); `names` (italic, names font size, centred, wraps); `cmd` (minimum width 75).
- Brushes by theme:

| Key | Light | Dark |
|---|---|---|
| `SectionBackground` | `#F7F7F7` | `#2B2B2B` |
| `SectionBorder` | `#E0E0E0` | `#3A3A3A` |
| `ValueBackground` | `#FFFFFF` | `#202020` |
| `ValueBorder` | `#D0D0D0` | `#454545` |
| `SectionHeader` | `#5A5A5A` | `#B8B8B8` |
| `SeparatorBrush` | `#D6D6D6` | `#4A4A4A` |

`SeparatorBrush` colours every separator (under the date and in the menus) as a subtle 1 px line.

## Resizing (v4's anchors)

| Element | v4 anchor | Wider | Taller |
|---|---|---|---|
| "Today is" with the date, line and calendar names | Top, Left, Right | stretches; texts stay centred; ⋯ stays at the top-right | unchanged, at the top |
| Namedays box | Top, Left, Right (fixed height) | stretches | unchanged height |
| Birthdays and Anniversaries box | Top, Left, Right (fixed height) | stretches | unchanged height |
| Next N days box | Top, Bottom, Left, Right | stretches | takes all the extra height |
| Yesterday, Tomorrow | Bottom, Left | stay at the bottom-left | stay at the bottom |
| Quit | Bottom, Right | stays at the bottom-right | stays at the bottom |

Text wraps to the box width; a box whose text is taller than the box scrolls, and the other boxes don't move. The date
and the calendar names wrap at the minimum width instead of being cut off.

## Start-up and closing

- The shown day is today (`DateOnly.FromDateTime(DateTime.Today)`), and keyboard focus is on **Quit**, as in v4.
- After the window has opened, each load warning is shown in its own message window, one after the other
  ([commands-and-dialogs.md](commands-and-dialogs.md#messages)).
- **Quit** closes the window, which ends the app.
- A parameterless constructor (built-in defaults, today, default settings) lets the XAML previewer show the window.
