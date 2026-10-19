# Feature Specification: Start at Login

**Feature Branch**: `011-start-at-login`

**Created**: 2026-10-10

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "add ability to run the app after login" … "You mentioned that the run after login
should be done after installers since you need installation path but do you? You know the path of the running app.
That's what you would be using or am I missing something?" … "add start after login feature. For start after login on
Windows, I see there are two ways to do that: HKCU/…/Run entry, Startup folder LNK. Is one of them preferred than the
other?" … "Got it, you think we should add it to the program menu as an option rather than installer option?" … "Let's
do that."

## Clarifications

### Session 2026-10-10

- Q: Can the app be registered to start after login on each platform, and is that a job for an installer? → A: It is
  possible on all three: a value under the Windows Run key; on macOS a LaunchAgent, or Apple's login items once there
  is an `.app`; an autostart entry on Linux. It works better as a setting in the app (⋯ ▸ Start at Login) than as an
  installer step: per user, no admin rights, it works for copies that weren't installed, and it can be turned off
  again. The maintainer had it added to the plan as an idea ("Add it to plan please."), which went in with the CI work
  of [005](../005-ci-pipeline/spec.md).
- Q: Does Start at Login have to wait for the installers, to know where the app is installed? → A: No: the app
  registers the copy that is running, wherever it is (inside an AppImage, the AppImage file), so it needs no installer.
  The maintainer kept the planned order anyway: the user guide, the packages, then Start at Login.
