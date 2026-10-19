# Tasks: macOS Native Look

**Input**: Design documents from `specs/004-macos-native-look/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, contracts/, quickstart.md

**Tests**: No automated tests. The feature changes only how the app looks, not logic, file formats or settings, so
the constitution (Principle III) asks for manual checks instead: they are the check tasks below, which point at
[quickstart.md](quickstart.md). The 78 existing tests must still pass.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. The work was done in two rounds on the evening of 2026-10-09 in the old
private repository (`Tools/csharp/Anniversary.v5` in `pokluda`), where the maintainer committed it (`be3673f`, not part
of this repository's history); it reached GitHub with `67a4092` ("Moving to GitHub from a private VSO repository",
2026-10-10).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US3)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): the look resources in `Anniversary.Shared/App.axaml.cs`, the styles in
  `Anniversary.Shared/App.axaml`, the main window in `Anniversary.Shared/Views/MainWindow.axaml`, documentation in
  `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and a way to see the window without screenshots

- [X] T001 Start from the state 001–003 left, which the maintainer built and ran on his Mac: `Anniversary.slnx` builds without warnings and the 78 tests pass with `dotnet test --project Anniversary.Tests`, but the window shows Helvetica, bold names, tight lines and an 8 px margin (his screenshot)
- [X] T002 Build an off-screen render harness outside the repository (never committed), because the agent's shell had no screen-recording permission: `Avalonia.Headless` and `Avalonia.Skia` with `UseHeadlessDrawing = false`, rendering the real main window with `RenderTargetBitmap` at 192 DPI (2×, Retina) through the real macOS font manager; it reproduces the maintainer's look (Helvetica) as the "before" render

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: A place for macOS-only values that leaves Windows and Linux alone

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Add an `if (OperatingSystem.IsMacOS())` block to `Initialize` in `Anniversary.Shared/App.axaml.cs`, after the resources every platform already got (`AppFontSize` 12 on Windows and 13 elsewhere, `AppNamesFontSize` +1, `AppDateFontSize` +4, and the calendar drop-down's sizes), so that macOS values replace the defaults before any window is created; each new look value below gets a default equal to what the window had before, so Windows and Linux don't change (FR-008)

**Checkpoint**: macOS can get values of its own without touching Windows or Linux; user stories can begin

---

## Phase 3: User Story 1 - Text as light and clean as on Windows (Priority: P1) 🎯 MVP

**Goal**: On macOS, one clean font, medium-weight names and subtler captions, as on Windows

**Independent Test**: Start the app on a Mac next to the Windows app: names aren't bold, captions are smaller than the
text, and the bold date's words are evenly spaced (quickstart scenario 1)

### Implementation for User Story 1

- [X] T004 [US1] Set Fluent's `ContentControlThemeFontFamily` to `new FontFamily("Helvetica Neue")` in the macOS block in `Anniversary.Shared/App.axaml.cs`, with a comment on why: Skia's macOS default is Helvetica, and the system font (SF), tried first, is a variable font whose bold text Avalonia draws cramped (FR-001)
- [X] T005 [US1] Add `AppStrongFontWeight` (`FontWeight.SemiBold`, and `FontWeight.Medium` in the macOS block, because Helvetica Neue has no semibold) to `Anniversary.Shared/App.axaml.cs`, and use it for the `SelectableTextBlock.names.strong` style in `Anniversary.Shared/App.axaml` and for `CalendarText` in `Anniversary.Shared/Views/MainWindow.axaml` (FR-002)
- [X] T006 [US1] Add `AppCaptionFontSize` (`fontSize`, and `fontSize - 1` in the macOS block, like macOS group box titles) to `Anniversary.Shared/App.axaml.cs`, and use it for the `TextBlock.caption` style in `Anniversary.Shared/App.axaml`, which the main window and the Change Date and Find dialogs use (FR-003)
- [X] T007 [US1] Compare the 2× renders before and after (`before.png` and `after.png`, kept in the session folder, outside the repository), build without warnings and run the 78 tests
- [X] T008 [US1] Check scenario 1 of `specs/004-macos-native-look/quickstart.md` on a Mac

**Checkpoint**: The Mac version is no longer heavier than Windows (the first round)

---

## Phase 4: User Story 2 - Room between the lines (Priority: P2)

**Goal**: On macOS, Windows' line height, a clear caron on Š and a window tall enough for "Next 7 days"

**Independent Test**: On a Mac, go to 9 October 2026 with the Czech calendar: the caron on Š is clearly visible, and
the lines in "Today is" and "Next 7 days" have room (quickstart scenario 2)

### Implementation for User Story 2

- [X] T009 [US2] Add `AppLineHeight`, `AppNamesLineHeight`, `AppDateLineHeight` and `AppCaptionLineHeight` to `Anniversary.Shared/App.axaml.cs`: `double.NaN` (the font's own line height) for all platforms, and in the macOS block `Math.Round` of the text, names, date and caption sizes times `LineHeightFactor` (4/3, Segoe UI's ratio), giving 17, 19, 23 and 16, with a comment on Helvetica Neue's tight lines (about 1.17 em) (FR-004)
- [X] T010 [P] [US2] Set `LineHeight` from those resources in the `TextBlock.caption`, `SelectableTextBlock.names` and `SelectableTextBlock.names.hint` styles in `Anniversary.Shared/App.axaml` (FR-004)
- [X] T011 [P] [US2] Set `LineHeight` on `DateText` (`AppDateLineHeight`) and on `CalendarText` (`AppNamesLineHeight`) in `Anniversary.Shared/Views/MainWindow.axaml` (FR-004)
- [X] T012 [US2] Add `AppWindowHeight` (468, and 492 in the macOS block, so "Next 7 days" keeps about the same room as on Windows) to `Anniversary.Shared/App.axaml.cs` and use it for the main window's `Height` in `Anniversary.Shared/Views/MainWindow.axaml`; `Width` 312 and the minimum 280 × 420 stay (FR-005)
- [X] T013 [US2] Check scenario 2 of `specs/004-macos-native-look/quickstart.md` on a Mac

**Checkpoint**: Accents are clear and the lines have room on macOS

---

## Phase 5: User Story 3 - Controls clear of the rounded window corners (Priority: P3)

**Goal**: On macOS, a wider margin and slightly rounder controls

**Independent Test**: On a Mac, the corners of Yesterday and Quit keep a visible gap from the window's rounded corners,
and the buttons are a little rounder than on Windows (quickstart scenario 3)

### Implementation for User Story 3

- [X] T014 [US3] Extend the render harness (outside the repository) to draw the macOS window's rounded bottom corners (about 16 pt, as on Tahoe-era windows) over the render, so the clearance of Yesterday and Quit is visible; at 8 px the Quit button's corner nearly touched the window's curve
- [X] T015 [US3] Add `AppWindowMargin` (`new Thickness(8)`, and `new Thickness(12)` in the macOS block) to `Anniversary.Shared/App.axaml.cs` and use it for the margin of the content grid in `Anniversary.Shared/Views/MainWindow.axaml` (FR-006)
- [X] T016 [US3] Set Fluent's `ControlCornerRadius` to `new CornerRadius(6)` in the macOS block in `Anniversary.Shared/App.axaml.cs`, with a comment on the large window corners and the rounder native controls (FR-007)
- [X] T017 [US3] Check scenario 3 of `specs/004-macos-native-look/quickstart.md` on a Mac

**Checkpoint**: All three user stories work (the second round)

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, the final checks and the lessons for the next platform

- [X] T018 [P] Update `docs/spec.md` (then `specs/spec.md`) after each round: in 7.1 the "macOS font" bullet, `AppCaptionFontSize` (12 px on macOS) and `AppStrongFontWeight` (Medium on macOS) in "Native sizes", and the "macOS spacing" bullet (the line heights, `AppWindowMargin` 12 instead of 8, `ControlCornerRadius` 6 instead of 4, `AppWindowHeight` 492 instead of 468); in 7.2 the start-up size 312 × 492 on macOS, the margin `AppWindowMargin` (12 on macOS) and `AppStrongFontWeight` for the calendar names
- [X] T019 [P] Update the handover (then `handover-macos.md`, the one the maintainer had brought from Windows) after each round: its check list and a note on the macOS look
- [X] T020 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: the 78 tests pass, none added
- [X] T021 Check the Windows and Linux items of the platform checks in `specs/004-macos-native-look/quickstart.md` (FR-008): their values are the defaults, which equal the earlier ones; the maintainer ran the app on macOS and on Fedora the next day and reported it working fine, and Linux needed no values of its own
- [X] T022 Record the lessons when the handover became the Linux one the next morning (`specs/copilot/handover-linux.md`, 2026-10-10; now "Lessons from the macOS look fixes" in `docs/copilot/handover.md`): Avalonia uses Skia's default font, it draws the bold text of a variable font cramped, a missing semibold falls back to bold, tight line boxes crowd accents, rounded window corners crowd the buttons, headless renders help compare variants, and platform-specific look values are resources set in `App.Initialize`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–5)**: depend on Foundational
- **Polish (Phase 6)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after Foundational; its 4/3 line height was chosen for Helvetica Neue's tight lines, so it
  followed User Story 1
- **User Story 3 (P3)**: after Foundational; uses the render harness (T002) with the corners drawn in (T014)

### Within Each User Story

- The resources in `App.axaml.cs` first, then the styles in `App.axaml` and the main window in `MainWindow.axaml`,
  then the renders and the manual check from quickstart.md
- `App.axaml.cs` holds both the defaults and the macOS block, so the tasks that change it run one after another

### Parallel Opportunities

- T010 and T011: the styles and the main window, once the line-height resources exist (T009)
- T018 and T019: two different documents

---

## Parallel Example: User Story 2

```bash
# Once the line-height resources exist (T009), the styles and the main window can be changed together:
Task: "Set LineHeight in the caption, names and names.hint styles in Anniversary.Shared/App.axaml"
Task: "Set LineHeight on DateText and CalendarText in Anniversary.Shared/Views/MainWindow.axaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (the font, the weights and the captions)

### Incremental Delivery

1. Setup and Foundational: the render harness and the macOS block
2. User Story 1: Helvetica Neue, Medium names and smaller captions (the first round)
3. User Story 2: the line heights and the taller window (the second round)
4. User Story 3: the wider margin and the rounder controls (the second round)
5. Polish: documentation, the test run and the platform checks

### What happened

On the evening of 2026-10-09 the maintainer ran the app on his Mac and found the fonts and the line spacing better on
Windows. The first round (User Story 1) changed the font, the weight of the names and the captions. He then reported
the barely visible caron on Š, the tight lines and the buttons close to the rounded corners, and asked how other macOS
apps handle margins and corners; the second round (User Stories 2 and 3) answered that. Both rounds were checked in
off-screen renders, not in the live app. The assistant had worked in a separate worktree, so the maintainer's own run
showed no difference; at his request ("I don't want you to use worktree. Make the changes directly in my local branch
please.") the changes were copied into his checkout on `main` as a patch, uncommitted, and the worktree copy was
removed. The maintainer committed them in the old repository (`be3673f`), and the next morning the handover became the
Linux one with the macOS lessons (T022).

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- The old repository's history isn't available. Which values are new here and which came with 001 follows the session
  (the captions "was 13", Medium "instead of falling back to Bold", the "8 px margins", the window "24 px taller") and
  the code as it reached GitHub in `67a4092`
- The documentation paths are today's; at the time they were under `specs/`
