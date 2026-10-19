# Quickstart: macOS Native Look

How to check the feature as built on 2026-10-09. Expected behaviour: [spec.md](spec.md); the values per platform:
[contracts/look-resources.md](contracts/look-resources.md). There are no automated tests for it: the checks below are
done by eye.

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- A Mac (the feature was checked on Apple Silicon, macOS 27) and, for comparison, the app on Windows or a screenshot of
  it; a Linux desktop for the platform checks.
- Optional: a people data file, for example v4's `Anniversary.Data.json`, in `~/Library/Application Support/Anniversary`
  (as at the time), so that Namedays and Birthdays and Anniversaries show names.

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 78 passing tests, none of them added by this feature.

## Scenario 1: text as light as on Windows (User Story 1)

1. Start the app on the Mac. **Expected**: the text is in Helvetica Neue, and the bold date shows normal spaces between
   its words (not run together like "Sunday,October11,2026").
2. Look at the calendar names under the date, and at the names in Namedays and in Birthdays and Anniversaries.
   **Expected**: a medium weight, not bold.
3. Look at the captions "Today is", "Namedays", "Birthdays and Anniversaries" and "Next 7 days". **Expected**: 12 px,
   a size smaller than the 13 px text.
4. Open the ⋯ menu (or right-click), **Change Date…** and **Find…**. **Expected**: the same font everywhere; the
   captions "Current Date" and "Options" are as small as those in the main window.

## Scenario 2: room between the lines (User Story 2)

1. Choose the Czech calendar, **Czechia (Česko)**: ⋯ ▸ Nameday Calendar at the time (now ⋯ ▸ Calendar; on macOS the
   **View** menu). At the time it was the default. Click the date (or choose ⋯ ▸ Change Date…) and pick 9 October 2026.
   **Expected**: "Štefan/Sára" under the date, with the caron on Š clearly visible and clear of the line above.
2. **Expected**: the date, the separator line and the calendar names in "Today is" have room between them, and the
   lines in "Next 7 days" are spaced as on Windows (4/3 of the text size).
3. Quit and start the app again. **Expected**: the window opens at 312 × 492, 24 px taller than on Windows, and "Next 7
   days" shows about as much as on Windows.
4. Without a data file (no people at all), look at the hint in Birthdays and Anniversaries. **Expected**: the same
   taller lines.

## Scenario 3: controls clear of the rounded corners (User Story 3)

1. Look at the bottom corners of the main window. **Expected**: 12 px between the content and the window edges; the
   corners of Yesterday and Quit keep a visible gap from the window's curve.
2. Look at the buttons in the main window and the dialogs. **Expected**: a little rounder than on Windows (6 px
   instead of 4), not capsules.

## Platform checks

- **Windows** (FR-008; docs/spec.md §12, criterion 2): as before this feature and as in docs/spec.md 7.1 and 7.2:
  Segoe UI at 12 px, semibold names, captions at the text size, the font's own line height, an 8 px margin, 4 px
  control corners, a start-up size of 312 × 468.
- **Linux** (FR-008): the same as Windows, with 13 px text in the desktop's default font. At the time Linux hadn't been
  run; the first run on Fedora the next day needed no Linux-specific values.
- **Themes** (all stories): switching ⋯ ▸ Theme ▸ Light and Dark (macOS **View ▸ Appearance**) changes the colours
  only; the font, weights, line spacing, margin and corners stay the same.
- **Resizing** (criterion 5): on macOS, as on Windows, only "Next N days" grows taller, and the minimum size is
  280 × 420.

## Off-screen renders (for agents)

At the time, the agent couldn't take screenshots (its shell had no screen-recording permission), so it compared the
variants in off-screen renders of the real main window: `Avalonia.Headless` with `Avalonia.Skia` and
`UseHeadlessDrawing = false`, then `RenderTargetBitmap` at 192 DPI (2×, like a Retina screen). The renderer uses the
real macOS font manager, so it shows the same fonts as the app. To judge the margins, the harness drew the window's
rounded bottom corners (about 16 pt) over the render. Keep such a harness outside the repository; details are in
"Lessons from the macOS look fixes" in [docs/copilot/handover.md](../../docs/copilot/handover.md). A render isn't the
live app: the final check is running the app on a Mac.
