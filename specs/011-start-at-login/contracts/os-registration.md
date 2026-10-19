# Contract: OS registration formats

What the app writes, reads and deletes in each OS to start at login, and what the Windows uninstaller removes, as built
on 2026-10-10 (`Anniversary.Desktop/StartAtLogin.cs`, `packaging/windows/Anniversary.iss`). States:
[data-model.md](../data-model.md); system spec: [docs/spec.md](../../../docs/spec.md) 7.12 and 10.

All entries are per user and need no admin rights. The program they start is `ProgramPath`: the running executable,
or inside an AppImage the AppImage file ([data-model.md](../data-model.md)).

## Windows

### Run value

| Item | Value |
|---|---|
| Key | `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run` |
| Value name | `Anniversary` |
| Type | string (`REG_SZ`) |
| Data | the executable's full path in double quotes (for the installed app, `Anniversary.exe` in `%LOCALAPPDATA%\Programs\Anniversary`) |

- **Turn on**: create the key if needed and write the value, replacing one written for another copy.
- **Turn off**: delete the value; a missing key or value is fine.
- **Read**: the value counts only if it equals the quoted `ProgramPath`, ignoring case.

### Task Manager's switch

| Item | Value |
|---|---|
| Key | `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run` |
| Value name | `Anniversary` |
| Type | binary (`REG_BINARY`), written by Task Manager or Settings ▸ Apps ▸ Startup |
| Meaning | an odd first byte means turned off; anything else (or no value) means on |

Task Manager keeps the Run value when it turns the app off and writes this value instead; the test uses
`03 00 00 00` followed by the 8-byte time it was turned off for "off", and `02` followed by eleven zero bytes for "on".
The app deletes this value whenever it turns Start at Login on or off, so turning it on in the app also clears an off
switch.

### Uninstaller (`packaging/windows/Anniversary.iss`, `[Code]`)

```pascal
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
```

At the `usUninstall` step: if `HKCU\…\Run` has `Anniversary` and it equals `"{app}\Anniversary.exe"` (the installed
copy, quoted; `CompareText`, so case doesn't matter), delete `Anniversary` under both the Run key and the
`StartupApproved\Run` key. An entry for another copy, for example one run from a `.zip`, is left alone. The app-data
folder is never touched.

## macOS (13 and later, the `.app` only)

- **API**: Apple's login-item API, `SMAppService.mainApp` from the ServiceManagement framework
  (`/System/Library/Frameworks/ServiceManagement.framework/ServiceManagement`, loaded on first use), called through the
  Objective-C runtime (`/usr/lib/libobjc.A.dylib`): `objc_getClass("SMAppService")`, then the messages
  `mainAppService`, `status`, `registerAndReturnError:` and `unregisterAndReturnError:`.
- **Status**: 0 not registered, 1 enabled, 2 requires approval, 3 not found. On means 1.
- **Turn on**:
  1. If `ProgramPath` contains `/AppTranslocation/`, stop with the reason `Move Anniversary to the Applications
     folder, open it from there and try again.`
  2. `registerAndReturnError:`; if it returns NO, stop with the error's `localizedDescription`.
  3. If the status is now 2, stop with the reason `Allow Anniversary in System Settings ▸ General ▸ Login Items.`
- **Turn off**: `unregisterAndReturnError:`; if it returns NO, stop with the error's `localizedDescription`.
- **Where users see it**: System Settings ▸ General ▸ Login Items ▸ Open at Login, the list the Dock's *Options ▸ Open
  at Login* uses.
- **Offered only** when `ProgramPath` contains `.app/Contents/MacOS/` and the OS is macOS 13 or later
  (`StartAtLogin.IsSupported`).

## Linux

### File

`anniversary.desktop` in the autostart folder: `Environment.SpecialFolder.ApplicationData` + `/autostart`, which .NET
resolves to `$XDG_CONFIG_HOME/autostart`, or `~/.config/autostart` when it isn't set.

```ini
[Desktop Entry]
Type=Application
Name=Anniversary
Comment=Name days, birthdays and anniversaries for any day
Exec="/home/david/Apps/Anniversary.AppImage"
Terminal=false
```

- **Turn on**: create the folder if needed and write these six lines, replacing any earlier file.
- **Turn off**: delete the file if it exists; a missing file or folder is fine.
- **Read**: on when the file exists, has a line exactly equal to this copy's `Exec` line, and has no line
  `Hidden=true` or `X-GNOME-Autostart-enabled=false` (an entry turned off by the desktop).

### `Exec` quoting (`StartAtLogin.ExecLine`)

The path goes in double quotes. Inside them, `"`, `` ` ``, `$` and `\` get a backslash and `%` is doubled, as the
Desktop Entry specification requires; then the file format doubles every backslash. The tests check:

```text
/home/david/Apps/Anniversary.AppImage   →  Exec="/home/david/Apps/Anniversary.AppImage"
/home/david/My Apps/Anniversary         →  Exec="/home/david/My Apps/Anniversary"
/tmp/a"b`c$d                            →  Exec="/tmp/a\\"b\\`c\\$d"
/tmp/back\slash                         →  Exec="/tmp/back\\\\slash"
/tmp/100%                               →  Exec="/tmp/100%%"
```

## Errors (all systems)

Reading treats an `IOException`, `UnauthorizedAccessException` or `SecurityException` as off. Changing it fails with
`InvalidOperationException` carrying the reason above or the message of such an error; the window shows it
([start-at-login-menu.md](start-at-login-menu.md#messages)).
