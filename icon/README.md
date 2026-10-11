# App icon

The Anniversary icon: a refreshed version of the v4 tear-off calendar with the yellow binding, red "24", blue lines
and stacked pages. It's drawn as vector shapes by `Build-Icon.ps1`, so every size is sharp. At small sizes the details
that would turn into noise are dropped: from 40 px down the binding is shallower and the "24" is centred with white space around it, and at 16 px it is drawn as crisp pixels (a 3 × 5 pixel font).

On macOS the calendar sits on a white rounded square (`New-MacIcon`): macOS 26 and later put any app icon that isn't a
rounded square on a grey tile, which washed the calendar out. The square follows Apple's grid (824 px with 185 px
corners, 100 px in from each edge of 1024), and the calendar takes 82% of the icon, drawn at its own size so small
icons keep their simpler drawing. Windows and Linux keep the plain calendar.

```powershell
pwsh -File Build-Icon.ps1                           # writes the files below
pwsh -File Build-Icon.ps1 -PreviewFolder preview    # also writes PNG previews of each size (App-mac-*.png for macOS)
```

Requires Windows (it uses System.Drawing). Edit the drawing code in the script and re-run; don't edit the outputs.

| File | Sizes | Used for |
|---|---|---|
| `../Anniversary.Shared/Assets/App.ico` | 16, 20, 24, 32, 40, 48, 64, 256 px, 32-bit colour | exe icon (Windows), window icons, Windows installer |
| `../Anniversary.Shared/Assets/App.png` | 256 px | About dialog, Linux AppImage |
| `App.icns` | 16–1024 px, including Retina @2x | macOS `.app` bundle (`packaging/Build-Package.ps1`): the calendar on a white rounded square |
| `App-1024.png` | 1024 px | master image: Linux desktop entries, store listings |


