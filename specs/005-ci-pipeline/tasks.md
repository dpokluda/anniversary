# Tasks: CI Pipeline

**Input**: Design documents from `specs/005-ci-pipeline/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, contracts/, quickstart.md

**Tests**: No new automated tests: the feature changes no logic, file format or setting (constitution, Principle III).
It runs the existing 78 tests on Windows, macOS and Ubuntu. The workflow is checked with actionlint and by running its
steps locally, and the run on GitHub, the downloads and the documents are checked by hand with
[quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. Everything went into one commit, `1409624` ("Adding GitHub actions and
updating docs", 2026-10-10), which the maintainer made and pushed; the checks of the first run came after it.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- The workflow is in `.github/workflows/`; documentation is in `README.md` and `docs/` (then `specs/`); the app's
  projects (`Anniversary.Shared/`, `Anniversary.Desktop/`, `Anniversary.Tests/`) don't change

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and the checks before writing the workflow

- [X] T001 Start from the state 001–004 left (commit `67a4092`, the move to GitHub): `dotnet build Anniversary.slnx` without warnings and 78 tests passing with `dotnet test --project Anniversary.Tests`; the public repository has no CI yet
- [X] T002 [P] Check the latest versions of the actions to use, `actions/checkout@v7`, `actions/setup-dotnet@v6` and `actions/upload-artifact@v7`, for breaking changes, and what upload-artifact v7 does with multi-file uploads (it still zips them) and with file permissions
- [X] T003 [P] Publish all six targets locally with the command in `README.md` (`dotnet publish Anniversary.Desktop -c Release -r <rid> --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true`) for `win-x64`, `win-arm64`, `osx-arm64`, `osx-x64`, `linux-x64` and `linux-arm64`: each builds without warnings, about 44–48 MB plus `THIRD-PARTY-NOTICES.md`; the outputs stay in the session folder, outside the repository

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The workflow file with its triggers, which every story's jobs go into

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create `.github/workflows/ci.yml` with the header comment "Builds and tests on Windows, macOS and Linux, then publishes every RID from spec section 10 as a download.", `name: CI`, the triggers `push` (branches `[main]`), `pull_request` and `workflow_dispatch`, `permissions: contents: read`, and `concurrency` with the group `${{ github.workflow }}-${{ github.ref }}` and `cancel-in-progress: ${{ github.event_name == 'pull_request' }}` (FR-001, FR-004)

**Checkpoint**: The workflow exists and starts on the right events; the jobs can be added

---

## Phase 3: User Story 1 - Every change is checked on Windows, macOS and Linux (Priority: P1) 🎯 MVP

**Goal**: Every push to `main`, every pull request and every manual run builds the app with warnings as errors and
runs the tests on three OSes

**Independent Test**: Push a commit to `main`: three "Build and test" jobs run and pass (quickstart scenario 1)

### Implementation for User Story 1

- [X] T005 [US1] Add the `test` job to `.github/workflows/ci.yml`: the name "Build and test (${{ matrix.os }})", `runs-on` the matrix `os: [windows-latest, macos-latest, ubuntu-latest]` with `fail-fast: false`, `timeout-minutes: 15`, and the steps `actions/checkout@v7`, `actions/setup-dotnet@v6` with `dotnet-version: 10.0.x`, "Build (warnings are errors)" running `dotnet build Anniversary.slnx -warnaserror`, and "Test" running `dotnet test --project Anniversary.Tests --no-build` (FR-002, FR-003)
- [X] T006 [P] [US1] Add the CI badge (the workflow's `badge.svg`, linking to its Actions page) under the title of `README.md`, and a paragraph under "Build, test, run": CI builds with warnings as errors and runs the tests on Windows, macOS and Linux for every push to `main` and every pull request, then publishes all the builds (FR-005)
- [X] T007 [US1] Check `.github/workflows/ci.yml` before it can run: actionlint, fetched into the session folder outside the repository, reports 0 errors; every input used exists in the actions' own `action.yml` files; and the test job's commands run locally: the build with `-warnaserror` and the 78 tests with `--no-build` pass (FR-002)
- [X] T008 [US1] After the maintainer's push, check the first run: the three "Build and test" jobs pass (quickstart scenario 1; docs/spec.md §12, criterion 1)

**Checkpoint**: Every change is built and tested on Windows, macOS and Ubuntu

---

## Phase 4: User Story 2 - Download a ready-to-run build for any platform (Priority: P2)

**Goal**: Each passing run offers one plain archive per target, kept for 30 days, and the README explains how to get
and run it

**Independent Test**: From a passing run's page, download the build for this computer, extract it and start the app
without the SDK (quickstart scenario 2)

### Implementation for User Story 2

- [X] T009 [US2] Add the `publish` job to `.github/workflows/ci.yml`: the name "Publish ${{ matrix.rid }}", `needs: test`, `fail-fast: false`, `timeout-minutes: 15`, a matrix of `include` entries with `rid`, `os` and `archive` (`win-x64` on `windows-latest` as `zip`; `osx-arm64` and `osx-x64` on `macos-latest`, and `linux-x64` on `ubuntu-latest`, as `tar.gz`), `env: ARCHIVE: Anniversary-${{ matrix.rid }}.${{ matrix.archive }}`, the same checkout and setup steps as the test job, and "Publish" running the README's command with `-o publish/${{ matrix.rid }}` and the comment "The README's publish command, run on the RID's own OS." (FR-006)
- [X] T010 [US2] Add the archive steps to `.github/workflows/ci.yml`: "Package (zip)" (`if: matrix.archive == 'zip'`, `shell: pwsh`, `Compress-Archive -Path publish/${{ matrix.rid }} -DestinationPath $env:ARCHIVE`) and "Package (tar.gz)" (`if: matrix.archive == 'tar.gz'`, `tar -czf "$ARCHIVE" -C publish ${{ matrix.rid }}`), with the comment "One archive per RID, uploaded as is (not zipped again). tar keeps the executable bit, which zipping loses." (FR-007, FR-008)
- [X] T011 [US2] Add the upload step to `.github/workflows/ci.yml`: `actions/upload-artifact@v7` with `path: ${{ env.ARCHIVE }}`, `archive: false` (the archive is offered as is), `retention-days: 30` and `if-no-files-found: error` (FR-007, FR-009)
- [X] T012 [P] [US2] Add to `README.md` the section "Downloading a build" (every passing run offers one download per platform, kept for 30 days: `Anniversary-{rid}.zip` for Windows and `Anniversary-{rid}.tar.gz` for macOS and Linux, downloading needs a GitHub sign-in, each holds a `{rid}` folder with the executable and `THIRD-PARTY-NOTICES.md`, and the `.tar.gz` keeps the executable flag), "(or extract a CI download)" in "Running on macOS", and the new section "Running on Linux" (`linux-x64` on PCs, `linux-arm64` on ARM machines; `chmod +x` isn't needed for a CI download; the app needs fontconfig, libX11, libICE and libSM, which desktop distributions such as Fedora Workstation already have) (FR-010)
- [X] T013 [US2] Run the two archive commands locally on the published folders, as the workflow does (FR-007, FR-008)
- [X] T014 [US2] Check the first run's downloads (quickstart scenario 2; criterion 9): all six are there; three downloaded ones are plain `.zip` or `.tar.gz` files, not wrapped in another zip, and the macOS and Linux executables keep their executable flag; `gh run download` can't fetch them (it expects a zip), so the run's page is the way to get them (FR-007, FR-008)

**Checkpoint**: Each passing run offers a ready-to-run build for every target in the matrix

---

## Phase 5: User Story 3 - Builds for ARM computers (Priority: P3)

**Goal**: `win-arm64` and `linux-arm64` join the four existing targets, in CI and in the developer documentation

**Independent Test**: A passing run offers `Anniversary-win-arm64.zip` and `Anniversary-linux-arm64.tar.gz`, and the
README lists six publish commands (quickstart scenario 3)

### Implementation for User Story 3

- [X] T015 [US3] Check that `win-arm64` is supported: Avalonia's three Windows native packages (Skia, HarfBuzz, ANGLE) include `win-arm64` libraries (FR-011)
- [X] T016 [US3] Add `win-arm64` (`windows-latest`, `zip`) and `linux-arm64` (`ubuntu-latest`, `tar.gz`) to the publish matrix in `.github/workflows/ci.yml`, for six entries (FR-011)
- [X] T017 [P] [US3] List six publish commands in `README.md` (adding `win-arm64` and `linux-arm64`), change "All four can be built on Windows" to "All six", and add which machines the `arm64` builds are for: `win-arm64` for Windows on ARM (e.g. Snapdragon laptops), `osx-arm64` for Apple Silicon Macs and `linux-arm64` for ARM Linux (e.g. a Linux virtual machine on an Apple Silicon Mac) (FR-012)
- [X] T018 [US3] Check scenario 3 of `specs/005-ci-pipeline/quickstart.md`: the first run offered `Anniversary-win-arm64.zip` and `Anniversary-linux-arm64.tar.gz`; with no ARM hardware at hand, the `win-arm64` build is recorded as not yet run on an ARM device (T019) (FR-011, FR-013)

**Checkpoint**: Six targets are built on every passing run

---

## Phase 6: User Story 4 - Pick up the work on any OS (Priority: P3)

**Goal**: One OS-neutral handover and documents that describe phase 1 as it was finished

**Independent Test**: Start a new session on any OS from the handover's prompt; no document refers to the old Linux
handover, and the relative links work (quickstart scenario 4)

### Implementation for User Story 4

- [X] T019 [US4] Replace `specs/copilot/handover-linux.md` with `docs/copilot/handover.md` (then `specs/copilot/handover.md`), "Read this first when you continue work in a new GitHub Copilot session (on any OS)": the status table (phase 1 done and verified on Windows, macOS and Linux; 78 tests; a CI row; a Publish row with the six RIDs and "`win-arm64` hasn't been run on an ARM device yet"), the decisions, the lessons from the macOS look fixes and the Avalonia 12 gotchas kept from the old file, "Setting up on any OS" (every CI run also offers ready-to-run builds), the things that need Windows, the known issue that dates at the ends of the date range crash the app (fixed later in `5bb1afe`), the next steps (Mac polish with an `.app` bundle, the holiday and observance calendars, iOS; a fourth item on installers and start at login was added later in the session, for 010 and 011) and a prompt to start a session (FR-013, FR-015)
- [X] T020 [P] [US4] Replace the finished "Things to check on a Mac" list in `README.md` with the commands in the macOS menu bar: **File** (Edit People…), **View** (Nameday Calendar, Appearance), **Go** (Yesterday, Tomorrow, Today, Change Date…), **Find** (Find… ⌘F, Find Next ⌘G) and **About Anniversary** in the app menu (FR-016)
- [X] T021 [US4] Run the final document checks of quickstart scenario 4: no leftover references to the old handover, the relative links in the changed Markdown files resolve, and the full diff reads right (FR-017)

**Checkpoint**: All four user stories work

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: The system spec and the plan, the version question, and the handover to the maintainer

- [X] T022 [P] Update `docs/spec.md` (then `specs/spec.md`): the status line "Phase 1 (desktop) implemented, verified on Windows, macOS and Linux"; 1, phase 1 on Windows (x64 + arm64), macOS (arm64 + x64) and Linux (x64 + arm64); 4, the layout with `.github\workflows\ci.yml`, `copilot\handover.md` in place of `copilot\handover-linux.md`, and the test helper `TestFiles.cs`; 7.2, the command bar's "Column spacing 6" (it said "Spacing 8"); 10, the six RIDs and the CI paragraph (when it runs, the checks, one file per RID, `Anniversary-{rid}.zip` or `Anniversary-{rid}.tar.gz`, kept for 30 days); 12, "CI checks this" in criteria 1 and 9 (FR-005, FR-012, FR-016)
- [X] T023 [P] Update `docs/plan.md` (then `specs/plan.md`): the layout (`.github\workflows\` and the handover), the window size (312 × 468 device-independent pixels, 312 × 492 on macOS: v4's 464 × 701 physical pixels at 150 % scaling), "Phase 1 is done and verified on Windows, macOS and Linux", every finished work item ticked, the new item 12, **ci** ✅, and next (agreed) **mac-app**, then the holiday calendars (FR-016)
- [X] T024 Leave the version alone: the workflow sets none, so every build carries `Version` 5.0.0 from `Directory.Build.props` ("Version 5.0" in About); the maintainer's question about automatic versioning is answered with "raise it by hand when something ships", the plan's notes on releases in `docs/plan.md` (then `specs/plan.md`) say so, and the offered changes to `Directory.Build.props` and the About dialog aren't made (FR-014)
- [X] T025 Hand the change over uncommitted: name the new and deleted files for the commit (`.github/workflows/ci.yml`, the new handover, the deleted Linux handover), delete the local build outputs in the session folder (about 350 MB), and let the maintainer commit and push (`1409624`), which starts the first run

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–6)**: depend on Foundational; User Story 2's `publish` job needs User Story 1's `test` job,
  and User Story 3 extends User Story 2's matrix
- **Polish (Phase 7)**: depends on all user stories; T025 comes last

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after User Story 1 (`needs: test`); its downloads exist only when the checks pass
- **User Story 3 (P3)**: after User Story 2 (it adds two entries to the `publish` matrix and two publish commands)
- **User Story 4 (P3)**: after Foundational (the handover describes the workflow); otherwise independent of the other
  stories, although the handover's CI and Publish rows describe User Stories 1–3

### Within Each User Story

- The workflow before its checks: local runs and actionlint before the push, the first run on GitHub after it
- The README sections next to the workflow jobs they describe

### Parallel Opportunities

- T002 and T003 together
- T006 next to T005, T012 next to T009–T011, T017 next to T016 and T020 next to T019: the README and the workflow or
  the handover are different files
- T022 and T023 together: two different documents

---

## Parallel Example: User Story 2

```bash
# The publish job and the README's download instructions can be written side by side:
Task: "Add the publish job, the archive steps and the upload to .github/workflows/ci.yml"
Task: "Add 'Downloading a build' and 'Running on Linux' to README.md"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (three "Build and test" jobs pass)

