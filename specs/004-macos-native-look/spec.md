# Feature Specification: macOS Native Look

**Feature Branch**: `004-macos-native-look`

**Created**: 2026-10-09

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "Attached is handover document from another session on PC where we created Anniversary v5
application (…). I have now compiled and ran it on MacOS, attached is the screenshot. The program works fine but it
seems that the fonts, line spacing is better on Windows (labels are more subtle, etc.)", and after the first fix: "I
still think the fonts are slightly off: - if you look at today's nameday (below the today's date, you can hardly see
that the first letter has special character on top `Š`. Also on Windows this particular font is slightly less bold
(weight) which looks a little better. - the line spacing is too tight: - in today is group (between today's date,
separator line and nameday) - within next 7 days. Also, it seems the rounded corners on MacOS are bigger and it seems
that lower/left part of Yesterday button gets too close to the edge (similarly Quit button). Don't you think? How do
other MacOS apps solve this? Do they have bigger margin on the sides? Should the buttons be also more rounded (I am not
proposing this, I am asking)?"

## Clarifications

### Session 2026-10-09

- Q: Which look should macOS aim for? → A: Windows': the maintainer found its fonts and line spacing better and its
  labels more subtle. Every change applies on macOS only; Windows and Linux keep their look.
- Q: Which font should the app use on macOS? → A: Helvetica Neue. The app was getting Helvetica, the default, which has
  no semibold, so the semibold names came out bold. The system font (San Francisco) was tried first, but the app draws
  it cramped in bold ("Sunday,October11,2026"), so it can't be used for now.
- Q: How are the labels made as subtle as on Windows? → A: On macOS the section captions are one size smaller than the
  text (12 px instead of 13), like macOS group box titles.
- Q: Windows' names look a little less bold; should macOS use Regular instead of Medium? → A: Medium, the closest to
  semibold that Helvetica Neue has; Regular looks plain and loses the emphasis Windows has. The assistant offered to
  switch to Regular if Medium still looked too heavy; Medium was kept.
- Q: The lines are too tight (in "Today is" and in "Next 7 days") and the caron on Š is hard to see; what changes? →
  A: On macOS the text gets Windows' taller line height (4/3 of the text size; Helvetica Neue's own is about 1.17
  times it), with the letters centred in each line, and the window opens 24 px taller (312 × 492), so "Next 7 days"
  keeps about the same room as on Windows.
- Q: How do other macOS apps keep controls clear of the large rounded window corners; do they use bigger margins? → A:
  Apple's guidelines suggest margins of about 20 pt, and newer macOS versions nest corners (a control's radius is about
  the window's radius minus its distance from the edge). An 8 px margin was too tight (in a render, the Quit button's
  corner nearly touched the window's curve), so macOS gets 12 px.
- Q: Should the buttons be more rounded too (asked, not proposed)? → A: A little: the controls' corners go from 4 to
  6 px on macOS, because native macOS buttons are rounder; full capsule buttons would look out of place.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Text as light and clean as on Windows (Priority: P1)

Someone who runs the app on a Mac sees text as light and clean as on Windows: one clean font in every window, the
names in Namedays and in Birthdays and Anniversaries and the calendar names under the date in a medium weight instead
of bold, and section captions a size smaller than the text, so the labels stay subtle. The bold date reads with normal
spaces between its words.

**Why this priority**: It is what the maintainer noticed first: the Mac version looked heavier than Windows. The font
chosen here also sets the line spacing that User Story 2 fixes.

**Independent Test**: Start the app on a Mac and put it next to the Windows app: the names aren't bold, the captions
are smaller than the text, and the date's words are evenly spaced.

**Acceptance Scenarios**:

1. **Given** the app on macOS, **When** the main window opens, **Then** its text, its dialogs and the ⋯ menu use
   Helvetica Neue, and the bold date shows normal spaces between its words.
2. **Given** names in Namedays or in Birthdays and Anniversaries, or calendar names under the date, **When** they are
   shown on macOS, **Then** they are in a medium weight, not bold.
