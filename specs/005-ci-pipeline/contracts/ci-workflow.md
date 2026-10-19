# Contract: CI workflow

The workflow as added on 2026-10-10 (`.github/workflows/ci.yml` in commit `1409624`): when it runs, what its jobs do,
and what it offers for download. Later, [010](../../010-install-packages/spec.md) renamed the two archive steps to
"Archive (zip)" and "Archive (tar.gz)" and added a "Package" step with a second upload for each target, and
[009](../../009-user-guide/spec.md) added the user guide, `README.md`, to every archive.

## Workflow

| Item | Value |
|---|---|
| File | `.github/workflows/ci.yml`, starting with the comment "Builds and tests on Windows, macOS and Linux, then publishes every RID from spec section 10 as a download." |
| Name | `CI` (shown in the Actions tab and on the badge) |
| Triggers | `push` to `main`; `pull_request` (default activity types, any target branch); `workflow_dispatch` (**Run workflow** in the Actions tab) |
| Token | `permissions: contents: read`; no secrets are used |
| Concurrency | group `${{ github.workflow }}-${{ github.ref }}`; `cancel-in-progress: ${{ github.event_name == 'pull_request' }}`, so a newer pull-request run cancels the older one, and on `main` a newer run waits |
| Jobs | `test` (3 matrix jobs), then `publish` (6 matrix jobs): 9 jobs per run when the checks pass |
| Badge | `https://github.com/dpokluda/anniversary/actions/workflows/ci.yml/badge.svg`, under the title of `README.md`, linking to the workflow's Actions page |

## Job `test`: build and test

- Display name: **Build and test ({os})**, for example "Build and test (windows-latest)".
- `runs-on: ${{ matrix.os }}` with `os: [windows-latest, macos-latest, ubuntu-latest]`; `fail-fast: false`;
  `timeout-minutes: 15`.

| # | Step | Does |
|---|---|---|
| 1 | `actions/checkout@v7` | checks out the commit |
| 2 | `actions/setup-dotnet@v6` | installs the .NET SDK, `dotnet-version: 10.0.x` |
| 3 | Build (warnings are errors) | `dotnet build Anniversary.slnx -warnaserror` |
| 4 | Test | `dotnet test --project Anniversary.Tests --no-build`: the tests of the build from step 3, through Microsoft.Testing.Platform (`global.json`) |

The job fails on any compiler warning (both app projects generate XML documentation files, so a missing doc comment
is a warning too) and on any failing test. The other OSes' jobs still finish.

## Job `publish`: one download per target

- Display name: **Publish {rid}**, for example "Publish win-arm64".
- `needs: test`: it runs only when all three `test` jobs passed. `fail-fast: false`; `timeout-minutes: 15`.
- `env: ARCHIVE: Anniversary-${{ matrix.rid }}.${{ matrix.archive }}`.

| `rid` | `os` (runner) | `archive` | Download |
|---|---|---|---|
| `win-x64` | `windows-latest` | `zip` | `Anniversary-win-x64.zip` |
| `win-arm64` | `windows-latest` | `zip` | `Anniversary-win-arm64.zip` |
| `osx-arm64` | `macos-latest` | `tar.gz` | `Anniversary-osx-arm64.tar.gz` |
| `osx-x64` | `macos-latest` | `tar.gz` | `Anniversary-osx-x64.tar.gz` |
| `linux-x64` | `ubuntu-latest` | `tar.gz` | `Anniversary-linux-x64.tar.gz` |
| `linux-arm64` | `ubuntu-latest` | `tar.gz` | `Anniversary-linux-arm64.tar.gz` |

`win-arm64` and `linux-arm64` are new with this feature; the other four were already in the README.

| # | Step | Condition | Does |
|---|---|---|---|
| 1 | `actions/checkout@v7` | | checks out the commit |
| 2 | `actions/setup-dotnet@v6` | | `dotnet-version: 10.0.x` |
| 3 | Publish | | the README's publish command on the RID's own OS: `dotnet publish Anniversary.Desktop -c Release -r {rid} --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/{rid}` |
| 4 | Package (zip) | `matrix.archive == 'zip'` | `Compress-Archive -Path publish/{rid} -DestinationPath $env:ARCHIVE` (`shell: pwsh`) |
| 5 | Package (tar.gz) | `matrix.archive == 'tar.gz'` | `tar -czf "$ARCHIVE" -C publish {rid}`; tar keeps the executable bit, which zipping loses |
| 6 | `actions/upload-artifact@v7` | | uploads the archive, see below |

Upload inputs:

| Input | Value | Effect |
|---|---|---|
| `path` | `${{ env.ARCHIVE }}` | one file per target |
| `archive` | `false` | the file is offered as is, not zipped again |
| `retention-days` | `30` | the download expires 30 days after the run |
| `if-no-files-found` | `error` | the job fails if the archive is missing |

## Downloads

| Property | Value |
|---|---|
| Where | the run's Summary page, under **Artifacts** (Actions ▸ CI ▸ a run); not under the repository's Packages or Releases |
| Who | people signed in to GitHub |
| How many | six per run whose checks passed; none when a `test` job failed; fewer when a `publish` job failed (the run fails) |
| Content | a `{rid}` folder with the single executable (`Anniversary.exe` on Windows, `Anniversary` on macOS and Linux) and `THIRD-PARTY-NOTICES.md` |
| Executable flag | kept in the `.tar.gz`, so a Linux download runs right after `tar -xzf`; a zip doesn't carry it, and Windows doesn't need it |
| macOS | a downloaded executable is quarantined; the README's "Running on macOS" removes the flag (`xattr -d com.apple.quarantine Anniversary`) |
| Version | unchanged by CI: every build carries `Version` 5.0.0 from `Directory.Build.props` ("Version 5.0" in About) |
| Command line | `gh run download` couldn't fetch these files at the time, because it expects a zip; the web page works |

## Not part of the workflow at the time

- Running any published build: CI builds every target, the ARM ones included, but runs none of them.
- Packages (an installer, a `.dmg` with an `.app`, an AppImage), signing and notarization: phase-1 non-goals then.
  [010](../../010-install-packages/spec.md) added the packages, with only an ad-hoc signature on the macOS app.
- Releases from a version tag, or any change of the version number.
