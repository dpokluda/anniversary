# Tasks: User Guide

**Input**: Design documents from `specs/009-user-guide/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, contracts/, quickstart.md

**Tests**: No new automated tests. The feature changes no logic, file format or settings (constitution Principle III),
and the 189 existing tests still pass. The guide in the publish folder and its content are checked by hand with
[quickstart.md](quickstart.md); those checks are tasks below.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own. All tasks are
complete (reconstructed); paths are today's. Everything went into one commit, `5868c42` ("Adding app README file to be
included when installed", 2026-10-10).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- Descriptions name the exact files and, in brackets, the requirements they serve

## Path Conventions

- Desktop app (see plan.md): the guide and the project file in `Anniversary.Desktop/`, documentation in `README.md` and
  `docs/` (then `specs/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The starting point

- [X] T001 Start from the state 008 left (`fb5312e`): `Anniversary.slnx` building without warnings, 189 tests passing with `dotnet test --project Anniversary.Tests`, and `.github/workflows/ci.yml` publishing each RID and attaching its whole folder as `Anniversary-{rid}.zip` (Windows) or `Anniversary-{rid}.tar.gz` (macOS, Linux)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the guide and ship it next to the executable in every download

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T002 [P] Create `Anniversary.Desktop/README.md`, the user guide, with the title `# Anniversary` and the opening paragraph "Anniversary shows the name days, birthdays and anniversaries of the people you care about, for today or any other day, together with the day's names or holidays from a calendar you choose. It runs on Windows, macOS and Linux." (FR-002, FR-003, FR-004)
- [X] T003 [P] Add `<None Update="README.md" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" ExcludeFromSingleFile="true" />` with the comment "The README for users (spec 10), shipped in every archive and installer." to `Anniversary.Desktop/Anniversary.Desktop.csproj`, in the item group of the `THIRD-PARTY-NOTICES.md` item and before it, so the guide is copied next to the executable in the build and publish output and stays out of the single-file bundle (FR-001)
- [X] T004 Check a local macOS build: its folder holds `Anniversary`, `README.md` and `THIRD-PARTY-NOTICES.md`; because `.github/workflows/ci.yml` archives the whole folder, every CI download includes the guide with no workflow change (FR-001; scenario 1 of `specs/009-user-guide/quickstart.md`)

**Checkpoint**: The guide, so far a title and one paragraph, is published next to the app for every RID; the user
stories can begin

---

## Phase 3: User Story 1 - Start the app with the guide in the download (Priority: P1) 🎯 MVP

**Goal**: "Starting it" gets a new user from the extracted download to the running app on each OS

**Independent Test**: Extract a download on each OS: `README.md` is next to the app, and following only "Starting it"
gets the app running (quickstart scenarios 1–3)

### Implementation for User Story 1

- [X] T005 [US1] Write "## Starting it" in `Anniversary.Desktop/README.md`: the app is the single file `Anniversary` (`Anniversary.exe` on Windows), there's nothing to install, and the folder can be kept anywhere, with the app started from there (FR-005)
- [X] T006 [US1] Add the **Windows** bullet to "Starting it" in `Anniversary.Desktop/README.md`: double-click `Anniversary.exe`; the app isn't signed, so the first time Windows may say "Windows protected your PC": click **More info**, then **Run anyway** (FR-005, FR-012)
- [X] T007 [US1] Add the **macOS** bullet to "Starting it" in `Anniversary.Desktop/README.md`: the `osx-arm64` download for Apple Silicon Macs (M1 and newer) and `osx-x64` for Intel Macs; macOS blocks the unsigned app the first time: double-click `Anniversary`, close the warning, open **System Settings ▸ Privacy & Security**, click **Open Anyway** next to the message about Anniversary, and double-click it again; a Terminal window opens with it and stays open while the app is used; or, in Terminal in that folder, `xattr -d com.apple.quarantine Anniversary` once, then `./Anniversary` (FR-005, FR-012)
- [X] T008 [US1] Add the **Linux** bullet to "Starting it" in `Anniversary.Desktop/README.md`: run `./Anniversary` in a terminal in that folder; it needs fontconfig, libX11, libICE and libSM, which desktop distributions already have (FR-005)
- [X] T009 [US1] Check scenarios 2 and 3 of `specs/009-user-guide/quickstart.md` by hand: start the app on each OS with only "Starting it", and read the guide as plain text (FR-003, FR-005)

