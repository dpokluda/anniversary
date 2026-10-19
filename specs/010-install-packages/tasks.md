# Tasks: Install Packages

**Input**: Design documents from `specs/010-install-packages/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, contracts/, quickstart.md

**Tests**: No new automated tests. The feature changes no logic, file format or setting, and packages are OS behaviour,
which the constitution (Principle III) checks by hand: [quickstart.md](quickstart.md) and docs/spec.md §12,
criterion 15. The script checks its own output (`plutil -lint`, `codesign --verify --strict`, the package file
exists), CI builds every package on every run, and the 189 existing tests keep passing.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. The work went into four commits on 2026-10-10: `b782f7c` ("Adding
installers"), `a90a9e1` ("Updating icon"), `8fbd8ff` ("Rebuilding icons") and `54c9cbb` ("Tweaking Mac icon and
README about first-time run on MacOS").

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US6)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Packaging in `packaging/` (the script and a template folder per OS), the icon in `icon/`, CI in
  `.github/workflows/ci.yml`, documentation in `Anniversary.Desktop/README.md`, `README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point and the new folder

- [X] T001 Start from the state 009 left (`5868c42`): `Anniversary.slnx` builds without warnings, 189 tests pass with `dotnet test --project Anniversary.Tests`, and `dotnet publish` writes `Anniversary` (`Anniversary.exe` on Windows), `README.md` and `THIRD-PARTY-NOTICES.md` to `publish/<rid>`, the input of every package
- [X] T002 [P] Create the `packaging/` folder with `windows/`, `macos/` and `linux/` folders for the templates, and add `packaging/linux/* text eol=lf` to `.gitattributes` ("Read on Linux inside the AppImage (AppRun is a shell script)")

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The package command that every platform's package is built with

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Create `packaging/Build-Package.ps1` (`#Requires -Version 7.4`) with comment-based help, `-Rid` (validated against the six RIDs), `-PublishFolder` (default `publish/<rid>`) and `-OutputFolder` (default `packages`), both made absolute, and `$ErrorActionPreference = 'Stop'` with `$PSNativeCommandUseErrorActionPreference = $true` (FR-015)
- [X] T004 In `packaging/Build-Package.ps1`, stop on another OS ("Build the {rid} package on its own OS (see README.md in this folder).") or when the executable, `README.md` or `THIRD-PARTY-NOTICES.md` is missing ("{file} is missing; publish {rid} to {folder} first."); read `Version` and `Copyright` from `Directory.Build.props`; recreate the work folder `anniversary-package-<rid>` in the temp folder and create the output folder (FR-002, FR-015)
- [X] T005 In `packaging/Build-Package.ps1`, call `New-MacPackage`, `New-WindowsPackage` or `New-LinuxPackage` for the platform, stop if the package file is missing ("The package wasn't created: {path}"), delete the work folder, print `Package: {path}` and, when `GITHUB_OUTPUT` is set, write `path={path}` to it (FR-001, FR-014, FR-015)

**Checkpoint**: The command checks its inputs and hands over to the platform; each package type can now be added

---

## Phase 3: User Story 1 - Install Anniversary on Windows (Priority: P1) 🎯 MVP

**Goal**: A per-user setup program for `win-x64` and `win-arm64` that installs without admin rights and uninstalls
without touching the app-data folder

**Independent Test**: Run the setup as a standard user, start the app from the Start menu, then uninstall it in
Settings ▸ Apps: `%APPDATA%\Anniversary` stays (quickstart scenario 1)

### Implementation for User Story 1

