# Research: CI Pipeline

Decisions made while planning and building the feature on 2026-10-10. Sources: the session of 2026-10-10 (the
maintainer's request and questions, the assistant's proposal, its reports and its check of the first run),
`git show 1409624` (the workflow, `README.md`, the spec, the plan and the new handover), the Linux handover it replaced
(`git show 67a4092:specs/copilot/handover-linux.md`), [docs/spec.md](../../docs/spec.md) 10 and 12, and
[docs/plan.md](../../docs/plan.md). The session excerpt keeps the requests and the answers but not every step in
between, so alternatives are listed only where a source names them.

## CI with downloadable builds, in GitHub Actions

- **Decision**: One workflow, `.github/workflows/ci.yml`, named `CI`, with two jobs: `test` (build and tests on three
  OSes) and `publish` (one download per target), which needs `test`.
- **Rationale**: The project had just moved to its own public GitHub repository, which had "no CI, issues or releases
  yet". Asked what CI and docs work it would do, the assistant proposed "CI with downloadable builds"; the maintainer
  agreed ("Yeah, let's do those").
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10.

## Build with warnings as errors, then test that build, on three OSes

- **Decision**: `dotnet build Anniversary.slnx -warnaserror`, then `dotnet test --project Anniversary.Tests --no-build`,
  on `windows-latest`, `macos-latest` and `ubuntu-latest`, with `fail-fast: false`.
- **Rationale**: Acceptance criterion 1 asks for a build without warnings and all tests passing; CI now checks it on
  each OS for every change (the criterion says so since this feature). Both app projects generate XML documentation
  files, so a missing doc comment is a warning and fails the check. `--no-build` runs the tests on the build that was
  just checked, and `global.json` already makes `dotnet test` use Microsoft.Testing.Platform, which xUnit v3 needs.
  With `fail-fast: false`, a failure on one OS doesn't cancel the others.
- **Alternatives considered**: none recorded.
- **Source**: the proposal ("Build with warnings treated as errors, then run the tests"); docs/spec.md 10 and 12;
  `README.md` (`global.json`).

## Publish every target on its own OS, after the checks

- **Decision**: The `publish` job (`needs: test`) runs the README's publish command for each RID on that RID's own OS:
  `win-*` on Windows, `osx-*` on macOS, `linux-*` on Ubuntu.
- **Rationale**: In the proposal's words, building "every target the README lists, each on its own OS" and attaching
  the results to the run "checks the spec's publish requirement on every push and gives you ready-to-run builds for the
  Mac and Fedora". The workflow comment says: "The README's publish command, run on the RID's own OS."
- **Alternatives considered**: none recorded. The README notes that all six can also be built on Windows.
- **Source**: session 2026-10-10; `.github/workflows/ci.yml`; acceptance criterion 9 ("CI checks this").

## Add `linux-arm64`

- **Decision**: `linux-arm64` becomes a build target, in the README, the spec and CI.
- **Rationale**: "It already builds fine, and it covers Fedora running in a virtual machine on an Apple Silicon Mac."
  The Linux handover had noted that `linux-arm64` published fine but wasn't in the README or the spec's list yet ("If
  it's used, add it to both").
- **Alternatives considered**: keeping the four targets; the assistant offered to settle this open point, and the
  maintainer agreed.
- **Source**: session 2026-10-10; `specs/copilot/handover-linux.md` (removed by this feature).

## Add `win-arm64` (the maintainer's request)

- **Decision**: `win-arm64` becomes a build target too, making six.
- **Rationale**: The maintainer asked for it ("we should also add win-arm64, shouldn't we?"). Avalonia supports it, all
  three of its Windows native packages (Skia, HarfBuzz, ANGLE) include `win-arm64` libraries, and it covers Windows
  laptops with Snapdragon chips. It published without warnings in the session, but nobody had run it on an ARM device,
  as the handover says.
- **Alternatives considered**: none.
- **Later**: the same evening, the maintainer said he hadn't tested the ARM builds because he doesn't have the right
  machines. The assistant pointed to virtual machines on the Mac and to GitHub's free ARM runners for public
  repositories (`windows-11-arm`, `ubuntu-24.04-arm`). The handover then recorded that `win-arm64`, `linux-arm64` and
  `osx-x64` haven't been run on matching machines, and that the maintainer accepts that and doesn't want ARM CI runners
  or virtual machines for it (`26063da`, "Closing the 5.0 plan").
- **Source**: session 2026-10-10; the handover at `1409624` and at `26063da`.

## One archive per target, uploaded as is

- **Decision**: Windows builds are packed with `Compress-Archive` into `Anniversary-{rid}.zip`, macOS and Linux builds
  with `tar -czf` into `Anniversary-{rid}.tar.gz`; each archive holds the `{rid}` folder and is uploaded with
  `actions/upload-artifact@v7` and `archive: false`.
- **Rationale**: upload-artifact v7 "still zips multi-file uploads", and zipping loses the executable bit, which tar
  keeps (the workflow comment). Uploaded as is, the download is the archive itself, not an archive wrapped in another
  zip, and a macOS or Linux build is ready to run after extracting. After the first run, three downloads were checked:
  plain `.zip` and `.tar.gz` files, with the executable flag kept.
- **Alternatives considered**: uploading the published folder and letting upload-artifact zip it, which would lose the
  executable bit.
- **Source**: session 2026-10-10; `.github/workflows/ci.yml`; `README.md` ("Downloading a build").

## Downloads on the run page for 30 days, not Releases or Packages

- **Decision**: Each run keeps its downloads as artifacts for 30 days (`retention-days: 30`); they are on the run's
  Summary page under Artifacts, for people signed in to GitHub. A missing archive fails the job
  (`if-no-files-found: error`).
- **Rationale**: Asked after the first run whether the builds were under Packages or Releases, the assistant explained
  that the run page is where they are: Releases are permanent downloads for anyone, tied to a tag, which the plan keeps
  as a later step; Packages is GitHub's registry for NuGet, npm and container packages, which the app doesn't use. The
  first run's downloads were to expire on 9 November.
- **Alternatives considered**: GitHub Releases from a version tag, recorded as a later step. Later the same evening,
  asked what Releases would need, the maintainer put them off ("Let's ignore it for now").
- **Source**: session 2026-10-10; `README.md` ("Downloading a build"); docs/plan.md.

## The version is raised by hand

- **Decision**: CI doesn't change the version. Every build carries the `Version` from `Directory.Build.props` (5.0.0;
  About shows "Version 5.0"), which is raised by hand when something ships.
