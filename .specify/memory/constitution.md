<!--
Sync Impact Report
==================
Version change: template (unversioned) → 1.0.0 (initial ratification)
Bump rationale: first concrete constitution replacing the Spec Kit template, so the baseline is 1.0.0.

Principles (template placeholder → new title):
  - [PRINCIPLE_1_NAME] → I. Concise Code-Behind Architecture
  - [PRINCIPLE_2_NAME] → II. Established Coding Style and Complete XML Documentation
  - [PRINCIPLE_3_NAME] → III. Classic Tests on Real Objects
  - [PRINCIPLE_4_NAME] → IV. Cross-Platform Parity with a Native Look
  - [PRINCIPLE_5_NAME] → V. Safe, Private and Compatible User Data
  - added: VI. Verifiable Calendar Data
  - added: VII. Decided, Specified and Documented Changes
Added sections:
  - Technology and Platform Constraints (was [SECTION_2_NAME])
  - Development Workflow and Quality Gates (was [SECTION_3_NAME])
Removed sections: none

Evidence (codifies existing practice; no new standards invented): README.md; the system spec,
plan and handover (moved from specs/ to docs/ on 2026-10-18); .github/workflows/ci.yml;
Directory.Build.props and the *.csproj files; the code and tests; the maintainer's directions in
the earlier Copilot sessions (coding guidelines of 2026-10-09).

Follow-up TODOs:
  - Move specs/spec.md, specs/plan.md, specs/copilot/ and specs/screens/ to docs/ and update the
    links (maintainer decision, 2026-10-18), so the docs/ paths cited below resolve.
  - Remove this report before committing the constitution.
-->
# Anniversary Constitution

## Core Principles

### I. Concise Code-Behind Architecture

The solution keeps three projects: `Anniversary.Shared` (every view and all logic, no platform
code), `Anniversary.Desktop` (the `Anniversary` executable and desktop-only platform code) and
`Anniversary.Tests`. Future platform heads (`Anniversary.iOS`, `Anniversary.Android`) are added
next to `Anniversary.Desktop` without restructuring.

- Views MUST use code-behind. MVVM frameworks, DI containers, service interfaces and extra layers
  MUST NOT be introduced.
- All non-UI logic MUST live in the one class `AnniversaryBook`. It MUST NOT read the clock or show
  UI: callers pass the day in as a `DateOnly` (no `TimeProvider`), and load problems are collected
  in `Warnings`.
- A new type MAY be added only when it clearly removes complexity (as the editor-only `PersonRow`
  and `AnniversaryRow` do), and the feature plan MUST justify it in Complexity Tracking.
- Package references MUST stay within the allowed list in Technology and Platform Constraints.

Rationale: the app is small, and the maintainer's goal is a concise code base that is easy to
navigate, understand and maintain. Every layer, type and package has to earn its place.

### II. Established Coding Style and Complete XML Documentation

- Code MUST follow the v4 style (there is no `.editorconfig`; v4 is the reference): Allman braces,
  4-space indentation, braces on every `if`/`else`/`foreach`/`while`, one statement per line,
  `_camelCase` private fields, PascalCase types and members, `On…` command handlers, file-scoped
  namespaces, nullable reference types and implicit usings, `string.Empty`, target-typed `new()`,
  and `var` where the type is obvious.
- Every class, struct, record, interface and enum, and every public member of `Anniversary.Shared`
  and `Anniversary.Desktop` (views and models included), MUST have XML documentation (`<summary>`,
  plus `<param>`/`<returns>` where useful). Both projects generate documentation files and CI
  builds with warnings as errors, so a missing comment fails the build.
- Test classes and test helpers MUST have a `<summary>` naming what they cover (with the
  docs/spec.md section); test methods are named as sentences instead.
- Inline comments MUST be limited to code that needs clarification.

Rationale: one familiar style and self-describing types keep a small code base readable for years.

### III. Classic Tests on Real Objects

- Tests MUST use the classic (Detroit) style: real `AnniversaryBook` instances loaded from real
  JSON files in a temporary folder (`TempFolder`, `TestFiles`), asserting on results and state,
  not on interactions.
- Mocking libraries MUST NOT be used. An awkward collaborator gets a small hand-written fake or
  helper instead.
