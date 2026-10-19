# Contract: the macOS disk image and app

`Anniversary-{version}-{rid}.dmg` for `osx-arm64` and `osx-x64`, holding `Anniversary.app`, as built on 2026-10-10 by
`New-MacPackage` in `packaging/Build-Package.ps1` from `packaging/macos/Info.plist` and `icon/App.icns`. Behaviour:
[spec.md](../spec.md) (User Stories 2 and 6); the command that builds it: [build-package.md](build-package.md); system
spec: [docs/spec.md](../../../docs/spec.md) §10, C17 and §12, criterion 15.

## The disk image

Created with `hdiutil create -volname 'Anniversary' -srcfolder <folder> -ov -format UDZO` (compressed, replacing an
existing file of the same name; tried up to 3 times, 5 seconds apart):

```text
Anniversary                     # the volume
├── Anniversary.app             # the app, below
├── Applications                # a link to /Applications to drag the app onto
├── README.md                   # the user guide
└── THIRD-PARTY-NOTICES.md
```

## The app bundle

```text
Anniversary.app/
└── Contents/
    ├── Info.plist              # from packaging/macos/Info.plist, filled in
    ├── MacOS/
    │   └── Anniversary         # the published single-file executable
    └── Resources/
        ├── App.icns            # icon/App.icns
        ├── README.md
        └── THIRD-PARTY-NOTICES.md
```

Inside the app, the folder next to the executable is read-only and holds no data files: users keep theirs in the
app-data folder (`~/Library/Application Support/Anniversary`), as with any other copy of the app.

## `Info.plist`

The template's `{Version}` becomes the `Version` from `Directory.Build.props`, and `{Copyright}` its `Copyright`,
XML-escaped. `plutil -lint` checks the result.

| Key | Value |
|---|---|
| `CFBundleIdentifier` | `cz.pokluda.anniversary` |
| `CFBundleName`, `CFBundleDisplayName`, `CFBundleExecutable` | `Anniversary` |
| `CFBundleIconFile` | `App.icns` |
| `CFBundleShortVersionString`, `CFBundleVersion` | `{Version}` (5.0.0) |
| `NSHumanReadableCopyright` | `{Copyright}` (`Copyright © 1997-2026 David Pokluda`) |
| `CFBundlePackageType` | `APPL` |
| `CFBundleDevelopmentRegion`, `CFBundleInfoDictionaryVersion` | `en`, `6.0` |
| `LSApplicationCategoryType` | `public.app-category.productivity` |
| `LSMinimumSystemVersion` | `12.0`, the minimum macOS that the published executable declares |
| `NSHighResolutionCapable` | `true` |

## Signature

After the bundle is assembled:

1. `xattr -cr Anniversary.app` clears extended attributes.
2. `codesign --force --sign - Anniversary.app` signs the whole bundle ad hoc (no certificate).
3. `codesign --verify --strict Anniversary.app` checks the signature; a failure stops the build.

Without a signature, Apple Silicon won't run the app, and a downloaded bundle is reported as damaged instead of
offering Open Anyway. The app isn't notarized, so a downloaded copy is still blocked at its first start
("Anniversary.app" Not Opened) until the user allows it once in **System Settings ▸ Privacy & Security**
([spec.md](../spec.md), User Story 4).

## The icon (`App.icns`)

Written by `Write-Icns` in `icon/Build-Icon.ps1` (Windows-only); every entry is a PNG drawn by `New-MacIcon`:

| Entry | Size (px) | Used as |
|---|---|---|
| `ic07`, `ic08`, `ic09` | 128, 256, 512 | 128, 256 and 512 pt |
| `ic10` | 1024 | 512 pt @2x |
| `ic11`, `ic12`, `ic13`, `ic14` | 32, 64, 256, 512 | 16, 32, 128 and 256 pt @2x |

- **No 1x 16, 32 or 64 px entries** (`icp4`, `icp5`, `icp6`): macOS read the 16 and 32 px PNGs as coloured noise on
  non-Retina screens, and Apple's own ARGB format there (`ic04`, `ic05`) put them on a grey tile; without them macOS
  scales the larger images down.
- **Drawing**: a white rounded square on Apple's icon grid, 824 px with 185 px corners and 100 px in from each edge of
  1024, filled with a gradient from white to `#EEF0F4` and with a faint shadow below it; on it the calendar, drawn at
  its own size so small images keep its simpler drawing, at 82% of the icon (an even number of pixels), with row 132.5
  of its 256 grid at the icon's middle.
- `App.ico` and `App.png` (Windows, Linux, the About dialog) keep the plain calendar.

## Installing and removing

Open the `.dmg`, drag Anniversary onto Applications, and start it from there; it shows as "Anniversary" with its icon
in the Dock and the menu bar, without a Terminal window. To remove it, drag it from Applications to the Trash; the
app-data folder stays.
