# Implementation Plan: Install Packages

**Branch**: `010-install-packages` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/010-install-packages/spec.md`

## Summary

Give each of the six platforms a package besides its archive: a per-user Inno Setup installer for Windows, a `.dmg`
holding an ad-hoc signed `Anniversary.app` for macOS, and an AppImage for Linux. One PowerShell 7 script,
`packaging/Build-Package.ps1 -Rid <rid>`, builds the package from the published folder (`publish/<rid>`) on the RID's
own OS, from templates in `packaging/` and the existing icons, taking the version and copyright from
`Directory.Build.props`. CI's publish job runs it after the archive and uploads the result next to it. After the
maintainer tried the packages, the macOS icon got a white rounded square of its own (`New-MacIcon` in
`icon/Build-Icon.ps1`, and `App.icns` without its 1x small images), and both READMEs got a section "Opening it the
first time". The app's code doesn't change.

## Technical Context

**Language/Version**: PowerShell 7.4 or newer for `packaging/Build-Package.ps1` (`#Requires -Version 7.4`) and
PowerShell 7 for `icon/Build-Icon.ps1` (Windows-only: System.Drawing); an Inno Setup script for the installer; a POSIX
`sh` script, a property list and a desktop entry as templates; YAML for the CI step. The app itself, C# on .NET 10
(`net10.0`), doesn't change.

