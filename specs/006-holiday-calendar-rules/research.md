# Research: Holiday Calendar Rules

Decisions made while planning and building the feature on 2026-10-10. Sources: the session of that morning in which
the maintainer first raised US public holidays and agreed to the plan (b414da5f), the afternoon session in which the
feature was built (7b9ee9b8), the plan's idea section as of `67a4092`, the Decisions table and the "Holiday and
observance calendars" section in [docs/plan.md](../../docs/plan.md), the handover's decisions in
[docs/copilot/handover.md](../../docs/copilot/handover.md), [docs/spec.md](../../docs/spec.md) 5.1, 5.7, 6.3 and C19,
[calendars/README.md](../../calendars/README.md), and `git diff 1409624 5bb1afe`. The session excerpt has no answer to
the request itself ("Can we discuss that?"), only the later reports, so alternatives are listed only where a source
names them.

## Rule keys next to `MM-dd`

- **Decision**: A calendar file may key an entry by a rule instead of an `MM-dd` date; `AnniversaryBook` works out the
  rule's date for the years it shows. The rule format came first, then the US calendar on top of it.
- **Rationale**: Calendar keys were fixed `MM-dd` dates, so a holiday that moves couldn't be stored. Of the US federal
  holidays, New Year's Day, Juneteenth, Independence Day, Veterans Day and Christmas work as fixed dates, but Martin
  Luther King Jr. Day, Presidents' Day, Memorial Day, Labor Day, Columbus Day and Thanksgiving don't, and neither do
  days tied to Easter, such as Easter Monday and Good Friday in the Czech public holidays.
- **Alternatives considered**: fixed dates only, which would have left out most federal holidays. None other recorded.
- **Source**: session 2026-10-10 (b414da5f: "the problem might be that those are not on the same day, right?", the
  assistant's proposal, and the maintainer's "Got it, I think it is a good plan. Save this for now."); docs/plan.md
  idea section as of `67a4092` ("calendar-rules", then "us-holidays").

## The key forms: `MM-Ddd-n`, `MM-Ddd-last`, `easter` and offsets

- **Decision**: Three forms, each optionally followed by `+n` or `-n` days (n from 1 to 99, no leading zero):
  `MM-Ddd-n` (the n-th weekday of the month, n from 1 to 4), `MM-Ddd-last` (the last weekday of the month) and
  `easter`. Keys are case-insensitive, and one regular expression recognises them.
- **Rationale**: A key reads month, weekday, which one. The n-th weekday is the first matching weekday on or after the
  1st plus `7 × (n − 1)` days; every month has a fourth of each weekday but not always a fifth, so `last` replaces a
  fifth. Offsets express days defined relative to another day: Easter Monday (`easter+1`), Good Friday
  (`easter-2`), Mardi Gras (`easter-47`) and Black Friday (`11-Thu-4+1`).
- **Alternatives considered**: after the maintainer asked what `11-Thu-4` and `05-Mon-last` mean, the assistant offered
  a more readable form such as `11-4th-Thu` or `05-last-Mon`; the keys stayed as they were. A "5th" weekday was not
  used, for the reason above.
- **Source**: session 2026-10-10 (7b9ee9b8, the explanation of `11-Thu-4` and `05-Mon-last`); spec 5.1 and 6.3.

## Work dates out when needed, for three years

- **Decision**: Rule dates aren't stored. `CalendarNamesOn(day)` checks every rule entry for the day's year and the
  years before and after, each time a day is shown or searched.
- **Rationale**: An offset of up to 99 days can move a date across New Year (`12-Mon-last+7` falls on 4 January
  2027), and Next N days and Find cross New Year too, so a rule must be worked out for whichever year the day is in.
- **Alternatives considered**: the plan of the morning said "resolved by `AnniversaryBook` for the displayed year (so
  "Next N days" works across a year boundary)"; checking the day's own year and its neighbours is how that was built.
  None other recorded.
- **Source**: docs/plan.md idea section as of `67a4092`; spec 6.3; `CalendarNamesOn` and `TryResolveRule` in
  `Anniversary.Shared/AnniversaryBook.cs`.

## Easter by the anonymous Gregorian algorithm

- **Decision**: `easter` is Western Easter Sunday, computed with the anonymous Gregorian algorithm (Meeus, Jones and
  Butcher) for any year `DateOnly` holds.
- **Rationale**: It gives a Sunday from 22 March to 25 April in every year from 1 to 9999, which a test checks, and
  the dates matched an independent implementation.
