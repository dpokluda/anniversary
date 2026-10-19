# Feature Specification: User Guide

**Feature Branch**: `009-user-guide`

**Created**: 2026-10-10

**Status**: Implemented (reconstructed retroactively on 2026-10-18 from the specs, the Copilot sessions and the git history)

**Input**: User description: "add some kind of concise .md documentation (README) to the installer(s) and archive(s) for
consumers to understand what and how regarding the app"

**Later changes**: [010](../010-install-packages/spec.md) rewrote "Starting it" for the installer, `.dmg` and AppImage,
added "Opening it the first time" (numbered steps naming the buttons macOS and Windows show) and the removal steps for
each package; [011](../011-start-at-login/spec.md) added Start at Login to "Using it" and "Removing it";
[012](../012-single-settings-file/spec.md) replaced the `appsettings.json` advice with all settings in
`usersettings.json`.

## Clarifications

### Session 2026-10-10

- Q: The maintainer's list of what to do before releases named the packages, running the app after login and this
  README. Where does the README come? → A: First. The plan recorded the order user README, packages, Start at Login,
  releases, and the maintainer kept it ("Keep the order"). So the guide describes the downloads that exist now, the
  plain archives, and each package step updates its "Starting it" section.
- Q: The request names installers and archives, but only the archives exist yet. How does the guide reach users? →
  A: It is published as `README.md` next to the app, so every archive has it now, and every installer will later
  ("in every archive (and later in every installer)").
- Q: Should the repository's README also serve the people who use the app? → A: No. The new guide is the one document
  for people who use the app; the repository's README is for developers and links to it.
- Q: What form should it take? → A: Concise Markdown that also reads well as plain text. It came to about 80 lines.
- Q: What must it cover? → A: What the app does, starting it on each OS (including the first-launch steps for unsigned
  builds), using it (the window, commands and shortcuts), the people and sharing them, the calendars, the data folder
  and `appsettings.json`, removing the app, and the license.
- Q: Signing isn't decided, so Windows and macOS warn about the app the first time. What should users do? → A: The
  guide gives the one-time steps: on Windows **More info**, then **Run anyway**; on macOS **Open Anyway** in **System
  Settings ▸ Privacy & Security**, or one command in Terminal.
- Q: How does the guide stay correct as the app changes? → A: Every change users can see updates it, and each package
  step updates its "Starting it" section.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start the app with the guide in the download (Priority: P1)

Someone you told about Anniversary, for example a family member, gets a download: a `.zip` for Windows or a `.tar.gz`
for macOS and Linux. They extract it and find `README.md` next to the app. Its first section, "Starting it", tells them
that the app is one file with nothing to install, that the folder can live anywhere, and how to start the app on their
OS, including what to click the first time, when Windows or macOS warns about an app that isn't signed.

**Why this priority**: It is what the maintainer asked for: documentation in every download, for the people who use the
app. Starting it is their first hurdle: the builds aren't signed, so Windows warns and macOS blocks the app the first
time, and someone new to the app needs the steps to get past that.

**Independent Test**: Extract a download on each OS: `README.md` is next to the app, and following only "Starting it"
gets the app running.

**Acceptance Scenarios**:

1. **Given** any of the six downloads (Windows, macOS and Linux, each for x64 and arm64), **When** it is extracted,
   **Then** its folder holds the app, `README.md` (the user guide) and `THIRD-PARTY-NOTICES.md`.
2. **Given** Windows says "Windows protected your PC" the first time, **When** the user follows the guide, **Then** they
   click **More info**, then **Run anyway**, and the app starts.
3. **Given** a Mac, **When** the user reads "Starting it", **Then** it names the download for their Mac (`osx-arm64` for
   Apple Silicon, M1 and newer; `osx-x64` for Intel), how to open the app the first time (double-click it, close the
   warning, click **Open Anyway** in **System Settings ▸ Privacy & Security**, double-click it again), that a Terminal
   window opens with the app and must stay open while it is used, and the Terminal alternative.
