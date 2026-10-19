# Feature Specification: Install Packages

**Feature Branch**: `010-install-packages`

**Created**: 2026-10-10

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "before we create releases, it would be good to produce .app file for MacOS, .appBundle
(or what it is) for Fedora, InnoSetup installer for Windows (besides the .zip and .tar.gz archives)" … "Done, let's
create the packages and InnoSetup installer." After trying them: "One comment is that on MacOS the icon has kind of
white background and so it kind of blends. Curious, do we have to have that background there?" and "Can we add section
to the README file (probably both - the one in code as well as the one in packages) about the necessary changes needed
to run (the one we discussed for MacOS - navigate to privacy & etc ....)"

**Later changes**: [011](../011-start-at-login/spec.md) made the Windows uninstaller also remove Start at Login's entry
when it starts the installed copy, and the user guide's "Removing it" now says to turn Start at Login off first. On
2026-10-11 a small icon follow-up without a feature folder made the calendar's page taller than wide, like v4's, so on
macOS the calendar takes 74% of the icon instead of 82%.

## Clarifications

### Session 2026-10-10

- Q: Which Linux package should the app get: an RPM for Fedora or an AppImage? → A: An AppImage (user decision): one
  file that runs on Fedora and other distributions, with nothing to install.
- Q: Should the Windows installer need admin rights? → A: No. It installs for the current user only, without admin
  rights, into `%LOCALAPPDATA%\Programs\Anniversary`, with a Start-menu shortcut and an uninstaller. This was proposed
  when the maintainer first asked about installers earlier that day and went into the plan at his request.
- Q: Does an installer have to create an empty data file? → A: No. The app already starts with the empty built-in
  people list and creates the data file in the app-data folder on the first Save. A package must never put a data file
  next to the executable, where saving could fail, and uninstalling keeps the user's data.
- Q: Will the packages be signed? → A: Not decided: it needs a paid Apple Developer account and a Windows code-signing
  certificate. The Mac app gets only an ad-hoc signature: without one, Apple Silicon Macs won't run it, and a
  downloaded copy is reported as damaged instead of offering Open Anyway. The user guide explains the first start.
- Q: macOS says "Anniversary.app" Not Opened and offers no way to allow it; how does a user open it? → A: That is
  expected for an app Apple hasn't checked: click **Done**, then **Open Anyway** in **System Settings ▸ Privacy &
  Security** (there for about an hour) and confirm. At the maintainer's request both READMEs got a section "Opening it
  the first time" with these steps, and with Windows' SmartScreen steps.
- Q: Does the Mac icon have to sit on the light background macOS draws behind it? → A: macOS 26 and later put any icon
  that isn't a rounded square on a grey tile, so the tile can only be replaced. Offered coloured squares (blue, green,
  red, graphite), the maintainer chose a white one, like most icons on his Mac, with the calendar bigger and centred
  (82% of the icon). Windows and Linux keep the plain calendar.
- Q: Should the packages also be published as GitHub Releases? → A: Not now ("Let's ignore it for now."). They are
  downloads of each CI run, kept for 30 days.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Install Anniversary on Windows (Priority: P1)

A Windows user downloads the setup program for their PC (`Anniversary-5.0.0-win-x64-setup.exe`, or `win-arm64` for
Windows on Arm) and runs it. It installs the app just for them, without admin rights, adds it to the Start menu and
starts it. Later, **Settings ▸ Apps** removes it again, while their people and settings stay.

**Why this priority**: Windows comes first in the app's platform order, and the maintainer asked for the Windows
installer by name. Until then a Windows user had to extract a `.zip`, keep the folder somewhere and start
`Anniversary.exe` from it.

**Independent Test**: On a Windows PC, run the setup as a standard user and start the app from the Start menu; then
uninstall it in Settings ▸ Apps: the app is gone and `%APPDATA%\Anniversary` is still there.

**Acceptance Scenarios**:

1. **Given** a standard user account, **When** the user runs the setup program, **Then** it installs without asking
   for admin rights into `%LOCALAPPDATA%\Programs\Anniversary`: the app, the user guide and the third-party notices.
2. **Given** the setup's last page with **Launch Anniversary** ticked, **When** the user clicks Finish, **Then** the
   app starts, and from then on the Start menu has an **Anniversary** entry.
