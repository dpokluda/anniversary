# Implementation Plan: Start at Login

**Branch**: `011-start-at-login` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/011-start-at-login/spec.md`

## Summary

Let the app start when the user logs in, as a check item in the app rather than an installer option (user decision).
**⋯ ▸ Start at Login** (macOS: **Open at Login**, also in the app menu after About Anniversary) is checked when the
copy that is running starts at login; the state is read from the OS each time a menu opens or the main window is
activated, and clicking the item turns it on or off. The platform code is a new internal static class,
`Anniversary.Desktop/StartAtLogin.cs`: the `Anniversary` value under
`HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (following Task Manager's `StartupApproved` flags) on Windows,
`SMAppService.mainApp` called through the Objective-C runtime on macOS 13 and later (the `.app` only, never an App
Translocation copy), and an XDG autostart entry on Linux (the AppImage file inside an AppImage). `Program.Main` hands
it to the shared UI through a nullable hook, `App.StartAtLogin`, so `Anniversary.Shared` keeps no platform code and
the items hide where the hook is `null`. The Windows uninstaller removes the value if it starts the installed copy.

## Technical Context

**Language/Version**: C# (latest) on .NET 10 (`net10.0`)

**Primary Dependencies**: Avalonia 12.1.4 with the Fluent theme (a `ToggleType="CheckBox"` `MenuItem` in the commands
`MenuFlyout`, a `NativeMenuItem` in the application's `NativeMenu`). OS features are called directly, with no new
package: the `Registry` API of `Microsoft.Win32` (part of .NET) on Windows; on macOS the Objective-C runtime
(`/usr/lib/libobjc.A.dylib`: `objc_getClass`, `sel_registerName`, `objc_msgSend`) through P/Invoke, with the
ServiceManagement framework loaded by `NativeLibrary.Load`; plain file I/O for the Linux autostart entry; Inno Setup 6
Pascal Script (`[Code]`) for the uninstaller.

**Storage**: none of the app's own: the OS keeps the registration per user (the Run value and Task Manager's flags in
`HKCU`, the macOS login item, `~/.config/autostart/anniversary.desktop`). `usersettings.json` and the data file don't
change.

**Testing**: xUnit v3 via Microsoft.Testing.Platform (`dotnet test --project Anniversary.Tests`), classic style; 14 new
test cases in `Anniversary.Tests/StartAtLoginTests.cs` (11 for the Linux entry on every OS, 3 for the registry that run
only on Windows and skip elsewhere): 203 tests in total, all passing on Windows, 200 passing and 3 skipped on macOS and
Linux. The test project now references `Anniversary.Desktop`. The macOS login item, the menus and the uninstaller are
checked by hand ([quickstart.md](quickstart.md)).

**Target Platform**: Windows, macOS and Linux desktops (x64 and arm64); on macOS only the `.app` on macOS 13 or later

**Project Type**: desktop app

**Performance Goals**: no background work and no polling. The state is read (one registry read, one `status` message
or one small file) when a menu opens, when the main window is activated and after a click.

**Constraints**: per user and without admin rights; no packages; platform code only in `Anniversary.Desktop`; register
the copy that is running (`Environment.ProcessPath`, or `$APPIMAGE` inside an AppImage); never register an App
Translocation path; keep no copy of the state (the OS's settings stay the source of truth); the app writes nothing
else, and the uninstaller still keeps the app-data folder.

**Scale/Scope**: one check item in the ⋯ menu and one in the macOS app menu, one internal static class of about 260
lines, one hook property on `App`, one uninstaller procedure, 14 test cases.

## Constitution Check

Checked retroactively against the constitution v1.0.0 (ratified 2026-10-18); the work predates it.

| Principle | Status | Evidence |
|---|---|---|
| I. Concise Code-Behind Architecture | PASS with justification | One new type, the internal static class `StartAtLogin`, holds non-UI logic outside `AnniversaryBook` because it is platform code (Complexity Tracking). No new project, package, interface or DI: the shared UI gets two delegates through the nullable `App.StartAtLogin`. `MainWindow` stays code-behind with three small methods. |
| II. Established Coding Style and Complete XML Documentation | PASS | `StartAtLogin` and all its non-private members have XML docs (`<summary>`, `<param>`, `<returns>`, `<exception>`), as does `App.StartAtLogin`; short comments explain the macOS status values, Task Manager's flags and the `Exec` quoting. `StartAtLoginTests` has a `<summary>` naming spec 7.12, and its test names are sentences; v4 style. |
| III. Classic Tests on Real Objects | PASS | `StartAtLoginTests` runs the real code against a real autostart folder in a `TempFolder` (on every OS) and the real registry under the scratch key `HKCU\Software\Anniversary.Tests` (on Windows, deleted in `Dispose`): the internal methods take the folder and keys as parameters, so no fakes or mocks are needed. The registry tests skip, not fail, elsewhere (`Assert.Skip`). docs/spec.md §9 lists the tests; the macOS login item is a manual check (§12 criterion 16, [quickstart.md](quickstart.md)). |
| IV. Cross-Platform Parity with a Native Look | PASS | Works on all three systems and hides where the OS has no support (the bare macOS executable, macOS 12; the constitution cites this feature as its example). macOS gets Apple's term, Open at Login, and the item in the app menu. All platform code is in `Anniversary.Desktop`. Checked on real machines: macOS by the assistant (the packaged app on macOS 27), then Windows, macOS and Fedora by the maintainer (recorded by `26063da`); the `win-arm64`, `linux-arm64` and `osx-x64` builds are recorded as not run on matching machines. |
| V. Safe, Private and Compatible User Data | PASS | This feature added "and, when asked, its Start at Login entry" to the rule about what the app writes (C9). No file format or setting changes, and the state isn't copied into `usersettings.json`. Reading the state doesn't throw for I/O, access or security errors (`IsOn` returns `false`); a failed change reaches the window as `InvalidOperationException` and becomes a message. The uninstaller removes only the app's own Run value, only if it starts the installed copy, and still keeps the app-data folder. |
| VI. Verifiable Calendar Data | N/A | No calendar data changes. |
| VII. Decided, Specified and Documented Changes | PASS | The in-app item over an installer option is a user decision ("Let's do that."), recorded in docs/spec.md 7.12 and docs/plan.md, which also records the Run key over a Startup-folder shortcut and the login item over a LaunchAgent. §11 got C22 and an updated C9. The same commit updated docs/spec.md (4, 7.2, 7.12, 8, 9, 10, 11, 12), docs/plan.md, the handover, `README.md`, `Anniversary.Desktop/README.md` and `packaging/README.md`. |

**Post-design re-check**: PASS. The design adds one justified type (`StartAtLogin`, in the desktop project), one
property to `App`, two items and three small methods to the existing views, one procedure to the installer script and
no package; `Anniversary.Shared` still has no platform code.

## Project Structure

### Documentation (this feature)

```text
specs/011-start-at-login/
├── spec.md                       # /speckit-specify + /speckit-clarify
├── checklists/
│   └── requirements.md           # spec quality checklist
├── plan.md                       # this file
├── research.md                   # Phase 0
├── data-model.md                 # Phase 1
├── quickstart.md                 # Phase 1
├── contracts/                    # Phase 1
│   ├── os-registration.md        # the Run value, the login item, the autostart entry, the uninstaller's cleanup
│   ├── start-at-login-hook.md    # App.StartAtLogin and the StartAtLogin class
│   └── start-at-login-menu.md    # the check items, their state and messages
└── tasks.md                      # /speckit-tasks
```

### Source Code (repository root)

```text
Anniversary.Desktop/
├── Anniversary.Desktop.csproj      # changed: InternalsVisibleTo Anniversary.Tests
├── Program.cs                      # changed: sets App.StartAtLogin where StartAtLogin.IsSupported
├── StartAtLogin.cs                 # new: Windows Run value, macOS login item, Linux autostart entry
└── README.md                       # changed: Start at Login under "Using it" and "Removing it"
Anniversary.Shared/
├── App.axaml                       # changed: macOS app menu gets a separator and Open at Login
├── App.axaml.cs                    # changed: the App.StartAtLogin hook
└── Views/
    ├── MainWindow.axaml            # changed: the Start at Login check item
    └── MainWindow.axaml.cs         # changed: wiring, visibility, ToggleStartAtLogin, UpdateStartAtLoginChecks
Anniversary.Tests/
├── Anniversary.Tests.csproj        # changed: references Anniversary.Desktop
└── StartAtLoginTests.cs            # new: 14 test cases
packaging/
├── README.md                       # changed: the uninstaller removes the Run value
└── windows/
    └── Anniversary.iss             # changed: [Code] CurUninstallStepChanged
README.md                           # changed: macOS app menu, Start at Login paragraph, projects table
docs/spec.md                        # changed (then specs/spec.md)
docs/plan.md                        # changed (then specs/plan.md)
docs/copilot/handover.md            # changed (then specs/copilot/handover.md)
```

**Structure Decision**: The three-project layout is kept. `StartAtLogin.cs` is the first platform code in
`Anniversary.Desktop` besides `Program.cs`; the shared views reach it only through `App.StartAtLogin`, which
`Program.Main` sets where `StartAtLogin.IsSupported`. `Anniversary.Tests` now references `Anniversary.Desktop` and
sees its internals through `InternalsVisibleTo`. The uninstaller's cleanup goes into the installer script from
[010](../010-install-packages/spec.md).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| New type `StartAtLogin`: non-UI logic outside `AnniversaryBook` (Principle I) | Writing the Run value, calling Apple's login-item API and writing the autostart entry is platform code, which belongs in `Anniversary.Desktop` (Principle IV; spec 7.12: "so platform code stays out of `Anniversary.Shared`"). One internal static class covers the three systems and takes the folder and registry keys as parameters, so the tests can use scratch ones. | Putting it in `AnniversaryBook` or `MainWindow` would bring platform code into `Anniversary.Shared`, which the future mobile apps share and where they leave Start at Login out (`App.StartAtLogin` stays `null`). |