- [X] T006 [P] [US1] Create `packaging/windows/Anniversary.iss` with a header comment showing how it is compiled, the required defines `AppVersion`, `Rid` and `PublishFolder` (each with an `#error` naming the value to pass), the optional `OutputFolder` (".") and `Copyright` ("David Pokluda"), and `Architectures`: `arm64` for `win-arm64`, otherwise `x64compatible`, which "also lets Windows on ARM install the x64 build" (FR-005)
- [X] T007 [US1] Fill `[Setup]` in `packaging/windows/Anniversary.iss`: one fixed `AppId` for every version and architecture ("so an installer updates the installed app in place"), `AppName=Anniversary`, the version, publisher, URLs and copyright, `PrivilegesRequired=lowest`, `DefaultDirName={autopf}\Anniversary`, `DisableProgramGroupPage=yes`, both architecture settings, `OutputBaseFilename=Anniversary-{#AppVersion}-{#Rid}-setup`, `SetupIconFile` (`Anniversary.Shared/Assets/App.ico`), `UninstallDisplayIcon`, the modern wizard and `lzma2/max` solid compression (FR-002, FR-004, FR-005)
- [X] T008 [US1] Add `[Languages]` (English), `[Tasks]` (`desktopicon`, unchecked), `[Files]` (`Anniversary.exe`, `README.md`, `THIRD-PARTY-NOTICES.md`, `ignoreversion`), `[Icons]` (the Start-menu shortcut, and the desktop shortcut with the task) and `[Run]` (start the app: `nowait postinstall skipifsilent`) to `packaging/windows/Anniversary.iss`; nothing installs a data file or touches the app-data folder (FR-003, FR-004, FR-006)
- [X] T009 [US1] Add `New-WindowsPackage` to `packaging/Build-Package.ps1`: find `ISCC.exe` on the `PATH` or in an `Inno Setup 6` folder under Program Files (x86), Program Files or `%LOCALAPPDATA%\Programs`, else stop with "Inno Setup 6 (ISCC.exe) was not found. Install it from https://jrsoftware.org/isinfo.php."; compile `Anniversary.iss` with `/DAppVersion`, `/DRid`, `/DPublishFolder`, `/DOutputFolder` and `/DCopyright`; return `Anniversary-{version}-{rid}-setup.exe` (FR-001, FR-002)
- [X] T010 [US1] Check scenario 1 of `specs/010-install-packages/quickstart.md` on Windows (x64) with the setup from the first CI run after the push (docs/spec.md §12, criterion 15)

**Checkpoint**: Windows users can install the app and remove it again, keeping their data

---

## Phase 4: User Story 2 - Install Anniversary on a Mac (Priority: P1)

**Goal**: A `.dmg` for `osx-arm64` and `osx-x64` holding an ad-hoc signed `Anniversary.app` and an Applications link

**Independent Test**: Drag the app from the CI `.dmg` to Applications and open it after Open Anyway: it runs with its
own name and icon and no Terminal window (quickstart scenario 2)

### Implementation for User Story 2

- [X] T011 [P] [US2] Create `packaging/macos/Info.plist`, the template with `{Version}` and `{Copyright}`: `CFBundleIdentifier` `cz.pokluda.anniversary`; name, display name and executable `Anniversary`; `CFBundleIconFile` `App.icns`; `APPL`; both version keys; `LSApplicationCategoryType` `public.app-category.productivity`; `LSMinimumSystemVersion` 12.0, "the minimum macOS that the published executable declares"; `NSHighResolutionCapable`; `NSHumanReadableCopyright` (FR-002, FR-008)
- [X] T012 [US2] Add `New-MacPackage` to `packaging/Build-Package.ps1`: create `Anniversary.app/Contents/MacOS` and `Contents/Resources`, copy the executable, `icon/App.icns`, `README.md` and `THIRD-PARTY-NOTICES.md`, write `Contents/Info.plist` with the version and the XML-escaped copyright, and check it with `plutil -lint` (FR-003, FR-008)
- [X] T013 [US2] Sign the bundle in `New-MacPackage` in `packaging/Build-Package.ps1`: `xattr -cr`, then `codesign --force --sign -` on the whole bundle and `codesign --verify --strict`, with the comment that Apple Silicon only runs signed code and that a downloaded bundle without a valid signature is reported as damaged instead of offering Open Anyway (FR-009)
- [X] T014 [US2] Finish `New-MacPackage` in `packaging/Build-Package.ps1`: put `README.md`, `THIRD-PARTY-NOTICES.md` and an `Applications` link to `/Applications` next to the app, and create `Anniversary-{version}-{rid}.dmg` with `hdiutil create -volname 'Anniversary' … -ov -format UDZO`, tried up to 3 times, 5 seconds apart, as it sometimes fails with "Resource busy" on build machines (FR-001, FR-002, FR-007)
- [X] T015 [US2] Build both `.dmg`s on the maintainer's Mac with `packaging/Build-Package.ps1`: the arm64 app started from the `.app` as "Anniversary" 5.0.0 within half a second, and Gatekeeper rejected it as expected for an unsigned app; the x64 `.dmg` was only checked statically, as Rosetta wasn't installed
- [X] T016 [US2] Check scenario 2 of `specs/010-install-packages/quickstart.md` on macOS (Apple Silicon) with the `.dmg` from CI (criterion 15); the maintainer's report that "Not Opened" offered no way to allow the app led to User Story 4

