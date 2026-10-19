# Feature Specification: CI Pipeline

**Feature Branch**: `005-ci-pipeline`

**Created**: 2026-10-10

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "What would you want to do with respect to CI and docs work …? … Yeah, let's do those and
we should also add win-arm64, shouldn't we?"

**Later changes**: [009](../009-user-guide/spec.md) added the user guide, `README.md`, next to the executable in every
download; [010](../010-install-packages/spec.md) added a second download for each target, its package (a Windows
installer, a macOS `.dmg` or a Linux AppImage), built in the same run.

## Clarifications

### Session 2026-10-10

- Q: What CI and docs work should be done? → A: Both, as the assistant proposed and the maintainer accepted ("Yeah,
  let's do those"): CI that builds the app with warnings treated as errors and runs the tests on Windows, macOS and
  Linux, then builds every target on its own OS and attaches the builds to the run; and docs work: one up-to-date
  handover in place of the one written for the Linux check, and the spec, plan and README brought in line with the app
  as built.
- Q: Should `linux-arm64` become a build target? → A: Yes. It already built, and it covers Fedora running in a virtual
  machine on an Apple Silicon Mac. It was one of two open points the assistant offered to settle; the maintainer
  agreed.
- Q: "We should also add win-arm64, shouldn't we?" → A: Yes. The app's UI framework supports it, and it covers Windows
  laptops with Snapdragon chips. That makes six targets.
- Q: The spec gives the command bar's buttons a spacing of 8, but the look the maintainer approved uses 6. Which one is
  right? → A: 6; the spec is corrected. This was the other open point.
- Q: "As soon as we introduce CI, should we somehow (auto) increment our version number?" → A: No, as the assistant
  advised: builds can already be told apart, because each one's product version includes its commit. The version is
  raised by hand when something ships (for example 5.0.1 for fixes and 5.1.0 for features), and a release can be
  marked with a version tag such as `v5.1.0`; automatic numbering only becomes necessary for iOS. The plan records that
  the version is raised by hand. The assistant's offer to also show the commit in About wasn't taken up, and the
  version stayed 5.0.0.
- Q: Where do the builds appear on GitHub: under Packages or under Releases? → A: Neither. Each CI run attaches its
  builds to its own page as artifacts (Actions ▸ CI ▸ a run ▸ Artifacts), for people signed in to GitHub, for 30 days.
  Releases, permanent downloads for anyone tied to a version tag, stay a later step; Packages is GitHub's registry for
  NuGet, npm and container packages, which the app doesn't use.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Every change is checked on Windows, macOS and Linux (Priority: P1)

The maintainer pushes a change to the main branch, or a pull request is opened. Without any further step, the app is
built and its automated tests run on Windows, macOS and Linux (Ubuntu), each on its own machine, and the run shows
whether each one passed. A compiler warning counts as a failure. The maintainer can also start a run by hand, and the
developer README shows the current status.

**Why this priority**: The maintainer works on one computer at a time (the app was built on Windows, then checked on a
Mac and on Fedora), and until now nothing checked a change on the other two. It checks the spec's first acceptance
criterion (a build without warnings, all tests passing) on every change, and the downloads build on it.

**Independent Test**: Push a commit to the main branch: the run shows three build-and-test checks, one per OS, and
they pass. Open a pull request that adds a compiler warning: its checks fail.

**Acceptance Scenarios**:

1. **Given** a push to the main branch, **When** CI runs, **Then** the app is built and all tests run on Windows, macOS
   and Linux, and the run passes only if all three pass.
2. **Given** a pull request, **When** it is opened or updated, **Then** the same checks run on it.
3. **Given** a change that makes the build report a warning, **When** CI builds it, **Then** the build check fails.
4. **Given** a test that fails on one OS only, **When** CI runs, **Then** that OS's check fails, the other two still
   finish, and the run fails.
5. **Given** the maintainer wants a run without pushing anything, **When** they start CI by hand, **Then** it runs the
   same checks.
6. **Given** the developer README on GitHub, **When** someone opens it, **Then** a badge at the top shows the CI status,
   and a paragraph says what CI checks and when.

---

### User Story 2 - Download a ready-to-run build for any platform (Priority: P2)

Anyone signed in to GitHub, for example the maintainer trying a change on the Mac or on Fedora, opens a passing CI run
and downloads the build for their computer: one file per target, a `.zip` for Windows and a `.tar.gz` for macOS and
Linux. It holds a folder with the app's single executable and its third-party notices, and on Linux it runs right after
extracting. No development tools are needed on that computer.

**Why this priority**: Downloadable builds were the core of the proposal ("CI with downloadable builds"): they let the
maintainer try a change on another OS without building it there, and every run checks that each target can still be
published (the spec's acceptance criterion 9). They need the checks of User Story 1 to pass first.

**Independent Test**: From a passing run's page, download the build for this computer, extract it and start the app
without installing the SDK.

**Acceptance Scenarios**:

1. **Given** a run whose checks passed on all three OSes, **When** it finishes, **Then** its page offers one download
   per target, named `Anniversary-{target}.zip` (Windows) or `Anniversary-{target}.tar.gz` (macOS and Linux), for
   example `Anniversary-linux-x64.tar.gz`.
2. **Given** a run whose checks failed, **When** it finishes, **Then** it offers no downloads.
3. **Given** a download, **When** it is saved, **Then** it is the archive itself, not wrapped in another zip, and it
   holds a `{target}` folder with the executable and `THIRD-PARTY-NOTICES.md`.
4. **Given** a Linux download, **When** it is extracted (`tar -xzf Anniversary-linux-x64.tar.gz`), **Then** the app
   starts without first being marked executable.
5. **Given** a macOS download, **When** it is extracted, **Then** the executable keeps its executable flag, and it
   starts once its download quarantine is removed as the README's "Running on macOS" shows.
6. **Given** someone who looks for the builds under the repository's Packages or Releases, **When** they read the
   README's "Downloading a build", **Then** it sends them to the CI runs.

---

### User Story 3 - Builds for ARM computers (Priority: P3)

Windows on ARM (for example laptops with Snapdragon chips) and ARM Linux (for example Fedora in a virtual machine on an
Apple Silicon Mac) get builds of their own, next to the existing four (Windows x64, macOS on Apple Silicon and on Intel,
Linux x64): six targets in all, built by CI and documented for developers.

**Why this priority**: The maintainer asked for `win-arm64`, and `linux-arm64` already built and covers Fedora in a
virtual machine on an Apple Silicon Mac. They widen User Story 2 to ARM computers, but nobody could run them yet.

**Independent Test**: A passing run offers `Anniversary-win-arm64.zip` and `Anniversary-linux-arm64.tar.gz`, and the
developer README and the system spec list six targets.

**Acceptance Scenarios**:

1. **Given** a passing run, **When** it finishes, **Then** it offers six downloads: `win-x64`, `win-arm64`, `osx-arm64`,
   `osx-x64`, `linux-x64` and `linux-arm64`.
2. **Given** the developer README, **When** a developer looks for the publish commands, **Then** it lists all six and
   says which computers the ARM builds are for.
3. **Given** the system spec, **When** it lists the phase-1 platforms and the publish targets, **Then** Windows and
   Linux include ARM64.
4. **Given** the ARM builds, **When** the maintainer reads the handover, **Then** it says that the `win-arm64` build
   hasn't been run on an ARM device yet.

---

### User Story 4 - Pick up the work on any OS (Priority: P3)

The maintainer, or a new Copilot session on Windows, macOS or Linux, starts from one handover and from documents that
describe phase 1 as it was finished: implemented and verified on all three OSes, with CI, the six targets, the
decisions and lessons worth keeping, the known issues and the next steps.

**Why this priority**: It is the "docs work" half of the request. The handover had been written for the Linux check,
which was now done, and the spec, plan and README still described an earlier state (the spec's status still said
"Approved; ready for implementation"). Nothing breaks without it, but every later session starts from these documents.

**Independent Test**: Start a new session on any OS with the prompt at the end of the handover: it finds the current
state and the next steps. No document refers to the old Linux handover, and the relative links in the changed
documents work.

**Acceptance Scenarios**:

1. **Given** a new session on any OS, **When** it reads the handover, **Then** it finds the current state (phase 1 done
   and verified on Windows, macOS and Linux, 78 tests passing, CI, the six targets), the decisions not to revisit, the
   lessons from the macOS look fixes, how to set up on any OS, the known issues, the next steps and a prompt to start a
   session.
2. **Given** the system spec, **When** it is compared with the app, **Then** its status says that phase 1 is implemented
   and verified on Windows, macOS and Linux, its file layout lists the CI workflow, the new handover and a test helper
   it lacked, and the command bar's spacing is 6.
3. **Given** the plan, **When** it is read, **Then** its window size is the one built (312 × 468, and 312 × 492 on
   macOS), every finished work item is ticked, and CI is a work item of its own.
4. **Given** the developer README, **When** a Mac user reads it, **Then** it lists the commands in the macOS menu bar
   (including File ▸ Edit People…) instead of the macOS check list, which was done.
5. **Given** the changed documents, **When** their references are checked, **Then** none points to the old Linux
   handover and every relative link resolves.

### Edge Cases

- **A check fails on one OS**: the other OSes still finish, so all failures show at once; the run offers no builds.
- **One target fails to build**: the other targets still build and offer their downloads; the run fails.
- **A build leaves no file to offer**: the run fails instead of quietly offering fewer downloads.
- **Pushes in quick succession**: in a pull request, a newer run cancels the one still in progress; on the main branch,
  a newer run waits until the one in progress has finished.
- **A push to another branch**: it is checked only through a pull request.
- **Old runs**: downloads are kept for 30 days; after that, only newer runs offer builds.
- **Not signed in**: GitHub offers the downloads only to people signed in to GitHub.
- **Command-line download**: GitHub's command-line tool couldn't download these files at the time (it expects zipped
  downloads); the run's page works.
- **macOS quarantine**: macOS blocks an executable that was downloaded until its quarantine flag is removed (README,
  "Running on macOS").
- **ARM builds**: CI builds them but doesn't run them, so whether they start is a manual check on ARM hardware; none
  was at hand, and the `win-arm64` build was recorded as not yet run on an ARM device.
- **Version**: CI leaves the version alone, so every build shows "Version 5.0" in About until the maintainer raises it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A CI run MUST start for every push to the main branch and for every pull request, and the maintainer MUST
  be able to start one by hand.
- **FR-002**: Each run MUST build the app with compiler warnings treated as errors and then run all automated tests on
  that build, separately on Windows, macOS and Linux.
- **FR-003**: A failure on one OS MUST NOT stop the checks on the others, and any failed check MUST fail the run.
- **FR-004**: Runs for the same pull request or branch MUST NOT overlap: for a pull request, a newer run MUST cancel the
  one still in progress; on the main branch, a newer run MUST wait until the one in progress has finished.
- **FR-005**: The developer README MUST show the current CI status and say what CI checks and when it runs. The system
  spec MUST describe CI (when it runs, what it checks, what it offers for download and for how long), and its
  acceptance criteria for a clean build, passing tests and publishing every target MUST say that CI checks them.
- **FR-006**: Builds for download MUST be made only after the checks passed on all three OSes, each target on its own
  OS and with the publish command the developer README documents (one self-contained executable, with
  `THIRD-PARTY-NOTICES.md` next to it).
- **FR-007**: Each passing run MUST offer one download per target, as the archive itself rather than wrapped in another
  one: `Anniversary-{target}.zip` for Windows and `Anniversary-{target}.tar.gz` for macOS and Linux, each holding a
  `{target}` folder with the executable and the notices.
- **FR-008**: The macOS and Linux downloads MUST keep the executable's permission, so that the app can be started right
  after extracting.
- **FR-009**: Downloads MUST be kept for 30 days, and a run MUST fail when a target's download wasn't produced.
- **FR-010**: The developer README MUST explain where the downloads are (on each passing run, for 30 days, with a
  GitHub sign-in), what each one holds, and how to run a downloaded build on macOS and on Linux.
- **FR-011**: CI MUST build six targets: Windows x64 and ARM64, macOS on Apple Silicon (arm64) and on Intel (x64), and
  Linux x64 and ARM64.
- **FR-012**: The developer README MUST give the publish command for each of the six targets and say which computers
  the ARM builds are for; the system spec MUST list the six targets and include the ARM platforms in phase 1.
- **FR-013**: The handover MUST record the builds that haven't been run on matching hardware (at the time,
  `win-arm64`).
- **FR-014**: CI MUST NOT change the app's version: every build carries the version set in the project (5.0.0), which
  the maintainer raises by hand when something ships.
- **FR-015**: One handover for continuing the work in a new session on any OS MUST replace the one written for the
  Linux check, and it MUST record the current state (phase 1 done and verified on Windows, macOS and Linux, the passing
  tests, CI and the six targets), the decisions and lessons still worth keeping, the known issues, the next steps and a
  prompt to start a session.
- **FR-016**: The system spec, the plan and the developer README MUST describe phase 1 as built: its status, the file
  layout, the window size, the command bar's spacing (6), the finished work items, and the commands in the macOS menu
  bar in place of the finished macOS check list.
- **FR-017**: Documents MUST NOT refer to the removed Linux handover, and the relative links in the changed documents
  MUST resolve.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every push to the main branch and every pull request gets a pass or a fail for the build and the tests on
  all three OSes with no manual step, so acceptance criterion 1 in [docs/spec.md](../../docs/spec.md) §12 ("CI checks
  this on Windows, macOS and Linux") holds for every change.
- **SC-002**: Every passing run offers 6 downloads, one per target, so publishing every target (criterion 9) is checked
  on every run.
- **SC-003**: The first run after the change was merged passed: all 9 jobs (3 checks and 6 builds) succeeded, and all 6
  downloads were there.
- **SC-004**: The three downloads checked from that run were plain `.zip` or `.tar.gz` files, not wrapped in another
  zip, and the macOS and Linux executables in them kept their executable flag.
- **SC-005**: Someone signed in to GitHub gets from the repository to the build for their computer in three steps
  (Actions ▸ CI, a run, Artifacts) and needs no SDK to run it.
- **SC-006**: Before the change was committed, the app built without warnings, all 78 tests passed, and all six targets
  published without warnings.
- **SC-007**: After the change, no document refers to the old Linux handover, every relative link in the changed
  documents resolves, and neither the handover nor its prompt for a new session is tied to one OS.

## Assumptions

- Builds on the app as [001](../001-cross-platform-rewrite/spec.md)–[004](../004-macos-native-look/spec.md) left it,
  with 78 tests: the self-contained, single-file publish of four targets documented in the developer README (001) and
  `THIRD-PARTY-NOTICES.md` published next to the executable ([003](../003-calendar-chooser/spec.md)).
- The repository is public on GitHub, which provides the Windows, macOS and Linux machines that run CI; no other
  service and no secret is needed.
- The downloads are for the maintainer and testers signed in to GitHub. Permanent downloads for anyone (Releases from a
  version tag) are a later step, recorded in the plan.
- Installers, a macOS `.app` bundle, signing and notarization stay out of scope (phase-1 non-goals in the spec at the
  time); the packages came with [010](../010-install-packages/spec.md).
- CI builds the ARM targets but doesn't run any build; running them is a manual check on ARM hardware, which the
  maintainer didn't have.
- The app itself doesn't change: only the CI workflow and the documents do.
- The same commit also added the plan's idea "installers and start at login" and a pointer to it in the handover's next
  steps, requested later in the same session; that idea belongs to [010](../010-install-packages/spec.md) and
  [011](../011-start-at-login/spec.md).