4. **Given** Linux, **When** the user reads "Starting it", **Then** it says to run `./Anniversary` in a terminal in that
   folder and names the libraries the app needs (fontconfig, libX11, libICE and libSM), which desktop distributions
   already have.
5. **Given** a computer that opens `README.md` in a plain text editor, **When** the user reads it, **Then** it reads
   well without any Markdown rendering.
6. **Given** someone looking at the repository on GitHub, **When** they read its README, **Then** it says that it is for
   developers and links to the user guide, which comes with every download.

---

### User Story 2 - Learn what the app shows and how to use it (Priority: P2)

The guide says in one paragraph what the app does. "Using it" explains the window, moving between days, where the
commands are, what Find searches, the shortcuts and Theme; "Calendars" explains the line under the date, the calendar
the app starts with and how to choose another.

**Why this priority**: Once the app runs, the window shows the day by itself, but the commands sit behind the ⋯ button,
right-click or the macOS menu bar, and the shortcuts and the calendar choice are easy to miss.

**Independent Test**: Read the opening paragraph, "Using it" and "Calendars" next to the running app: every command,
shortcut and calendar the guide names is there under that name, on that OS.

**Acceptance Scenarios**:

1. **Given** a new user, **When** they read the first paragraph, **Then** they learn that the app shows the name days,
   birthdays and anniversaries of the people they care about, for today or any other day, together with the day's
   names or holidays from a calendar they choose, and that it runs on Windows, macOS and Linux.