**Checkpoint**: A new user can find the guide in the download and start the app

---

## Phase 4: User Story 2 - Learn what the app shows and how to use it (Priority: P2)

**Goal**: "Using it" and "Calendars" explain the window, the commands, the shortcuts, Theme and the calendars

**Independent Test**: Read the opening paragraph, "Using it" and "Calendars" next to the running app: every command,
shortcut and calendar the guide names is there under that name (quickstart scenario 4, steps 1–4)

### Implementation for User Story 2

- [X] T010 [US2] Write "## Using it" in `Anniversary.Desktop/README.md`: the window shows the day, the calendar's names or holidays for it, your people's name days, their birthdays and anniversaries (with the years), and everything coming up in the next 7 days (FR-006)
- [X] T011 [US2] Add the command bullets to "Using it" in `Anniversary.Desktop/README.md`: **Yesterday** and **Tomorrow** move by one day and clicking the date picks any day; the other commands are in the **⋯** button at the top right, or on right-click anywhere in the window (on macOS also in the menu bar): Today, Change Date, Find, Find Next, Edit People, Data File, Calendar, Theme and About; **Find** looks for a name among your people and in the calendar (FR-006, FR-012)
- [X] T012 [US2] Add the shortcut and Theme bullets to "Using it" in `Anniversary.Desktop/README.md`: Ctrl+F finds, F3 finds the next one, Ctrl+O opens a data file and Ctrl+Shift+S saves it under a new name (macOS: ⌘F, ⌘G, ⌘O and ⇧⌘S); **Theme** switches between light and dark, or follows your system (macOS: **View ▸ Appearance**) (FR-006, FR-012)
- [X] T013 [US2] Write "## Calendars" in `Anniversary.Desktop/README.md`: the line under the date shows the calendar's names for the day; the app starts with the calendar for your region (for example Czech in Czechia), or the United States calendar of holidays and observances elsewhere; **⋯ ▸ Calendar** (macOS: **View ▸ Calendar**) offers name days for Austria, Bulgaria, Czechia, France, Hungary, Italy, Latvia, Poland, Russia and Slovakia, the United States, or International Days (well-known UN days plus a few fun ones such as Pi Day and International Beer Day) (FR-008, FR-012)
- [X] T014 [US2] Check scenario 4, steps 1–4, of `specs/009-user-guide/quickstart.md` by hand: the menus, the right-click menu, the shortcuts, the macOS menu bar and the Calendar menu match the guide (FR-004, FR-006, FR-008, FR-012)

**Checkpoint**: A new user can find every command, shortcut and calendar from the guide

---

## Phase 5: User Story 3 - Keep your people and share them (Priority: P2)

**Goal**: "Your people" explains editing the people, sending the data file to family and using it on several computers

**Independent Test**: Follow "Your people" on two computers or user accounts: add a person, send the file found with
Show in Folder, open it on the other with Open… (quickstart scenario 4, step 5)

### Implementation for User Story 3

