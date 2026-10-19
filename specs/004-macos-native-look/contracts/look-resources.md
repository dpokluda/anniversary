# Contract: look resources per platform

The application resources behind the per-platform look, as built on 2026-10-09 and as they reached GitHub in
`67a4092` (`Anniversary.Shared/App.axaml.cs`, `App.Initialize`); later features didn't change them. Behaviour:
[spec.md](../spec.md); system spec: [docs/spec.md](../../../docs/spec.md) 7.1 and 7.2.

## Where they are set

1. `App.Initialize` loads `App.axaml`, then sets every resource below in the application's resources for all
   platforms, with `fontSize = OperatingSystem.IsWindows() ? 12 : 13`.
2. An `if (OperatingSystem.IsMacOS())` block then replaces the macOS values (in bold below). The line heights there are
   `Math.Round(size * 4.0 / 3.0)` for the text, names, date and caption sizes.
3. The styles in `Anniversary.Shared/App.axaml` and the main window in `Anniversary.Shared/Views/MainWindow.axaml`
   read them with `{DynamicResource …}`. They aren't theme resources, so light and dark mode get the same values.
4. A value for another platform goes into an `OperatingSystem.Is…()` block of its own and leaves the other platforms'
   values as they are.

## Values

| Key | Type | Windows | Linux | macOS | Origin |
|---|---|---|---|---|---|
| `AppFontSize` | `double` | 12 | 13 | 13 | 001, unchanged |
| `AppNamesFontSize` | `double` | 13 | 14 | 14 | 001, unchanged |
| `AppDateFontSize` | `double` | 16 | 17 | 17 | 001, unchanged |
| `AppCaptionFontSize` | `double` | 12 | 13 | **12** | new |
| `AppStrongFontWeight` | `FontWeight` | `SemiBold` | `SemiBold` | **`Medium`** | new |
| `AppLineHeight` | `double` | `NaN` | `NaN` | **17** | new |
| `AppNamesLineHeight` | `double` | `NaN` | `NaN` | **19** | new |
| `AppDateLineHeight` | `double` | `NaN` | `NaN` | **23** | new |
| `AppCaptionLineHeight` | `double` | `NaN` | `NaN` | **16** | new |
| `AppWindowMargin` | `Thickness` | 8 | 8 | **12** | new |
| `AppWindowHeight` | `double` | 468 | 468 | **492** | new |
| `ContentControlThemeFontFamily` | `FontFamily` | not set: the default, Segoe UI | not set: the default, fontconfig's `sans-serif` | **Helvetica Neue** (the default was Helvetica) | Fluent's; set on macOS |
| `ControlCornerRadius` | `CornerRadius` | not set: Fluent's 4 | not set: Fluent's 4 | **6** | Fluent's; set on macOS |

`NaN` means the font's own line height. `CalendarFontSize` and `CalendarDayButtonFontSize` (the calendar drop-down's
text sizes) are set in the same place to `AppFontSize`; this feature doesn't change them.

## Consumers

| Resource | Used by |
|---|---|
| `AppCaptionFontSize`, `AppCaptionLineHeight` | the `TextBlock.caption` style: "Today is", "Namedays", "Birthdays and Anniversaries" and "Next N days" in the main window, "Current Date" in `Anniversary.Shared/Views/ChangeDateDialog.axaml`, "Options" in `Anniversary.Shared/Views/FindDialog.axaml` |
| `AppStrongFontWeight` | the `SelectableTextBlock.names.strong` style (the Namedays and Birthdays and Anniversaries names) and `CalendarText` (the calendar names under the date) |
| `AppNamesLineHeight` | the `SelectableTextBlock.names` style (also the "Next N days" text) and `CalendarText` |
| `AppDateLineHeight` | `DateText` (the date) |
| `AppLineHeight` | the `SelectableTextBlock.names.hint` style (the hint shown when there are no people) |
| `AppWindowMargin` | the margin of the main window's content grid; no other window uses it |
| `AppWindowHeight` | the main window's start-up `Height`; `Width` stays 312 and the minimum 280 × 420 |
| `ContentControlThemeFontFamily` | the Fluent theme's font for windows, pop-ups and menus |
| `ControlCornerRadius` | the Fluent theme's corner radius for controls such as buttons |
| `AppFontSize`, `AppNamesFontSize`, `AppDateFontSize` | the text, names and date sizes from 001 (the `Window` style, the menu, tooltip, input, button and data grid styles, the names, the date, the About dialog's title) |

## Not affected

- The section and value boxes keep their own corner radii (`Border.section` 4, `Border.value` 3).
- The date stays `Bold`, and the "Next N days" names stay regular.
- The row spacing (6), the window width and the resizing rules are the same on every platform.
