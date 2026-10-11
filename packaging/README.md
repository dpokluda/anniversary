# Packages

`Build-Package.ps1` turns a published folder into the package people install (spec section 10). CI runs it for every
RID. To build one yourself, publish first (see the repository README), then run the script on the RID's own OS:

```powershell
dotnet publish Anniversary.Desktop -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/osx-arm64
pwsh packaging/Build-Package.ps1 -Rid osx-arm64
```

The package goes to `packages/` (ignored by Git); `-PublishFolder` and `-OutputFolder` change the folders. The version
and copyright come from `Directory.Build.props`.

| RIDs | Package | Built from | Needs |
|---|---|---|---|
| `win-x64`, `win-arm64` | `Anniversary-{version}-{rid}-setup.exe` | `windows/Anniversary.iss`, `App.ico` | Windows with [Inno Setup](https://jrsoftware.org/isinfo.php) 6.3 or newer (preinstalled on GitHub's Windows runners) |
| `osx-arm64`, `osx-x64` | `Anniversary-{version}-{rid}.dmg` | `macos/Info.plist`, `icon/App.icns` | macOS (`codesign` and `hdiutil` come with it) |
| `linux-x64`, `linux-arm64` | `Anniversary-{version}-{rid}.AppImage` | `linux/AppRun`, `linux/anniversary.desktop`, `App.png` | Linux on x86_64 or aarch64, online: appimagetool and the AppImage runtime are downloaded at pinned versions and checked by SHA-256 |

- **Windows:** the installer is per user, without admin rights, into `%LOCALAPPDATA%\Programs\Anniversary`. Never
  change the `AppId` in `Anniversary.iss`: it's how a newer installer finds the installed app and updates it. The
  uninstaller leaves the app-data folder alone, and removes Start at Login's Run value if it starts the installed copy
  (spec 7.12; its `[Code]` section).
- **macOS:** the `.app` gets an ad-hoc signature (`codesign --sign -`), checked with `codesign --verify --strict`.
  Apple Silicon refuses unsigned code, and a downloaded bundle without a valid signature is reported as damaged instead
  of offering Open Anyway. It isn't notarized, so Gatekeeper still blocks the first start (the user README explains
  Open Anyway).
- **Linux:** `AppRun` must keep LF line endings and its executable flag (`.gitattributes` and Git's file mode keep
  both). The runtime is static, so the AppImage doesn't need libfuse2. To move to newer tools, change the versions and
  hashes at the top of the script.
- **Checking:** try each package on its own OS (spec acceptance criterion 15): install, start and uninstall on Windows;
  drag to Applications and start on macOS; make the AppImage executable and start it on Fedora.
