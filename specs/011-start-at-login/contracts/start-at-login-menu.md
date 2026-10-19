# Contract: the Start at Login menu items

The user interface this feature added, as built on 2026-10-10 (`Anniversary.Shared/Views/MainWindow.axaml`,
`MainWindow.axaml.cs` and `Anniversary.Shared/App.axaml`). Behaviour: [spec.md](../spec.md); states:
[data-model.md](../data-model.md); system spec: [docs/spec.md](../../../docs/spec.md) 7.2 and 7.12.

## Items

| Menu | Item | Kind | Shortcut | Where |
|---|---|---|---|---|
| ⋯ button and right-click menu | **Start at Login** (macOS: **Open at Login**) | check item (`ToggleType="CheckBox"`, `StartAtLoginItem`) | none | after Theme ▸, before the separator above About Anniversary… |
| macOS app menu (**Anniversary**) | **Open at Login** | check item (`NativeMenuItem`, `ToggleType="CheckBox"`) | none | after About Anniversary and a separator; macOS adds its own items (Services, Hide, Quit) after it |

The ⋯ menu then reads: Today, Change Date…, ─, Find…, Find Next, ─, Edit People…, Data File ▸, ─, Calendar ▸, Theme ▸,
**Start at Login**, ─, About Anniversary…. Windows and Linux ignore the macOS app menu.

## Visibility

Both items, and the app menu's separator before Open at Login, are shown only when `App.StartAtLogin` isn't `null`
([start-at-login-hook.md](start-at-login-hook.md)): always on Windows and Linux; on macOS 13 and later for the `.app`.
The separator is found as the item right before Open at Login, because macOS appends its own items after it.

## Check mark

- Checked when `App.StartAtLogin.IsOn()` is `true`: this copy starts at login and the OS's settings haven't turned it
  off. Both items always show the same state.
- Read from the OS, never remembered, at these moments: when the ⋯ or right-click menu opens (`OnCommandsMenuOpening`,
  which also refreshes the calendars), when the macOS app menu opens, when the main window is activated
  (`OnActivated`, the handler [007](../../007-data-file-sharing/spec.md) added for the data file; the check runs before
  its reload), and after every click.

## Click

1. `Set(!IsOn())`: turns it on when it's off (including when another copy is registered or the OS's settings turned
   it off) and off when it's on.
2. On `InvalidOperationException`, show the [message](#messages).
3. Read the state again and update both check marks, so they show the real state even after a failure.

## Messages

One message box (the app's message dialog with **OK**), titled with the item's name: **Start at Login**, on macOS
**Open at Login**.

| When | Text (Windows and Linux / macOS) |
|---|---|
| Any failure | `Could not change Start at Login: {reason}` / `Could not change Open at Login: {reason}` |
| macOS, the app runs from a temporary copy (Downloads, the `.dmg`) | `Could not change Open at Login: Move Anniversary to the Applications folder, open it from there and try again.` |
| macOS asks for approval after registering | `Could not change Open at Login: Allow Anniversary in System Settings ▸ General ▸ Login Items.` |
| macOS refuses to register or unregister | `Could not change Open at Login: {macOS's localized error description}` |
| Windows or Linux can't write or delete the entry | `Could not change Start at Login: {the error's message}` |

When the state can't be read, the items show unchecked and no message appears.

## User guide texts (`Anniversary.Desktop/README.md`)

- **Using it**: the command list ends "… Theme, Start at Login and About", and: "**Start at Login** starts the app
  whenever you log in; choose it again to stop that. On macOS it's called **Open at Login** (also in the
  **Anniversary** menu) and works for the app in Applications. Windows (Task Manager ▸ Startup apps) and macOS (System
  Settings ▸ General ▸ Login Items) list it too, and turning it off there works as well."
- **Removing it** starts: "If you turned on **Start at Login**, turn it off first (the Windows uninstaller does that for
  you)."