**Checkpoint**: Mac users can install the app by dragging it to Applications

---

## Phase 5: User Story 3 - Run Anniversary on Linux from one file (Priority: P2)

**Goal**: An AppImage for `linux-x64` and `linux-arm64` that runs once it is executable

**Independent Test**: On Fedora, make the AppImage from CI executable and double-click it: the app starts with nothing
installed (quickstart scenario 3)

### Implementation for User Story 3

- [X] T017 [P] [US3] Create `packaging/linux/AppRun`, a POSIX `sh` script that finds its own folder with `readlink -f` and runs `usr/bin/Anniversary` with the arguments through `exec`; committed with the executable mode `100755` (FR-010)
- [X] T018 [P] [US3] Create `packaging/linux/anniversary.desktop`: `Type=Application`, `Name=Anniversary`, `Comment=Name days, birthdays and anniversaries for any day`, `Exec=Anniversary`, `Icon=anniversary`, `Terminal=false`, `Categories=Office;Calendar;`, `StartupWMClass=Anniversary` (FR-010)
- [X] T019 [US3] Pin the AppImage tools in `packaging/Build-Package.ps1`: appimagetool 1.9.1 and the type2 runtime 20251108, each with SHA-256 hashes for `x86_64` and `aarch64`, and `Get-PinnedFile`, which downloads into the work folder and stops with "The SHA-256 of {url} doesn't match the pinned one." (FR-016)
- [X] T020 [US3] Add `New-LinuxPackage` to `packaging/Build-Package.ps1`: appimagetool for the build machine's architecture and the runtime for the target's; the AppDir with `AppRun`, `anniversary.desktop`, `anniversary.png` (from `Anniversary.Shared/Assets/App.png`), `usr/bin/Anniversary` and the README and notices in `usr/share/doc/anniversary`; `chmod +x` on `AppRun` and the executable; `appimagetool --no-appstream --runtime-file` with `APPIMAGE_EXTRACT_AND_RUN=1` and `ARCH`, writing `Anniversary-{version}-{rid}.AppImage` (FR-001, FR-002, FR-003, FR-010)
- [X] T021 [US3] Check scenario 3 of `specs/010-install-packages/quickstart.md` on Fedora (x64) with the AppImage from the first CI run (criterion 15)

**Checkpoint**: Linux users can run the app from one file

---

## Phase 6: User Story 4 - Open the app the first time (Priority: P2)

**Goal**: The shipped guide explains installing, removing and allowing the unsigned app the first time; the developer
README repeats the first-start steps briefly

**Independent Test**: With only the guide's "Opening it the first time", open a freshly downloaded copy on macOS and on
Windows (quickstart scenario 4)

### Implementation for User Story 4

