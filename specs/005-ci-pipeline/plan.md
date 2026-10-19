# Implementation Plan: CI Pipeline

**Branch**: `005-ci-pipeline` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/005-ci-pipeline/spec.md`

## Summary

Give the new GitHub repository CI with downloadable builds, and bring the documents in line with the finished phase 1.
One GitHub Actions workflow, `.github/workflows/ci.yml` ("CI"), runs on pushes to `main`, on pull requests and on
demand. Its `test` job builds `Anniversary.slnx` with `-warnaserror` and runs the tests on `windows-latest`,
`macos-latest` and `ubuntu-latest`. When all three pass, its `publish` job runs the README's self-contained,
single-file publish for each of six RIDs on the RID's own OS, packs the folder as `Anniversary-{rid}.zip` (Windows,
`Compress-Archive`) or `Anniversary-{rid}.tar.gz` (macOS and Linux, `tar`, which keeps the executable bit), and uploads
it as is with `actions/upload-artifact@v7` (`archive: false`), kept for 30 days. `win-arm64` and `linux-arm64` join the
four existing RIDs. The app's code doesn't change, and neither does its version. The README (badge, CI, six publish
commands, downloading a build, Linux, the macOS menu bar), the spec and the plan are updated, and an OS-neutral
handover replaces the one written for the Linux check.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`) for the app, unchanged; the workflow is GitHub Actions YAML
with PowerShell (`pwsh`) and bash steps.

**Primary Dependencies**: GitHub Actions `actions/checkout@v7`, `actions/setup-dotnet@v6` (SDK `10.0.x`) and
`actions/upload-artifact@v7`, on GitHub-hosted runners. The app's own dependencies are unchanged: Avalonia 12.1.4 with
the Fluent theme, `Avalonia.Controls.DataGrid` 12.1.2, `System.Text.Json`. No new packages, and no versioning package.

**Storage**: N/A for the app. The downloads are run artifacts on GitHub, kept for 30 days.

**Testing**: the existing 78 tests in `Anniversary.Tests` (xUnit v3 via Microsoft.Testing.Platform, which `global.json`
selects), now run by CI on Windows, macOS and Ubuntu. The workflow was checked before the push with actionlint
(0 errors), against each action's own `action.yml`, and by running its steps locally; the first run on GitHub passed.
The downloads are checked by hand ([quickstart.md](quickstart.md)).

**Target Platform**: GitHub-hosted runners `windows-latest`, `macos-latest` and `ubuntu-latest`; output for `win-x64`,
`win-arm64`, `osx-arm64`, `osx-x64`, `linux-x64` and `linux-arm64` (Windows, macOS and Linux desktops, x64 and arm64).

**Project Type**: desktop app (this feature adds its CI workflow)

**Performance Goals**: every job ends within 15 minutes (`timeout-minutes: 15`); a run has 9 jobs (3 checks, then 6
builds in parallel); each published target is about 44–48 MB (local check).

**Constraints**: a build with warnings as errors on every OS, so a missing XML doc comment fails CI; builds only after
all three checks pass; the same publish command as the README, on each RID's own OS; one plain download per target that
keeps the executable bit; a read-only token and no secrets; no change to the app's code, packages or version; the
assistant doesn't commit or push, so CI first runs when the maintainer pushes.

**Scale/Scope**: one 76-line workflow with 2 jobs (9 matrix jobs) and 6 downloads per passing run; 6 files changed in
`1409624` (319 insertions, 214 deletions, including the installer idea in the plan, which isn't part of this feature).

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | No code, type, project or package is added to the app; the three projects are unchanged. The workflow lives outside them, in `.github/workflows/`. |
| II. Established Coding Style and Complete XML Documentation | PASS | No C# changes. CI builds with warnings as errors on all three OSes, and both app projects generate documentation files, so a missing XML comment fails the build. The workflow has a header comment and short comments where a step needs one. |
| III. Classic Tests on Real Objects | PASS | No logic, file format or settings change, so no new tests; CI runs the existing 78 classic tests on Windows, macOS and Ubuntu for every push and pull request. What only a real OS can show (that a download runs) is a manual check in [quickstart.md](quickstart.md); docs/spec.md §12 criteria 1 and 9 now say that CI checks the build, the tests and publishing. |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | Build and tests run on all three OSes, and each RID is published on its own OS. CI runs no published build, and the `win-arm64` and `linux-arm64` builds could only be run on ARM hardware, which wasn't at hand. The handover recorded `win-arm64` as not yet run on an ARM device; that `linux-arm64` and `osx-x64` weren't run on matching machines either was recorded later the same evening (`26063da`). |
| V. Safe, Private and Compatible User Data | PASS | The downloads hold only the executable and the notices; the built-in people list is empty, so no personal data ships. The app's files and behaviour are unchanged. The workflow can only read the repository. |
| VI. Verifiable Calendar Data | PASS | Every download carries `THIRD-PARTY-NOTICES.md` next to the executable, as the calendar licenses require; the publish command copies it. No calendar data changes. |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer accepted the proposal ("Yeah, let's do those") and added `win-arm64`; the answers on versioning and on where the downloads appear are in [research.md](research.md), and the plan records that the version is raised by hand. docs/spec.md (status, 1, 4, 7.2, 10, 12), docs/plan.md, `README.md` and the new handover changed in the same commit. Users see no change in the app, so there's no §11 entry; the user guide didn't exist yet (009). |

**Post-design re-check**: PASS. The design adds one workflow file and changes only documents; the app, its projects
and its packages are untouched. The constitution's automated gates (Development Workflow and Quality Gates) come from
this feature, and so does its rule that the version is raised by hand: the answer to the maintainer's question about
automatic versioning. The gap under Principle IV (ARM builds that nobody could run) remains as noted.

## Project Structure

### Documentation (this feature)

```text
specs/005-ci-pipeline/
├── spec.md                 # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md     # spec quality checklist
├── plan.md                 # this file
├── research.md             # Phase 0
├── quickstart.md           # Phase 1
├── contracts/              # Phase 1
│   └── ci-workflow.md      # triggers, jobs, matrix, steps, downloads and retention
└── tasks.md                # /speckit-tasks
```

There is no `data-model.md`: the feature has no data.

### Source Code (repository root)

```text
.github/
└── workflows/
    └── ci.yml                  # new: the CI workflow, jobs test and publish
README.md                       # changed: CI badge and paragraph, six publish commands and the ARM targets,
                                #   "Downloading a build", "Running on Linux", the macOS menu bar instead of the
                                #   finished Mac check list
docs/spec.md                    # changed (then specs/spec.md): status line, 1 (platforms), 4 (layout),
                                #   7.2 (command bar spacing 6), 10 (RIDs and CI), 12 (criteria 1 and 9)
docs/plan.md                    # changed (then specs/plan.md): layout, window size, work breakdown (12. ci),
                                #   next steps; the same commit added the installer idea (010, 011)
docs/copilot/handover.md        # new (then specs/copilot/handover.md), replacing specs/copilot/handover-linux.md
```

**Structure Decision**: The three-project layout is kept, and no project file changes. The workflow sits where GitHub
looks for workflows and runs the commands the README documents (build, test, publish), so CI checks what developers
are told to run. The documents keep their places; only the handover is renamed, because it is no longer about one OS.

## Complexity Tracking

No violations.
