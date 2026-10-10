# App icon

The Anniversary icon: a refreshed version of the v4 tear-off calendar with the yellow binding, red "24", blue lines
and stacked pages. It's drawn as vector shapes by `Build-Icon.ps1`, so every size is sharp. At small sizes the details
that would turn into noise are dropped: from 40 px down the binding is shallower and the "24" is centred with white space around it, and at 16 px it is drawn as crisp pixels (a 3 × 5 pixel font).

```powershell
pwsh -File Build-Icon.ps1                           # writes the files below
pwsh -File Build-Icon.ps1 -PreviewFolder preview    # also writes PNG previews of each size
```

Requires Windows (it uses System.Drawing). Edit the drawing code in the script and re-run; don't edit the outputs.

| File | Sizes | Used for |
|---|---|---|
| `../Anniversary.Shared/Assets/App.ico` | 16, 20, 24, 32, 40, 48, 64, 256 px, 32-bit colour | exe icon (Windows), window icons |
| `../Anniversary.Shared/Assets/App.png` | 256 px | About dialog |
| `App.icns` | 16–1024 px, including Retina @2x | macOS `.app` bundle (when one is added) |
| `App-1024.png` | 1024 px | master image: Linux desktop entries, store listings |