- [X] T022 [US4] Rewrite "Starting it" in `Anniversary.Desktop/README.md` (`b782f7c`): each download as a package or a plain archive; Windows: the setup (just for you, no admin rights, Start menu, starts the app) or the `.zip`; macOS: `osx-arm64` or `osx-x64`, the `.dmg`'s app dragged to Applications, or the bare app from the `.tar.gz` with its Terminal window; Linux: make the AppImage executable and double-click it (tools such as Gear Lever add it to the menu), or `./Anniversary` from the `.tar.gz`. Rewrite "Removing it" for each package, with the data folder staying, and say that `THIRD-PARTY-NOTICES.md` "comes with the app" (FR-011)
- [X] T023 [US4] Add "Opening it the first time" to `Anniversary.Desktop/README.md` (`54c9cbb`): not signed by Microsoft or Apple, allow it once, a new version may need it again; macOS steps 1–3 (**Done**; **System Settings ▸ Privacy & Security**, **Security**, **Open Anyway** or **Open**, there for about an hour; confirm with **Open Anyway** and the password or Touch ID) and the `xattr` commands; Windows steps 1–2 (keep the download, in Edge **⋯ ▸ Keep ▸ Show more ▸ Keep anyway**; **More info**, **Run anyway**); Linux: nothing asks. "Starting it" now points to the section, drops its own warning notes and names **Properties ▸ Executable as Program** for the AppImage (FR-012)
- [X] T024 [P] [US4] Update `README.md`: "Running on macOS" and "Running on Linux" for the `.dmg` and the AppImage (`b782f7c`), then a new "Opening it the first time" with the steps in brief and a link to the user guide's section, which "Running on macOS" now refers to (`54c9cbb`) (FR-013)
- [X] T025 [US4] Rebuild both `.dmg`s with `packaging/Build-Package.ps1` so that they carry the new guide (FR-012)
- [X] T026 [US4] Check scenario 4 of `specs/010-install-packages/quickstart.md` against the macOS steps that the maintainer was given for his installed app (**Done**, Privacy & Security, **Open Anyway**), after which he reported all platforms validated; SmartScreen's **More info ▸ Run anyway** was part of the Windows check given to him

**Checkpoint**: Users can get past the first-start warnings with the shipped guide alone

---

## Phase 7: User Story 5 - Get every package from each build (Priority: P2)

**Goal**: Every passing CI run offers each platform's package next to its archive; one command builds a package
locally

**Independent Test**: Push a change: the run offers twelve downloads, a package and an archive per platform
(quickstart scenario 5)

### Implementation for User Story 5

- [X] T027 [US5] In the publish job of `.github/workflows/ci.yml`, add the **Package** step (`id: package`, `shell: pwsh`, running `./packaging/Build-Package.ps1 -Rid ${{ matrix.rid }}`) and a second `actions/upload-artifact@v7` (the step output `path`, `archive: false`, `retention-days: 30`, `if-no-files-found: error`) after the archive's upload; rename the archive steps **Archive (zip)** and **Archive (tar.gz)** and mention the packages in the header comment (FR-014)
- [X] T028 [P] [US5] Update `README.md`: CI "publishes all the builds and packages"; the macOS output is "a plain executable; its package (below) wraps it in an `.app` bundle"; the new "Packages" section with `pwsh packaging/Build-Package.ps1 -Rid osx-arm64` and a link to `packaging/README.md`; "Downloading a build" with two downloads per platform; the `packaging/` row under "Projects" (FR-013)
- [X] T029 [P] [US5] Create `packaging/README.md`: publish first, then build one package on the RID's own OS; `-PublishFolder` and `-OutputFolder`; the table of packages (RIDs, file name, templates, what each OS needs); the notes per OS (never change the `AppId`; the uninstaller leaves the app-data folder alone; the ad-hoc signature and why; `AppRun`'s LF endings and executable flag; the static runtime; how to move to newer AppImage tools); and how to check each package (criterion 15) (FR-013, FR-015)
- [X] T030 [US5] Let CI build the packages after the push of `b782f7c`; the maintainer took the Windows, macOS and Fedora packages for T010, T016 and T021 from CI, and no failing Package step was reported (quickstart scenario 5)