2. **Given** the main window, **When** they read "Using it", **Then** it explains what the window shows (the day, the
   calendar's names or holidays, the name days, birthdays and anniversaries with the years, and the next 7 days), that
   **Yesterday** and **Tomorrow** move by one day, and that clicking the date picks any day.
3. **Given** they look for a command, **When** they read "Using it", **Then** they learn that the commands are in the
   **⋯** button at the top right or on right-click anywhere in the window (on macOS also in the menu bar): Today,
   Change Date, Find, Find Next, Edit People, Data File, Calendar, Theme and About, and that Find looks for a name among
   their people and in the calendar.
4. **Given** Windows or Linux, or macOS, **When** they look up the shortcuts, **Then** the guide gives Ctrl+F, F3,
   Ctrl+O and Ctrl+Shift+S, and ⌘F, ⌘G, ⌘O and ⇧⌘S on macOS; and **Theme** switches between light and dark or follows
   the system (macOS: **View ▸ Appearance**).
5. **Given** they want another calendar, **When** they read "Calendars", **Then** they learn that the app starts with
   the calendar for their region (for example Czech in Czechia) or the United States calendar of holidays and
   observances elsewhere, and that **⋯ ▸ Calendar** (macOS: **View ▸ Calendar**) offers the name days of Austria,
   Bulgaria, Czechia, France, Hungary, Italy, Latvia, Poland, Russia and Slovakia, the United States, and
   International Days.

---

### User Story 3 - Keep your people and share them (Priority: P2)

"Your people" explains adding people with Edit People… and saving them, sending the data file to family, and using one
data file on several computers through a cloud drive.

**Why this priority**: The people are the user's own data, and sharing the data file
([007](../007-data-file-sharing/spec.md)) was built for exactly the people who read this guide: family members who get
the app and a data file.

**Independent Test**: Follow "Your people" on two computers (or two user accounts): add a person, send the file found
with Show in Folder, open it on the other with Open…, and see the same people there.

**Acceptance Scenarios**:

1. **Given** an empty list, **When** the user reads "Your people", **Then** it tells them to choose **⋯ ▸ Edit
   People…** to add people with their name day, birthday and anniversaries, and that **Save** writes them to their data
   file.
2. **Given** they want to share with family, **When** they follow "Sharing with family", **Then** **⋯ ▸ Data File ▸
   Show in Folder** opens the folder with the data file to send, and the family member saves it somewhere permanent,
   such as Documents, and chooses **⋯ ▸ Data File ▸ Open…** (macOS: **File ▸ Open Data File…**); the app then uses that
   file and saves changes into it.
3. **Given** several computers, **When** they follow "The same people on several computers", **Then** they save the
   file with **Save As…** into a cloud-drive folder (OneDrive, iCloud Drive, Dropbox, …) and choose **Open…** on each
   other computer, and the guide says that a change shows on the others once the cloud has synced it and they switch
   back to the app, and that the app asks before overwriting a file changed elsewhere while they were editing.
4. **Given** more than one computer, **When** they read the section, **Then** they learn that each computer remembers
   which data file it uses.

---

### User Story 4 - Find your data, change settings and remove the app (Priority: P3)

"Files and settings" names the data folder on each OS and the files in it, and shows how to change the two settings
that have no menu. "Removing it" explains what deleting the app leaves behind, and "License" gives the terms and where
the source code is.

**Why this priority**: These are needed less often: to find or back up the files, to look further ahead, to remove the
app, or to check the license.

**Independent Test**: Follow "Files and settings": create `appsettings.json` from the example in the data folder and
start the app: the window shows "Next 14 days" and no years. Follow "Removing it": the app is gone and the data folder
stays.

**Acceptance Scenarios**:

1. **Given** the user looks for their files, **When** they read "Files and settings", **Then** it names the data
   folder (`%APPDATA%\Anniversary`, `~/Library/Application Support/Anniversary`, `~/.config/Anniversary`) and says that
   `Anniversary.Data.json` holds their people (unless they chose another data file) and `usersettings.json` their theme,
   calendar and data file.
2. **Given** they want to look 14 days ahead and hide the years, **When** they create `appsettings.json` in the data
   folder with the guide's example and start the app, **Then** the window shows "Next 14 days" and no years for
   birthdays and anniversaries.
3. **Given** they want to remove the app, **When** they read "Removing it", **Then** they delete the app's folder and
   learn that their people and settings stay in the data folder unless they delete it too.
4. **Given** they want to know the terms, **When** they read "License", **Then** it states the MIT License and the
   copyright holder, says that the name-day calendars come from other sources under their own licenses, listed in
   `THIRD-PARTY-NOTICES.md` next to the app, and gives the address of the source code and issues.

### Edge Cases

- **No Markdown viewer**: `README.md` opens in a plain text editor and still reads well.
- **First start of an unsigned app**: Windows may say "Windows protected your PC", and macOS blocks the app the first
  time; the guide says what to click.
- **The wrong Mac download**: the guide names the download for each kind of Mac (`osx-arm64` for Apple Silicon,
  `osx-x64` for Intel).
- **Terminal window on macOS**: the macOS download is a bare executable, not an app bundle, so a Terminal window opens
  with it; the guide says to leave it open while using the app.
- **Linux libraries**: the guide names the four the app needs; desktop distributions already have them.
- **Another data file chosen**: the people are then in that file, not in `Anniversary.Data.json` in the data folder;
  the guide says so.
- **Removing the app**: deleting its folder keeps the people and settings; the guide says to delete the data folder too
  to remove everything.
- **Two READMEs**: only the user guide goes into the downloads, under the name `README.md`; the repository's README is
  for developers and links to it.
- **The app changes**: a change users can see updates the guide in the same change; the packages rewrite "Starting it".

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every download of the app (Windows, macOS and Linux, each for x64 and arm64) MUST contain the user guide,
  named `README.md`, in the same folder as the app and `THIRD-PARTY-NOTICES.md`.
- **FR-002**: The user guide MUST be the one document for people who use the app. The repository's README MUST say that
  it is for developers and link to the guide, which comes with every download.
- **FR-003**: The guide MUST be concise Markdown that also reads well as plain text.
- **FR-004**: The guide MUST open by saying what the app does (the name days, birthdays and anniversaries of the people
  you care about, for today or any other day, together with the day's names or holidays from a calendar you choose) and
  that it runs on Windows, macOS and Linux.
- **FR-005**: The guide MUST explain how to start the app on each OS: that the app is a single file with nothing to
  install; on Windows, double-clicking it and the one-time **More info**, **Run anyway**; on macOS, which download fits
  which Mac, the one-time **Open Anyway** in **System Settings ▸ Privacy & Security**, the Terminal window that opens
  with the app and must stay open, and the Terminal alternative; on Linux, starting it from a terminal and the
  libraries it needs.
- **FR-006**: The guide MUST explain the main window and its commands: what the window shows, Yesterday and Tomorrow,
  clicking the date to pick a day, the commands in the ⋯ button, on right-click and in the macOS menu bar, what Find
  searches, the shortcuts on Windows and Linux and on macOS, and Theme.
- **FR-007**: The guide MUST explain the people: adding them with Edit People… and saving them; sharing the data file
  with family (Show in Folder, then Open… on their computer); using one data file on several computers through a cloud
  drive (Save As…, then Open…), when a change from another computer shows, that the app asks before overwriting changes
  made elsewhere, and that each computer remembers its data file.
- **FR-008**: The guide MUST explain the calendars: the calendar's names on the line under the date, the default (the
  calendar for the user's region, otherwise the United States), how to choose another, and which calendars ship.
- **FR-009**: The guide MUST name the app's data folder on Windows, macOS and Linux, say what `Anniversary.Data.json`
  and `usersettings.json` hold, and show, with an example, how to set the number of days ahead and hide the years with
  an `appsettings.json` in that folder.
- **FR-010**: The guide MUST explain how to remove the app and that the people and settings stay in the data folder
  unless that folder is deleted too.
- **FR-011**: The guide MUST state the license and the copyright holder, say that the name-day calendars come from other
  sources under their own licenses, listed in `THIRD-PARTY-NOTICES.md` next to the app, and give the address of the
  source code and issues.
- **FR-012**: The guide MUST use the names users see (the app's menus, commands and buttons, and the OS's own warnings
  and settings), with the macOS menu names and shortcuts where they differ.
- **FR-013**: Every change that users can see MUST update the guide in the same change, so that it matches the app in
  the download.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All six downloads (Windows, macOS and Linux, each for x64 and arm64) contain the user guide as `README.md`
  in the same folder as the app and `THIRD-PARTY-NOTICES.md`.
- **SC-002**: The guide covers all eight topics agreed for it (what the app does, starting it, using it, the people,
  the calendars, the data folder and settings, removing the app, the license) in about 80 lines.
- **SC-003**: Every command, shortcut, menu path, calendar and file the guide names exists under that name in the app
  of the same download, on the OS the guide gives it for; a read-through next to the running app finds no difference.
- **SC-004**: Someone who has never used the app needs no other source to start it on Windows, macOS or Linux,
  including the first start of an app that isn't signed.
- **SC-005**: From the repository's README, the user guide is one click away, in its first paragraph.
- **SC-006**: The app itself is unchanged: the build succeeds and all 189 automated tests still pass.

## Assumptions

- The downloads at the time are the CI run's plain archives ([005](../005-ci-pipeline/spec.md)): `Anniversary-{rid}.zip`
  for Windows and `Anniversary-{rid}.tar.gz` for macOS and Linux, each with a folder holding the app and
  `THIRD-PARTY-NOTICES.md`; downloading them needs a GitHub sign-in. The installer, `.dmg` and AppImage came next
  ([010](../010-install-packages/spec.md)).
- The builds aren't signed, and whether to sign them isn't decided. On macOS the download is a bare executable, not an
  app bundle.
- The guide describes the app as it is after [008](../008-international-days/spec.md): the commands menu, the
  shortcuts and Theme ([001](../001-cross-platform-rewrite/spec.md)), Edit People…
  ([002](../002-people-editor/spec.md)), the 12 calendars in the **Calendar** menu with the default chosen by region
  ([003](../003-calendar-chooser/spec.md), [006](../006-holiday-calendar-rules/spec.md), 008) and the Data File menu
  ([007](../007-data-file-sharing/spec.md)).
- The two settings without a menu (`numberOfDaysAhead`, `displayYearsOnBirthdays`) are then read from an
  `appsettings.json` that users may create in the data folder (v4's file, which the app never writes);
  [012](../012-single-settings-file/spec.md) later moved them into `usersettings.json`.
- The app's behaviour doesn't change: only the guide, what is published next to the app, and the documentation.
- Releases on GitHub are a later step of the plan, not part of this feature.
