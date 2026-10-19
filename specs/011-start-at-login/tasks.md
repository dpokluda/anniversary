# Tasks: Start at Login

**Input**: Design documents from `specs/011-start-at-login/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/,
quickstart.md

**Tests**: Included. The constitution (Principle III) requires tests for logic, file format and settings changes: the
14 new test cases are in `Anniversary.Tests/StartAtLoginTests.cs` (the Linux entry on every OS, the registry on Windows
only). The macOS login item, the menus, real logins and the uninstaller are checked by hand with
[quickstart.md](quickstart.md).

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. Everything went into one commit, `6af4c27` ("Adding start after login
feature", 2026-10-10); the maintainer's check on Windows and Fedora was recorded afterwards by `26063da` ("Closing the
5.0 plan").

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): platform code in `Anniversary.Desktop/`, views in `Anniversary.Shared/`, tests in
  `Anniversary.Tests/`, the installer script in `packaging/windows/`, documentation in `README.md`,
  `Anniversary.Desktop/README.md`, `packaging/README.md` and `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point, and a way for the tests to reach the desktop project

- [X] T001 Start from the state 010 left (`54c9cbb`): `Anniversary.slnx` building without warnings and 189 tests passing with `dotnet test --project Anniversary.Tests`
- [X] T002 [P] Let the tests reach the desktop project: `<InternalsVisibleTo Include="Anniversary.Tests" />` in `Anniversary.Desktop/Anniversary.Desktop.csproj` and a `ProjectReference` to `Anniversary.Desktop.csproj` in `Anniversary.Tests/Anniversary.Tests.csproj`
- [X] T003 [P] Create `Anniversary.Tests/StartAtLoginTests.cs`: a sealed `IDisposable` test class with the summary "Tests of StartAtLogin (spec 7.12): the Linux autostart entry in a temporary folder on every OS, and the Windows Run value under a scratch registry key on Windows. macOS's login items are checked by hand.", a `TempFolder`, and the scratch keys `Software\Anniversary.Tests\Run` and `Software\Anniversary.Tests\StartupApproved\Run`; `Dispose` deletes the folder and, on Windows, the key tree `Software\Anniversary.Tests`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The platform class with its public surface, and the hook that hands it to the shared UI

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create `Anniversary.Desktop/StartAtLogin.cs`: `internal static class StartAtLogin` with the summary (starts this copy when the user logs in, spec 7.12: a Run value on Windows, a login item on macOS, an XDG autostart entry on Linux), the constants (`Name` "Anniversary", `RunKey`, `ApprovedKey`, `AutostartFileName` "anniversary.desktop", the Objective-C library path, `MacEnabled` 1, `MacRequiresApproval` 2) and `ProgramPath`: `Environment.ProcessPath`, or `$APPIMAGE` when `$APPIMAGE` and `$APPDIR` are set and the executable's path starts with `$APPDIR` + `/` ("an AppImage app that started this one passes its own on") (FR-008)
- [X] T005 Add `IsSupported` (Windows and Linux always; macOS 13 or later when `ProgramPath` contains `.app/Contents/MacOS/`), `IsOn()` (by OS; `false` on `IOException`, `UnauthorizedAccessException` or `SecurityException`) and `Set(bool on)` (by OS; those exceptions rethrown as `InvalidOperationException(ex.Message, ex)`) to `Anniversary.Desktop/StartAtLogin.cs` (FR-004, FR-009, FR-012)
- [X] T006 [P] Add the hook `public static (Func<bool> IsOn, Action<bool> Set)? StartAtLogin { get; set; }` to `App` in `Anniversary.Shared/App.axaml.cs`, documented as: `IsOn` tells whether this copy starts when the user logs in, `Set` turns that on or off and throws `InvalidOperationException` with the reason when it can't, and `null` hides the menu items (FR-009)
- [X] T007 Set the hook in `Main` in `Anniversary.Desktop/Program.cs`: `if (StartAtLogin.IsSupported)`, `App.StartAtLogin = (StartAtLogin.IsOn, StartAtLogin.Set)`, before `BuildAvaloniaApp().StartWithClassicDesktopLifetime(args)` (FR-009)

**Checkpoint**: The shared UI can ask whether this copy starts at login and change it; user stories can begin

---