- Every change to logic, file formats or settings MUST add or update tests in `Anniversary.Tests`
  and the test list in docs/spec.md §9. Behaviour that only a real UI or OS can show (themes,
  menus, login items, packages) MUST instead be listed as a manual acceptance check in
  docs/spec.md §12 and in the feature's `quickstart.md`.
- OS-specific tests MUST skip, not fail, on other operating systems (as the registry tests do).

Rationale: real files catch the serialization and lookup bugs that mocks hide, and clock-free
logic keeps every test deterministic.

### IV. Cross-Platform Parity with a Native Look

- Windows, macOS and Linux are first-class targets (priority: Windows → macOS → Linux → iOS →
  Android). Every feature MUST work from the shared code on all three, or hide itself where the OS
  has no support (as Start at Login does for a bare macOS executable).
- The app MUST blend with each OS and keep v4's compact layout: the Fluent theme with
  `DensityStyle="Compact"`, the platform's native UI font size, v4's window size, the OS light/dark
  setting by default, and each platform's conventions for shortcuts and menus (⌘ shortcuts and the
  `NativeMenu` menu bar on macOS).
- Platform-specific look values MUST be resources set in `App.Initialize` inside an
  `OperatingSystem.Is…()` block. A fix for one platform MUST NOT change the look of the others.
- Platform-specific code (registry, login items, autostart entries) MUST live in
  `Anniversary.Desktop`, never in `Anniversary.Shared`.
- A feature with visible or platform behaviour is done only after it has been checked on a real
  machine of each affected OS. Builds that could not be run on matching hardware (currently
  `win-arm64`, `linux-arm64` and `osx-x64`) MUST be recorded as unverified.

Rationale: the rewrite exists to run natively everywhere, and platform fixes leak easily across
operating systems.

### V. Safe, Private and Compatible User Data

- Personal data MUST NOT be compiled into the binaries or installed by a package: the built-in
  people list is empty.
- The v4 data and calendar formats MUST stay readable. New fields are optional, and the year
  `9999` keeps meaning "year unknown".
- Loading MUST NOT throw or show UI. Problems become warnings shown after the main window opens,
  and the app continues with defaults or empty data.
- The app MUST NOT lose user data: the people file is written to a temporary file that then
  replaces it; a settings file the app could not read is never saved over; the editor asks before
  overwriting a data file that changed elsewhere or could not be loaded; a chosen data file is used
  where it is, never copied; and uninstalling keeps the app-data folder.
- The app MUST write only `usersettings.json`, the data file (from the editor or Save As…) and,
  when asked, its Start at Login entry. Writing anywhere else is a spec-level decision.

Rationale: the data is a family's personal list, often shared between computers through a cloud
drive; a silent overwrite or a lost hand edit is the worst failure this app can have.

### VI. Verifiable Calendar Data

- Shipped calendars MUST be generated reproducibly by `calendars/Build-Calendars.ps1`, and the
  committed files MUST match its output.
- Name days MUST come from pinned, openly licensed sources and MUST NEVER be typed in or made up
  by hand. Holidays and observance days (defined by law, by the UN or by long tradition) are
  written in the script, each with the source behind it.
- Every calendar file MUST carry `source` and `license` metadata; `calendars/README.md` MUST list
  the sources, the checks and what was left out and why; and `calendars/THIRD-PARTY-NOTICES.md`
  MUST ship next to the executable in every archive and package.
- Sources whose license is unclear or restrictive (copyrighted official lists, commercial
  "national day" sites) MUST NOT be used.

Rationale: calendar entries are the first thing users read; a wrong or unlicensed name undermines
trust and the MIT release.

### VII. Decided, Specified and Documented Changes