### Incremental Delivery

1. Setup and Foundational: the workflow starts on pushes, pull requests and by hand
2. User Story 1: build and tests on three OSes (MVP)
3. User Story 2: one download per target
4. User Story 3: the ARM targets
5. User Story 4: the handover and the documents
6. Polish: the spec, the plan, the version answer and the handover to the maintainer

### What happened

On 2026-10-10, after the move to GitHub, the maintainer asked what CI and docs work the assistant would do. It proposed
CI with downloadable builds and a set of document fixes, and offered to settle two open points itself (the button
spacing and `linux-arm64`); the maintainer agreed and added `win-arm64`. The assistant checked that all six targets
build, wrote the workflow, linted it, ran its steps locally and updated the documents, without committing. Later in the
session the maintainer asked about versioning (answered: raise it by hand; nothing changed) and asked for the installer
idea to be added to the plan; that went into the same commit but belongs to 010 and 011. The maintainer committed and
pushed everything as `1409624`. The first run passed (9 jobs, 6 downloads), and the maintainer then asked where the
downloads appear.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Step names are as they were at the time: 010 renamed "Package (zip)" and "Package (tar.gz)" to "Archive (zip)" and
  "Archive (tar.gz)" and added a "Package" step
- The documentation paths are today's; at the time they were under `specs/`