## Phase 3: User Story 1 - Start the app when I log in (Priority: P1) 🎯 MVP

**Goal**: ⋯ ▸ Start at Login (macOS: Open at Login, also in the app menu) turns starting this copy at login on and off
on Windows, macOS and Linux

**Independent Test**: Turn it on and log out and in: the app starts; turn it off: it doesn't (quickstart scenario 1)

### Tests for User Story 1

- [X] T008 [P] [US1] Add `LinuxEntryStartsThisCopy` (the file starts with `[Desktop Entry]`, has `Type=Application`, `Name=Anniversary` and `ExecLine(ProgramPath)`, and reads as on), `LinuxEntryIsDeletedWhenTurnedOff`, `TurningOffWithoutAnEntryIsFine` (a folder that doesn't exist), `ExecLineQuotesThePath` (five cases: a plain path, a space, `"` with a backtick and `$`, a backslash, `%`) and the Windows-only `WindowsRunValueStartsThisCopy` (the `Anniversary` value is the quoted `ProgramPath` and reads as on; turning it off deletes the value; elsewhere `Assert.Skip("The registry exists only on Windows.")`) to `Anniversary.Tests/StartAtLoginTests.cs`

### Implementation for User Story 1

- [X] T009 [US1] Windows in `Anniversary.Desktop/StartAtLogin.cs`: `SetWindows(on, runKey, approvedKey)` writes the string value `Anniversary` = `"{ProgramPath}"` under `HKCU\{runKey}` (`CreateSubKey`) or deletes it (no error when it's missing), and `IsOnWindows(runKey, approvedKey)` compares the value with the quoted `ProgramPath`, ignoring case; both `[SupportedOSPlatform("windows")]`, and `IsOn`/`Set` call them with `Software\Microsoft\Windows\CurrentVersion\Run` (FR-005, FR-008)
- [X] T010 [US1] macOS in `Anniversary.Desktop/StartAtLogin.cs`: P/Invokes into `/usr/lib/libobjc.A.dylib` (`objc_getClass`, `sel_registerName` and two `objc_msgSend` overloads), `MainAppService()` (loads the ServiceManagement framework with `NativeLibrary.Load` and sends `mainAppService` to `SMAppService`), `MacStatus()` (`status`: 0 not registered, 1 enabled, 2 requires approval, 3 not found; on means 1) and `SetMac(on)` (`registerAndReturnError:` or `unregisterAndReturnError:`; when it returns NO, throw `InvalidOperationException` with the error's `localizedDescription`) (FR-006)
- [X] T011 [US1] Linux in `Anniversary.Desktop/StartAtLogin.cs`: `AutostartFolder` (`ApplicationData` + `autostart`, which .NET resolves to `$XDG_CONFIG_HOME/autostart` or `~/.config/autostart`), `SetLinux(on, folder)` (creates the folder and writes `[Desktop Entry]`, `Type=Application`, `Name=Anniversary`, `Comment=Name days, birthdays and anniversaries for any day`, the `Exec` line and `Terminal=false`, or deletes the file if it exists), `IsOnLinux(folder)` (the file has exactly this copy's `Exec` line) and `ExecLine(program)` (inside the quotes `"`, the backtick, `$` and `\` get a backslash and `%` is doubled, then every backslash is doubled) (FR-007, FR-008)
- [X] T012 [P] [US1] Add `<MenuItem x:Name="StartAtLoginItem" Header="Start at Login" ToggleType="CheckBox" Click="OnStartAtLogin" />` after the Theme submenu and before the separator above About Anniversary… in the commands `MenuFlyout` in `Anniversary.Shared/Views/MainWindow.axaml` (FR-001)
- [X] T013 [P] [US1] Add a `NativeMenuItemSeparator` and `<NativeMenuItem Header="Open at Login" ToggleType="CheckBox" />` after About Anniversary in the macOS application menu in `Anniversary.Shared/App.axaml`, whose comment now says that `MainWindow` wires Open at Login (spec 7.12) (FR-001)
- [X] T014 [US1] Wire the items in `Anniversary.Shared/Views/MainWindow.axaml.cs`: the field `_nativeStartAtLoginItem`, found with `NativeItem(appMenu, "Open at Login")` in `NativeMenu.GetMenu(App.Current!)`; its `Click` and the new `OnStartAtLogin` call `ToggleStartAtLogin()`, which does nothing without the hook, calls `Set(!IsOn())` and then `UpdateStartAtLoginChecks()`; `UpdateStartAtLoginChecks()` sets `IsChecked` on both items from `App.StartAtLogin?.IsOn() == true`; on macOS the ⋯ item's header becomes "Open at Login" (FR-001, FR-002, FR-004, FR-014)
- [X] T015 [US1] Check scenario 1 of `specs/011-start-at-login/quickstart.md` by hand on each system (docs/spec.md §12, criterion 16)

**Checkpoint**: The app can start at login on all three systems

---

## Phase 4: User Story 2 - See the real state, even after changing it elsewhere (Priority: P2)

**Goal**: The check mark shows whether this copy really starts at login, including changes made in Task Manager, Login
Items or the desktop's settings, and for copies in other places

**Independent Test**: Turn it on in the app, turn it off in Task Manager or Login Items, and open the ⋯ menu: the item
is unchecked (quickstart scenario 2)

### Tests for User Story 2

- [X] T016 [P] [US2] Add `LinuxEntryForAnotherCopyIsOff` (`Exec="/opt/Other/Anniversary.AppImage"`), `LinuxEntryTurnedOffByTheDesktopIsOff` (two cases: `Hidden=true` and `X-GNOME-Autostart-enabled=false`), and the Windows-only `WindowsRunValueForAnotherCopyIsOff` (`"C:\Other\Anniversary.exe"`) and `WindowsTaskManagerFlagsAreFollowedAndClearedWhenTurnedOn` (flags starting with 2 read as on; what Task Manager writes when the app is turned off there, 3 and then the time, reads as off; turning it on again reads as on and deletes the flags) to `Anniversary.Tests/StartAtLoginTests.cs`

### Implementation for User Story 2

- [X] T017 [US2] Follow Task Manager's switch in `Anniversary.Desktop/StartAtLogin.cs`: `IsOnWindows` also reads the `Anniversary` binary value under `HKCU\{approvedKey}` (`Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run`), where an odd first byte means off, and `SetWindows` deletes that value before it writes or deletes the Run value (FR-002, FR-005)
- [X] T018 [US2] Count an entry the desktop turned off as off in `IsOnLinux` in `Anniversary.Desktop/StartAtLogin.cs`: a line `Hidden=true` or `X-GNOME-Autostart-enabled=false` (FR-002, FR-007)
- [X] T019 [US2] Read the state from the OS every time in `Anniversary.Shared/Views/MainWindow.axaml.cs`: `UpdateStartAtLoginChecks()` in the existing `OnCommandsMenuOpening` (the ⋯ and right-click menu, which already refreshed the calendars), in the macOS app menu's `Opening` event, and in the `OnActivated` handler that 007 added to reload a data file changed elsewhere, before that reload; its comment now names both kinds of change made elsewhere: Start at Login in the OS settings (spec 7.12) and the data file (spec 7.11) (FR-003, FR-014)
- [X] T020 [US2] Check scenario 2 of `specs/011-start-at-login/quickstart.md` by hand (docs/spec.md §12, criterion 16)

**Checkpoint**: The check mark stays right after changes made in the OS's own settings

---

## Phase 5: User Story 3 - Know why it can't be turned on (Priority: P3)

**Goal**: Clear messages where Start at Login can't be turned on, and no item where the OS can't start this copy at
login

**Independent Test**: Open the app from the `.dmg` and choose Open at Login: the message asks to move it to
Applications; start the bare macOS executable: there is no item (quickstart scenario 3)

### Implementation for User Story 3

- [X] T021 [US3] Guard macOS in `SetMac` in `Anniversary.Desktop/StartAtLogin.cs`: before registering, a `ProgramPath` containing `/AppTranslocation/` throws `InvalidOperationException("Move Anniversary to the Applications folder, open it from there and try again.")`; after registering, status 2 throws `InvalidOperationException("Allow Anniversary in System Settings ▸ General ▸ Login Items.")` (FR-010, FR-011)
- [X] T022 [US3] Report failures in `ToggleStartAtLogin()` in `Anniversary.Shared/Views/MainWindow.axaml.cs`: catch `InvalidOperationException` and show `MessageDialog.Show(this, name, $"Could not change {name}: {ex.Message}")`, where `name` is the ⋯ item's header ("Start at Login", on macOS "Open at Login"); the check marks are updated afterwards either way (FR-011)
- [X] T023 [US3] Hide the items where the hook is `null`, in the `MainWindow` constructor in `Anniversary.Shared/Views/MainWindow.axaml.cs`: `StartAtLoginItem.IsVisible`, `_nativeStartAtLoginItem.IsVisible` and the app menu's separator follow `App.StartAtLogin != null`; the separator is found as the item right before Open at Login, because macOS adds its own items (Services, Hide, Quit) after it (FR-009)
- [X] T024 [US3] Check scenario 3 of `specs/011-start-at-login/quickstart.md` by hand on macOS (docs/spec.md §12, criterion 16)

**Checkpoint**: Every case where Start at Login can't work either hides the item or says why

---

## Phase 6: User Story 4 - Remove the app without leaving it to start at login (Priority: P3)

**Goal**: The Windows uninstaller removes the installed copy's entry, and the user guide covers copies without an
uninstaller

**Independent Test**: Turn it on in the installed app on Windows and uninstall it: the value is gone and the app-data
folder stays (quickstart scenario 4)

### Implementation for User Story 4

- [X] T025 [US4] Add a `[Code]` section to `packaging/windows/Anniversary.iss`: the constants `RunKey` and `ApprovedKey`, and `CurUninstallStepChanged`, which at `usUninstall` deletes the `Anniversary` value under both keys in `HKCU` if the Run value equals the quoted `{app}\Anniversary.exe` (`CompareText`), with the comment "Start at Login (spec 7.12) is a Run value the app writes itself; it goes with the app if it starts this copy." (FR-013)
- [X] T026 [P] [US4] Start "Removing it" in `Anniversary.Desktop/README.md` with "If you turned on **Start at Login**, turn it off first (the Windows uninstaller does that for you)." (FR-015)
- [X] T027 [US4] Check scenario 4 of `specs/011-start-at-login/quickstart.md` with the installer CI built (docs/spec.md §12, criterion 16)

**Checkpoint**: All four user stories work

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and the final checks

- [X] T028 [P] Add Start at Login to "Using it" in `Anniversary.Desktop/README.md`: in the list of commands, and a bullet saying it starts the app whenever you log in, is called Open at Login on macOS (also in the Anniversary menu) and works for the app in Applications, and that Task Manager ▸ Startup apps and System Settings ▸ General ▸ Login Items list it and can turn it off (FR-015)
- [X] T029 [P] Update `README.md`: Open at Login next to About Anniversary in the macOS app menu, a paragraph on Start at Login (`Anniversary.Desktop/StartAtLogin.cs` writes the `Anniversary` value under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, registers an `SMAppService` login item on macOS 13 and later for the `.app` only, and writes `~/.config/autostart/anniversary.desktop` on Linux, always for the running copy and inside an AppImage for the AppImage file), and `StartAtLogin.cs` in the projects table
- [X] T030 [P] Update `packaging/README.md`: the uninstaller leaves the app-data folder alone and removes Start at Login's Run value if it starts the installed copy (spec 7.12; its `[Code]` section)
- [X] T031 [P] Update `docs/spec.md` (then `specs/spec.md`): §4 (`StartAtLogin.cs`; `StartAtLoginTests.cs`, which also references `Anniversary.Desktop`), §7.2 (the menu row "Start at Login (macOS: Open at Login)", hidden where the OS has no support, and Open at Login after About Anniversary in the macOS app menu), the new §7.12 "Start at Login", §8 (`Program.Main` sets `App.StartAtLogin`), §9 (`StartAtLoginTests`), §10 (the uninstaller removes the Run value), §11 (C9: the app also writes its Start at Login entry when asked; the new C22) and §12 (criterion 16) (FR-014)
- [X] T032 [P] Update `docs/plan.md` (then `specs/plan.md`): "Next" item 3, **start-at-login** ✅ (a check item in the app rather than an installer option, user decision; the Run key chosen over a Startup-folder shortcut; `SMAppService`, tested with the ad-hoc signed app on macOS 27; XDG autostart; the uninstaller; still to check on Windows and Fedora), and "Idea: installers and start at login (done)" with the new Start at login row (a LaunchAgent would show up under "Allow in the Background" with a notification instead)
- [X] T033 [P] Update `docs/copilot/handover.md` (then `specs/copilot/handover.md`): 203 tests (the 3 registry tests run only on Windows, so 200 pass and 3 skip elsewhere), `StartAtLogin.cs` in the code row, Start at Login in the command list, the app writes its Start at Login entry when asked, the lessons "Headless isn't the real menu bar" and "Start at Login on macOS", and ✅ Start at Login in the next steps (checked on macOS; to check on Windows and Fedora)
- [X] T034 Run `dotnet build Anniversary.slnx` (no warnings) and `dotnet test --project Anniversary.Tests`: 203 tests, 200 passed and 3 skipped on macOS; CI then ran all 203 on Windows, including the three registry tests, and built the installer with its uninstall code
- [X] T035 Smoke-test the packaged, ad-hoc signed app on macOS (`specs/011-start-at-login/quickstart.md`, platform checks): Apple's API works with it, and clicking the item in the real app turned it on (✓) and off again, leaving nothing behind; the smoke test caught a launch crash the headless run had missed (macOS adds its own items to the app menu), which was fixed and rechecked before the commit
- [X] T036 After the commit, the maintainer validated Start at Login on Windows and Fedora ("I validated the start after login."); `26063da` recorded it in `docs/plan.md` and `docs/copilot/handover.md` as checked by the user on Windows, macOS and Fedora (acceptance criterion 16), with the `win-arm64`, `linux-arm64` and `osx-x64` builds not run on matching machines

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–6)**: depend on Foundational; User Stories 2 and 3 refine code from User Story 1
- **Polish (Phase 7)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after User Story 1's Windows and Linux code (T009, T011) and `UpdateStartAtLoginChecks()`
  (T014)
- **User Story 3 (P3)**: after User Story 1's `SetMac` (T010) and `ToggleStartAtLogin()` (T014)
- **User Story 4 (P3)**: after User Story 1's Run value (T009), which the uninstaller removes; independent of User
  Stories 2 and 3

### Within Each User Story

- Tests first, then `StartAtLogin.cs`, then the views, then the manual check from quickstart.md
- The tasks on `StartAtLogin.cs` run one after another (one file); `MainWindow.axaml` and `App.axaml` change before
  `MainWindow.axaml.cs`, which uses the generated `StartAtLoginItem` field and finds Open at Login by its header

### Parallel Opportunities

- T002 and T003 next to each other
- T006 next to T004 and T005
- In User Story 1, the tests (T008) and the markup (T012, T013) next to the platform code (T009–T011)
- T016 next to T017 and T018; T026 next to T025
- T028–T033 together: six different documents

---

## Parallel Example: User Story 1

```bash
# The tests and the menu markup can be written next to the platform code:
Task: "Add the Linux and Windows on/off tests to Anniversary.Tests/StartAtLoginTests.cs"
Task: "Write the Run value, the login item and the autostart entry in Anniversary.Desktop/StartAtLogin.cs"
Task: "Add the Start at Login check item to Anniversary.Shared/Views/MainWindow.axaml"
Task: "Add Open at Login to the macOS app menu in Anniversary.Shared/App.axaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenario 1 on each system

### Incremental Delivery

1. Setup and Foundational: the platform class and the hook
2. User Story 1: turning it on and off on all three systems
3. User Story 2: a check mark that follows the OS's own settings
4. User Story 3: messages and hidden items where it can't work
5. User Story 4: the uninstaller's cleanup and the user guide
6. Polish: documentation, the test run, the macOS smoke test and the maintainer's checks

### What happened

The maintainer first asked about starting after login together with the installers, and had it added to the plan as
an idea (the section "Idea: installers and start at login", committed with 005's `1409624`). In the evening he put it
on his list, questioned whether it needed the installers, asked about the Run key and the menu, and answered "Let's do
that."; the assistant then built it, with the tests and the documentation, in one session and checked it on macOS,
where smoke-testing the packaged app caught a launch crash. The maintainer committed it as `6af4c27`, validated it on
Windows and Fedora, and `26063da` recorded that.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- Test names are as they were at the time; no later feature changed them
- The documentation paths are today's; at the time they were under `specs/`
