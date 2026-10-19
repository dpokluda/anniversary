# Research: macOS Native Look

Decisions made while the feature was built on the evening of 2026-10-09, in the old private repository. Sources: the
Copilot session of 2026-10-09 (the maintainer's two requests and the assistant's answers),
[docs/spec.md](../../docs/spec.md) 7.1 and 7.2, "Lessons from the macOS look fixes" in
[docs/copilot/handover.md](../../docs/copilot/handover.md) (written the next morning as the macOS lessons in
`specs/copilot/handover-linux.md`), and the code as it reached GitHub
(`git show 67a4092:Anniversary.Shared/App.axaml.cs`). Alternatives are listed only where a source names them.

## Font on macOS: Helvetica Neue

- **Decision**: On macOS, `App.Initialize` sets Fluent's `ContentControlThemeFontFamily` to Helvetica Neue, which covers
  windows, pop-ups and menus.
- **Rationale**: Avalonia draws text with Skia, whose default font on macOS is Helvetica, not the system font.
  Helvetica has no semibold, so every semibold name was drawn bold, which made the Mac version look heavier. Helvetica
  Neue is a static font, renders correctly and looks lighter and cleaner. The assistant checked how Avalonia resolves
  family names, to pick a family that also gives a real italic and semibold (the names are semibold italic); Helvetica
  Neue has no semibold, hence Medium (below).
- **Alternatives considered**: the system font, San Francisco (SF), tried first; the first plan was to make it the
  default family through `FontManagerOptions.DefaultFamilyName`. SF is a variable font, and Avalonia takes glyph
  advances from its regular instance, so its bold text is drawn cramped ("Sunday,October11,2026"); the renders also
  showed tight word spacing and taller lines, which the assistant measured against CoreText. So SF "can't be used for
  now". Helvetica, the default, was the heavy look being fixed.
- **Source**: session 2026-10-09; spec 7.1; the comment in `App.Initialize`.

## Weight of the semibold names: Medium

- **Decision**: `AppStrongFontWeight` is SemiBold, and Medium on macOS. The names in Namedays and in Birthdays and
  Anniversaries (`SelectableTextBlock.names.strong`) and the calendar names under the date (`CalendarText`) use it.
- **Rationale**: Helvetica Neue has no semibold, which falls back to bold. Medium is the closest; the next step down,
  Regular, looks plain and loses the emphasis Windows has.
- **Alternatives considered**: SemiBold (drawn as bold); Regular, which the assistant offered in case Medium still
  looked too heavy to the maintainer, who had found Windows' names "slightly less bold". Medium was kept.
- **Source**: session 2026-10-09; spec 7.1 and 7.2.

## Caption size: one size smaller on macOS

- **Decision**: `AppCaptionFontSize` is the text size (`AppFontSize`), and one less on macOS (12 px); the
  `TextBlock.caption` style uses it.
- **Rationale**: The maintainer found the labels more subtle on Windows. A smaller caption is like a macOS group box
  title.
- **Alternatives considered**: none recorded; the captions were 13 px, the text size, before.
- **Source**: session 2026-10-09 ("12 px on macOS (was 13)"); spec 7.1.

## Text size: kept from 001

- **Decision**: The text stays 13 px on macOS (`AppFontSize`), with the names at 14 px and the date at 17 px.
- **Rationale**: 001 set the platform's standard interface font size: 12 px on Windows (Segoe UI 9 pt, as in v4) and
  13 px on macOS (the size of the macOS system font) and Linux. This feature changed the font, not the sizes; only the
  captions on macOS got smaller.
- **Alternatives considered**: none recorded.
- **Source**: spec 7.1 ("Native sizes"); the comment in `App.Initialize`; session 2026-10-09.

## Line height: 4/3 of the font size

- **Decision**: On macOS, `AppNamesLineHeight`, `AppDateLineHeight`, `AppCaptionLineHeight` and `AppLineHeight` are
  4/3 of their font size, rounded (19, 23, 16 and 17 px); elsewhere they are `NaN`, the font's own line height. The
  names, the calendar names, the date, the captions and the hint shown when there are no people use them.
