# Implementation Plan: User Guide

**Branch**: `009-user-guide` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/009-user-guide/spec.md`

## Summary

Give the people who download the app a short guide inside the download. `Anniversary.Desktop/README.md`, 81 lines of
plain Markdown, says what the app does and explains starting it on each OS (with the first-start steps for unsigned
builds), using it, the people and sharing them, the calendars, the data folder and settings, removing the app, and the
license. One item in `Anniversary.Desktop/Anniversary.Desktop.csproj` copies it as `README.md` next to the
single-file executable in the build and publish output, outside the single-file bundle, as the project already does
for `THIRD-PARTY-NOTICES.md`. CI archives each RID's whole publish folder, so every download includes the guide without
a workflow change. The repository README now says it is for developers and links to the guide; the system spec
records the guide (§4, §10) and the rule that every change users can see updates it. No code changes.

## Technical Context

**Language/Version**: Markdown for the guide; an MSBuild item in the C# desktop project on .NET 10 (`net10.0`)

**Primary Dependencies**: the .NET SDK's self-contained single-file publish (`PublishSingleFile`,
`IncludeNativeLibrariesForSelfExtract`); Avalonia 12.1.4 with the Fluent theme and `Avalonia.Controls.DataGrid` 12.1.2
unchanged. No new packages.

**Storage**: N/A: no data or settings change. The guide is a file in the repository, copied into the build and publish
output.

**Testing**: no new automated tests (no logic, file format or settings change);
`dotnet test --project Anniversary.Tests` still passes all 189 tests (xUnit v3 via Microsoft.Testing.Platform). The
publish output and the guide's content are checked by hand ([quickstart.md](quickstart.md)).

**Target Platform**: Windows, macOS and Linux desktops (x64 and arm64): all six RIDs ship the same guide

**Project Type**: desktop app

**Performance Goals**: N/A: the guide is a separate text file next to the executable; the executable and the app's
start-up are unchanged.

**Constraints**: concise (about 80 lines) and readable as plain text; the names users see, with the macOS menu names
and shortcuts; it describes the downloads as they are at the time (plain archives, unsigned builds, a bare executable
on macOS); kept out of the single-file bundle; nothing else in the publish folder changes.

**Scale/Scope**: one new Markdown file (a title and opening paragraph plus seven sections), one project item, four
documents updated (`README.md`, the spec, the plan, the handover).

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | No code, type, project or package: one Markdown file and one item in the existing desktop project, next to the existing `THIRD-PARTY-NOTICES.md` item. |
| II. Established Coding Style and Complete XML Documentation | N/A | No C# changed. The new project item has a one-line comment naming spec 10, like the item beside it. |
| III. Classic Tests on Real Objects | PASS with justification | No logic, file format or settings changed, so no test was added; the 189 tests still passed. What there is to verify, the guide in the publish folder and its content, is checked by hand ([quickstart.md](quickstart.md)); docs/spec.md §12 got no criterion for it. |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | One guide for all six RIDs (the item has no OS condition), with steps for each OS and the macOS names and shortcuts (⌘F, ⌘G, ⌘O, ⇧⌘S; File ▸ Open Data File…; View ▸ Calendar; View ▸ Appearance). Only a local macOS build was checked; the Windows and Linux downloads get the file through the same publish step in CI. No record says the "Starting it" steps were then followed on each OS; [010](../010-install-packages/spec.md) later rewrote the first-start steps with the buttons each OS shows. |
| V. Safe, Private and Compatible User Data | PASS | No data or settings file changes; the app still writes only `usersettings.json` and the data file. The guide holds no personal data (its JSON example has two settings) and tells users that removing the app keeps the data folder. |
| VI. Verifiable Calendar Data | N/A | No calendar data changes. The guide lists the shipped calendars and points to `THIRD-PARTY-NOTICES.md`, which still ships next to the executable. |
| VII. Decided, Specified and Documented Changes | PASS | Requested by the maintainer, done in the order the maintainer kept ("Keep the order") and approved after review ("Looks good."). This feature is the origin of the rule that every change users can see updates the user guide (docs/spec.md §10; later part of Principle VII). docs/spec.md (§4, §10), docs/plan.md, the handover and `README.md` changed in the same commit. |

**Post-design re-check**: PASS. The design adds one Markdown file and one project item, changes no code, workflow or
package, and keeps the documents for users and for developers apart; the manual-check gap under Principle IV remains
as noted.

## Project Structure

### Documentation (this feature)

```text
specs/009-user-guide/
├── spec.md                     # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md         # spec quality checklist
├── plan.md                     # this file
├── research.md                 # Phase 0
├── quickstart.md               # Phase 1
├── contracts/                  # Phase 1
│   ├── user-guide-outline.md   # the guide's form, sections and what each covers
│   └── publish-folder.md       # the publish folder, the project item and the CI downloads
└── tasks.md                    # /speckit-tasks
```

No `data-model.md`: the feature has no data.

### Source Code (repository root)

```text
Anniversary.Desktop/
├── Anniversary.Desktop.csproj  # changed: copies README.md next to the executable (build and publish output),
│                               #   outside the single-file bundle
└── README.md                   # new: the user guide, shipped as README.md in every download
README.md                       # changed: for developers; links to the user guide (opening, Publish, Downloading a
                                #   build, Projects)
docs/spec.md                    # changed (then specs/spec.md): §4 solution layout, §10 publishing and "User README"
docs/plan.md                    # changed (then specs/plan.md): Next, 1. user-readme ✅
docs/copilot/handover.md        # changed (then specs/copilot/handover.md): the Publish row and the next steps
```

**Structure Decision**: The three-project layout is kept. The guide lives in `Anniversary.Desktop`, the project that
builds and publishes the `Anniversary` executable, and keeps the plain name `README.md`; the repository's `README.md`
is in no build output. `.github/workflows/ci.yml` is unchanged, because it archives the whole publish folder.

## Complexity Tracking

No violations.
