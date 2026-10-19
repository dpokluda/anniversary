# Quickstart: CI Pipeline

How to check the feature as built on 2026-10-10. Expected behaviour: [spec.md](spec.md); the workflow's triggers,
jobs, steps and downloads: [contracts/ci-workflow.md](contracts/ci-workflow.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository, for the local equivalents of the CI steps.
- A GitHub account signed in to github.com (downloading run artifacts needs a sign-in) and, for scenario 1, the right
  to push to `main` or to open a pull request.
- For the downloads: a Windows PC, a Mac and a Linux machine (the maintainer used Fedora). For the ARM builds: a
  Windows on ARM computer and an ARM Linux machine, for example Fedora in a virtual machine on an Apple Silicon Mac;
  neither was at hand at the time.
- Optional: actionlint, the linter for GitHub workflows.

## Build, test and publish locally (what CI runs)

```bash
dotnet build Anniversary.slnx -warnaserror
dotnet test --project Anniversary.Tests --no-build
dotnet publish Anniversary.Desktop -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true -o publish/linux-x64
tar -czf publish/Anniversary-linux-x64.tar.gz -C publish linux-x64
actionlint
```

On Windows, pack a Windows target the way CI does, in PowerShell:
`Compress-Archive -Path publish/win-x64 -DestinationPath publish/Anniversary-win-x64.zip`. CI writes the archive into
its working folder; locally, `publish/` keeps it out of git. Repeat the publish for the other RIDs (`win-x64`,
`win-arm64`, `osx-arm64`, `osx-x64`, `linux-arm64`); CI publishes each one on its own OS.

Expected at the time: no warnings, 78 tests passing, each of the six RIDs published (about 44–48 MB, with
`THIRD-PARTY-NOTICES.md` next to the executable), and actionlint reporting no errors.

## Scenario 1: every change is checked (User Story 1; docs/spec.md §12, criterion 1)

1. Push a commit to `main`. **Expected**: the repository's **Actions** tab shows a **CI** run with three jobs, "Build
   and test (windows-latest)", "(macos-latest)" and "(ubuntu-latest)"; once all three pass, six "Publish {rid}" jobs
   run. At the time, the first run passed with all 9 jobs.
2. Open `README.md` on GitHub. **Expected**: the CI badge under the title shows the status and links to the workflow;
   the paragraph under "Build, test, run" says what CI checks and when.
3. Open a pull request that adds a public member without an XML doc comment. **Expected**: "Build (warnings are
   errors)" fails on all three OSes and no "Publish" job runs. Push to the pull request again while its run is still
   going: the older run is cancelled.
4. In **Actions ▸ CI**, use **Run workflow**. **Expected**: a run with the same jobs.

## Scenario 2: download a ready-to-run build (User Story 2; criterion 9)

1. In **Actions ▸ CI**, open a passing run and scroll to **Artifacts** on its Summary page. **Expected**: six
   downloads, `Anniversary-win-x64.zip`, `Anniversary-win-arm64.zip`, `Anniversary-osx-arm64.tar.gz`,
   `Anniversary-osx-x64.tar.gz`, `Anniversary-linux-x64.tar.gz` and `Anniversary-linux-arm64.tar.gz`, kept for 30 days.
2. Download one. **Expected**: the file itself (a plain `.zip` or `.tar.gz`), not an archive wrapped in another zip.
3. **Linux**: `tar -xzf Anniversary-linux-x64.tar.gz`, then `./linux-x64/Anniversary`. **Expected**: the folder holds
   `Anniversary` and `THIRD-PARTY-NOTICES.md`, and the app starts with no `chmod`.
4. **macOS**: `tar -xzf Anniversary-osx-arm64.tar.gz` (`osx-x64` on an Intel Mac), `cd osx-arm64`, then
   `xattr -d com.apple.quarantine Anniversary` and `./Anniversary`, as in the README's "Running on macOS".
   **Expected**: the executable flag is kept and the app starts.
5. **Windows**: extract `Anniversary-win-x64.zip`. **Expected**: a `win-x64` folder with `Anniversary.exe` and
   `THIRD-PARTY-NOTICES.md`; the app starts.
6. Open **⋯ ▸ About Anniversary…** (macOS: **About Anniversary** in the app menu). **Expected**: "Version 5.0", as
   for a local build; CI doesn't change the version.
7. Try `gh run download` for the run. **Expected at the time**: it can't fetch these files, because it expects a zip;
   the run's page works.
8. Look under the repository's **Packages** and **Releases**. **Expected**: no builds there; the README's "Downloading
   a build" points to the CI runs.

## Scenario 3: builds for ARM computers (User Story 3)

1. In a passing run, find `Anniversary-win-arm64.zip` and `Anniversary-linux-arm64.tar.gz`. **Expected**: both are
   there.
2. Read the "Publish" section of `README.md` and section 10 of the spec. **Expected**: six publish commands and six
   RIDs; the README says the `arm64` builds are for Windows on ARM (for example Snapdragon laptops), Apple Silicon Macs
   and ARM Linux (for example a Linux virtual machine on an Apple Silicon Mac).
3. On ARM hardware, start each ARM build as in scenario 2. **At the time**: not done. The handover says that
   `win-arm64` hasn't been run on an ARM device yet.

## Scenario 4: the documents (User Story 4)

1. On any OS, start a new Copilot session with the prompt at the end of `docs/copilot/handover.md` (then
   `specs/copilot/handover.md`). **Expected**: the handover gives the current state (phase 1 done and verified on
   Windows, macOS and Linux, 78 tests, CI, the six targets), the decisions, the lessons, setting up on any OS, the known
   issues and the next steps; nothing in it is specific to one OS's session.
2. Check the commit for leftovers: `git grep -n handover-linux 1409624`. **Expected**: no matches.
3. Check the relative links in `README.md`, the spec, the plan and the handover as they were at `1409624`.
   **Expected**: they resolve.
4. Compare the documents with the app at `1409624`. **Expected**: the spec's status says that phase 1 is implemented
   and verified on Windows, macOS and Linux, and its command bar spacing is 6; the plan gives the window as 312 × 468
   (312 × 492 on macOS) and ticks the finished work items; the README lists the commands in the macOS menu bar, File ▸
   Edit People… among them.

## Platform checks

- **Windows, macOS and Linux**: each OS's "Build and test" job passes on every push and pull request (criterion 1),
  and each target's "Publish" job passes (criterion 9).
- **Downloads**: at the time, three downloads of the first run were checked: plain `.zip` and `.tar.gz` files, with
  the executable flag kept in the macOS and Linux ones. The maintainer had already checked the app on macOS and Fedora
  before this feature.
- **Not verified at the time**: running the `win-arm64` build on an ARM device. Later records add `linux-arm64` and
  `osx-x64` to the builds never run on matching machines.
