# Quickstart: Start at Login

How to check the feature as built on 2026-10-10. Expected behaviour: [spec.md](spec.md); the items and messages:
[contracts/start-at-login-menu.md](contracts/start-at-login-menu.md); what each OS stores:
[contracts/os-registration.md](contracts/os-registration.md); states: [data-model.md](data-model.md).

## Prerequisites

- The .NET 10 SDK and a clone of the repository.
- The builds as users get them, from a CI run or built locally with `packaging/Build-Package.ps1` (see
  `packaging/README.md`): on Windows the installer and the `.zip`, on macOS (13 or later) the `.dmg`, on Fedora the
  AppImage. On macOS, `dotnet run` and the `.tar.gz` give a bare executable, for which the item is hidden.
- A user account you can log out of and back into.

## Build, test and run

```bash
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

Expected at the time: a build without warnings and 203 tests, 14 of them in `Anniversary.Tests/StartAtLoginTests.cs`
(the list is in [tasks.md](tasks.md)). On Windows all 203 pass; on macOS and Linux 200 pass and the 3 registry tests
are skipped ("The registry exists only on Windows.").

## Scenario 1: start at login (User Story 1; docs/spec.md §12, criterion 16)

1. **Windows**, installed with the setup and again from the `.zip`: choose **⋯ ▸ Start at Login**. **Expected**: the
   item is checked; Task Manager ▸ Startup apps lists Anniversary; `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
   has the value `Anniversary` with the quoted path of the copy you started. Sign out and in. **Expected**: the app
   starts.
2. **macOS**, the app dragged from the `.dmg` to Applications and opened from there: choose **⋯ ▸ Open at Login** (or
   **Anniversary ▸ Open at Login**). **Expected**: both items are checked, and System Settings ▸ General ▸ Login Items
   lists Anniversary under Open at Login. Log out and in. **Expected**: the app opens.
3. **Fedora**, the AppImage made executable: choose **⋯ ▸ Start at Login**. **Expected**:
   `~/.config/autostart/anniversary.desktop` exists, with `Exec=` and the AppImage file's path in quotes. Log out and
   in. **Expected**: the app starts.
4. On each system, choose the item again. **Expected**: it is unchecked, the value, login item or file is gone, and the
   app doesn't start at the next login.

## Scenario 2: changes made in the OS (User Story 2; criterion 16)

1. **Windows**: turn it on in the app, then turn Anniversary off in Task Manager ▸ Startup apps. Open the ⋯ menu.
   **Expected**: Start at Login is unchecked, and the Run value is still there. Choose it. **Expected**: checked
   again; the `Anniversary` value under `…\Explorer\StartupApproved\Run` is gone, and the app starts at the next
   sign-in.
2. **macOS**: turn it on, then turn it off in System Settings ▸ General ▸ Login Items. Click into the app's window, or
   open either menu. **Expected**: Open at Login is unchecked in both menus.
3. **Fedora**: turn it on, then add the line `Hidden=true` to the autostart file. **Expected**: the item is unchecked
   the next time the menu opens.
4. Turn it on in one copy, then start a copy from another folder (for example a second `.zip` folder on Windows).
   **Expected**: the second copy shows the item unchecked; choosing it there makes the entry point to that copy, and
   the first copy then shows it unchecked.

## Scenario 3: where it can't be turned on (User Story 3; criterion 16)

1. **macOS**: open the app straight from the mounted `.dmg` (or from Downloads) and choose **Open at Login**.
   **Expected**: a message titled **Open at Login**: "Could not change Open at Login: Move Anniversary to the
   Applications folder, open it from there and try again."; the item stays unchecked and Login Items doesn't list the
   app.
2. **macOS**: run the bare executable from the `.tar.gz` (or `dotnet run`). **Expected**: no Open at Login in the ⋯
   menu, and the Anniversary menu shows About Anniversary with no separator or Open at Login after it.
3. If macOS asks for approval after you turn it on, **Expected**: "Could not change Open at Login: Allow Anniversary in
   System Settings ▸ General ▸ Login Items."

## Scenario 4: uninstalling on Windows (User Story 4; criterion 16)

1. Turn Start at Login on in the installed app, quit the app, and uninstall it in **Settings ▸ Apps**. **Expected**:
   the `Anniversary` value is gone from the Run key (and from `…\Explorer\StartupApproved\Run` if Task Manager had
   written one), and `%APPDATA%\Anniversary` is still there.
2. Turn it on in a copy from the `.zip` instead, then uninstall the installed app. **Expected**: the `.zip` copy's
   value stays.
3. Read "Removing it" in the user guide (`README.md` next to the executable). **Expected**: it says to turn Start at
   Login off first and that the Windows uninstaller does that itself.

## Platform checks

- **macOS**: check the packaged app, not only a headless run: Avalonia adds its own items to the real app menu at
  start-up, which the headless platform doesn't, and a launch crash showed up only there (fixed before the commit).
  At the time the assistant checked the ad-hoc signed app on macOS 27: register, status, unregister, and the item in
  the real app menu turning on (✓) and off.
- **Windows and Fedora**: the maintainer validated Start at Login after the commit ("I validated the start after
  login."), having been asked to try the installed app and the `.zip`, Task Manager's switch and the uninstaller on
  Windows, and the AppImage on Fedora; docs/plan.md records it as "Checked by the user on Windows, macOS and Fedora".
- Not run on matching machines: the `win-arm64`, `linux-arm64` and `osx-x64` builds.
- Quit Anniversary before uninstalling: the uninstaller doesn't close a running app, so Windows can't delete
  `Anniversary.exe` while it runs.