- **Alternatives considered**: Orthodox Easter (for example an `orthodox+1` key for Bulgarian, Russian and Greek
  holidays and name days) was recorded as a further idea, not built. Greek name days stay excluded, now because they
  move with Orthodox Easter, which the rule keys don't support yet.
- **Source**: spec 6.3; session 2026-10-10 ("Easter dates match an independent implementation"); docs/plan.md further
  ideas; calendars/README.md ("Not included").

## Several names on one day

- **Decision**: A day shows its `MM-dd` entry first, then each rule entry that falls on it, in file order, joined
  with ", ".
- **Rationale**: Not recorded beyond the result: one text per day on the "Today is" line, for example "Vendula,
  Velikonoční pondělí" on Easter Monday 2026 in the Czech calendar, which Next N days splits on "," like any calendar
  text.
- **Alternatives considered**: none recorded.
- **Source**: spec 6.3; the handover ("Names on the same day are joined with ", "").

## Report keys the app doesn't understand

- **Decision**: When a calendar loads, a key that is neither a valid `MM-dd` date (one that exists in a leap year,
  checked as people's namedays are) nor a rule is ignored, and one warning lists such keys: "Unknown dates in {name}:
  …", at most five, then "and {n} more".
- **Rationale**: Loading never throws or stops (spec 5.4), so the rest of the calendar still works. Before this feature
  every key was kept, and an entry with a wrong key was silently never shown.
- **Alternatives considered**: none recorded.
- **Source**: spec 5.1 and 5.4; the session report ("A key the app doesn't understand produces a warning");
  `AddCalendarEntries`.

## The US calendar: federal holidays and well-known observances

- **Decision**: `Anniversary.Calendar.US.json` ("United States", `en-US`) has the 11 federal holidays of
  5 U.S.C. 6103(a), 6 observances designated by law (Mother's Day, 36 U.S.C. 117; Father's Day, 36 U.S.C. 109; Flag
  Day, 36 U.S.C. 110; Patriot Day, 36 U.S.C. 144; the start and end of daylight saving time, 15 U.S.C. 260a) and 13
  traditional dates: 30 entries. Names are the common ones (Presidents' Day for the law's Washington's Birthday).
- **Rationale**: Holidays are defined by law or long tradition, so there is no list to download; the entries are
  written in `Build-Calendars.ps1`, each with its source in a comment, and the file records its `source` and
  `license`.
- **Alternatives considered**: the plan of the morning listed only the federal holidays; the calendar as built adds
  well-known observances. No discussion of that is recorded.
- **Source**: docs/plan.md, Decisions ("Holiday calendars") and the us-holidays item; calendars/README.md ("The US
  holiday calendar"); session 2026-10-10 ("11 federal holidays plus 19 observances, 30 entries in total").

## Legal dates, checked against OPM's schedules

- **Decision**: Holidays are listed on their legal date. The tests check the federal holidays of 2025–2027 against
  OPM's federal holiday schedules, with fixed-date holidays on their legal date.
- **Rationale**: When a holiday falls on a weekend, OPM lists the weekday off instead (for example Friday 3 July 2026);
  the calendar shows the holiday itself. The U.S. Code sections and the Czech law cited were also checked online.
- **Alternatives considered**: showing observed days off as well (the Friday before or the Monday after, and
  31 December of the year before for New Year's Day on a Saturday) was recorded as a further idea, not built.
- **Source**: `UsCalendarHasTheFederalHolidays` in `Anniversary.Tests/CalendarRuleTests.cs`; spec 5.7; docs/plan.md
  further ideas; session 2026-10-10.

## Czech Easter holidays in the Czech calendar itself

- **Decision**: The build script adds `easter-2` Velký pátek and `easter+1` Velikonoční pondělí (zákon č. 245/2000
  Sb., § 2) to the Czech calendar and names the law in its `source`; the v4 values stay unchanged. The file is its own
  source, so running the script again changes nothing.
- **Rationale**: These are the Czech public holidays that move with Easter; the v4 calendar already shows several
  fixed-date public holidays, such as Svátek práce on 1 May.
- **Alternatives considered**: an overlay calendar, a holiday calendar shown together with the name-day calendar
  (Czech name days plus Czech public holidays), was an idea not agreed. After this feature the idea's example became
  Czech name days plus US holidays.
- **Source**: spec 5.7; docs/plan.md further ideas as of `67a4092` and `5bb1afe`; `calendars/Build-Calendars.ps1`.

## The default calendar follows the OS region, with US as the fallback

- **Decision**: With no calendar chosen in the menu and no `calendarFile` configured, `Load` uses the first calendar,
  by display name, whose `locale` has the region of `CultureInfo.CurrentCulture` (`cs-CZ` and `en-CZ` → `CZ` → Czech);
  otherwise the US calendar. A neutral or invariant culture has no region.
- **Rationale**: "A sensible default for everyone" (spec C19): the OS sets that culture (the Regional format on Windows,
  Language & Region on macOS, `LANG`/`LC_ALL` on Linux), so new users get the calendar for their country where one
  ships, and the US calendar everywhere else. Only the region counts, so the language doesn't matter. On Windows the
  Regional format decides, not "Country or region": a Czech user with an English (United States) format gets the US
  calendar and chooses Czech once.
- **Alternatives considered**: the US calendar for everyone who hasn't chosen or configured one, as first built in this
  feature; the maintainer asked whether that was a good idea, and the region rule replaced it. Czech for everyone, as
  in v4 and until this feature. Choosing a calendar by the OS locale had been a phase-1 non-goal in the spec, with
  `locale` reserved for it (003); this feature removed the non-goal.
- **Source**: session 2026-10-10 ("If there is no config file, we are currently selecting the US one, correct? Do you
  think it is the good idea?" and the answer); spec 1 and 5.7 as of `1409624` and `5bb1afe`; spec C19.

## Keep `calendarFile`, but not in the built-in configuration

- **Decision**: The order is: the calendar chosen in the menu (`UserSettings.Calendar`), then `calendarFile` from the
  user's own `appsettings.json`, then the OS region, then the US calendar. The built-in `appsettings.json` no longer
  has `calendarFile`, and `AppConfig.CalendarFile` is `null` unless a file sets it.
- **Rationale**: A copied v4 `appsettings.json` names Czech, so a v4 user moving over keeps Czech; a fresh install has
  no `calendarFile` and so follows the region.
- **Alternatives considered**: none recorded. [012](../012-single-settings-file/spec.md) later stopped reading
  `appsettings.json`, which removed the `calendarFile` step.
- **Source**: session 2026-10-10 (the four steps in the answer); spec 5.1 and 5.7; `README.md` ("Moving from v4").

## Keep the menu name Nameday Calendar

- **Decision**: The menu stays **Nameday Calendar** although the US calendar lists holidays.
- **Rationale**: Recorded in the spec as a user decision; no reason is recorded.
- **Alternatives considered**: renaming it; the maintainer later proposed **Calendar**, which
  [008](../008-international-days/spec.md) did.
- **Source**: spec 7.10 as of `5bb1afe` ("(user decision)"); the handover; session 2026-10-10 ("the menu is still
  "Nameday Calendar"").

## Build script: rule keys, holiday calendars and ordinal order

- **Decision**: `Build-Calendars.ps1` accepts rule keys with the same pattern as the app (`Test-RuleKey`), skips the
  full-year check for holiday calendars (`-Holidays`), and sorts keys ordinally.
- **Rationale**: The US calendar has 30 entries, not the 360 or more days the check asks of a name-day calendar.
  Ordinal order makes the files the same whichever OS runs the script; run on macOS (PowerShell 7.6 from Homebrew), it
  reproduced the committed files exactly.
- **Alternatives considered**: the previous `Sort-Object` order, replaced by the ordinal one. None other recorded.
- **Source**: `git diff 1409624 5bb1afe -- calendars/Build-Calendars.ps1`; calendars/README.md; the handover.

## Verification approach

- **Decision**: Tests with real files and the shipped calendars (`CalendarRuleTests`, 75 cases; 10 new cases in
  `CalendarTests`, with the culture set by `TestFiles.LoadInCulture`), the data checked against its sources (OPM's
  schedules, the U.S. Code, the Czech law, an independent Easter implementation), a headless run of the real window
  (Thanksgiving, the calendar menu and the switch to Czech), the real main window started with
  `LC_ALL=cs_CZ.UTF-8` (Czech, "Vendula, Velikonoční pondělí" on 6 April 2026), and the build script run on macOS.
- **Rationale**: The tests set the culture explicitly, so they pass on any machine; what depends on the real OS
  setting is checked by hand.
- **Alternatives considered**: none recorded.
- **Source**: session 2026-10-10 (both reports); the handover's lessons (headless renders, clicking buttons and macOS
  menu items).