- [X] T015 [US3] Write "## Your people" in `Anniversary.Desktop/README.md`: choose **⋯ ▸ Edit People…** to add people with their name day, birthday and anniversaries (a wedding, for example); **Save** writes them to your data file (FR-007, FR-012)
- [X] T016 [US3] Add "**Sharing with family:**" to "Your people" in `Anniversary.Desktop/README.md`: **⋯ ▸ Data File ▸ Show in Folder** opens the folder with your data file, so you can send the file; they save it somewhere permanent, such as Documents, and choose **⋯ ▸ Data File ▸ Open…** (macOS: **File ▸ Open Data File…**); the app then uses that file and saves changes into it (FR-007, FR-012)
- [X] T017 [US3] Add "**The same people on several computers:**" and the closing line "Each computer remembers which data file it uses." to "Your people" in `Anniversary.Desktop/README.md`: **⋯ ▸ Data File ▸ Save As…** into a cloud-drive folder (OneDrive, iCloud Drive, Dropbox, …), then **Open…** on each other computer; a change made on one computer shows on the others once the cloud has synced it and you switch back to the app; if the file was changed elsewhere while you were editing, the app asks before overwriting it (FR-007)
- [X] T018 [US3] Check scenario 4, step 5, of `specs/009-user-guide/quickstart.md` by hand: following "Your people" adds a person, finds the data file and opens it on another computer as described (FR-007)

**Checkpoint**: Family members can keep and share their people with the guide alone

---

## Phase 6: User Story 4 - Find your data, change settings and remove the app (Priority: P3)

**Goal**: "Files and settings", "Removing it" and "License" cover the data folder, the settings without a menu,
removal and the terms

**Independent Test**: Create `appsettings.json` from the example and start the app: "Next 14 days" and no years; delete
the app's folder: the data folder stays (quickstart scenario 5)

### Implementation for User Story 4

- [X] T019 [US4] Write "## Files and settings" in `Anniversary.Desktop/README.md`: the data folder (`%APPDATA%\Anniversary`, `~/Library/Application Support/Anniversary`, `~/.config/Anniversary`); `Anniversary.Data.json` holds your people (unless you chose another data file) and `usersettings.json` your theme, calendar and data file; for more settings, create `appsettings.json` there, for example `{ "numberOfDaysAhead": 14, "displayYearsOnBirthdays": false }` to look 14 days ahead and hide the years (FR-009)
- [X] T020 [US4] Write "## Removing it" in `Anniversary.Desktop/README.md`: delete the app's folder; your people and settings stay in the data folder, and deleting it too removes everything (FR-010)
- [X] T021 [US4] Write "## License" in `Anniversary.Desktop/README.md`: free and open source under the MIT License, Copyright (c) David Pokluda; the name-day calendars come from other sources under their own licenses, listed in `THIRD-PARTY-NOTICES.md` next to the app; source code and issues at https://github.com/dpokluda/anniversary (FR-011)
- [X] T022 [US4] Check scenario 5 of `specs/009-user-guide/quickstart.md` by hand: the `appsettings.json` example gives "Next 14 days" without years, removing the app keeps the data folder, and "License" names the terms (FR-009, FR-010, FR-011)

**Checkpoint**: All four user stories work; the guide is complete

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, the test run and the review