3. **Given** the choice **Create a desktop shortcut**, which isn't ticked at first, **When** the user ticks it,
   **Then** a desktop shortcut is created too.
4. **Given** an installed app, **When** the user runs the setup of a newer version or of the other architecture,
   **Then** the installed app is updated in place, not installed a second time.
5. **Given** an installed app and saved people, **When** the user uninstalls it in Settings ▸ Apps, **Then** the app
   and its shortcuts are removed and the app-data folder with the people and settings stays.
6. **Given** a PC with Windows on Arm, **When** the user runs the x64 setup, **Then** it installs as well; the Arm
   setup is the native one.

---

### User Story 2 - Install Anniversary on a Mac (Priority: P1)

A Mac user downloads the disk image for their Mac (`osx-arm64` for Apple Silicon, `osx-x64` for Intel), opens it and
drags Anniversary onto the Applications shortcut next to it. From then on it's a normal Mac app: it starts from
Applications with its own name and icon in the Dock and the menu bar, without the Terminal window that the bare
executable opened. Removing it means dragging it to the Trash.

**Why this priority**: A proper macOS app, with its own name and icon in the Dock, had been the agreed next step since
CI was added, and the maintainer's request names it first. Until then the Mac download was a bare executable that
opened a Terminal window next to the app.

**Independent Test**: Open the disk image from a CI run on a Mac, drag the app to Applications and start it (after the
one-time approval of User Story 4): it runs with its own name and icon and no Terminal window.

**Acceptance Scenarios**:

1. **Given** the downloaded disk image, **When** the user opens it, **Then** it shows Anniversary, a shortcut to
   Applications to drag it onto, the user guide and the third-party notices.
2. **Given** the app dragged to Applications, **When** the user opens it the first time, **Then** macOS says that it
   couldn't verify the app ("Anniversary.app" Not Opened), not that the app is damaged, and the user can allow it with
   Open Anyway (User Story 4).
3. **Given** the app was allowed, **When** it starts, **Then** the Dock and the menu bar show "Anniversary" with the
   app's icon, and no Terminal window opens.
4. **Given** the app in Applications, **When** the user drags it to the Trash, **Then** it is gone and the data folder
   (`~/Library/Application Support/Anniversary`) stays.

---

### User Story 3 - Run Anniversary on Linux from one file (Priority: P2)

A Linux user downloads the AppImage for their machine (`linux-x64`, or `linux-arm64` on Arm), makes it executable and
double-clicks it. It is the whole app in one file, with nothing to install; tools such as Gear Lever can add it to the
applications menu. Removing it means deleting the file.

**Why this priority**: Linux is third in the app's platform order, and the plain archive already ran there; the
AppImage makes it a single file that desktop tools can pick up.

**Independent Test**: On Fedora, make the downloaded AppImage executable and double-click it: the app starts without
anything being installed.

**Acceptance Scenarios**:

1. **Given** the downloaded file, **When** the user makes it executable (right-click it, **Properties**, turn on
   **Executable as Program**, or `chmod +x Anniversary-*.AppImage`) and double-clicks it, **Then** the app starts.
2. **Given** a desktop distribution such as Fedora Workstation, **When** the AppImage starts, **Then** nothing has to
   be installed first, not even an extra system library.
3. **Given** the AppImage, **When** a desktop tool such as Gear Lever reads it, **Then** it finds the name Anniversary,
   the description "Name days, birthdays and anniversaries for any day" and the app's icon.
4. **Given** the AppImage, **When** the user deletes it, **Then** the app is gone and the data folder
   (`~/.config/Anniversary`) stays.

---

### User Story 4 - Open the app the first time (Priority: P2)

The packages aren't signed by Microsoft or Apple, so the first start brings a warning, and on macOS the warning has no
button to allow the app. The user guide that comes with every download explains how to allow the app once, step by
step, naming the buttons as the operating system shows them; the developer README repeats the steps briefly.

**Why this priority**: Without these steps a Mac user can't open the app at all: the maintainer himself found no way
to allow it from the warning. It is P2 because it completes User Stories 1 and 2 rather than standing alone.