- **Rationale**: Helvetica Neue's line box is tight (about 1.17 em, against about 1.33 em for Segoe UI), so the caron
  on Š nearly touched the line above, and "Today is" and "Next 7 days" looked cramped. With Segoe UI's ratio, Avalonia
  centres the glyphs in the taller line.
- **Alternatives considered**: none recorded; Avalonia's line-spacing options were checked first.
- **Source**: session 2026-10-09; spec 7.1; the handover's lessons.

## Start-up height: 492 on macOS

- **Decision**: `AppWindowHeight` is 468, and 492 on macOS (24 px taller); the main window's `Height` uses it.
- **Rationale**: With the taller lines, the taller window lets "Next 7 days" keep about the same room as on Windows.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-09; spec 7.1 and 7.2.

## Window margin: 12 on macOS

- **Decision**: `AppWindowMargin` is 8, and 12 on macOS; it is the margin of the main window's content.
- **Rationale**: macOS windows have large rounded corners; at 8 px, the Quit button's corner nearly touched the window
  curve in a render. Apple's guidelines suggest margins of about 20 pt, and newer macOS versions nest corners so that
  they share a centre: a control's corner radius is about the window's corner radius minus its distance from the edge.
  With 12 px the render "looks balanced".
- **Alternatives considered**: 8 px (too tight); about 20 pt as in Apple's guidelines (not taken; no reason is recorded
  beyond the balanced render at 12 px).
- **Source**: session 2026-10-09; spec 7.1.

## Control corners: 6 on macOS

- **Decision**: On macOS, Fluent's `ControlCornerRadius` is 6 instead of 4.
- **Rationale**: Native macOS buttons are rounder than Fluent's, so the controls are a little rounder. The maintainer
  had asked, without proposing it, whether the buttons should be more rounded.
- **Alternatives considered**: full capsule buttons, which would look out of place here; Fluent's 4.
- **Source**: session 2026-10-09; spec 7.1.

## Per-platform values as resources in `App.Initialize`

- **Decision**: Every value that differs is an application resource set in `App.Initialize`: the values for all
  platforms first, then the macOS values in an `OperatingSystem.IsMacOS()` block. The styles and the main window read
  them with `DynamicResource`.
- **Rationale**: The platform text sizes from 001 were already set there. One block per platform keeps the other
  platforms unchanged; the handover written the next morning says that Windows and Linux use the defaults and that a
  Linux fix "should add an `OperatingSystem.IsLinux()` block there".
- **Alternatives considered**: none recorded.
- **Source**: `specs/copilot/handover-linux.md` (2026-10-10); spec 7.1; `Anniversary.Shared/App.axaml.cs`.

## Verification: off-screen renders

- **Decision**: Compare the variants in off-screen renders of the real main window, made with `Avalonia.Headless` and
  `Avalonia.Skia` (`UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })`, then
  `RenderTargetBitmap` at 192 DPI, which is 2× like a Retina screen), in a harness kept outside the repository. First
  reproduce the maintainer's screenshot (Helvetica). For the margins, the harness drew the macOS window's rounded bottom
  corners over the render (about 16 pt on Tahoe-era windows; corner radii differ a lot between macOS Sequoia and
  Tahoe). The before and after renders stayed in the session folder.
- **Rationale**: The agent's shell had no screen-recording permission, so it couldn't capture the running app. The
  headless renderer uses the real macOS font manager, so it shows the same fonts as the app. A render isn't the live
  app, so the maintainer's own run was the final check.
- **Alternatives considered**: screenshots of the running app (not possible).
- **Source**: session 2026-10-09; the handover's lessons ("Headless renders for comparing variants").

## Where the changes were made

- **Decision**: In the maintainer's own checkout, on `main`, uncommitted; the maintainer commits.
- **Rationale**: The assistant first worked in a separate worktree, so when the maintainer ran his checkout he saw
  no difference. He asked: "I don't want you to use worktree. Make the changes directly in my local branch please." The
  changes were copied over as a patch after checking that the checkout had no conflicting edits, and the worktree copy
  was removed.
- **Alternatives considered**: committing on the worktree's branch for the maintainer to merge into `main` (the
  assistant's offer); the maintainer chose his own checkout.
- **Source**: session 2026-10-09.
