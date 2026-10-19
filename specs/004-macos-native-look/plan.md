# Implementation Plan: macOS Native Look

**Branch**: `004-macos-native-look` | **Date**: 2026-10-09 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/004-macos-native-look/spec.md`

## Summary

Make the app look as light and clean on macOS as it does on Windows, without changing Windows or Linux. On macOS the
app was getting Skia's default font, Helvetica, which has no semibold, and its lines were too tight for accents such as
the caron on Š. Every value that differs is an application resource set in `App.Initialize`: all platforms get the
values the window already had, and an `OperatingSystem.IsMacOS()` block replaces them with Helvetica Neue (Fluent's
`ContentControlThemeFontFamily`), Medium for the semibold names (`AppStrongFontWeight`), 12 px captions
(`AppCaptionFontSize`), line heights at 4/3 of the font size (`AppLineHeight`, `AppNamesLineHeight`,
`AppDateLineHeight`, `AppCaptionLineHeight`), a 12 px window margin (`AppWindowMargin`), Fluent's `ControlCornerRadius`
of 6 and a start-up height of 492 (`AppWindowHeight`). The styles in `App.axaml` and the main window read them as
dynamic resources. The variants were compared in off-screen renders of the real window with the macOS font manager,
because the agent couldn't take screenshots; the maintainer then ran the app on his Mac.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`)

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme (`DensityStyle="Compact"`), whose
`ContentControlThemeFontFamily` and `ControlCornerRadius` resources are replaced on macOS; Skia draws the text with the
fonts macOS provides. No new packages: `Avalonia.Headless` and `Avalonia.Skia` were used only by a render harness kept
outside the repository.

**Storage**: N/A (no files or settings change)

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`dotnet test --project Anniversary.Tests`): the 78 existing tests
pass and none were added, because only the look changes. The variants were compared in off-screen renders at 2×; the
look is checked by hand on a Mac and on Windows and Linux ([quickstart.md](quickstart.md)).

**Target Platform**: Windows, macOS and Linux desktops (then published for `win-x64`, `osx-arm64`, `osx-x64` and
`linux-x64`); the changes apply on macOS only and were checked on an Apple Silicon Mac (macOS 27)

**Project Type**: desktop app

**Performance Goals**: none specific; the values are set once when the app starts.

**Constraints**: Windows and Linux must not change; keep v4's compact layout, the window's width, minimum size and
resizing rules, and the platform text sizes from 001; no bundled font (spec 7.1); no platform code in the shared
project beyond the `OperatingSystem.IsMacOS()` check.

**Scale/Scope**: one main window, its dialogs and menus; 8 new resources (`AppCaptionFontSize`, `AppStrongFontWeight`,
the four `App…LineHeight` resources, `AppWindowMargin`, `AppWindowHeight`) and 2 Fluent resources, giving 10 macOS
values in one `if` block; 3 source files changed.

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | No new type, project or package. The look values are application resources set in `App.Initialize`; `AnniversaryBook` and the views' code-behind don't change. The render harness (`Avalonia.Headless`, `Avalonia.Skia`) stayed outside the repository. |
| II. Established Coding Style and Complete XML Documentation | PASS | No new public member. The macOS block follows the v4 style, and its comments explain only what needs it: Skia's Helvetica default, the cramped variable system font, the missing semibold, the tight line box and the rounded window corners. The build has no warnings. |
| III. Classic Tests on Real Objects | PASS with justification | No logic, file format or setting changes, so no test was added; the 78 tests still pass. The look is behaviour that only a real OS shows: it was compared in off-screen renders that use the real macOS font manager, and the maintainer ran the app on his Mac; [quickstart.md](quickstart.md) lists the manual checks. docs/spec.md §12 got no criterion of its own for the macOS look (criterion 2 covers Windows). |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | The macOS values are in one `OperatingSystem.IsMacOS()` block, and Windows and Linux keep the values they had. The platform text size in `App.Initialize` came with 001; this feature added the per-platform block and the rule that a fix for one platform leaves the others alone (the next morning's handover: a Linux fix "should add an `OperatingSystem.IsLinux()` block there"). The agent could only render off-screen (its shell had no screen-recording permission); the maintainer committed the result and, the next day, reported the app working fine on macOS and Fedora. Intel Macs (`osx-x64`) weren't run, and no Windows run after the change is recorded (its values didn't change). |
| V. Safe, Private and Compatible User Data | N/A | No data, file or setting changes. |
| VI. Verifiable Calendar Data | N/A | No calendar data changes. |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer asked for the change and kept the result; the choice between Medium and Regular was put to him, and Medium was kept. docs/spec.md 7.1 and 7.2 (then `specs/spec.md`) and the handover changed with the code. No §11 entry: v4 ran only on Windows, so this isn't a change from v4. `README.md` wasn't changed; its Mac check list still said "the system font" until 005 replaced it. The user guide didn't exist yet (009). The maintainer asked for the changes to be made in his own checkout instead of a worktree, one of the directions behind the constitution's rule for agents. |

**Post-design re-check**: PASS. The design adds resources and style setters only: no type, package or platform code.
The gaps noted under III and IV remain: no §12 criterion for the macOS look, no recorded Windows run after the change,
and `osx-x64` not run.

## Project Structure

### Documentation (this feature)

```text
specs/004-macos-native-look/
├── spec.md                 # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md     # spec quality checklist
├── plan.md                 # this file
├── research.md             # Phase 0
├── quickstart.md           # Phase 1
├── contracts/              # Phase 1
│   └── look-resources.md   # the look resources and their values per platform
└── tasks.md                # /speckit-tasks
```

There is no `data-model.md`: the feature has no data.

### Source Code (repository root)

```text
Anniversary.Shared/
├── App.axaml                  # changed: the caption style takes its size and line height, the names and names.hint
│                              #   styles their line height and names.strong its weight from the look resources
├── App.axaml.cs               # changed: the new look resources with their defaults, and the macOS block in Initialize
└── Views/
    └── MainWindow.axaml       # changed: start-up height and content margin from resources; line height of the date
                               #   and the calendar names; weight of the calendar names
docs/spec.md                   # changed (then specs/spec.md): 7.1 and 7.2
docs/copilot/handover.md       # changed (then handover-macos.md; see tasks.md)
```

**Structure Decision**: Everything stays in `Anniversary.Shared`: the values are resources, so no platform code is
needed beyond the `OperatingSystem.IsMacOS()` check, and `Anniversary.Desktop` and `Anniversary.Tests` are unchanged.
No file is added.

## Complexity Tracking

No violations.