**Checkpoint**: Every passing build offers all packages

---

## Phase 8: User Story 6 - A Mac icon like other Mac apps (Priority: P3)

**Goal**: The Mac icon is the calendar on a white rounded square, clean at every size; Windows and Linux unchanged

**Independent Test**: Look at the installed app in the Dock, Finder and Launchpad on a regular and a Retina screen
(quickstart scenario 6)

### Implementation for User Story 6

- [X] T031 [US6] Add `New-MacIcon` to `icon/Build-Icon.ps1` (`a90a9e1`): a transparent bitmap with a white rounded square on Apple's grid (824 px with 185 px corners, 100 px in from each edge of 1024; a gradient from white to `#EEF0F4`; a faint shadow) and the calendar from `New-Icon` drawn at its own size at 82% of the icon, on whole pixels, with row 132.5 of its 256 grid at the middle. `Write-Icns` draws every entry with it, the previews add `App-mac-*.png`, and the header says what `App.icns` now is; `App.ico` and `App.png` keep the plain calendar (FR-017, FR-019)
- [X] T032 [US6] Regenerate `icon/App.icns` on Windows with `pwsh -File icon/Build-Icon.ps1` (`8fbd8ff`, by the maintainer) (FR-017)
- [X] T033 [US6] In `icon/Build-Icon.ps1` (`54c9cbb`), leave the 1x entries `icp4`, `icp5` and `icp6` (16, 32 and 64 px) out of `Write-Icns`, with the comment why; remove the extra shrinking of the small layout, which only the 16 px image needed; start the macOS previews at 32 px. Write `icon/App.icns` on the Mac from the images drawn on Windows without those entries, byte for byte what the script now writes (FR-018)
- [X] T034 [P] [US6] Describe the macOS icon in `icon/README.md`: the white rounded square (`New-MacIcon`), why macOS 26 and later need it, Apple's grid and the 82% (`a90a9e1`); no 1x 16 and 32 px images and why, checked on macOS 27, and the `App.icns` sizes "32–1024 px: 128–512 pt, and 16–512 pt at Retina @2x" (`54c9cbb`) (FR-017, FR-018)
- [X] T035 [US6] Check scenario 6 of `specs/010-install-packages/quickstart.md` on the maintainer's Mac (macOS 27): no grey frame at Dock, Finder and Launchpad sizes, seen with `NSWorkspace.shared.icon(forFile:)` in a Swift script, and after T033 the rebuilt `.dmg`'s icon clean at 16–128 pt on regular and Retina screens

**Checkpoint**: All six user stories work

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: The system spec, plan and handover, and the final check

- [X] T036 [P] Update `docs/spec.md` (then `specs/spec.md`): §1 non-goals (now MSIX, code signing and notarization; the packages unsigned, the macOS app ad-hoc signed), §10 the "Packages" paragraph, table and notes and the CI paragraph with two files per RID, and §12 criterion 15 (`b782f7c`); C17 with the white rounded square, a user decision, at 82% (`a90a9e1`); the "User README" paragraph (opening it the first time, naming the buttons as the OS shows them; the repository README repeats the steps) and the signing note (`54c9cbb`)
- [X] T037 [P] Update `docs/plan.md` (then `specs/plan.md`): **packages** ✅ in the user's plan (the AppImage a user decision over an RPM; the installer and the AppImages still to be checked on Windows and Fedora), the user README shipped "in every archive and package", "Idea: installers and start at login (installers done, start at login next)" with the Install row done, and "No data file is installed" and "Uninstalling keeps the user's data" marked done
- [X] T038 [P] Update `docs/copilot/handover.md` (then `specs/copilot/handover.md`): the CI row (two downloads per RID), a new Packages row (the `.dmg`s built and the arm64 app started on macOS; the installer and the AppImages to be checked on Windows and Fedora), "Things that need Windows" (each package on its own OS) and the next steps (packages ✅; Start at Login should register the running executable, `$APPIMAGE` in an AppImage, and the uninstaller should remove its Run value) (`b782f7c`); the Icon row and the lesson "App icon shape" (`a90a9e1`, `54c9cbb`)
- [X] T039 [P] Update the file table in `icon/README.md` for the packages: `App.ico` is also the Windows installer's icon, `App.png` the Linux AppImage's, and `App.icns` is for the macOS `.app` bundle (`packaging/Build-Package.ps1`) (`b782f7c`)
- [X] T040 Run `dotnet build Anniversary.slnx` and `dotnet test --project Anniversary.Tests`: the build is clean and all 189 tests pass, as the app's code didn't change (SC-007)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–8)**: depend on Foundational; User Story 4 documents the packages of User Stories 1–3
- **Polish (Phase 9)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories. Its check (T010) used the CI run of T030;
  a local build on Windows works too
