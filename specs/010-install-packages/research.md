# Research: Install Packages

Decisions made while planning and building the feature on 2026-10-10. Sources: the sessions of 2026-10-10 (the
maintainer's installer question in the afternoon, his plan and requests in the evening, his test reports and the
assistant's answers), [docs/plan.md](../../docs/plan.md) (item 2 **packages** and "Idea: installers and start at
login"), [docs/spec.md](../../docs/spec.md) §1, §10, C17 and §12,
[docs/copilot/handover.md](../../docs/copilot/handover.md), [packaging/README.md](../../packaging/README.md),
[icon/README.md](../../icon/README.md) and the commits `b782f7c`, `a90a9e1`, `8fbd8ff` and `54c9cbb`. Alternatives are
listed only where a source names them.

## A package per platform, besides the archives

- **Decision**: Each RID gets a package: a setup program (`-setup.exe`) for `win-x64` and `win-arm64`, a `.dmg`
  holding `Anniversary.app` for `osx-arm64` and `osx-x64`, and an AppImage for `linux-x64` and `linux-arm64`. The
  `.zip` and `.tar.gz` archives stay.
- **Rationale**: The maintainer wanted them before any release, "besides the .zip and .tar.gz archives". Each follows
  its OS's usual way to install an app; for macOS the idea table says "macOS apps rarely use installers".
- **Alternatives considered**: From the idea table the assistant drew up when the maintainer first asked about an
  installer (2026-10-10, afternoon): MSIX on Windows, possible but only with a trusted signing certificate; on Linux,
  the `.tar.gz` plus a small install script that adds a `.desktop` entry and the icon, or an RPM or a Flatpak.
- **Source**: session 2026-10-10; docs/plan.md, "Idea: installers and start at login" (added at the maintainer's
  request in `1409624`) and the plan's next steps, in his order since `fb5312e`: user README, packages, Start at Login,
  releases.

## An AppImage rather than an RPM

- **Decision**: Linux gets an AppImage.
- **Rationale**: "one file that runs on Fedora and other distributions, nothing to install" (docs/plan.md). With the
  static AppImage runtime it doesn't even need libfuse2.
- **Alternatives considered**: An RPM, which was suggested first ("an RPM is suggested, an AppImage is the
  alternative; not decided yet"); a Flatpak "later, if wanted". The maintainer chose the AppImage (user decision).
- **Source**: docs/plan.md, item 2 ("user decision over an RPM"); spec 10; session 2026-10-10 ("Two decisions are still
  open for packaging: RPM or AppImage for Fedora, and whether to pay for code signing").

## A per-user installer built with Inno Setup

- **Decision**: Inno Setup 6 compiles `packaging/windows/Anniversary.iss` into `Anniversary-{version}-{rid}-setup.exe`:
  `PrivilegesRequired=lowest` and `DefaultDirName={autopf}\Anniversary`, which is `%LOCALAPPDATA%\Programs\Anniversary`
  for an install without admin rights; a Start-menu shortcut, an optional desktop shortcut (unticked) and "Launch
  Anniversary" on the last page.
- **Rationale**: No admin rights, and per user like the app's data. Inno Setup is a free installer builder and is
  preinstalled on GitHub's Windows runners, so CI can build the installer.
- **Alternatives considered**: MSIX, which needs a trusted signing certificate.
- **Source**: the maintainer's request ("InnoSetup installer for Windows"); docs/plan.md, "Idea: installers and start
  at login"; `packaging/README.md`; session 2026-10-10.

## One AppId for every version and architecture

- **Decision**: One fixed `AppId` (`7773EC74-CEFF-475D-B70F-64346BA207FA`) for both architectures and every version.
  The installer allows `arm64` for `win-arm64` and `x64compatible` for `win-x64`.
- **Rationale**: The `AppId` is "how a newer installer finds the installed app and updates it", so it must never
  change; "x64compatible also lets Windows on ARM install the x64 build".
- **Alternatives considered**: none recorded.
- **Source**: `packaging/windows/Anniversary.iss`; `packaging/README.md`; spec 10.

## No data file in any package; uninstalling keeps the data

- **Decision**: The packages hold only the app, `README.md` and `THIRD-PARTY-NOTICES.md`. The uninstaller removes only
  what it installed and never the app-data folder.
- **Rationale**: The app already starts with the empty built-in people list and creates `Anniversary.Data.json` in the
  app-data folder on the first Save (spec 5.6). "An installer must never put a data file next to the executable:
  Program Files is read-only for users, so saving would fail"; inside the `.app` and the AppImage, too, the folder next
  to the executable is read-only (spec 10).
- **Alternatives considered**: The maintainer's first idea included an installer step that creates an empty data file
  ("maybe not necessary if the problem simply assume one when missing"); the answer was that it isn't needed.
- **Source**: session 2026-10-10 (afternoon); docs/plan.md, "No data file is installed" and "Uninstalling keeps the
  user's data"; spec 10.

## An ad-hoc signed `Anniversary.app` in a `.dmg`

- **Decision**: The script assembles the bundle (`Contents/MacOS/Anniversary`, `Contents/Resources` with `App.icns`,
  the README and the notices, and `Contents/Info.plist` from the template), clears its extended attributes
  (`xattr -cr`), signs the whole bundle ad hoc (`codesign --force --sign -`) and checks the signature
  (`codesign --verify --strict`). The `.dmg` holds the app, an `Applications` link to drag it onto, the README and the
  notices.
- **Rationale**: In the script's words: "Apple Silicon only runs signed code, and a downloaded bundle without a valid
  signature is reported as damaged instead of offering Open Anyway, so the whole bundle gets an ad-hoc signature." When
  the maintainer opened the installed app, macOS said it could not verify the app rather than that it was damaged, "so
  the ad-hoc signature works". Dragging an app from a `.dmg` into Applications is the usual way on macOS.
- **Alternatives considered**: No signature (reported as damaged); Developer ID signing with notarization, which needs a
  paid Apple Developer account and isn't decided; an installer, which macOS apps rarely use.
- **Source**: `packaging/Build-Package.ps1`; `packaging/README.md`; spec 10; session 2026-10-10.

## The bundle's `Info.plist`

- **Decision**: Bundle identifier `cz.pokluda.anniversary`; name, display name and executable `Anniversary`; icon file
  `App.icns`; the version from `Directory.Build.props` as both `CFBundleShortVersionString` and `CFBundleVersion`; the
  XML-escaped copyright as `NSHumanReadableCopyright`; `LSMinimumSystemVersion` 12.0; category
  `public.app-category.productivity`; `NSHighResolutionCapable`. `plutil -lint` checks the filled-in file.
- **Rationale**: A bundle with its `Info.plist`, `App.icns` and app name was the agreed **mac-app** step since CI was
  added, "for a proper Dock icon and name"; 12.0 "is the minimum macOS that the published executable declares".
- **Alternatives considered**: none recorded.
- **Source**: `packaging/macos/Info.plist`; docs/plan.md as of `1409624` (**mac-app**).

## Signing: not decided

- **Decision**: Nothing is signed by Microsoft or Apple. The first start needs one approval on Windows and macOS, and
  the user guide explains it ("Opening it the first time", below).
- **Rationale**: Signing needs a Windows code-signing certificate and a paid Apple Developer account (Developer ID
  signing and notarization); later that evening the assistant listed code signing among the things parked for later
  because "it costs money".
- **Alternatives considered**: Apple Developer ID signing with notarization, and a Windows code-signing certificate,
  which would remove the first-start steps.
- **Source**: spec 1 (non-goals) and 10; docs/plan.md; session 2026-10-10.

## Pinned AppImage tools

- **Decision**: The script downloads appimagetool 1.9.1 (for the build machine's architecture) and the AppImage type2
  runtime 20251108 (for the target's) from their GitHub releases, checks each against a SHA-256 written in the script,
  and runs appimagetool with `APPIMAGE_EXTRACT_AND_RUN=1`, `ARCH`, `--no-appstream` and `--runtime-file`.
- **Rationale**: Fixed, checked downloads. The static runtime means the AppImage needs no libfuse2. "Extracting itself
  first means appimagetool doesn't need FUSE on the build machine." The runtime "becomes the start of the AppImage, so
  it's for the target", which lets CI's x86_64 `ubuntu-latest` runner build the `linux-arm64` AppImage too.
- **Alternatives considered**: none recorded. To move to newer tools, the versions and hashes at the top of the script
  change.
- **Source**: `packaging/Build-Package.ps1`; `packaging/README.md`; spec 10; `.github/workflows/ci.yml`.

## One script, run on each RID's own OS

- **Decision**: `packaging/Build-Package.ps1 -Rid <rid>` builds one package from `publish/<rid>` into `packages/`
  (ignored by Git). It refuses to run on another OS or without the published files, and takes the version and
  copyright from `Directory.Build.props`. It is PowerShell 7, like `calendars/Build-Calendars.ps1` and
  `icon/Build-Icon.ps1`.
- **Rationale**: Each package needs its OS's tools: Inno Setup on Windows, `codesign` and `hdiutil` on macOS,
  appimagetool on Linux. (All six publishes can be built on Windows; the packages can't.) CI's publish job already runs
  on each RID's own OS, and taking the version from the build keeps the package names and the installer and bundle
  versions in step with the app.
- **Alternatives considered**: none recorded.
- **Source**: `packaging/Build-Package.ps1`; `packaging/README.md`; the handover ("Things that need Windows").

## CI attaches the package next to the archive

- **Decision**: In the publish job, a **Package** step runs the script after the archive is uploaded, and a second
  `actions/upload-artifact@v7` uploads the file named by the step output `path` as is (`archive: false`), for 30 days,
  with `if-no-files-found: error`. The archive steps were renamed **Archive (zip)** and **Archive (tar.gz)**.
- **Rationale**: The installer and the AppImages can only be built on their own OS, which CI has; each run then offers
  both forms for every platform; a package that isn't built fails the run.
- **Alternatives considered**: GitHub Releases from a version tag, the plan's next item after Start at Login. Asked
  what a release would need, the assistant described a tag-triggered job; the maintainer put it off ("Let's ignore it
  for now.").
- **Source**: `.github/workflows/ci.yml`; session 2026-10-10.

## "Opening it the first time" in both READMEs

- **Decision**: The user guide gets a section "Opening it the first time" with numbered steps for macOS and Windows
  that name the buttons as the OS shows them, the Terminal alternative and a line for Linux; "Starting it" is only about
  installing and points to the section. The developer README repeats the steps briefly and links to the guide's
  section.
- **Rationale**: Running the installed app, the maintainer saw "Not Opened" and no way to allow it: "On recent macOS
  versions, this warning never has an Open button for apps Apple hasn't checked. You allow it in System Settings
  instead." He then asked for these steps in both READMEs.
- **Alternatives considered**: The short notes on the warnings in "Starting it" from the first commit (`b782f7c`),
  which the section replaced.
- **Source**: session 2026-10-10; spec 10 (User README); `Anniversary.Desktop/README.md`; `README.md`.

## A white rounded square for the macOS icon

- **Decision**: `New-MacIcon` in `icon/Build-Icon.ps1` draws the calendar on a white rounded square on Apple's icon
  grid (824 px with 185 px corners, 100 px in from each edge of 1024), with a light top-to-bottom gradient and a faint
  shadow. The calendar is drawn at its own size, so small icons keep their simpler drawing, at 82% of the icon, with
  row 132.5 of its 256 grid at the icon's middle. Only `App.icns` uses it; Windows and Linux keep the plain calendar.
- **Rationale**: macOS 26 and later put any app icon that isn't a rounded square on a grey tile, which washed the
  calendar out; an `App.icns` that is itself a rounded square is shown as is (tried on the maintainer's macOS 27). The
  maintainer chose white ("Most of the icons I see on my system are bright(er) white and icon inside") and a bigger,
  centred calendar ("the icon inside looks more down than centered").
- **Alternatives considered**: Keeping the plain calendar and the grey tile; a coloured square: blue (the assistant's
  suggestion), green, red or graphite.
- **Source**: session 2026-10-10; spec C17 ("user decision: white, calendar at 82% of the icon"); `icon/README.md`;
  the handover's lesson "App icon shape".

## No 1x small images in `App.icns`

- **Decision**: `Write-Icns` writes only `ic07`–`ic10` (128, 256, 512 and 1024 px) and the @2x `ic11`–`ic14` (32, 64,
  256 and 512 px); the 1x PNG entries `icp4`, `icp5` and `icp6` (16, 32 and 64 px) are gone. `App.icns` was rewritten
  on the Mac from the images drawn on Windows, minus those entries, which is byte for byte what the changed script
  writes, so the script didn't have to run on Windows again.
- **Rationale**: On non-Retina screens macOS showed the 16 and 32 px PNGs as coloured noise ("The icon has had this
  from the start; it only shows now that the app ships as a .app"). Without them it scales the larger images down
  cleanly: the rebuilt `.dmg`'s icon was clean at 16–128 pt on regular and Retina screens.
- **Alternatives considered**: Storing those sizes the way Apple's tools do (`ic04` and `ic05`, ARGB, what `iconutil`
  writes), which "just swapped the noise for the grey frame".
- **Source**: session 2026-10-10; `icon/README.md`; the handover's lesson "App icon shape".

## Verification approach

- **Decision**: Build and start the macOS packages on the maintainer's Mac while working; let CI build the Windows
  installer and the AppImages on its first run; have the maintainer try the CI packages on Windows, macOS and Fedora
  (criterion 15). For the icon: try the white square with a Swift copy of the script's drawing (the script needs
  Windows), and look at the result the way macOS draws it, with `NSWorkspace.shared.icon(forFile:)` in a small Swift
  script.
- **Rationale**: Each package needs its own OS. On the Mac, the arm64 app started from the `.app` as "Anniversary"
  5.0.0 within half a second, and Gatekeeper rejected it as expected for an unsigned app; the x64 `.dmg` was only
  checked statically, as Rosetta wasn't installed. A local build isn't marked as downloaded, so the maintainer was asked
  to try the `.dmg` from CI. He reported "All platform were validated."
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; the handover (the Packages row and the lessons).

## Offered later and not taken up

- **Decision**: Not part of this feature: having setup and uninstall ask the user to close a running Anniversary first
  (Inno Setup's `AppMutex`), and opening a data file by double-clicking it, which
  [007](../007-data-file-sharing/spec.md) had left for the installers and which needs a file extension of its own.
- **Rationale**: Both were offered as optional items later that evening, after Start at Login; the maintainer didn't
  take them up.
- **Alternatives considered**: none.
- **Source**: session 2026-10-10.
