# Contract: the Linux AppImage

`Anniversary-{version}-{rid}.AppImage` for `linux-x64` and `linux-arm64`, as built on 2026-10-10 by `New-LinuxPackage`
in `packaging/Build-Package.ps1` from `packaging/linux/AppRun`, `packaging/linux/anniversary.desktop` and
`Anniversary.Shared/Assets/App.png`. Behaviour: [spec.md](../spec.md) (User Story 3); the command that builds it:
[build-package.md](build-package.md); system spec: [docs/spec.md](../../../docs/spec.md) §10 and §12, criterion 15.

## Contents (the AppDir)

```text
Anniversary.AppDir/
├── AppRun                      # packaging/linux/AppRun (executable)
├── anniversary.desktop         # packaging/linux/anniversary.desktop
├── anniversary.png             # Anniversary.Shared/Assets/App.png, 256 px
└── usr/
    ├── bin/
    │   └── Anniversary         # the published single-file executable (executable)
    └── share/
        └── doc/
            └── anniversary/
                ├── README.md   # the user guide
                └── THIRD-PARTY-NOTICES.md
```

Inside the AppImage the folder next to the executable is read-only and holds no data files: users keep theirs in the
app-data folder (`~/.config/Anniversary`), as with any other copy of the app.

## `AppRun`

```sh
#!/bin/sh
# Starts Anniversary inside the AppImage (spec 10).
here="$(dirname "$(readlink -f "$0")")"
exec "$here/usr/bin/Anniversary" "$@"
```

It must keep LF line endings and its executable flag: `.gitattributes` has `packaging/linux/* text eol=lf`, Git keeps
the file mode (`100755`), and the script also runs `chmod +x` on `AppRun` and `usr/bin/Anniversary` in the AppDir.

## Desktop entry (`anniversary.desktop`)

| Key | Value |
|---|---|
| `Type` | `Application` |
| `Name` | `Anniversary` |
| `Comment` | `Name days, birthdays and anniversaries for any day` |
| `Exec` | `Anniversary` |
| `Icon` | `anniversary` (the `anniversary.png` next to it) |
| `Terminal` | `false` |
| `Categories` | `Office;Calendar;` |
| `StartupWMClass` | `Anniversary` |

## Build

| Tool | Version | Downloaded from | SHA-256 |
|---|---|---|---|
| appimagetool, for the build machine (`x86_64` or `aarch64`) | 1.9.1 | `https://github.com/AppImage/appimagetool/releases/download/1.9.1/appimagetool-{arch}.AppImage` | x86_64 `ed4ce84f0d9caff66f50bcca6ff6f35aae54ce8135408b3fa33abfc3cb384eb0`; aarch64 `f0837e7448a0c1e4e650a93bb3e85802546e60654ef287576f46c71c126a9158` |
| AppImage type2 runtime, for the target (`x86_64` for `linux-x64`, `aarch64` for `linux-arm64`) | 20251108 | `https://github.com/AppImage/type2-runtime/releases/download/20251108/runtime-{arch}` | x86_64 `2fca8b443c92510f1483a883f60061ad09b46b978b2631c807cd873a47ec260d`; aarch64 `00cbdfcf917cc6c0ff6d3347d59e0ca1f7f45a6df1a428a0d6d8a78664d87444` |

- Each download is checked against its hash before use; a mismatch stops the build with `The SHA-256 of {url} doesn't
  match the pinned one.` To move to newer tools, change the versions and hashes at the top of the script.
- appimagetool runs as
  `appimagetool --no-appstream --runtime-file <runtime> Anniversary.AppDir Anniversary-{version}-{rid}.AppImage`, with
  `APPIMAGE_EXTRACT_AND_RUN=1` (it extracts itself first, so the build machine needs no FUSE) and `ARCH` set to the
  target architecture.
- The runtime becomes the start of the AppImage, so it is the target's; appimagetool is the build machine's. CI builds
  both AppImages on `ubuntu-latest`.

## Running and removing

- Make the file executable (in its file properties, e.g. **Properties ▸ Executable as Program**, or
  `chmod +x Anniversary-*.AppImage`), then double-click it. The runtime is static, so the AppImage doesn't need
  libfuse2; the app needs fontconfig, libX11, libICE and libSM, which desktop distributions already have. Nothing asks
  at the first start.
- Tools such as Gear Lever add it to the applications menu (user guide).
- To remove it, delete the file; the app-data folder stays.
