# Data Model: Start at Login

The state as built on 2026-10-10 (commit `6af4c27`), with the real type and member names. The app stores none of it:
each OS keeps its own per-user registration, which the app reads each time it shows the item and writes only when the
user clicks it. The exact formats are in [contracts/os-registration.md](contracts/os-registration.md); the
user-visible behaviour is in [spec.md](spec.md).

## Entities

### This copy (`StartAtLogin.ProgramPath`, `Anniversary.Desktop/StartAtLogin.cs`)

| Situation | `ProgramPath` |
|---|---|
| Normal start (installed, from an archive, the macOS `.app`) | `Environment.ProcessPath` (empty if unknown) |
| Inside an AppImage: `$APPIMAGE` and `$APPDIR` are set and the executable's path starts with `$APPDIR` + `/` | `$APPIMAGE`, the AppImage file |
| `$APPIMAGE` set by an AppImage app that started this one, but the executable isn't under `$APPDIR` | `Environment.ProcessPath` |

### Support (`StartAtLogin.IsSupported`) and the hook (`App.StartAtLogin`)

| OS | `IsSupported` | `App.StartAtLogin` | Items |
|---|---|---|---|
| Windows | always | `(StartAtLogin.IsOn, StartAtLogin.Set)` | shown |
| Linux | always | `(StartAtLogin.IsOn, StartAtLogin.Set)` | shown |
| macOS 13 or later, `ProgramPath` contains `.app/Contents/MacOS/` | yes | `(StartAtLogin.IsOn, StartAtLogin.Set)` | shown (Open at Login) |
| macOS 12, or the bare executable (from the `.tar.gz`, `dotnet run`) | no | `null` | hidden, with the app menu's separator |

`Program.Main` sets the hook before Avalonia starts; `MainWindow` reads it once for visibility and on every update.

### Startup entry per OS

| OS | Where | Content | Written by |
|---|---|---|---|
| Windows | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, value `Anniversary` (string) | `"{ProgramPath}"` | the app (`SetWindows`) |
| Windows | `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run`, value `Anniversary` (binary) | Task Manager's or Settings' switch: an odd first byte means off | Windows; the app and the uninstaller only delete it |
| macOS 13+ | the login item of the running app bundle (`SMAppService.mainApp`) | status: 0 not registered, 1 enabled, 2 requires approval, 3 not found | the app (`register` / `unregister`); the user in System Settings ▸ General ▸ Login Items |
| Linux | `$XDG_CONFIG_HOME/autostart/anniversary.desktop` (default `~/.config/autostart`) | six lines, with `Exec="{ProgramPath}"` quoted per the Desktop Entry rules | the app (`SetLinux`); the desktop may add `Hidden=true` or `X-GNOME-Autostart-enabled=false` |

`usersettings.json`, the data file and the app-data folder are not involved.

## Rules

- **On** (`StartAtLogin.IsOn()`, shown as the check mark):
  - Windows: the Run value equals `"{ProgramPath}"`, ignoring case, and the `StartupApproved` value is missing, empty,
    not binary, or has an even first byte.
  - macOS: the status is 1 (*enabled*).
  - Linux: the file exists, has a line equal to `ExecLine(ProgramPath)`, and has no line `Hidden=true` or
    `X-GNOME-Autostart-enabled=false`.
- **Unreadable**: an `IOException`, `UnauthorizedAccessException` or `SecurityException` while reading gives `false`.
- **Turning on** (`Set(true)`): Windows deletes the `StartupApproved` value and writes the Run value (creating the
  key); macOS refuses an App Translocation path, registers, and fails with the approval message if the status is then
  *requires approval*; Linux creates the folder and writes the file, replacing any earlier one.
- **Turning off** (`Set(false)`): Windows deletes the `StartupApproved` value and the Run value; macOS unregisters;
  Linux deletes the file if it exists. A missing value or file isn't an error.
- **Failures**: `Set` throws `InvalidOperationException` with a message for the user (the macOS reasons, the error
  macOS returns, or the message of an I/O, access or security error).
- **One entry per user**: on Windows and Linux the value and file names are fixed, so the entry names one copy; turning
  it on from another copy replaces it.

## State transitions

What the item shows for the running copy:

| State | Windows | macOS | Linux | Item |
|---|---|---|---|---|
| **Off** | no Run value | status 0 or 3 | no file | unchecked |
| **On** | Run value for this copy, not switched off | status 1 | file with this copy's `Exec` line, not turned off | checked |
| **Another copy** | Run value with another path | (not distinguished) | file with another `Exec` line | unchecked |
| **Switched off in the OS** | Run value kept, odd first byte in `StartupApproved` | any status other than 1 | `Hidden=true` or `X-GNOME-Autostart-enabled=false` | unchecked |
| **Waiting for approval** | | status 2 | | unchecked |
| **Unreadable** | read error | read error | read error | unchecked |

| From | Event | To | Also |
|---|---|---|---|
| Off, Another copy or Switched off | the user clicks the item | On | Windows also clears Task Manager's switch; Linux rewrites the file |
| On | the user clicks the item | Off | |
| any (macOS, App Translocation path) | the user clicks the item while it's off | unchanged | message: "Move Anniversary to the Applications folder, open it from there and try again." |
| Off (macOS) | the user clicks the item and macOS wants approval | Waiting for approval | message: "Allow Anniversary in System Settings ▸ General ▸ Login Items." |
| any | the change fails | unchanged | message: "Could not change {item}: {reason}" |
| On | the user turns it off in Task Manager, Settings ▸ Apps ▸ Startup, Login Items or the desktop's settings | Switched off (or Off) | shown the next time a menu opens or the window is activated |
| On (Windows, the installed copy) | the Windows uninstaller runs | Off | the uninstaller deletes the Run value and the `StartupApproved` value |
| On | the copy is moved or deleted | the entry still names the old place | a moved copy shows the item unchecked |

The item's check mark is refreshed by `UpdateStartAtLoginChecks()` when the ⋯ or right-click menu opens, when the macOS
app menu opens, when the main window is activated, and after every click.
