# Research: International Days

Decisions made while planning and building the feature on 2026-10-10. Sources: the session of 2026-10-10 (the idea in
the morning; the example, the questions the maintainer answered and the assistant's reports in the evening), the
Decisions table and "Holiday and observance calendars" in [docs/plan.md](../../docs/plan.md),
[docs/spec.md](../../docs/spec.md) 5.7 and 7.10, [calendars/README.md](../../calendars/README.md), and
`git diff bb8676b fb5312e`. Alternatives are listed only where a source names them.

## An International Days calendar for places without name days

- **Decision**: Add an "International Days" calendar: well-known days from the UN's official list of international
  days, plus a few fun days kept by tradition, compiled for the app.
- **Rationale**: The maintainer heard on the radio that it was International Beer Day and asked whether such days could
  serve countries without name days. The UN list is official and sets each day by a resolution, so every entry can
  name its source.
- **Alternatives considered**: copying the popular "national day" sites, which are commercial and copyrighted (not
  used). The same answer listed other ideas (overlay calendars, saints' days, Greek name days, Swedish and Finnish name
  days, astronomical days); they were saved as ideas, not agreed. The assistant suggested adding the rule format first,
  then a US calendar and an International Days calendar on top of it, and the maintainer agreed ("Got it, I think it
  is a good plan. Save this for now.").
- **Source**: session 2026-10-10 (morning); docs/plan.md, "Holiday and observance calendars".

## One calendar at a time

- **Decision**: International Days is a calendar of its own, chosen in the menu instead of a country's calendar.
- **Rationale**: The app shows one calendar at a time; the assistant's example pointed this out ("you'd pick it
  instead of Czech or US").
- **Alternatives considered**: overlay calendars, a holiday calendar shown together with a name-day calendar; an idea in
  docs/plan.md, not agreed.
- **Source**: session 2026-10-10 (the example); docs/plan.md, "Further ideas, not agreed yet".

## A selection of well-known days, not the whole UN list

- **Decision**: About 50 well-known UN days plus the fun days, with the list shown to the maintainer before building.
- **Rationale**: The UN list (read on 10 October 2026) has 239 days on 187 dates, two or more on 41 dates, up to 6 on
  one day, and 33 names longer than 60 characters; the longest is "International Day of Commemoration and Dignity of the
  Victims of the Crime of Genocide and of the Prevention of this Crime". That is too much text for "Today is" in a small
  window.
- **Alternatives considered**: all 239 UN days plus the fun days (offered; the maintainer chose the selection).
- **Source**: session 2026-10-10 (user decision); docs/plan.md ("The full UN list (239 days on 187 dates, many with
  long names) was too much for the small window").

## The approved list

- **Decision**: 56 UN days and 5 fun days: Pi Day (14 March), Star Wars Day (4 May), World Emoji Day (17 July), Talk
  Like a Pirate Day (19 September) and International Beer Day (the first Friday of August); the full list is in
  [contracts/international-calendar-file.md](contracts/international-calendar-file.md). Names are the UN's, two of them
  shortened: "International Holocaust Remembrance Day" (officially the International Day of Commemoration in Memory of
  the Victims of the Holocaust) and "International Volunteer Day" (officially International Volunteer Day for Economic
  and Social Development).
- **Rationale**: The maintainer approved the proposed list ("Looks good, go ahead"). International Day of Mathematics
  (14 March), a UNESCO day, isn't on the UN list, so it was dropped from the draft; Pi Day is on that date.
- **Alternatives considered**: "Make it shorter: drop the less familiar ones" (offered with the list; not chosen). The
  UN list's weeks (for example World Space Week) and Vesak (a full-moon day) aren't included.
- **Source**: session 2026-10-10 (user decision); calendars/README.md.

## Copy each day from the UN list, with its source

- **Decision**: The script's `$international` table was generated from the parsed UN data, so every name, date and
  resolution is copied rather than retyped. Each line's comment gives the resolution, with the UN body where it isn't
  the General Assembly (for example `UNESCO: 30 C/Resolution 29` or `WHO: WHA/A.2/Res.35`), or "UN list" where the list
  gives none (for example International Women's Day); the fun days say "tradition". The file's `source` names the
  list's address and "as of 10 October 2026". Every date was checked against the UN list.
- **Rationale**: Like the US holidays (006), observance days have no file to download, so the script is where each
  entry and its source are written (spec 5.7). The UN page is awkward to parse: a first quick parse paired some names
  with the wrong dates, so the data came from the page's markup, entry by entry (date, name, resolution).
- **Alternatives considered**: none recorded, apart from choosing the "UN list" fallback after counting the picks that
  have no resolution number.
- **Source**: session 2026-10-10; `calendars/Build-Calendars.ps1`; calendars/README.md.

## Days that move: rule keys

- **Decision**: International Beer Day is `08-Fri-1` (the first Friday of August); World Migratory Bird Day is
  `05-Sat-2` and `10-Sat-2` (the second Saturday of May and of October).
- **Rationale**: These days move from year to year, and the rule keys of [006](../006-holiday-calendar-rules/spec.md)
  express them. When the idea came up, the assistant noted that "International Beer Day itself floats (the first Friday
  of August), so it needs the rules too".
- **Alternatives considered**: none; fixed `MM-dd` keys can't express them.
- **Source**: session 2026-10-10; docs/spec.md 5.1.

## No locale

- **Decision**: The calendar has no `locale`. `Write-Calendar` writes the `locale` line only when a locale is given, so
  the file has no such key.
- **Rationale**: It isn't tied to a country, so the OS region never picks it as the starting calendar; users choose it
  in the menu.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; docs/spec.md 5.7; calendars/README.md ("The calendar has no `locale`, so it's never
  the default for an OS region; choose it in the menu.").

## Rename the menu to "Calendar"

- **Decision**: **Nameday Calendar ▸** becomes **Calendar ▸**, in the ⋯ menu and in the macOS View menu. The main
  window's **Namedays** box keeps its name.
- **Rationale**: Two of the twelve calendars, US and International Days, aren't name days, so the old name was
  misleading, and "Calendar ▸" matches "Theme ▸" next to it. The Namedays box shows the people's name days, not the
  calendar.
- **Alternatives considered**: keeping "Nameday Calendar", as decided with the US calendar ("The menu keeps the name
  *Nameday Calendar* although the US calendar lists holidays (user decision)"); the maintainer raised the rename
  himself. Asked "Yes, both", "Only the rename" or "Only the separator", he chose both (user decision).
- **Source**: session 2026-10-10; docs/plan.md, Decisions ("renamed from Nameday Calendar to **Calendar** (user
  decision)"); docs/spec.md 7.10 and C15.

## International Days last, after a separator

- **Decision**: `AnniversaryBook.Calendars` lists the calendars with a `locale` first, by display name, then those
  without one, by display name; both menus put a separator between the two groups.
- **Rationale**: Most people look for their country, and the starting calendar is picked by country, so the countries
  come first. Grouping by `locale` instead of singling out the file also places a calendar a user adds without a locale
  (or a v4-style file without metadata) after the separator.
- **Alternatives considered**: International Days first (the maintainer asked "first or last"); special-casing the
  International Days file (the assistant proposed the `locale` grouping instead); only one of the two changes.
- **Source**: session 2026-10-10 (user decision: "Yes, both"); docs/spec.md 5.7 and 7.10.

## A plain variable for the separator

- **Decision**: `UpdateCalendarMenus` remembers whether the previous calendar had a locale (`previousHasLocale`) and
  adds the separators before the first calendar without one.
- **Rationale**: The first version "stashes a flag in `Tag`", which was harder to read than it needed to be.
- **Alternatives considered**: that first version, replaced before the commit.
- **Source**: session 2026-10-10; `Anniversary.Shared/Views/MainWindow.axaml.cs`.

## Verification approach

- **Decision**: Tests on the real shipped calendar (`InternationalCalendarHasTheseDays`, 8 days) and on the real
  discovery (`CalendarTests`: count and order); running the calendar build script to see that only the new file
  appears; a headless run of the real menus on the Mac (Austria … United States, the separator, International Days);
  manual checks with [quickstart.md](quickstart.md). The calendar-menu mock in `docs/screens/` wasn't redrawn; spec 7.10
  says it still shows *Nameday Calendar*.
- **Rationale**: The sample days cover each kind of entry: two names on one date, the second Saturday of May and of
  October, the first Friday of August in two years, and a day without an entry. The menus can only be seen in a running
  app.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10; docs/spec.md 7.10 and 9.
