# Contract: `Build-Package.ps1 -Rid`

The command that turns one published folder into its package, as built on 2026-10-10
(`packaging/Build-Package.ps1`; unchanged since). The packages it writes: [windows-installer.md](windows-installer.md),
[macos-dmg.md](macos-dmg.md) and [linux-appimage.md](linux-appimage.md). Behaviour: [spec.md](../spec.md) (User
Story 5); system spec: [docs/spec.md](../../../docs/spec.md) §10; developer notes:
[packaging/README.md](../../../packaging/README.md).

## Synopsis

```powershell
pwsh packaging/Build-Package.ps1 -Rid <rid> [-PublishFolder <folder>] [-OutputFolder <folder>]
```

| Parameter | Required | Values | Default |
|---|---|---|---|
| `-Rid` | yes | `win-x64`, `win-arm64`, `osx-arm64`, `osx-x64`, `linux-x64`, `linux-arm64` (any other value is rejected) | – |
| `-PublishFolder` | no | the published app: the executable, `README.md` and `THIRD-PARTY-NOTICES.md` | `publish/<rid>` under the repository root |
| `-OutputFolder` | no | where the package goes | `packages` under the repository root (ignored by Git) |

Both folders are made absolute before use. The script needs PowerShell 7.4 or newer (`#Requires -Version 7.4`) and
stops at the first error (`$ErrorActionPreference = 'Stop'`), including a failing native tool
(`$PSNativeCommandUseErrorActionPreference = $true`).

## Steps

1. Check the OS: `win-*` RIDs only on Windows, `osx-*` only on macOS, `linux-*` only on Linux.
2. Check the published files: `Anniversary.exe` (Windows) or `Anniversary` (macOS, Linux), `README.md` and
   `THIRD-PARTY-NOTICES.md` in the publish folder.
3. Read the version and copyright from the first `PropertyGroup` in `Directory.Build.props` that has a `Version`
   (then `5.0.0` and `Copyright © 1997-2026 David Pokluda`).
4. Make a fresh work folder, `anniversary-package-<rid>` in the system's temp folder, and the output folder.
5. Build the package for the platform (`New-WindowsPackage`, `New-MacPackage` or `New-LinuxPackage`).
6. Check that the package file exists, delete the work folder, and print `Package: {full path}`. When
   `GITHUB_OUTPUT` is set (GitHub Actions), also write `path={full path}` to it.

## Inputs per platform

| Platform | Templates | Icon | Tools |
|---|---|---|---|
| Windows | `packaging/windows/Anniversary.iss` | `Anniversary.Shared/Assets/App.ico` (named in the `.iss`) | Inno Setup 6.3 or newer: `ISCC.exe` from the `PATH`, else from `Inno Setup 6` under Program Files (x86), Program Files or `%LOCALAPPDATA%\Programs` |
| macOS | `packaging/macos/Info.plist` | `icon/App.icns` | `plutil`, `xattr`, `codesign`, `hdiutil` (part of macOS) |
| Linux | `packaging/linux/AppRun`, `packaging/linux/anniversary.desktop` | `Anniversary.Shared/Assets/App.png` | an x86_64 or aarch64 host with internet access; appimagetool and the AppImage runtime are downloaded at pinned versions ([linux-appimage.md](linux-appimage.md#build)) |

## Output

| RIDs | Package (in the output folder) |
|---|---|
| `win-x64`, `win-arm64` | `Anniversary-{version}-{rid}-setup.exe` |
| `osx-arm64`, `osx-x64` | `Anniversary-{version}-{rid}.dmg` |
| `linux-x64`, `linux-arm64` | `Anniversary-{version}-{rid}.AppImage` |

## Errors

The script stops with an error, and a non-zero exit code, in these cases:

| Situation | Message |
|---|---|
| Not on the RID's own OS | `Build the {rid} package on its own OS (see README.md in this folder).` |
| A published file is missing | `{file} is missing; publish {rid} to {publish folder} first.` |
| Inno Setup isn't found (Windows) | `Inno Setup 6 (ISCC.exe) was not found. Install it from https://jrsoftware.org/isinfo.php.` |
| A download's hash differs (Linux) | `The SHA-256 of {url} doesn't match the pinned one.` |
| No package file at the end | `The package wasn't created: {path}` |
| A tool fails (`ISCC.exe`, `plutil`, `codesign`, appimagetool, …) | the tool's own error |

The first two checks run before anything is written. `hdiutil create` is the one exception to stopping at once: it
sometimes fails with "Resource busy" on build machines, so it is tried up to 3 times, 5 seconds apart. After a failure
the work folder is left in the temp folder; the next run deletes it first.

## Use in CI

`.github/workflows/ci.yml`, job **publish** (one job per RID, on `windows-latest`, `macos-latest` or `ubuntu-latest`),
after **Publish** and the archive's upload:

```yaml
# The installer, .dmg or AppImage, built from the published folder on the RID's own OS.
- name: Package
  id: package
  shell: pwsh
  run: ./packaging/Build-Package.ps1 -Rid ${{ matrix.rid }}
- uses: actions/upload-artifact@v7
  with:
    path: ${{ steps.package.outputs.path }}
    archive: false
    retention-days: 30
    if-no-files-found: error
```

- The package is uploaded as is (`archive: false`), so the run offers it under its own name next to the archive,
  `Anniversary-{rid}.zip` or `Anniversary-{rid}.tar.gz`: twelve downloads per run, kept for 30 days.
- A missing package fails the job (`if-no-files-found: error`), as does any error of the script.
- With this step, the two archive steps were renamed from **Package (zip)** and **Package (tar.gz)** to **Archive
  (zip)** and **Archive (tar.gz)**, and the workflow's header comment now mentions the packages.