- **Rationale**: Builds can already be told apart, because the SDK writes each build's commit into the product version
  (`5.0.0+67a4092…`, shown on Windows under the executable's Properties ▸ Details). The assistant suggested 5.0.1 for
  fixes and 5.1.0 for features, a git tag such as `v5.1.0` for each release, and automatic numbering only for iOS,
  where TestFlight needs a build number that increases with every upload. The plan's notes on releases, written in the
  same commit, say: "The version is raised by hand in `Directory.Build.props` when something ships."
- **Alternatives considered**: incrementing the version automatically on every build; versioning packages such as
  Nerdbank.GitVersioning or MinVer (the spec rules out extra packages, and they add little here). Also offered but not
  taken up: removing `AssemblyVersion` and `FileVersion` from `Directory.Build.props`, so that raising the version is a
  one-line edit, and showing the commit in About (for example "Version 5.0.0 (67a4092)", a change to spec 7.5). Both
  stayed as they were.
- **Source**: session 2026-10-10 ("Should we somehow (auto) increment our version number?"); docs/plan.md.

## Triggers, concurrency, permissions and time limits

- **Decision**: Runs on `push` to `main`, on `pull_request` and on `workflow_dispatch`; concurrency per workflow and
  ref, cancelling a run in progress only for pull requests; `permissions: contents: read`; `timeout-minutes: 15` for
  every job.
- **Rationale**: The spec describes the triggers ("runs on every push to `main`, on pull requests and on demand"). No
  reason is recorded for the other settings; their effect is that a superseded pull-request run stops, runs on `main`
  never cancel each other, the workflow can only read the repository, and a stuck job ends after 15 minutes.
- **Alternatives considered**: none recorded.
- **Source**: `.github/workflows/ci.yml`; docs/spec.md 10.

## Current action versions, checked before the first run

- **Decision**: `actions/checkout@v7`, `actions/setup-dotnet@v6` and `actions/upload-artifact@v7`. Before handing over,
  the workflow was linted with actionlint (0 errors), every input was checked against the actions' own `action.yml`
  files, and its steps were run locally: the build with warnings as errors, the tests with `--no-build`, and the two
  archive commands.
- **Rationale**: CI couldn't run before the maintainer pushed, and the assistant doesn't commit or push. The assistant
  checked the latest versions for breaking changes, and checked the inputs against `action.yml` because actionlint's
  "built-in list of action inputs may not include the newest versions". actionlint was fetched into the session
  folder, outside the repository.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10.

## One handover for any OS

- **Decision**: `specs/copilot/handover-linux.md` is replaced by `specs/copilot/handover.md` (now
  `docs/copilot/handover.md`), to be read first by a new session on any OS.
- **Rationale**: The Linux check it was written for was done, and phase 1 was verified on Windows, macOS and Linux. The
  new file keeps the decisions and lessons that still matter (the macOS look fixes, the Avalonia 12 gotchas), updates
  the status (78 tests, CI, the six targets, `win-arm64` not yet run on an ARM device), explains setting up on any OS,
  records a known issue found in review (dates at the ends of the date range crash the app; fixed later in `5bb1afe`)
  and lists the next steps.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; the handover at `1409624`.

## Documents brought in line with the app

- **Decision**: The spec's status becomes "Phase 1 (desktop) implemented, verified on Windows, macOS and Linux", its
  command bar spacing becomes 6 (the code's `ColumnSpacing="6"`), and its layout gains the workflow, the handover and
  `TestFiles.cs`. The plan's window size becomes 312 × 468 (312 × 492 on macOS), and its finished work items are ticked.
  The README lists the macOS menu bar instead of the finished Mac check list.
- **Rationale**: Before answering, the assistant pinned down "the exact doc drift". The spacing was one of two open
  points the assistant offered to settle: "I'd change the spec's button spacing to 6, to match the look you approved."
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; `git diff 67a4092 1409624`.

## Verification approach

- **Decision**: Static checks and local runs before the push (actionlint, the action inputs, all six targets published
  locally, about 44–48 MB each, the build with warnings as errors and the 78 tests, the archive commands, the links in
  the changed Markdown files and leftover references to the old handover); then the first run on GitHub (9 jobs passed,
  6 downloads) and three of its downloads checked by hand.
- **Rationale**: Nothing else could check the workflow before it ran on GitHub; running the ARM builds needed hardware
  that wasn't at hand.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10.