**Independent Test**: Give someone who hasn't seen the warnings a freshly downloaded copy and the shipped guide on
macOS and on Windows: following only its section "Opening it the first time", they get the app running.

**Acceptance Scenarios**:

1. **Given** a freshly downloaded app on macOS, **When** macOS says "Anniversary.app" Not Opened, **Then** the guide's
   steps open it: click **Done** (not Move to Trash); in **System Settings ▸ Privacy & Security**, under **Security**,
   click **Open Anyway** (**Open** on some macOS versions) next to the message about Anniversary; confirm with **Open
   Anyway** and the password or Touch ID. After that it opens normally.
2. **Given** about an hour has passed since the blocked start and the button is gone, **When** the user follows the
   guide, **Then** it tells them to open Anniversary again first.
3. **Given** Terminal, **When** the user runs the one-line command from the guide
   (`xattr -dr com.apple.quarantine /Applications/Anniversary.app`; for the `.tar.gz`,
   `xattr -d com.apple.quarantine Anniversary` in its folder), **Then** the app opens without the steps.
4. **Given** a download on Windows, **When** the browser says the file isn't commonly downloaded, **Then** the guide's
   step keeps it (Edge: **⋯ ▸ Keep ▸ Show more ▸ Keep anyway**); **When** Windows then says "Windows protected your
   PC", **Then** **More info**, then **Run anyway** runs it.
5. **Given** Linux, **When** the user reads the section, **Then** it says that nothing asks: the AppImage only has to
   be executable.
6. **Given** the developer README, **When** a developer reads its "Opening it the first time", **Then** it gives the
   same steps briefly and links to the guide's section.

---

### User Story 5 - Get every package from each build (Priority: P2)

Every build that passes offers, for each of the six platforms, the package next to the plain archive, so the
maintainer can download the right file and hand it out. He can also build a single package himself, on that
platform's own operating system, with one command.

**Why this priority**: The packages reach people only through these downloads, since permanent release pages were put
off, and the Windows installer and the AppImages were first built there. It is P2 because the packages themselves
(User Stories 1–3) come first.

**Independent Test**: Push a change and open the build run: it offers twelve downloads, a package and an archive for
each platform.

**Acceptance Scenarios**:

1. **Given** a passing build, **When** the maintainer opens the run, **Then** it offers
   `Anniversary-{version}-{rid}-setup.exe` for both Windows platforms, `Anniversary-{version}-{rid}.dmg` for both
   Macs and `Anniversary-{version}-{rid}.AppImage` for both Linux platforms, each next to the archive
   (`Anniversary-{rid}.zip` or `Anniversary-{rid}.tar.gz`), kept for 30 days.
2. **Given** a package that can't be built, **When** the build runs, **Then** that platform's build fails instead of
   leaving the package out.
3. **Given** the app published for `osx-arm64` on a Mac, **When** the maintainer runs the package command for it,
   **Then** it writes `Anniversary-5.0.0-osx-arm64.dmg` and prints its path.
4. **Given** the wrong operating system (for example the Windows package on a Mac), or an app that wasn't published
   first, **When** the maintainer runs the command, **Then** it stops and says what to do.

---

### User Story 6 - A Mac icon like other Mac apps (Priority: P3)

macOS 26 and later put any app icon that isn't a rounded square onto a light-grey tile, so the calendar blended into
it. The Mac app's icon becomes the calendar on a white rounded square of its own, like most icons on the maintainer's
Mac, with the calendar bigger and centred. Windows and Linux keep the plain calendar.

**Why this priority**: It is about looks: the app works without it. The maintainer raised it after trying the
packages.

**Independent Test**: Install the Mac app and look at it in the Dock, Finder and Launchpad, on a regular and on a
Retina screen: the calendar on a white rounded square, with no grey frame and no noise at small sizes.

**Acceptance Scenarios**:

1. **Given** the app in Applications on macOS 26 or later, **When** it shows in the Dock, Finder or Launchpad,
   **Then** its icon is the calendar on a white rounded square, without a grey tile around it.
2. **Given** the icon, **When** the user looks at it, **Then** the calendar fills 82% of it and sits in the middle
   (in the first previews it looked too low).
