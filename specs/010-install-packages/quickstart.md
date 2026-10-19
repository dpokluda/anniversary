# Quickstart: Install Packages

How to check the feature as built on 2026-10-10. Expected behaviour: [spec.md](spec.md); the packages:
[contracts/windows-installer.md](contracts/windows-installer.md), [contracts/macos-dmg.md](contracts/macos-dmg.md) and
[contracts/linux-appimage.md](contracts/linux-appimage.md); the build command and the CI step:
[contracts/build-package.md](contracts/build-package.md).

## Prerequisites

- To try the packages: a Windows PC, a Mac and a Linux desktop (the maintainer uses Fedora), and the downloads of a
  passing CI run (**Actions ▸ CI ▸** the run's artifacts; a GitHub sign-in is needed). On the Mac, use the `.dmg` from
  CI, so that it is marked as downloaded; a local build isn't.
- To build a package yourself: the .NET 10 SDK, PowerShell 7.4 or newer (`pwsh`), a clone of the repository, and the
  RID's own OS: Windows with Inno Setup 6.3 or newer; macOS (its `codesign`, `hdiutil`, `plutil` and `xattr`); Linux on
  x86_64 or aarch64 with internet access.
- The app-data folder, which every check below expects to survive: `%APPDATA%\Anniversary` (Windows),
  `~/Library/Application Support/Anniversary` (macOS), `~/.config/Anniversary` (Linux).

## Build, test and package

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet publish Anniversary.Desktop -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/osx-arm64
pwsh packaging/Build-Package.ps1 -Rid osx-arm64
```

Expected at the time: a build without warnings and 189 passing tests (the app's code is unchanged), then
`Package: …/packages/Anniversary-5.0.0-osx-arm64.dmg`. On Windows use `win-x64` or `win-arm64`, on Linux `linux-x64` or
`linux-arm64`.

## Scenario 1: install on Windows (User Story 1; docs/spec.md §12, criterion 15)

1. As a standard user, run `Anniversary-5.0.0-win-x64-setup.exe` (on Windows on Arm, `win-arm64`). If the browser or
   SmartScreen warns, get past it as in scenario 4. **Expected**: the setup doesn't ask for admin rights; it offers
   **Create a desktop shortcut** (unticked) and ends with **Launch Anniversary** ticked; the app starts.
2. **Expected**: `%LOCALAPPDATA%\Programs\Anniversary` holds `Anniversary.exe`, `README.md` and
   `THIRD-PARTY-NOTICES.md`; the Start menu has **Anniversary**, and the app starts from it.
3. Add a person in Edit People… and save, then run the setup again. **Expected**: the installed app is updated in
   place; Settings ▸ Apps lists Anniversary once.
4. Uninstall Anniversary in **Settings ▸ Apps**. **Expected**: the app folder and the shortcuts are gone;
   `%APPDATA%\Anniversary`, with the people, is still there.

## Scenario 2: install on a Mac (User Story 2; criterion 15)

1. Download `Anniversary-5.0.0-osx-arm64.dmg` (Intel Macs: `osx-x64`) and open it. **Expected**: the **Anniversary**
   volume shows `Anniversary.app`, **Applications**, `README.md` and `THIRD-PARTY-NOTICES.md`.
2. Drag Anniversary onto Applications and open it from there. **Expected**: "Anniversary.app" Not Opened, saying macOS
   couldn't verify it (not that it is damaged). Allow it as in scenario 4.
3. **Expected**: the app starts with "Anniversary" and its icon in the Dock and the menu bar, and no Terminal window.
   `codesign --verify --strict /Applications/Anniversary.app` reports no error.
4. Drag the app from Applications to the Trash. **Expected**: it is gone; the app-data folder stays.

## Scenario 3: run on Linux (User Story 3; criterion 15)

1. Download `Anniversary-5.0.0-linux-x64.AppImage` (on Arm, `linux-arm64`) on Fedora.
2. Make it executable (right-click it, **Properties**, turn on **Executable as Program**, or
   `chmod +x Anniversary-*.AppImage`) and double-click it. **Expected**: the app starts; nothing had to be installed and
   nothing asks.
3. Delete the file. **Expected**: the app is gone; `~/.config/Anniversary` stays.

## Scenario 4: open it the first time (User Story 4)

1. Open `README.md` from any package or archive and find **Opening it the first time**. **Expected**: numbered steps
   for macOS (3) and Windows (2), a Terminal alternative for macOS, and a line saying nothing asks on Linux.
2. On a Mac, open a freshly downloaded app: click **Done** on "Anniversary.app" Not Opened (not Move to Trash); in
   **System Settings ▸ Privacy & Security**, under **Security**, click **Open Anyway** (**Open** on some versions) next
   to the message about Anniversary; confirm with **Open Anyway** and your password or Touch ID. **Expected**: the app
   opens, and from then on it opens normally. More than about an hour after the blocked start the button is gone: open
   the app again first.
3. Alternatively, run `xattr -dr com.apple.quarantine /Applications/Anniversary.app`. **Expected**: the app opens
   without the steps.
4. On Windows, if the browser says the file isn't commonly downloaded, keep it (Edge: **⋯ ▸ Keep ▸ Show more ▸ Keep
   anyway**); on "Windows protected your PC", click **More info**, then **Run anyway**. **Expected**: the setup (or
   `Anniversary.exe` from the `.zip`) runs.
5. Open the repository's `README.md`. **Expected**: its **Opening it the first time** gives the same steps briefly and
   links to the user guide's section.

## Scenario 5: packages from every build (User Story 5)

1. Push to `main`, or run CI by hand. **Expected**: every **Publish {rid}** job runs **Archive (…)** and **Package**;
   the run offers 12 downloads, kept for 30 days: `Anniversary-5.0.0-win-x64-setup.exe`,
   `Anniversary-5.0.0-win-arm64-setup.exe`, `Anniversary-5.0.0-osx-arm64.dmg`, `Anniversary-5.0.0-osx-x64.dmg`,
   `Anniversary-5.0.0-linux-x64.AppImage`, `Anniversary-5.0.0-linux-arm64.AppImage` and the six archives.
2. On a Mac, run `pwsh packaging/Build-Package.ps1 -Rid win-x64`. **Expected**: `Build the win-x64 package on its own
   OS (see README.md in this folder).`, and nothing is written.
3. On a Mac without `publish/osx-x64`, run `pwsh packaging/Build-Package.ps1 -Rid osx-x64`. **Expected**:
   `… is missing; publish osx-x64 to … first.`

## Scenario 6: the Mac icon (User Story 6)

1. With the app in Applications on macOS 26 or later, look at it in the Dock, Finder and Launchpad. **Expected**: the
   calendar on a white rounded square, filling most of it and centred, with no grey tile around it.
2. In Finder, look at small sizes (16 to 128 points) on a regular and on a Retina screen. **Expected**: clean, with no
   coloured noise and no grey frame.
3. On Windows and Linux, look at the app's icon. **Expected**: the plain calendar, as before.
4. If Finder still shows an old icon after reinstalling, `touch /Applications/Anniversary.app` refreshes it. To see the
   icon the way macOS draws it without a screenshot, render `NSWorkspace.shared.icon(forFile:)` for the `.app` (by
   absolute path) in a small Swift script (the handover's lessons).
5. To redraw the icon: on Windows, `pwsh -File icon/Build-Icon.ps1 -PreviewFolder preview` writes the icon files and
   previews, `App-mac-*.png` for macOS.

## Platform checks

- At the time, on the maintainer's Mac (Apple Silicon, macOS 27): both `.dmg`s were built; the arm64 app started from
  the `.app` as "Anniversary" 5.0.0 with its window within half a second, and Gatekeeper rejected it, as expected for an
  unsigned app; the x64 `.dmg` was only checked statically, as Rosetta wasn't installed. After the icon fix, the
  rebuilt `.dmg`'s icon was clean at 16–128 pt on regular and Retina screens.
- The Windows installer and the AppImages were first built by CI after the push. The maintainer then tried the CI
  packages on Windows (x64), macOS (Apple Silicon) and Fedora (x64), with the checks asked of him (Windows: install,
  start from the Start menu, uninstall, `%APPDATA%\Anniversary` kept; macOS: drag to Applications, Open Anyway, own
  name and icon in the Dock, no Terminal window; Fedora: `chmod +x` and double-click), and reported "All platform were
  validated."
- Not run on matching machines: the `win-arm64` and `linux-arm64` packages, and the `osx-x64` `.dmg` beyond the static
  check.