**Primary Dependencies**: Inno Setup 6.3 or newer (`ISCC.exe`, preinstalled on GitHub's Windows runners); the macOS
tools `plutil`, `xattr`, `codesign` and `hdiutil`; appimagetool 1.9.1 and the AppImage type2 runtime 20251108,
downloaded at pinned versions and checked by SHA-256; `actions/upload-artifact@v7` in CI. No NuGet package is added:
the app stays on Avalonia 12.1.4 with the Fluent theme, `Avalonia.Controls.DataGrid` 12.1.2 and `System.Text.Json`.

**Storage**: N/A. The packages hold no data: they place the app, `README.md` and `THIRD-PARTY-NOTICES.md`, and the
people and settings stay in the app-data folder, which the uninstaller doesn't touch.

**Testing**: No new automated tests, as no logic, file format or setting changes; the 189 xUnit v3 tests
(Microsoft.Testing.Platform, `dotnet test --project Anniversary.Tests`) still pass. The script checks its own output
(`plutil -lint`, `codesign --verify --strict`, the package file exists), CI builds all six packages on every run, and
the packages are checked by hand on each OS ([quickstart.md](quickstart.md); docs/spec.md §12, criterion 15).

**Target Platform**: Windows x64 and arm64 (the x64 installer also installs on Windows on Arm), macOS 12.0 or newer on
Apple Silicon and Intel, Linux on x86_64 and aarch64 (checked on Fedora)

**Project Type**: desktop app (this feature adds packaging scripts and templates)

**Performance Goals**: none beyond CI's existing 15-minute limit per publish job, which now also builds the package.
The arm64 `.app` showed its window within half a second on the maintainer's Mac.

**Constraints**: each package is built on its own OS from the published folder; installing needs no admin rights; no
package holds a data file, and nothing touches the app-data folder; no code signing beyond the macOS ad-hoc signature;
downloaded tools are pinned and verified; `AppRun` keeps LF line endings and its executable bit; the installer's
`AppId` never changes; the version and copyright come from `Directory.Build.props` (5.0.0).

**Scale/Scope**: six RIDs and three package kinds, so six packages next to the six archives in every CI run; one
script, four templates and a README in `packaging/`, one CI step, one new function in the icon script, and the docs.

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS | No app code, type, project or NuGet package changes. Packaging lives in the new `packaging/` folder outside the solution (one script, four templates); the icon change is one function in the existing icon script. The allowed-package list concerns the app's references; Inno Setup, appimagetool and the macOS tools are build tools that only the script uses. |
| II. Established Coding Style and Complete XML Documentation | PASS | No C# changes, so no XML documentation is due. The script follows the repository's PowerShell scripts (comment-based help, `[CmdletBinding()]`, Allman braces, `$ErrorActionPreference = 'Stop'`), and its comments explain only what isn't obvious: why the bundle is signed, why `hdiutil` is retried, why appimagetool extracts itself, why `App.icns` has no 1x small images. |
| III. Classic Tests on Real Objects | PASS with justification | Packages are OS behaviour, which the principle lists as a manual acceptance check: criterion 15 in docs/spec.md §12 and [quickstart.md](quickstart.md). No logic, format or setting changed, so no tests were added, and the 189 tests still pass. The script checks its own output, and CI builds every package on every run. |
| IV. Cross-Platform Parity with a Native Look | PASS with justification | Each platform gets its own convention (an installer with a Start-menu entry, an app dragged to Applications from a `.dmg`, an AppImage), built on its own OS. The white tile is macOS-only (`App.icns`); `App.ico` and `App.png` don't change. The arm64 `.app` was started on the maintainer's Mac during the work, and the maintainer then checked the CI packages on Windows (x64), macOS (Apple Silicon) and Fedora (x64). The `osx-x64` `.dmg` was only checked statically (Rosetta wasn't installed), and the `win-arm64` and `linux-arm64` packages haven't run on matching machines; the handover records them as unverified. |
| V. Safe, Private and Compatible User Data | PASS | No package holds a data file or personal data (the plan's "no data file is installed"); the uninstaller never touches the app-data folder; inside the `.app` and the AppImage the folder next to the executable is read-only, so the data stays in the app-data folder (spec 10). The app writes nothing new. |
| VI. Verifiable Calendar Data | PASS with justification | No calendar data changes. `THIRD-PARTY-NOTICES.md` ships in every package: next to `Anniversary.exe` in the installed folder, beside `Anniversary.app` on the `.dmg` and in its `Contents/Resources`, and in the AppImage's `usr/share/doc/anniversary`, where each platform keeps such documents rather than literally next to the executable. |
| VII. Decided, Specified and Documented Changes | PASS | The maintainer's decisions are recorded with their reasons: the AppImage over an RPM (docs/plan.md and spec 10), the white icon tile (spec C17), signing not decided (spec 1 and 10, docs/plan.md); releases were put off in the session. The same commits updated docs/spec.md (§1 non-goals, §10 Packages and CI, the user README paragraph, C17, criterion 15), docs/plan.md, the handover, the user guide, `README.md`, `packaging/README.md` and `icon/README.md`. |

**Post-design re-check**: PASS. The design adds files only under `packaging/`, one CI step and one icon function,
changes no C# and adds no package; the verification gap for the Arm and Intel-Mac packages remains as noted under
Principle IV.

## Project Structure

### Documentation (this feature)

```text
specs/010-install-packages/
├── spec.md                     # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md         # spec quality checklist
├── plan.md                     # this file
├── research.md                 # Phase 0
├── quickstart.md               # Phase 1
├── contracts/                  # Phase 1
│   ├── build-package.md        # Build-Package.ps1 -Rid: parameters, outputs, errors, use in CI
│   ├── windows-installer.md    # the setup program: settings, files, shortcuts, uninstall
│   ├── macos-dmg.md            # the .dmg, Anniversary.app, Info.plist, signing, App.icns
│   └── linux-appimage.md       # the AppImage: AppDir, AppRun, desktop entry, pinned tools
└── tasks.md                    # /speckit-tasks
```

No `data-model.md`: the feature has no data.

### Source Code (repository root)

```text
packaging/
├── Build-Package.ps1             # new: -Rid <rid>; builds the installer, .dmg or AppImage from publish/<rid>
├── README.md                     # new: how to build a package, what each holds and needs
├── windows/
│   └── Anniversary.iss           # new: the per-user Inno Setup installer
├── macos/
│   └── Info.plist                # new: Info.plist template of Anniversary.app ({Version}, {Copyright})
└── linux/
    ├── AppRun                    # new: starts usr/bin/Anniversary inside the AppImage (LF, executable)
    └── anniversary.desktop       # new: the AppImage's desktop entry
.gitattributes                    # changed: packaging/linux/* text eol=lf
.github/workflows/ci.yml          # changed: Package step and its upload; "Package (…)" steps renamed "Archive (…)"
icon/
├── Build-Icon.ps1                # changed: New-MacIcon (white rounded square); App.icns without its 1x small images
├── App.icns                      # changed: regenerated (8fbd8ff on Windows; 54c9cbb without the 1x small images)
└── README.md                     # changed: the macOS tile, the missing small images, each file's package use
Anniversary.Desktop/README.md     # changed: Starting it, Opening it the first time, Removing it, License
README.md                         # changed: CI, Publish, Packages, Downloading a build, Opening it the first time,
                                  #   Running on macOS and Linux, Projects
docs/spec.md                      # changed (then specs/spec.md): §1 non-goals, §10, C17, §12 criterion 15
docs/plan.md                      # changed (then specs/plan.md): packages ✅, the installers idea
docs/copilot/handover.md          # changed (then specs/copilot/handover.md)
```

**Structure Decision**: Packaging gets its own top-level folder, `packaging/`, next to `calendars/` and `icon/` and
outside the solution: one script plus a template folder per OS. The app's projects don't change, because the packages
wrap the existing self-contained single-file publish output. CI's existing publish job, which already runs once per RID
on that RID's own OS, runs the script after the archive, so no new job is needed.

## Complexity Tracking

No violations.