3. **Given** a regular (non-Retina) or a Retina screen, **When** Finder shows the icon at any size from 16 to 128
   points, **Then** it is clean, without coloured noise or a grey frame.
4. **Given** Windows or Linux, **When** the app's icon shows, **Then** it is the plain calendar, as before.

### Edge Cases

- **Windows on Arm**: the x64 setup installs there too; the `win-arm64` setup installs only on Arm.
- **Setup run over an installed copy** (a newer version, or the other architecture): the installed app is updated in
  place; there is never a second copy.
- **A new version, or a replaced Mac app**: the first-start approval may be needed again; the guide says so.
- **Open Anyway is gone**: the button is there for about an hour after the blocked start; open the app again.
- **The Mac `.tar.gz`**: it still holds the bare app, which opens a Terminal window next to it; the guide's steps (or
  `xattr -d com.apple.quarantine Anniversary` in its folder) apply to it as well.
- **No data next to the app**: inside the Mac app and the AppImage the folder next to the executable is read-only, so
  data files belong in the app-data folder, as before.
- **Small icons on regular screens**: once the app shipped as a Mac app, its smallest icons (16 and 32 px) showed as
  coloured noise on non-Retina screens; the Mac icon leaves those small images out, so macOS scales larger ones down.
- **Building a package on the wrong operating system, or before publishing**: the command stops with a message that
  says what to do; nothing is written.
- **A downloaded build tool that doesn't match its checksum**: the Linux build stops.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Besides the plain archives (`.zip`, `.tar.gz`), each of the six platforms (Windows, macOS and Linux,
  each on x64 and Arm) MUST get a package that installs or runs the app in that operating system's usual way: a setup
  program on Windows, a disk image holding the app on macOS, and a single runnable file (AppImage) on Linux.
- **FR-002**: Each package's file name MUST carry the app's version and the platform:
  `Anniversary-{version}-{rid}-setup.exe`, `Anniversary-{version}-{rid}.dmg` and `Anniversary-{version}-{rid}.AppImage`
  (for example `Anniversary-5.0.0-win-x64-setup.exe`). The version and copyright that the setup program and the Mac app
  show MUST be the app's own.
- **FR-003**: Every package MUST hold the same app as the platform's archive plus the user guide (`README.md`) and the
  third-party notices (`THIRD-PARTY-NOTICES.md`), and MUST NOT hold a data file or any personal data: the app starts
  with its empty built-in list and keeps people and settings in the app-data folder, as before.
- **FR-004**: The Windows setup program MUST install the app for the current user only, without admin rights, into
  `%LOCALAPPDATA%\Programs\Anniversary`, add a Start-menu entry, offer a desktop shortcut that is off by default, and
  offer to start the app when it finishes.
- **FR-005**: Running the setup of a newer version, or of the other architecture, MUST update the installed app in
  place. The x64 setup MUST also install on Windows on Arm.
- **FR-006**: The app MUST be removable in Settings ▸ Apps; uninstalling MUST remove the installed app and its
  shortcuts and MUST NOT touch the app-data folder.
- **FR-007**: The Mac disk image MUST hold the app (`Anniversary.app`), a shortcut to Applications to drag it onto,
  the user guide and the notices.
- **FR-008**: The Mac app MUST start with its own name and icon in the Dock and the menu bar and without a Terminal
  window, carry the app's version, copyright, user guide and notices inside, and declare macOS 12.0 as its minimum, as
  the published executable does.
- **FR-009**: A downloaded copy of the Mac app MUST be one that macOS reports as not verified, which the user can allow
  once with Open Anyway, and never as damaged.
- **FR-010**: The Linux package MUST be one file that runs once it is made executable, with nothing to install and
  nothing needed beyond what desktop distributions already have. It MUST carry the app's name, a short description and
  the icon for desktop tools, plus the user guide and the notices.
- **FR-011**: The user guide shipped with every download MUST explain which download to choose (x64 or Arm; Apple
  Silicon or Intel Mac), how to install or run each package and archive, and how to remove each, with the data folder
  staying.