- [X] T023 [P] Update `README.md` for developers: the opening adds "This README is for developers; the guide for people using the app is `Anniversary.Desktop/README.md`, which comes with every download" (with a link); "Publish" lists `README.md`, the guide for users, next to the executable with `THIRD-PARTY-NOTICES.md`; "Downloading a build" says each archive's `{rid}` folder holds the executable, `README.md` and `THIRD-PARTY-NOTICES.md`; the Projects table adds "the user guide shipped with it (`README.md`)" to `Anniversary.Desktop` (FR-002)
- [X] T024 [P] Update `docs/spec.md` (then `specs/spec.md`): §4 lists `README.md` under `Anniversary.Desktop` as "the user guide, shipped next to the executable (10)"; §10 says that publishing copies the user README and `THIRD-PARTY-NOTICES.md` next to the executable, and the new paragraph **User README** gives its place (shipped as `README.md` in every archive and installer), its audience (the one document for people who use the app; the repository's README is for developers), its form (concise Markdown that also reads well as plain text), its topics, and the rule "Every change users can see updates it." (FR-001, FR-002, FR-003, FR-013)
- [X] T025 [P] Update `docs/plan.md` (then `specs/plan.md`): in "Next (the user's plan, in this order)", item 1 becomes "**user-readme** ✅ – `Anniversary.Desktop/README.md`, the guide for users, shipped as `README.md` next to the executable in every archive (and later in every installer); spec 10."
- [X] T026 [P] Update `docs/copilot/handover.md` (then `specs/copilot/handover.md`): the Publish row adds "each with `README.md` (the user guide) and `THIRD-PARTY-NOTICES.md` next to the executable"; the next steps mark the README for users ✅ (published next to the executable, so it's in every archive; each package step updates its "Starting it" section, and every change users see updates it) (FR-013)
- [X] T027 Run `dotnet test --project Anniversary.Tests`: all 189 tests still pass, since no code changed (SC-006)
- [X] T028 Leave `Anniversary.Desktop/README.md` open in the editor for the maintainer's review (approved: "Looks good.") and check scenario 6 of `specs/009-user-guide/quickstart.md`: the developer README's links lead to the guide (FR-002)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies
- **Foundational (Phase 2)**: depends on Setup; blocks all user stories
- **User Stories (Phases 3–6)**: depend on Foundational; they all write into `Anniversary.Desktop/README.md`, so they
  run one after another
- **Polish (Phase 7)**: depends on all user stories

### User Story Dependencies

- **User Story 1 (P1)**: after Foundational; no dependency on other stories
- **User Story 2 (P2)**: after Foundational; independent of User Story 1
- **User Story 3 (P2)**: after Foundational; describes the Data File commands of
  [007](../007-data-file-sharing/spec.md); independent of the other stories
- **User Story 4 (P3)**: after Foundational; independent of the other stories

### Within Each User Story

- Write the sections where they belong in the guide's order (Starting it, Using it, Your people, Calendars, Files and
  settings, Removing it, License), then do the story's check from quickstart.md
- Use the names users see: the app's menus and buttons and the OS's own warnings, with the macOS names in parentheses

### Parallel Opportunities

- T002 and T003: the guide and the project item are different files
- T023–T026 together: four different documents
- The story phases share one file, so they don't run in parallel

---

## Parallel Example: Foundational and Polish

```bash
# The guide and the project item can be written at the same time:
Task: "Create Anniversary.Desktop/README.md with the title and the opening paragraph"
Task: "Add the README.md item to Anniversary.Desktop/Anniversary.Desktop.csproj"

# The four documents can be updated together:
Task: "Point README.md to the user guide"
Task: "Describe the user README in docs/spec.md §4 and §10"
Task: "Mark user-readme done in docs/plan.md"
Task: "Update the Publish row and the next steps in docs/copilot/handover.md"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: quickstart scenarios 1–3 (the guide in the download, starting the app, plain text)

### Incremental Delivery

1. Setup and Foundational: the guide is published next to the app
2. User Story 1: Starting it
3. User Story 2: Using it and Calendars
4. User Story 3: Your people
5. User Story 4: Files and settings, Removing it, License
6. Polish: the developer README, the spec, plan and handover, the test run and the review

### What happened

The maintainer listed what to do before releases, including "some kind of concise .md documentation (README)" for the
installers and archives. The plan put the user README first, and the maintainer kept that order ("Keep the order"). On
"Done, let's now move on to the next step." the assistant wrote the whole guide (about 80 lines), the project item and
the documentation updates, checked a local macOS build and ran the tests, and left the guide open in the editor for
review. The maintainer answered "Looks good." and committed it as `5868c42`.

---

## Notes

- Tasks marked [P] touch different files and depend on no unfinished task
- The story label maps each task to its user story for traceability
- The guide's texts quoted here are as of `5868c42`; [010](../010-install-packages/spec.md),
  [011](../011-start-at-login/spec.md) and [012](../012-single-settings-file/spec.md) later changed "Starting it",
  "Using it", "Files and settings", "Removing it" and "License", and added "Opening it the first time"
- The documentation paths are today's; at the time they were under `specs/`