- **User Story 2 (P1)**: after Foundational; independent of User Story 1. Its check (T016) used the `.dmg` from CI, so
  that it was marked as downloaded
- **User Story 3 (P2)**: after Foundational; independent of User Stories 1 and 2. Its check (T021) used the CI run
- **User Story 4 (P2)**: after User Stories 1–3, whose packages it describes; T025 rebuilds the `.dmg` of User Story 2
- **User Story 5 (P2)**: after Foundational and at least one package type; the CI step runs whichever package the RID
  has
- **User Story 6 (P3)**: independent of the packages' code, but it is seen in the `.app` of User Story 2 (T014)

### Within Each User Story

- Templates first, then the script's function for the platform, then a build and the manual check from quickstart.md
- `packaging/Build-Package.ps1` is one file: its platform functions are written one after another

### Parallel Opportunities

- T002 next to T001
- The templates of different platforms (T006, T011, T017, T018) together, and next to the script's functions
- T024 next to T022 and T023 (`README.md` and the user guide); T028 and T029 next to T027 (three different files)
- T034 next to T031–T033
- T036–T039 together: four different documents

---

## Parallel Example: User Story 3

```bash
# The two templates and the pinned downloads can be written side by side:
Task: "Create packaging/linux/AppRun"
Task: "Create packaging/linux/anniversary.desktop"
Task: "Pin appimagetool and the AppImage runtime with Get-PinnedFile in packaging/Build-Package.ps1"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 (install and uninstall on Windows)

### Incremental Delivery

1. Setup and Foundational: the package command
2. User Story 1: the Windows installer (MVP)
3. User Story 2: the macOS `.dmg`
4. User Story 3: the AppImage
5. User Story 4: the first-start steps in both READMEs
6. User Story 5: the packages in every CI run
7. User Story 6: the Mac icon
8. Polish: the system spec, plan and handover, and the final build and test run

### What happened

The maintainer first asked about installers on the afternoon of 2026-10-10, and at his request the idea went into the
plan (`1409624`). That evening he listed the packages among the steps before releases, and the plan's next steps took
his order: user README, packages, Start at Login, releases (`fb5312e`). After the user guide (009) he said "Done, let's
create the packages and InnoSetup installer." The assistant had proposed starting with macOS, which could be built and
tested on the Mac; in one session it wrote the script, the three package types, the CI step and the docs, built and
checked the two `.dmg`s, and the maintainer committed the result as `b782f7c` (19:46). CI first built the installer
and the AppImages. Trying the packages, the maintainer met macOS's "Not Opened" warning without a way to allow the app;
given the steps, he reported all platforms validated and asked about the icon's background. The white square went in
as `a90a9e1` and was drawn on Windows (`8fbd8ff`); checking it found the noise in the small images, which was fixed
together with the new "Opening it the first time" sections in `54c9cbb` (20:33).

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Times are local (UTC−7); the documentation paths are today's, at the time they were under `specs/`
- The uninstaller's Start at Login cleanup came later, with 011
