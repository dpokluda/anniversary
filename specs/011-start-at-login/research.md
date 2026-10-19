# Research: Start at Login

Decisions made while planning and building the feature on 2026-10-10. Sources: the sessions of 2026-10-10 (the first
question, asked with the installers idea in the afternoon; in the evening the request, the questions about the
installation path, the Run key and the menu, the assistant's answers and its implementation report; after the commit,
the maintainer's check), [docs/spec.md](../../docs/spec.md) 7.12, [docs/plan.md](../../docs/plan.md) ("Next" item 3
and "Idea: installers and start at login", a section first added as "requested, not started" by `1409624`, the CI
commit of [005](../005-ci-pipeline/spec.md)), the lessons in [docs/copilot/handover.md](../../docs/copilot/handover.md),
and `git show 6af4c27` with its code comments.

## A setting in the app, not an installer option (user decision)

- **Decision**: **⋯ ▸ Start at Login**, a check item in the app (on macOS also in the menu bar), instead of a choice
  in an installer.
- **Rationale**: It works for every download: the macOS `.dmg`, the AppImage and the archives have no installer, so an
  installer option would only exist on Windows. People can turn it on or off at any time without reinstalling. The app
  registers the file it is running from, so the path is always right. It is per user and needs no admin rights.
- **Alternatives considered**: an installer step. The assistant had recommended the in-app setting when the idea first
  came up, together with the installers ("Start after login works better as an in-app setting … than as an installer
  step"); the maintainer had that added to the plan ("Add it to plan please."), which `1409624` committed as the
  suggestion "Start at login as an in-app setting". The assistant recommended it again when the maintainer asked "you
  think we should add it to the program menu as an option rather than installer option?", and the maintainer answered
  "Let's do that."
- **Source**: session 2026-10-10; docs/spec.md 7.12 and C22; docs/plan.md ("a check item in the app rather than an
  installer option (user decision)").

## Register the copy that is running

- **Decision**: The program registered is the running executable (`Environment.ProcessPath`). Inside an AppImage it is
  the AppImage file (`$APPIMAGE`), used only when the executable runs from `$APPDIR`. A copy whose registration names
  another place reads as off, and turning it on registers the new place.
- **Rationale**: The maintainer pointed out that the app knows its own path ("You know the path of the running app"),
  so Start at Login doesn't depend on an installation path or on the installers. The AppImage file is what the user
  starts, not the program inside it. The `$APPDIR` check is there because "an AppImage app that started this one
  passes its own on" (code comment).
- **Alternatives considered**: tying Start at Login to the installers' installation path, which the maintainer
  questioned; not needed. The maintainer still kept the planned order: the user guide, the packages, Start at Login,
  releases ("Keep the order.").
- **Source**: session 2026-10-10; docs/spec.md 7.12; `StartAtLogin.ProgramPath`.

## Windows: the Run key, not a Startup-folder shortcut

- **Decision**: The string value `Anniversary` under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, holding the
  quoted path of the executable.
- **Rationale**: The two are equal in most ways: both are per user, need no admin rights, start at the same point
  after login and appear in Task Manager ▸ Startup apps. But the Run key is a single registry value that .NET's
  built-in `Registry` API writes, reads and deletes, so the menu can show its state and the uninstaller can remove it
  easily; most apps with a "start at login" setting use it, such as OneDrive, Teams and Spotify. A Run-key start uses
  `C:\Windows\System32` as the working directory; the assistant checked that the app never resolves paths against it.
- **Alternatives considered**: a `.lnk` shortcut in the Startup folder, which people can see and delete themselves.
  .NET has no API for creating shortcuts, so it needs extra Windows-specific code, and it suits an installer checkbox
  better than an in-app setting.
- **Source**: session 2026-10-10 (the maintainer's question "Is one of them preferred than the other?"); docs/spec.md
  7.12; docs/plan.md ("chosen over a Startup-folder shortcut").

## Windows: follow Task Manager's switch

- **Decision**: Read the `Anniversary` binary value under
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run`: an odd first byte means turned off,
  so the item shows unchecked. Turning Start at Login on or off in the app deletes that value.
- **Rationale**: Turning the app off in Task Manager or Settings ▸ Apps ▸ Startup keeps the Run value and records the
  switch in that separate place. "The app should read that so its check mark stays accurate, and clear it when turned
  back on."
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; docs/spec.md 7.12; the test
  `WindowsTaskManagerFlagsAreFollowedAndClearedWhenTurnedOn` (Task Manager writes 3, then the time it was turned off).

## macOS: Apple's login items, not a LaunchAgent

- **Decision**: `SMAppService.mainApp` (`register`, `unregister`, `status`; macOS 13 and later), called through the
  Objective-C runtime with `objc_msgSend` P/Invokes; the ServiceManagement framework is loaded on first use. The item
  is checked when the status is *enabled*.
- **Rationale**: The app is then listed in System Settings ▸ General ▸ Login Items ▸ Open at Login, the list the Dock's
  Options ▸ Open at Login uses; a LaunchAgent would show up under "Allow in the Background" with a notification
  instead. It works with the ad-hoc signed app (checked on macOS 27: register, status, unregister, and the item in the
  real app menu). Calling the runtime directly needs no package.
- **Alternatives considered**: a LaunchAgent plist in `~/Library/LaunchAgents`, the first option in the plan's table
  and the one the handover's next steps named before this feature.
- **Source**: docs/plan.md ("Idea: installers and start at login", as updated by this feature); the handover lesson
  "Start at Login on macOS"; session 2026-10-10 (the implementation report).

## macOS: the `.app` only, and never a temporary copy

- **Decision**: The item exists only on macOS 13 and later and only when the executable runs inside an app bundle (a
  path containing `.app/Contents/MacOS/`). An app running from App Translocation (a path containing
  `/AppTranslocation/`) isn't registered; the reason given is "Move Anniversary to the Applications folder, open it
  from there and try again." If macOS answers *requires approval* after registering, the reason given is "Allow
  Anniversary in System Settings ▸ General ▸ Login Items."
- **Rationale**: An app opened from Downloads or the `.dmg` runs from a temporary read-only copy, "which would be gone
  by the next login" (code comment). macOS even registers a bare executable, but the app doesn't offer it, because a
  bare executable opens Terminal.
- **Alternatives considered**: registering the bare executable from the `.tar.gz`, which macOS accepts; not offered.
- **Source**: docs/spec.md 7.12; the handover lesson "Start at Login on macOS"; `StartAtLogin.IsSupported` and
  `SetMac`.

## macOS: Apple's name, also in the app menu

- **Decision**: On macOS the item is called **Open at Login** and is in the ⋯ menu and in the app menu, after About
  Anniversary and a separator. Both check items show the same state.
- **Rationale**: "Open at Login" is Apple's term (the Login Items list and the Dock use it). macOS apps keep their
  commands in the menu bar; the assistant proposed the item "in the macOS menu bar too".
- **Alternatives considered**: none recorded.
- **Source**: docs/spec.md 7.2 and 7.12; session 2026-10-10.

## Linux: an XDG autostart entry

- **Decision**: `anniversary.desktop` in `$XDG_CONFIG_HOME/autostart` (default `~/.config/autostart`) with
  `Type=Application`, `Name=Anniversary`, a `Comment`, `Exec=` the quoted path and `Terminal=false`. It counts as on
  when it has exactly that `Exec` line and no `Hidden=true` or `X-GNOME-Autostart-enabled=false`; turning it off
  deletes the file.
- **Rationale**: XDG autostart is the Linux entry in the plan's table from the start. Quoting follows the Desktop Entry
  rules (`"`, `` ` ``, `$` and `\` get a backslash, `%` is doubled, then the file format doubles every backslash), so
  any path works. An entry turned off by the desktop has one of the two lines and shows unchecked (test
  `LinuxEntryTurnedOffByTheDesktopIsOff`).
- **Alternatives considered**: none recorded.
- **Source**: docs/spec.md 7.12; docs/plan.md; `StartAtLogin.ExecLine` and `IsOnLinux`.

## Read the state from the OS every time

- **Decision**: The app keeps no copy of the setting. It asks the OS whenever the ⋯ or right-click menu or the macOS
  app menu opens, when the main window is activated, and after each click. The menu check joins the calendar refresh
  already in the menu's `Opening` handler, and the activation check goes into the handler that
  [007](../007-data-file-sharing/spec.md) added to reload a data file changed elsewhere, just before that reload.
- **Rationale**: Windows and macOS list the app in their own settings, where users can turn it off; "the check mark is
  read from the system each time, so turning it off in Task Manager or System Settings shows up in the app".
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10 (the implementation report); docs/spec.md 7.12.

## Platform code in the desktop project, behind a hook

- **Decision**: `Anniversary.Desktop/StartAtLogin.cs` holds all the platform code. `Program.Main` sets
  `App.StartAtLogin = (StartAtLogin.IsOn, StartAtLogin.Set)` where `StartAtLogin.IsSupported`; `null` hides the items.
  `Set` throws `InvalidOperationException` with the reason; `IsOn` returns `false` when the state can't be read.
- **Rationale**: Platform code stays out of `Anniversary.Shared` (spec 7.12), and the plan had foreseen "a little
  platform code in the desktop project and no packages". Left `null` (the bare macOS executable, future mobile apps),
  the items are hidden.
- **Alternatives considered**: none recorded.
- **Source**: docs/spec.md 7.12 and 8; docs/plan.md (the suggestion "Start at login as an in-app setting").

## The uninstaller cleans up; no installer checkbox

- **Decision**: The Inno Setup uninstaller deletes the `Anniversary` Run value and its `StartupApproved` flags if the
  Run value starts the installed copy (`"{app}\Anniversary.exe"`, compared without regard to case). The installer gets
  no Start at Login checkbox. Copies without an uninstaller are turned off by the user before deleting them, as the
  user guide says.
- **Rationale**: "The installer's only part is cleanup: the uninstaller removes the Run value." A setup checkbox could
  write the same value, "but then every upgrade would have to decide whether an unchecked box should undo a choice made
  in the app". Comparing with the installed path leaves an entry for another copy, such as one from a `.zip`, alone.
- **Alternatives considered**: a checkbox in the setup that writes the same value; left out.
- **Source**: session 2026-10-10; docs/spec.md 7.12 and 10; `packaging/windows/Anniversary.iss` (its comment: "it goes
  with the app if it starts this copy").

## Verification approach

- **Decision**: Automated tests for the Linux entry on every OS and for the registry on Windows; manual checks for the
  macOS login item, the menus, the real logins and the uninstaller; a smoke test of the packaged macOS app.
- **Rationale**: The tests use a temporary autostart folder and a scratch registry key; macOS's login items are
  checked by hand (the test class's summary). Headless runs aren't the real menu bar: on macOS, Avalonia appends its
  own items (separators, Services, Hide, Quit) to the app menu at start-up, which the headless platform doesn't.
  Smoke-testing the packaged app caught a launch crash the headless run had missed; it was fixed and rechecked before
  the commit, and in the real packaged app clicking the item turned it on (✓) and off again, leaving nothing behind.
  The lesson recorded for later sessions: System Events (`osascript`) can read and click the real app's menu items and
  read their check marks (`AXMenuItemMarkChar`). CI then ran all 203 tests on Windows, including the three registry
  tests, and built the installer with its uninstall code. Asked to check Windows (installed and from the `.zip`, Task
  Manager's switch, the uninstaller) and Fedora (the AppImage), the maintainer answered "I validated the start after
  login.", which covered Start at Login on all three systems.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; the handover lessons "Headless isn't the real menu bar" and "Start at Login on
  macOS"; docs/plan.md ("Checked by the user on Windows, macOS and Fedora", added by `26063da`).
