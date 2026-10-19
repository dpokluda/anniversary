# Research: Calendar Chooser

Decisions made while specifying and building the feature on 2026-10-09. Sources: the session of 2026-10-09 (the request,
"Please look harder!" and the assistant's summary of the sources it found), the question about the license line later
that night, the Decisions table and work breakdown of docs/plan.md and docs/spec.md 1, 5.1, 5.5, 5.7 and 7.10 as they
read at the move to GitHub (`67a4092`), and [calendars/README.md](../../calendars/README.md). The session excerpts hold
only the requests and summaries of the answers, so alternatives are listed only where a source names them.

## Calendars as separate files, found like every other file

- **Decision**: Each calendar is its own `Anniversary.Calendar.{Name}.json`. The available calendars are all such files
  next to the executable and in the app-data folder plus the embedded ones, merged by file name with the precedence of
  spec 5.3 (the executable's folder, then app-data, then built in). A user's file with a shipped calendar's name
  replaces it.
- **Rationale**: The maintainer suggested separate JSON files. With the existing lookup order, "a user can add a
  calendar by dropping a file into the app-data folder, or override the Czech one" (spec 5.7), with no new mechanism.
- **Alternatives considered**: none recorded. Before this feature there was one calendar, named by `calendarFile` in
  `appsettings.json` ("Phase 1 ships only the Czech name-day calendar; another calendar can be selected with
  `calendarFile`", docs/plan.md, Data files row).
- **Source**: session 2026-10-09; docs/plan.md, Decisions, "Name-day calendars"; spec 5.7.

## A Nameday Calendar submenu, rebuilt when the menu opens

- **Decision**: **⋯ ▸ Nameday Calendar ▸** lists the calendars as radio items by display name, the active one checked,
  between Edit People… and Theme; macOS has the same list under **View ▸ Nameday Calendar**, above Appearance. The ⋯
  menu discovers the files again and rebuilds both lists each time it opens.
- **Rationale**: "The submenu is rebuilt each time the menu opens, so a calendar file added while the app runs appears
  without a restart" (spec 7.10). The macOS View menu offers the same choices as the ⋯ submenus (spec 7.2).
- **Alternatives considered**: the maintainer's working name, "choose calendar (I am not sure if that's the right
  name)"; the submenu was called Nameday Calendar. The user renamed it **Calendar** later
  ([008](../008-international-days/spec.md)).
- **Source**: session 2026-10-09; spec 7.2 and 7.10; the mocks.

## Remember the choice in `usersettings.json`

- **Decision**: `UserSettings.Calendar` holds the chosen file name. The main window saves it after a choice; at start-up
  `App` passes it to `AnniversaryBook.Load`, which uses it if that calendar is still available, otherwise `calendarFile`
  from `appsettings.json`, otherwise Czech.
- **Rationale**: `usersettings.json` is the file the app writes for choices made in its menus (the theme was already
  there), while the app never writes `appsettings.json` (spec C9). Falling back without a warning means that "removing
  the file falls back to `calendarFile`/Czech without errors" (criterion 11).
- **Alternatives considered**: none recorded besides the hand-edited `calendarFile`, which keeps working as the
  fallback.
- **Source**: docs/plan.md, Decisions ("the choice is remembered in `usersettings.json`"); spec 5.5 and 5.7; criterion
  11.

## Czech stays the default; no automatic choice by OS locale yet

- **Decision**: Without a remembered choice the calendar comes from `calendarFile`, which names the Czech calendar in
  the built-in `appsettings.json` and by default. Each calendar records its `locale`, which is informational for now.
- **Rationale**: v4 used the Czech calendar, and v5 behaves like v4 unless the spec says otherwise. Choosing the
  calendar by OS locale was a phase-1 non-goal, and `locale` is "reserved for automatic choice by OS locale later" (spec
  1, 5.1).
- **Alternatives considered**: choosing by the OS locale, which the maintainer had in mind ("Nameday calendar would be
  per the locale - in our case we use Czech one only I think"); deferred. [006](../006-holiday-calendar-rules/spec.md)
  later made the default follow the OS region.
- **Source**: session 2026-10-09; spec 1 (non-goals) and 5.1.

## Sources for other countries: radoone/nameday-mcp and the Latvian official list

- **Decision**: Eight calendars (Austria, Bulgaria, France, Hungary, Italy, Poland, Russia, Slovakia) come from the
  static per-country JSON files of [radoone/nameday-mcp](https://github.com/radoone/nameday-mcp) (MIT) at commit
  `5d3f301f0796b2fca8048db8c092e9bd09504dfe`; Latvia from the official list of the Latvian State Language Centre (Valsts
  valodas centrs), published on data.gov.lv under CC0 1.0 (the traditional list); Czech from v4, its names unchanged.
- **Rationale**: They are openly licensed, can be pinned and converted by a script, and passed the checks below. The
  radoone data looked plausible on known dates (Slovak 9 October Dionýz, Hungarian Dénes), and its Czech file agrees
  with v4's calendar on 359 of 366 days.
- **Alternatives considered**: the assistant's first answer stopped at licensing ("I was too quick to stop at
  licensing"). When the maintainer asked it to "look harder", it searched Wikipedia's name-day lists per country,
  open-source GitHub datasets and official national lists; one lead was a Linux Mint desklet with per-country name-day
  JSON files. The session doesn't record why the Wikipedia lists and the desklet weren't used. Copyrighted official
  lists (Sweden, Finland) were ruled out.
- **Source**: session 2026-10-09; `calendars/README.md`; `calendars/THIRD-PARTY-NOTICES.md`.

## Generated by a script from pinned sources, never by hand

- **Decision**: `calendars/Build-Calendars.ps1` (PowerShell 7) downloads each source at its pinned version, converts it
  to the calendar format, validates it (valid `MM-dd` keys, no empty entries, at least 360 days, the written file parses
  back) and writes the files; the Czech file is v4's with the metadata added. The generated files are kept, and two runs
  give identical files.
- **Rationale**: Name days are "**never** typed in or made up by hand" (spec 5.7). Generating them from pinned sources
  makes the shipped files reproducible and checkable against those sources; the constitution later made this its
  Principle VI. "To add a country, extend the script with a pinned source plus its license and validation."
- **Alternatives considered**: typing names in by hand; ruled out.
- **Source**: spec 5.7; `calendars/README.md`; session 2026-10-09 ("Reproducible, and the Czech data is identical to
  v4.").

## How the sources were checked

- **Decision**: Compare radoone's Czech file with v4's Czech calendar; spot-check well-known dates (Slovak 07-05 Cyril,
  Metod; Polish 12-24 Adam, Ewa; Hungarian 08-20 István; Austrian 11-11 Martin; Bulgarian 05-06 Георги; Russian 12-19
  Николай; Italian 10-04 Francesco; French 12-06 Nicolas); check that the native letters are intact (no replacement
  characters) in every included file.
- **Rationale**: The agreement with v4 (359 of 366 days; the 7 differences are spelling variants such as Evženie/Evžénie
  or public-holiday labels in v4) shows the source is reliable; the spot checks and the encoding check catch damaged
  data such as that of the Croatian file.
- **Alternatives considered**: none recorded.
- **Source**: `calendars/README.md`, "How the sources were checked"; session 2026-10-09.

## Countries left out

- **Decision**: No calendar for Croatia, Greece, Sweden, Finland, Lithuania, Estonia, Denmark, Germany or Spain.
- **Rationale**: Croatia: the only open source found (radoone `hr.json`) has damaged entries (lost letters such as
  `mucenici`, `Rodenje`, and fragments). Greece: the open source covers only 176 days, and many Greek name days move
  with Easter, which fixed `MM-dd` dates can't represent. Sweden: the official list is the Swedish Academy's copyrighted
  *namnlängd*. Finland: the official lists are copyrighted by the University of Helsinki Almanac Office. The others: no
  source with both a clear license and verifiable quality was found yet.
- **Alternatives considered**: the same countries from those sources; rejected for the reasons above. They "can be added
  later the same way: pinned source + license + validation in `Build-Calendars.ps1`".
- **Source**: `calendars/README.md`, "Not included (and why)"; spec 5.7; docs/plan.md, Decisions.

## Source and license in every file; the notices next to the executable

- **Decision**: Every shipped file carries `source` and `license`. `calendars/THIRD-PARTY-NOTICES.md` holds the MIT
  license of radoone/nameday-mcp word for word and the CC0 dedication of the Latvian list, and `Anniversary.Desktop`
  copies it next to the executable. The eight radoone files say "MIT, Copyright (c) 2024 Slovak Name Days MCP Server
  (see THIRD-PARTY-NOTICES.md)".
- **Rationale**: The notices must ship with the app "because the MIT license requires it" (spec 5.7). Asked later that
  night whether the "Slovak Name Days MCP Server" line in every file was on purpose, the assistant explained that the
  radoone license names that copyright holder although the project has data for many countries, that the notice must be
  kept word for word, and that the Czech and Latvian files have their own lines.
- **Alternatives considered**: adding the repository's name to the field, for example "MIT, Copyright (c) 2024 Slovak
  Name Days MCP Server (radoone/nameday-mcp)", offered by the assistant; the files kept the original text.
- **Source**: spec 5.7 and 10; session 2026-10-09 (the question about the license line);
  `calendars/THIRD-PARTY-NOTICES.md`.

## Built-in calendars embedded under their plain file names

- **Decision**: `Anniversary.Shared` links `..\calendars\Anniversary.Calendar.*.json` as embedded resources with
  `LogicalName="%(Filename)%(Extension)"`. Discovery lists the resources whose names follow the calendar pattern, and
  the lookup finds a built-in calendar by its file name.
- **Rationale**: "All embedded JSON uses `LogicalName="%(Filename)%(Extension)"`, so a built-in default is found by its
  plain file name (e.g. "Anniversary.Calendar.Czech.json"), as 5.3 step 3 needs" (spec 4). The desktop project copies no
  JSON files next to the executable (spec 5.3), so the built-in calendars work with no files there.
- **Alternatives considered**: none recorded.
- **Source**: spec 4 and 5.3; `Anniversary.Shared/Anniversary.Shared.csproj`.

## Verification approach

- **Decision**: `CalendarTests` (10 cases) with real files in temporary folders and the embedded calendars, for
  discovery, precedence, display names, the remembered choice and switching; the generator's own checks and a second run
  for the data; manual checks of the menus with `dotnet run` (criterion 11).
- **Rationale**: The logic is UI-free and can be tested with real files; the menus need the real app.
- **Alternatives considered**: none recorded.
- **Source**: spec 9 and 12; `Anniversary.Tests/CalendarTests.cs`; session 2026-10-09. At the move to GitHub the
  handover (`specs/copilot/handover-linux.md`, renamed by [005](../005-ci-pipeline/spec.md), now
  [docs/copilot/handover.md](../../docs/copilot/handover.md)) still listed View ▸ Nameday Calendar among the macOS
  menu-bar items not yet checked.