- Q: On Windows, a Run-key entry or a shortcut in the Startup folder? → A: The Run key
  (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`). Both are per user, need no admin rights, start at the same
  point and appear in Task Manager ▸ Startup apps, but the Run key is one registry value the app can read, write and
  delete directly, so the menu can show its state and the uninstaller can remove it; a shortcut needs extra
  Windows-specific code and suits an installer checkbox better. The app also reads Task Manager's off switch, so its
  check mark stays accurate, and clears it when the user turns Start at Login back on.
- Q: Should it be an option in the app's menu or an installer option? → A: A check item in the app (⋯ ▸ Start at
  Login, and in the macOS menu bar too): it works for every download (the `.dmg`, the AppImage and the archives have no
  installer), it can be changed at any time without reinstalling, and it always registers the right path. The
  maintainer agreed ("Let's do that."; user decision).
- Q: Should the Windows installer also offer a Start at Login checkbox? → A: No. The installer's only part is cleanup:
  the uninstaller removes the entry if it starts the installed copy. With a checkbox, every upgrade would have to
  decide whether an unchecked box undoes a choice made in the app.
- Q: On macOS, a LaunchAgent or Apple's login items? → A: Apple's login items (macOS 13 and later), for the `.app`
  only and under Apple's name, **Open at Login**, also in the Anniversary menu. The app is then listed under System
  Settings ▸ General ▸ Login Items ▸ Open at Login, the list the Dock's Options ▸ Open at Login uses; a LaunchAgent
  would show up under "Allow in the Background" with a notification instead. Chosen while building it and reported in
  the session.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start the app when I log in (Priority: P1)

The user wants the day's names, birthdays and anniversaries in front of them every morning without starting the app
by hand. They choose **⋯ ▸ Start at Login** (macOS: **Open at Login**, also in the **Anniversary** menu); from the next
login on, the app starts by itself. Choosing the item again stops that. It works for the copy they run, however they
got it: the Windows installer or `.zip`, the macOS app in Applications, the Linux AppImage or archive.

**Why this priority**: It is the request itself ("add ability to run the app after login"); the other stories only
keep the check mark honest, explain the cases where it can't work and clean up.

**Independent Test**: Turn it on and log out and in: the app starts. Turn it off and log out and in: it doesn't.

**Acceptance Scenarios**:

1. **Given** the app on Windows, installed or from the `.zip`, **When** the user chooses ⋯ ▸ Start at Login, **Then**
   the item is checked, Task Manager ▸ Startup apps lists Anniversary, and the app starts at the next sign-in.
2. **Given** the app in Applications on macOS 13 or later, **When** the user chooses ⋯ ▸ Open at Login or
   **Anniversary ▸ Open at Login**, **Then** both items are checked, System Settings ▸ General ▸ Login Items lists
   Anniversary under Open at Login, and the app opens at the next login.
3. **Given** the AppImage on Fedora, **When** the user chooses ⋯ ▸ Start at Login, **Then** an autostart entry for the
   AppImage file appears in the user's autostart folder and the app starts at the next login.
4. **Given** Start at Login is on, **When** the user chooses the item again, **Then** it is unchecked, the OS no longer
   lists the app, and the app doesn't start at the next login.

---

### User Story 2 - See the real state, even after changing it elsewhere (Priority: P2)

Windows and macOS list the app in their own settings (Task Manager ▸ Startup apps, System Settings ▸ General ▸ Login
Items), and users may turn it off there; Linux desktops can turn an autostart entry off too. The check mark always shows
whether **this copy** really starts at login, because the app asks the OS each time a menu opens. A copy started from
another folder shows unchecked until it is turned on there.

**Why this priority**: Without it, the check mark would be wrong after a change in the OS's settings, and the user
would believe the app starts at login when it doesn't (or the other way round).

**Independent Test**: Turn it on in the app, turn it off in Task Manager (or Login Items), and open the ⋯ menu: the item
is unchecked. Choose it: it is checked again, and the app starts at the next login.

**Acceptance Scenarios**:

1. **Given** Start at Login is on, **When** the user turns Anniversary off in Task Manager ▸ Startup apps (or Settings
   ▸ Apps ▸ Startup), **Then** the item is unchecked the next time the menu opens, and the app doesn't start at login.
2. **Given** it was turned off in Task Manager, **When** the user chooses Start at Login in the app, **Then** the item
   is checked and the app starts at login again.
3. **Given** Open at Login is on, **When** the user turns it off in System Settings ▸ General ▸ Login Items, **Then**
   both Open at Login items are unchecked the next time a menu opens or the window is activated.
4. **Given** the Linux desktop has turned the app's autostart entry off, **When** the user opens the ⋯ menu, **Then**
   Start at Login is unchecked.
5. **Given** Start at Login is on for a copy in one folder, **When** the user starts a copy from another folder (or
   moves the app), **Then** that copy shows the item unchecked, and choosing it makes that copy the one that starts.

---

### User Story 3 - Know why it can't be turned on (Priority: P3)

On macOS, the login item works only for the app as a real app bundle. An app opened straight from Downloads or from the
`.dmg` runs from a temporary copy that would be gone by the next login, so the app asks the user to move it to
Applications instead of registering it. Where the OS can't start this copy at login at all (the bare macOS app from the
`.tar.gz`, or macOS 12), the item isn't shown. Any other failure is reported with the OS's reason.

**Why this priority**: These are the cases where Start at Login can't work; a clear message is better than a check
mark that silently does nothing.

**Independent Test**: Open the app straight from the `.dmg` and choose Open at Login: a message asks to move the app to
Applications, and nothing is registered. Start the bare macOS app: there is no Open at Login item.

**Acceptance Scenarios**:

1. **Given** the app opened from Downloads or the `.dmg` on macOS, **When** the user chooses Open at Login, **Then** a
   message titled **Open at Login** says "Could not change Open at Login: Move Anniversary to the Applications folder,
   open it from there and try again." and the item stays unchecked.
2. **Given** the bare macOS app from the `.tar.gz`, or macOS 12, **When** the user opens the ⋯ menu or the Anniversary
   menu, **Then** neither shows Open at Login (nor the separator before it in the Anniversary menu).
3. **Given** macOS registers the login item but waits for the user's approval, **When** the user turns it on, **Then**
   the message says "Could not change Open at Login: Allow Anniversary in System Settings ▸ General ▸ Login Items."
4. **Given** the OS refuses the change, **When** the user chooses the item, **Then** the app says "Could not change
   Start at Login: {reason}" (macOS: "Could not change Open at Login: {reason}") and the item shows the real state.

---

### User Story 4 - Remove the app without leaving it to start at login (Priority: P3)

Uninstalling on Windows also removes the app's startup entry if it starts the installed copy. Copies without an
uninstaller (the macOS app, the AppImage, the archives) are deleted by hand, so the user guide says to turn Start at
Login off first.

**Why this priority**: Removing the app is rare, but a leftover entry would keep trying to start an app that's gone.

**Independent Test**: Turn Start at Login on in the installed app on Windows and uninstall it: the entry is gone and the
app-data folder is still there.

**Acceptance Scenarios**:

1. **Given** the installed app on Windows starts at login, **When** the user uninstalls it in Settings ▸ Apps, **Then**
   its startup entry (and Task Manager's switch for it) is removed and the app-data folder stays.
2. **Given** the startup entry starts another copy (for example one from a `.zip`), **When** the installed app is
   uninstalled, **Then** that entry is left alone.
3. **Given** a copy without an uninstaller, **When** the user reads "Removing it" in the user guide, **Then** it says to
   turn Start at Login off first and that the Windows uninstaller does that itself.

### Edge Cases

- **Another copy is registered** (the app was moved, or a `.zip` copy and an installed one): the item is unchecked in
  the copy that isn't registered; turning it on there makes that copy the one that starts.
- **Turned off in the OS's settings**: the item is unchecked; turning it on in the app turns it back on (on Windows it
  also clears Task Manager's off switch).
- **A temporary copy on macOS** (opened from Downloads or the `.dmg`): nothing is registered; the message asks to move
  the app to Applications.
- **Approval needed on macOS**: the message points to System Settings ▸ General ▸ Login Items.
- **No support** (the bare macOS app, macOS 12): no item, and no stray separator in the Anniversary menu.
- **The state can't be read**: the item is shown unchecked, without a message.
- **The change fails**: "Could not change Start at Login: {reason}" (the title and the wording use the item's name, so
  "Open at Login" on macOS); the item then shows the real state.
- **Paths with spaces or special characters**: the entry quotes the path, so any folder works.
- **Inside an AppImage**: the AppImage file is registered, not the program inside it; a copy that was merely started by
  another AppImage app registers itself, not that app.
- **Deleting a copy that starts at login**: only the Windows uninstaller removes the entry; otherwise the user turns it
  off first (user guide).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The commands menu (the ⋯ button and right-click) MUST have a **Start at Login** check item after Theme
  and before the separator above About Anniversary…. On macOS it MUST be called **Open at Login** (Apple's term) and
  MUST also be in the Anniversary (app) menu, after About Anniversary and a separator.
- **FR-002**: The item MUST be checked exactly when the copy of the app that is running starts when the user logs in
  and the OS's own settings haven't turned that off.
- **FR-003**: The app MUST read the state from the OS each time the menu opens (and when the main window is activated)
  instead of remembering it, so changes made in the OS's settings show.
- **FR-004**: Choosing the item MUST turn starting at login on when it is off, and off when it is on, for the current
  user only and without admin rights.
- **FR-005**: On Windows, the app MUST start at login through its per-user Run-key entry, which Task Manager ▸ Startup
  apps lists; turning it on in the app MUST also undo an off switch set in Task Manager or Settings ▸ Apps ▸ Startup.
- **FR-006**: On macOS 13 and later, the app MUST register itself as a login item, listed in System Settings ▸ General
  ▸ Login Items under Open at Login, and only when it runs as the app bundle (`Anniversary.app`).
- **FR-007**: On Linux, the app MUST write an autostart entry, `anniversary.desktop` in the user's autostart folder
  (`$XDG_CONFIG_HOME/autostart`, by default `~/.config/autostart`), that starts this copy with its path quoted as the
  desktop-entry format requires; turning it off MUST delete the entry. An entry the desktop has turned off
  (`Hidden=true` or `X-GNOME-Autostart-enabled=false`) MUST count as off.
- **FR-008**: The program registered MUST be the copy that is running, wherever it is; inside an AppImage, the
  AppImage file. An entry for a copy in another place MUST show as off, and turning it on MUST register this copy.
- **FR-009**: Where the OS can't start this copy at login (macOS before 13, or the app not running as an app bundle),
  the item MUST be hidden, together with its separator in the macOS Anniversary menu.
- **FR-010**: On macOS, the app MUST NOT register a temporary copy (an app opened from Downloads or the `.dmg`); it
  MUST say "Move Anniversary to the Applications folder, open it from there and try again." instead.
- **FR-011**: If turning it on or off fails, the app MUST show "Could not change {item}: {reason}" with the item's name
  as the title ("Start at Login", on macOS "Open at Login") and then show the real state. When macOS waits for the
  user's approval, the reason MUST be "Allow Anniversary in System Settings ▸ General ▸ Login Items."
- **FR-012**: If the state can't be read, the item MUST show unchecked, without an error message.
- **FR-013**: The Windows uninstaller MUST remove the startup entry, and Task Manager's switch for it, if the entry
  starts the installed copy, and MUST leave an entry for another copy alone; it still keeps the app-data folder.
- **FR-014**: The app MUST write or delete its startup entry only when the user chooses the item, and MUST keep no
  other record of it: `usersettings.json` and the data file are unchanged by this feature.
- **FR-015**: The user guide shipped with every download MUST describe Start at Login (Open at Login on macOS), where
  Windows and macOS list it, and that a copy should be turned off before it is deleted.

### Key Entities

- **Startup entry**: the per-user record each OS uses to start an app at login: a Run-key value on Windows (with Task
  Manager's on/off switch kept separately), a login item on macOS, an autostart file on Linux. It names one copy of the
  app. The app creates or deletes it on request and otherwise only reads it.
- **This copy**: the app as it is running: its executable, the AppImage file on Linux when it runs inside an AppImage,
  and on macOS the app bundle it runs from.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With Start at Login turned on, the app starts at the next login on Windows (installed and from the
  `.zip`), on macOS (the app in Applications, listed under Login Items) and on Fedora (the AppImage); with it turned
  off, it doesn't (acceptance criterion 16 in [docs/spec.md](../../docs/spec.md) §12).
- **SC-002**: Turning it off in the app, or in Task Manager or Login Items, shows the item unchecked the next time the
  menu opens, every time (criterion 16).
- **SC-003**: On macOS, an app opened from Downloads or the `.dmg` never registers itself; the user gets the "Move
  Anniversary to the Applications folder…" message (criterion 16).
- **SC-004**: After uninstalling on Windows, no startup entry for the installed copy is left, and the app-data folder
  is kept (criterion 16).
- **SC-005**: One menu choice turns it on or off, with no admin rights and no reinstalling, for the installed app and
  the `.zip` on Windows, the app in Applications on macOS and the AppImage on Linux.
- **SC-006**: 14 new automated tests cover the Linux entry (on every OS) and the Windows entry (on Windows); all 203
  tests pass on Windows, 200 pass with the 3 Windows-only tests skipped on macOS and Linux, and the build has no
  warnings.

## Assumptions

- Builds on [001](../001-cross-platform-rewrite/spec.md) (the ⋯ and right-click menu, the macOS menu bar, message
  dialogs), [009](../009-user-guide/spec.md) (the user guide shipped with every download) and
  [010](../010-install-packages/spec.md) (the Windows installer and uninstaller, the `.app` in a `.dmg`, the AppImage).
- Starting at login is per user and needs no admin rights on any of the three systems.
- The OS's registration is the only record: the app doesn't remember the setting itself.
- On Windows and Linux the entry has one fixed name, so it starts one copy at a time.
- macOS 12, the oldest version the macOS app declares, has no Open at Login: Apple's login-item API needs macOS 13.
- Future mobile apps leave the item out.
- Not part of this feature: a Start at Login checkbox in the installer; moving the window to the new day after midnight
  or after sleep; one copy at a time; asking to close the app before uninstalling. The assistant suggested the last
  three once the feature was done, since starting at login makes those situations more noticeable or more likely; none
  of them was built.
- The maintainer checked the feature on Windows, macOS (Apple Silicon) and Fedora. The Windows and Linux ARM builds
  and the Intel Mac build weren't run on matching machines.
