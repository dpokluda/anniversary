# Research: User Guide

Decisions made while planning and building the feature on 2026-10-10. Sources: the session of 2026-10-10 (the
maintainer's list of what to do before releases, "Keep the order", the assistant's report on the guide and "Looks
good."), [docs/spec.md](../../docs/spec.md) §10, [docs/plan.md](../../docs/plan.md) and
[docs/copilot/handover.md](../../docs/copilot/handover.md) as `5868c42` left them, and `git show 5868c42`. The session
excerpt has only the request, the report and the approval, so alternatives are listed only where a source names them.

## A separate guide for users, published as `README.md`

- **Decision**: `Anniversary.Desktop/README.md` is the user guide, "the one document for people who use the app", and
  every download carries it as `README.md`. The repository's `README.md` stays the developers' document: its opening
  now says so and links to the guide, which "comes with every download".
- **Rationale**: The maintainer asked for "some kind of concise .md documentation (README)" in the installers and
  archives, "for consumers". The repository's README covered building, testing and publishing, the CI downloads,
  running the bare executables on macOS and Linux, the data files and the projects.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; spec 10 ("User README"); `README.md`.

## Publish it next to the executable with a project item

- **Decision**: `Anniversary.Desktop/Anniversary.Desktop.csproj` gets a `None Update="README.md"` item with
  `CopyToOutputDirectory="PreserveNewest"`, `CopyToPublishDirectory="PreserveNewest"` and
  `ExcludeFromSingleFile="true"`, in the same item group as the existing `THIRD-PARTY-NOTICES.md` item and with the
  same copy metadata. The guide is copied into the build and publish output and kept out of the single-file bundle, so
  it is a separate file next to `Anniversary` (`Anniversary.exe`).
- **Rationale**: The project already shipped the calendars' license notices next to the executable this way. CI
  archives each RID's whole publish folder, so every download gets the guide without a change to
  `.github/workflows/ci.yml`. Spec 10 names the installers too: "shipped as `README.md` in every archive and
  installer".
- **Alternatives considered**: none recorded.
- **Source**: `git show 5868c42`; session 2026-10-10 ("The project file now publishes it as `README.md` next to the
  app"); [contracts/publish-folder.md](contracts/publish-folder.md).

## Concise Markdown that also reads well as plain text

- **Decision**: About 80 lines (81 at the time): a title and an opening paragraph, then seven sections with short
  paragraphs and bullet lists, bold for the names users see, backticks for files, folders and commands, and one JSON
  example. No tables, images or links; the web address is written out.
- **Rationale**: The maintainer asked for "concise .md documentation"; spec 10 adds that it "also reads well as plain
  text".
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; spec 10; `git show 5868c42:Anniversary.Desktop/README.md`.

## What the guide covers, and in which order

- **Decision**: What the app does (the opening paragraph), then "Starting it", "Using it", "Your people", "Calendars",
  "Files and settings", "Removing it" and "License"
  ([contracts/user-guide-outline.md](contracts/user-guide-outline.md)).
- **Rationale**: The request was for consumers "to understand what and how regarding the app". Spec 10 lists the
  topics: "what the app does, starting it on each OS (including the first-launch steps for unsigned builds), using it
  (the window, commands and shortcuts), the people and sharing them (7.11), the calendars, the data folder and
  `appsettings.json`, removing the app, and the license".
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; spec 10.

## Write it before the packages, and let each package step update "Starting it"

- **Decision**: The guide is written first, for the downloads that exist (the CI run's `.zip` and `.tar.gz`
  archives), and each package step updates its "Starting it" section, "starting with the Mac `.app`".
- **Rationale**: The plan recorded the remaining work as "the user's plan, in this order": user README, packages,
  Start at Login, releases. The maintainer said "Keep the order", and the assistant confirmed that the plan's order
  already matched: the user README next.
- **Alternatives considered**: the maintainer's own list (2026-10-10) named the packages and "add ability to run the
  app after login" before the README; the plan put the README first, and that order was kept. What exactly "Keep the
  order" answered isn't in the session excerpt.
- **Source**: session 2026-10-10; docs/plan.md ("Next (the user's plan, in this order)"); the handover's next steps.

## Explain the first start of an unsigned app

- **Decision**: "Starting it" tells Windows users to click **More info**, then **Run anyway** when Windows says "Windows
  protected your PC", and Mac users to close the warning, click **Open Anyway** in **System Settings ▸ Privacy &
  Security** and start the app again, or to run `xattr -d com.apple.quarantine Anniversary` once in Terminal.
- **Rationale**: Unsigned downloads get a SmartScreen warning on Windows and are blocked by Gatekeeper on macOS, and
  signing isn't decided; the plan said that "unsigned builds need the first-launch steps the user README will explain".
- **Alternatives considered**: signing, which needs a Windows code-signing certificate and a paid Apple Developer
  account (Developer ID signing and notarization); not decided.
- **Source**: docs/plan.md (the "Next" list and "Idea: installers and start at login", "Signing"); the handover's next
  steps. Later, [010](../010-install-packages/spec.md) replaced these steps with a section "Opening it the first time"
  that names the buttons as each OS shows them, after the maintainer had tried the packaged Mac app.

## Name the Mac downloads and the Terminal window

- **Decision**: The macOS bullet names `osx-arm64` for Apple Silicon Macs (M1 and newer) and `osx-x64` for Intel Macs,
  and says that a Terminal window opens with the app and must stay open while it is used.
- **Rationale**: The macOS download is a bare executable, not an `.app`; in the assistant's words, "On macOS a Terminal
  window also opens next to the app until there's a proper `.app`."
- **Alternatives considered**: none recorded; the `.app` in a `.dmg` came with [010](../010-install-packages/spec.md).
- **Source**: session 2026-10-10; `README.md` ("Running on macOS", at the time).

## Settings: the two file-only options through `appsettings.json`

- **Decision**: "Files and settings" says what `Anniversary.Data.json` and `usersettings.json` hold, and that more
  settings go into an `appsettings.json` the user creates in the data folder, with the example
  `{ "numberOfDaysAhead": 14, "displayYearsOnBirthdays": false }` (look 14 days ahead and hide the years). It doesn't
  mention `calendarFile` or `dataFile`.
- **Rationale**: Spec 10 asks for "the data folder and `appsettings.json`". At the time, v4's `appsettings.json` was
  the only place for these two options, and the app never writes it.
- **Alternatives considered**: none recorded.
- **Source**: spec 10; `git show 5868c42:Anniversary.Desktop/README.md`. Later, on 2026-10-11, the maintainer put the
  two options into `usersettings.json`, where the app ignored them; that confusion between the two files led to
  [012](../012-single-settings-file/spec.md), which moved all settings into `usersettings.json` and rewrote this part
  of the guide.

## Keep the guide in step with the app

- **Decision**: "Every change users can see updates it" (spec 10); the handover adds that each package step updates
  "Starting it".
- **Rationale**: None given beyond the rule itself; the guide is the one document users get with the app.
- **Alternatives considered**: none recorded.
- **Source**: spec 10; the handover's next steps. The rule later became part of the constitution's Principle VII
  (2026-10-18).

## Verification approach

- **Decision**: No new automated tests. A local macOS build was checked: its folder held `Anniversary`, `README.md` and
  `THIRD-PARTY-NOTICES.md`. The 189 tests still passed, and the maintainer reviewed the guide in the editor and approved
  it ("Looks good.").
- **Rationale**: No logic, file format or settings changed. Whether the guide is in a download is a fact of the publish
  output, and whether it is right is checked by reading it next to the app ([quickstart.md](quickstart.md)).
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10.