- The maintainer decides product questions. Each decision is recorded with its reason (in the
  feature's `research.md` and in docs/plan.md), and a decision recorded as the user's MUST NOT be
  revisited without asking.
- Unless docs/spec.md says otherwise, v5 behaves exactly like v4. Every deliberate difference MUST
  be added to docs/spec.md §11.
- A change MUST update, in the same change: docs/spec.md (the living current-state spec) for any
  behaviour, file format or UI change; `Anniversary.Desktop/README.md` (the user guide shipped
  with every download) for anything users can see; `README.md` for build, run or publish changes;
  and docs/copilot/handover.md for state, decisions or lessons a new session needs.

Rationale: the specification is what agents and people read first; it is trustworthy only if it
moves with the code.

## Technology and Platform Constraints

- **Runtime and UI**: .NET 10 (`net10.0`, latest C#) and Avalonia 12.1.x with `FluentTheme` and
  compiled bindings by default. Shared versions and properties live in `Directory.Build.props`.
- **Allowed packages**: `Avalonia`, `Avalonia.Themes.Fluent` and `Avalonia.Controls.DataGrid`
  (shared library); `Avalonia.Desktop` (desktop); `xunit.v3`, `xunit.runner.visualstudio` and
  `Microsoft.NET.Test.Sdk` (tests). JSON uses `System.Text.Json`. OS features are called directly
  (registry, the Objective-C runtime through P/Invoke, files), not through packages. Adding a
  package requires a constitution amendment.
- **Storage**: JSON files only. `Anniversary.Data.json` (people) and `Anniversary.Calendar.*.json`
  (calendars) are found next to the executable, then in the per-user app-data folder, then in the
  embedded defaults; `usersettings.json` (all settings) lives in the app-data folder.
- **Targets and distribution**: `win-x64`, `win-arm64`, `osx-arm64`, `osx-x64`, `linux-x64` and
  `linux-arm64`, each published self-contained as a single file with the user guide and the
  third-party notices next to it, plus one package per platform: a per-user Inno Setup installer,
  an ad-hoc signed `.app` in a `.dmg`, and an AppImage.
- **Tooling**: the calendar, icon and packaging scripts use PowerShell 7. Generated calendars and
  icons are committed, so building the app never needs those scripts.
- **Non-goals until the maintainer decides otherwise**: localized UI strings, trimming or AOT,
  MSIX, and code signing or notarization.

## Development Workflow and Quality Gates

- **Spec Kit flow**: a new feature runs `/speckit-specify` → `/speckit-clarify` → `/speckit-plan`
  → `/speckit-tasks` → `/speckit-analyze` → `/speckit-implement` → `/speckit-converge`. A small
  bug fix MAY skip the feature folder, but it still follows Principles III and VII.
- **Feature folders are history** (flow-forward): each `specs/NNN-short-name/` records one change.
  Once a feature is complete, its artifacts are not rewritten; new requirements for the same area
  get a new feature folder that links to the earlier one. The current behaviour is described by
  docs/spec.md, which the implementing feature updates.
- **Automated gates** (CI on every push to `main`, on every pull request and on demand):
  `dotnet build Anniversary.slnx -warnaserror` and `dotnet test --project Anniversary.Tests` on
  Windows, macOS and Ubuntu, then publishing every RID and building its archive and package.
  A failing gate blocks the merge.
- **Manual gates**: the maintainer checks UI, theme, platform and packaging behaviour on the
  affected operating systems against docs/spec.md §12 and the feature's `quickstart.md`.
- **Versioning of the app**: the version in `Directory.Build.props` (currently 5.0.0) is raised by
  hand when something ships.
- **Agents**: AI coding agents MUST NOT commit, push, or create branches or worktrees unless the
  maintainer asks; the maintainer reviews, merges and commits.

## Governance

- This constitution supersedes other practices. If docs/spec.md, docs/plan.md or the handover
  conflicts with it, the conflict MUST be resolved by amending one of them, not ignored.
- **Amendments** are proposed with `/speckit-constitution`, include a Sync Impact Report, and take
  effect when the maintainer (David Pokluda) approves them.
- **Versioning** follows semantic versioning: MAJOR for removing or redefining a principle, MINOR
  for a new principle or section or materially expanded guidance, PATCH for clarifications and
  wording.
- **Compliance**: every `plan.md` MUST pass the Constitution Check before research and again after
  design; a violation MUST be removed or justified in Complexity Tracking. `/speckit-analyze`
  reports constitution conflicts as CRITICAL. Reviews, by people or agents, verify compliance.
- **Runtime guidance**: docs/copilot/handover.md (current state, agreed decisions, lessons, and how
  to continue in a new session) and README.md (build, test, run and publish).

**Version**: 1.0.0 | **Ratified**: 2026-10-18 | **Last Amended**: 2026-10-18