3. **Given** the section captions ("Today is", "Namedays", "Birthdays and Anniversaries", "Next 7 days", and "Current
   Date" and "Options" in the Change Date and Find dialogs), **When** they are shown on macOS, **Then** they are 12 px,
   one size smaller than the 13 px text.
4. **Given** the app on Windows or Linux, **When** it starts, **Then** its font, its semibold names and its captions at
   the text size are as before.

---

### User Story 2 - Room between the lines (Priority: P2)

On a Mac, accents on capital letters are clear and the lines aren't cramped. On 9 October with the Czech calendar, the
caron on Š in "Štefan/Sára" under the date is easy to see; the date, the separator line and the calendar names in
"Today is" have room between them; and the lines in "Next 7 days" are spaced as on Windows. The window opens a little
taller, so "Next 7 days" keeps about the same room as on Windows.

**Why this priority**: The maintainer reported it after the font fix: the accent on the first letter of that day's
name day was hard to see, and the lines looked cramped.

**Independent Test**: On a Mac, go to 9 October 2026 with the Czech calendar: the caron on Š is clearly visible, and
the lines in "Today is" and "Next 7 days" have space between them.

**Acceptance Scenarios**:

1. **Given** 9 October 2026 and the Czech calendar on macOS, **When** the main window shows "Štefan/Sára" under the
   date, **Then** the caron on Š is clearly visible and keeps clear of the line above.
2. **Given** the "Today is" section on macOS, **When** it shows the date, the separator line and the calendar names,
   **Then** the date and the calendar names each get a line 4/3 of their text size tall, as on Windows, so they have
   room around the separator line.
3. **Given** several lines in "Next 7 days" on macOS, **When** they are shown, **Then** they are spaced 4/3 of their
   text size apart, as on Windows.
4. **Given** the app is started on macOS, **When** the main window opens, **Then** it is 312 × 492 instead of
   312 × 468, and "Next 7 days" has about the same room as on Windows.
5. **Given** the app on Windows or Linux, **When** it starts, **Then** its line spacing and its start-up size
   (312 × 468) are as before.

---

### User Story 3 - Controls clear of the rounded window corners (Priority: P3)

macOS windows have large rounded corners. On a Mac, the main window's content keeps a wider margin, so the Yesterday
and Quit buttons stay clear of the rounded bottom corners, and the buttons and the other controls are a little rounder,
closer to native macOS controls, without becoming capsules.

**Why this priority**: A finishing touch that the maintainer raised last, partly as a question; the app works without
it.

**Independent Test**: On a Mac, look at the bottom corners of the main window: the corners of Yesterday and Quit keep a
visible gap from the window's curve, and the buttons are slightly rounder than on Windows.

**Acceptance Scenarios**:

1. **Given** the main window on macOS, **When** it opens, **Then** its content keeps 12 px from the window edges, and
   the bottom corners of Yesterday and Quit stay clear of the window's rounded corners.
2. **Given** buttons and other controls on macOS, in the main window and in the dialogs, **When** they are shown,
   **Then** their corners have a 6 px radius instead of 4, and they aren't capsule-shaped.
3. **Given** the app on Windows or Linux, **When** it starts, **Then** the margin (8 px) and the controls' corners
   (4 px) are as before.

### Edge Cases

- **The system font**: macOS's own interface font (San Francisco) isn't used, because the app draws it cramped in bold,
  with the words run together; Helvetica Neue is used instead for now.
- **No semibold**: Helvetica Neue has no semibold, which would fall back to bold, so macOS uses medium for the names
  that are semibold elsewhere. The bold date stays bold, and the names in "Next N days" stay regular.
- **Light and dark**: the macOS values are the same in both themes; switching the theme doesn't change them.
- **Dialogs and menus**: the font and the rounder controls apply in every window and pop-up, including the dialogs, the
  people editor and the ⋯ menu, and the smaller captions in the Change Date and Find dialogs too. The wider margin and
  the taller start-up size apply to the main window only.
- **Resizing**: only the start-up height changes on macOS; the width (312), the minimum size (280 × 420) and v4's
  resizing rules (only "Next N days" grows taller) stay as on the other platforms.
- **Text sizes**: the text stays 13 px on macOS (names 14, the date 17), as before this feature; only the captions get
  smaller.
- **Linux**: it keeps the values Windows has, with its 13 px text. A fix for Linux would get values of its own without
  changing macOS or Windows.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: On macOS, the app MUST draw the text of its windows, dialogs and pop-up menus in Helvetica Neue instead of
  the default it got (Helvetica).
- **FR-002**: On macOS, the names that are semibold on Windows and Linux (in Namedays, in Birthdays and Anniversaries,
  and the calendar names under the date) MUST use a medium weight, not bold.
- **FR-003**: On macOS, section captions MUST be one size smaller than the text: 12 px instead of 13, in the main window
  and in the dialogs that have captions (Change Date, Find).
- **FR-004**: On macOS, the names, the date, the calendar names, the section captions and the hint shown when there are
  no people MUST get a line height of 4/3 of their text size, rounded to whole pixels, with the letters centred in the
  line, so accents on capital letters keep clear of the line above.
- **FR-005**: On macOS, the main window MUST open at 312 × 492 instead of 312 × 468, so "Next N days" keeps about the
  same room as on Windows; its width, its minimum size (280 × 420) and its resizing rules MUST stay as on the other
  platforms.
- **FR-006**: On macOS, the main window's content MUST keep a margin of 12 px from the window edges instead of 8 px.
- **FR-007**: On macOS, buttons and the other controls MUST have a corner radius of 6 px instead of 4 px, without
  becoming capsule-shaped.
- **FR-008**: Windows and Linux MUST look exactly as before this feature: the same font, semibold names, captions at the
  text size, the font's own line height, an 8 px margin, 4 px control corners and a start-up size of 312 × 468.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On macOS, none of the names that are semibold on Windows is drawn bold, and the bold date shows its words
  with normal spacing (no run-together words such as "Sunday,October11,2026").
- **SC-002**: On 9 October 2026 with the Czech calendar, the caron on "Š" in "Štefan/Sára" is clearly visible on macOS
  and keeps clear of the line above.
- **SC-003**: On macOS, the lines are 4/3 of their text size apart, the same ratio as on Windows: 19 px for the 14 px
  names, 23 px for the 17 px date, 16 px for the 12 px captions and 17 px for the 13 px hint.
- **SC-004**: On macOS, the main window opens 24 px taller than on Windows and Linux (312 × 492), and its content keeps
  12 px from the edges, 4 px more than elsewhere, so the corners of Yesterday and Quit stay visibly clear of the
  window's rounded corners.
- **SC-005**: On Windows and Linux, every look value (font, weights, caption size, line height, margin, corner radius,
  start-up size) is the same as before the feature.
- **SC-006**: The build has no warnings, and all 78 existing tests pass unchanged.
- **SC-007**: The maintainer keeps the macOS look after running the app on his Mac: he committed it and, the next day,
  reported that the app works fine on macOS and Fedora.

## Assumptions

- Builds on [001](../001-cross-platform-rewrite/spec.md): the main window with v4's layout and size (312 × 468), the
  compact look, and the platform text sizes: 12 px on Windows (Segoe UI 9 pt, as in v4) and 13 px on macOS and Linux,
  with names 1 px and the date 4 px larger. These sizes predate this feature and don't change; the captions on macOS
  are the only size that does.
- The dialogs and menus of [001](../001-cross-platform-rewrite/spec.md), [002](../002-people-editor/spec.md) (Edit
  People… and the Anniversaries dialog) and [003](../003-calendar-chooser/spec.md) (the Nameday Calendar menu) get the
  macOS font and the rounder controls too.
- Windows is the reference look; it had been checked against v4 on Windows before this feature.
- The look was checked on an Apple Silicon Mac (macOS 27), first in off-screen renders and then by the maintainer, who
  ran the app; Intel Macs (`osx-x64`) weren't run.
- Linux hadn't been run yet. It was first run on Fedora the next day and needed no values of its own.
- No font is bundled with the app ([docs/spec.md](../../docs/spec.md) 7.1); Helvetica Neue is one of the fonts macOS
  provides.
- Out of scope: the rest of the macOS check list (the menu bar and the theme switching among others), and an `.app`
  bundle with its own icon, which came with [010](../010-install-packages/spec.md).
