# Contract: the Windows installer

`Anniversary-{version}-{rid}-setup.exe` for `win-x64` and `win-arm64`, as built on 2026-10-10 from
`packaging/windows/Anniversary.iss` with Inno Setup 6 (6.3 or newer). Behaviour: [spec.md](../spec.md) (User
Story 1); the command that builds it: [build-package.md](build-package.md); system spec:
[docs/spec.md](../../../docs/spec.md) §10 and §12, criterion 15.

## Compiling

`Build-Package.ps1` runs:

```text
ISCC.exe /DAppVersion={version} /DRid={rid} /DPublishFolder={publish folder} /DOutputFolder={output folder}
         /DCopyright={copyright} packaging\windows\Anniversary.iss
```

| Define | Required | Without it |
|---|---|---|
| `AppVersion` | yes | `#error Pass /DAppVersion=<version>` |
| `Rid` | yes | `#error Pass /DRid=win-x64 or /DRid=win-arm64` |
| `PublishFolder` | yes | `#error Pass /DPublishFolder=<the folder with Anniversary.exe>` |
| `OutputFolder` | no | `.` |
| `Copyright` | no | `David Pokluda` |

## Setup settings

| Setting | Value | Effect |
|---|---|---|
| `AppId` | `{7773EC74-CEFF-475D-B70F-64346BA207FA}` (written `{{…}` in the script) | The same for every version and both architectures, so a newer setup, or the other architecture's, updates the installed app in place. Never change it. |
| `AppName`, `AppVersion` | `Anniversary`, `{version}` (5.0.0) | The name and version the setup shows and registers for Settings ▸ Apps |
| `AppPublisher`, `AppPublisherURL`, `AppSupportURL` | `David Pokluda`, https://github.com/dpokluda/anniversary, https://github.com/dpokluda/anniversary/issues | The publisher and links of the uninstall entry |
| `AppCopyright`, `VersionInfoVersion` | `{copyright}`, `{version}` | The setup program's own file details |
| `PrivilegesRequired` | `lowest` | Installs for the current user only; never asks for admin rights |
| `DefaultDirName` | `{autopf}\Anniversary` | `%LOCALAPPDATA%\Programs\Anniversary` for this per-user install |
| `DisableProgramGroupPage` | `yes` | No Start-menu folder page; the shortcut goes straight into Programs |
| `ArchitecturesAllowed`, `ArchitecturesInstallIn64BitMode` | `arm64` for `win-arm64`; `x64compatible` for `win-x64` | The x64 setup also installs on Windows on Arm; the arm64 setup only there |
| `OutputBaseFilename` | `Anniversary-{version}-{rid}-setup` | e.g. `Anniversary-5.0.0-win-x64-setup.exe` |
| `SetupIconFile`, `UninstallDisplayIcon` | `Anniversary.Shared\Assets\App.ico`, `{app}\Anniversary.exe` | The app's icon on the setup program and in Settings ▸ Apps |
| `WizardStyle`, `Compression`, `SolidCompression` | `modern`, `lzma2/max`, `yes` | |
| Languages | `english` (`compiler:Default.isl`) | English texts only |

## What it installs

| Item | Where |
|---|---|
| `Anniversary.exe`, `README.md` (the user guide), `THIRD-PARTY-NOTICES.md` | `{app}` = `%LOCALAPPDATA%\Programs\Anniversary` (replaced on every install: `ignoreversion`) |
| Start-menu shortcut **Anniversary** | the user's Start-menu programs (`{autoprograms}`), always |
| Desktop shortcut **Anniversary** | the user's desktop (`{autodesktop}`), only if the task **Create a desktop shortcut** (under **Additional shortcuts:**) is ticked; it is unticked at first |

No data file, settings file or registry value of the app's own is installed; the app-data folder
(`%APPDATA%\Anniversary`) is never touched.

## Finishing

The last page has a ticked **Launch Anniversary** check box (`{cm:LaunchProgram,Anniversary}`, `postinstall`): the app
starts without the setup waiting for it (`nowait`). A silent install skips it (`skipifsilent`).

## Uninstalling

Through **Settings ▸ Apps** (the uninstaller that Inno Setup registers): it removes what the setup installed (the
three files and the shortcuts) and leaves the app-data folder, with the people and settings, alone. As built here it
has no `[Code]` section; [011](../../011-start-at-login/spec.md) later added one that also removes Start at Login's
Run value when it starts the installed copy.

## First start

The setup program and the app aren't signed. A browser may say the file isn't commonly downloaded, and Windows may
say "Windows protected your PC"; the user guide's steps are in [spec.md](../spec.md) (User Story 4).