- **FR-012**: The user guide MUST have a section "Opening it the first time" with numbered steps through the macOS and
  Windows warnings for an unsigned app, naming the buttons as the operating system shows them: on macOS **Done** on
  "Anniversary.app" Not Opened, then **System Settings ▸ Privacy & Security**, **Security**, **Open Anyway** (**Open**
  on some versions; there for about an hour), confirmed with **Open Anyway** and the password or Touch ID, plus a
  one-line Terminal alternative; on Windows the browser's **Keep** for a file that isn't commonly downloaded (Edge:
  **⋯ ▸ Keep ▸ Show more ▸ Keep anyway**) and **More info**, **Run anyway** on "Windows protected your PC"; and that
  nothing asks on Linux.
- **FR-013**: The developer README MUST repeat the first-start steps briefly, link to the user guide's section, and
  explain how to build and download the packages.
- **FR-014**: Every passing build MUST offer each platform's package as a download of its own next to the archive
  (two downloads per platform, twelve in all), kept for 30 days; a package that isn't produced MUST fail the build.
- **FR-015**: The maintainer MUST be able to build any one package with one command, on that platform's own operating
  system, from the published app. The command MUST stop with a clear message on another operating system, or when the
  published app, the user guide or the notices are missing.
- **FR-016**: Any build tool downloaded while building a package MUST be a fixed version checked against a known
  checksum before use; a mismatch MUST stop the build.
- **FR-017**: On macOS, the app's icon MUST be the calendar on a white rounded square of the shape and size Apple's own
  app icons use, with the calendar filling 82% of the icon, so that macOS 26 and later show it without a grey tile.
- **FR-018**: The Mac icon MUST stay clean (no noise, no grey frame) at every size from 16 to 128 points, on regular and
  Retina screens.
- **FR-019**: Windows and Linux MUST keep the plain calendar icon.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On Windows, the setup installs the app without asking for admin rights, the app starts from the Start
  menu, and uninstalling removes it while the app-data folder stays (acceptance criterion 15 in
  [docs/spec.md](../../docs/spec.md) §12).
- **SC-002**: On a Mac, the app dragged from the disk image to Applications starts after Open Anyway, with its own name
  and icon in the Dock and the menu bar, and without a Terminal window (criterion 15).
- **SC-003**: On Fedora, the AppImage starts after being made executable, with nothing else installed (criterion 15).
- **SC-004**: Every passing build offers 12 downloads, a package and an archive for each of the 6 platforms, kept for
  30 days.
- **SC-005**: With only the shipped guide, a first-time user gets from the warning to a running app in 3 numbered steps
  on macOS and 2 on Windows; on Linux nothing asks.
- **SC-006**: The Mac icon shows as a white rounded square without a grey frame in the Dock, Finder and Launchpad, and
  is clean from 16 to 128 points on regular and Retina screens; the Windows and Linux icons don't change.
- **SC-007**: The app itself doesn't change: the build stays free of warnings and all 189 automated tests pass.

## Assumptions

- Builds on [001](../001-cross-platform-rewrite/spec.md) (the self-contained single-file app, the app-data folder and
  the empty built-in people list), [003](../003-calendar-chooser/spec.md) (`THIRD-PARTY-NOTICES.md` next to the
  executable), [005](../005-ci-pipeline/spec.md) (CI that publishes every platform on its own operating system and
  attaches the archives) and [009](../009-user-guide/spec.md) (the user guide, shipped as `README.md` next to the
  executable).
- The plain archives stay as they are; the packages come in addition.
- Signing by Microsoft or Apple (a Windows code-signing certificate, Apple Developer ID and notarization) isn't
  decided and isn't part of this feature; neither are Microsoft Store packages (MSIX), which need a trusted signing
  certificate.
- The packages are downloads of each build run (a GitHub sign-in is needed, and they are kept for 30 days); permanent
  public downloads (GitHub Releases) were put off.
- Starting the app at login is the next feature ([011](../011-start-at-login/spec.md)); it isn't an installer option.
- Opening a data file by double-clicking it, which [007](../007-data-file-sharing/spec.md) left for the installers,
  isn't part of this feature: it needs a file extension of its own.
- The maintainer checked the packages on Windows (x64), macOS (Apple Silicon) and Fedora (x64). The Intel Mac disk
  image was only checked statically, and the Windows and Linux Arm packages haven't run on matching machines.
