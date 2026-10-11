# Calendars

`Anniversary.Calendar.*.json` files for Anniversary v5 (format: [spec 5.1](../specs/spec.md), choosing calendars: spec 5.7 and 7.10).
**Generated. Don't edit by hand.** Re-create them with:

```powershell
pwsh -File Build-Calendars.ps1
```

The script downloads each name-day source at a pinned version, converts it, validates it (valid `MM-dd` or rule keys,
no empty entries, a full year for name days) and checks that each written file parses back. Running it twice gives
identical files, on any OS (keys are sorted ordinally).

| File | Display name | Days | Source | License |
|---|---|---|---|---|
| `Anniversary.Calendar.US.json` | United States | 16 dates + 14 rules | Federal holidays and observances from U.S. law, plus traditional dates (see below) | MIT (part of Anniversary) |
| `Anniversary.Calendar.Czech.json` | Czechia (Česko) | 366 + 2 rules | Anniversary v4 (values unchanged; metadata added), plus Velký pátek (`easter-2`) and Velikonoční pondělí (`easter+1`) from zákon č. 245/2000 Sb., § 2 | MIT (part of Anniversary) |
| `Anniversary.Calendar.Austrian.json` | Austria (Österreich) | 366 | [radoone/nameday-mcp](https://github.com/radoone/nameday-mcp) `at.json` @ `5d3f301` | MIT |
| `Anniversary.Calendar.Bulgarian.json` | Bulgaria (България) | 366 | radoone/nameday-mcp `bg.json` | MIT |
| `Anniversary.Calendar.French.json` | France | 365 | radoone/nameday-mcp `fr.json` | MIT |
| `Anniversary.Calendar.Hungarian.json` | Hungary (Magyarország) | 364 | radoone/nameday-mcp `hu.json` | MIT |
| `Anniversary.Calendar.Italian.json` | Italy (Italia) | 365 | radoone/nameday-mcp `it.json` | MIT |
| `Anniversary.Calendar.Polish.json` | Poland (Polska) | 366 | radoone/nameday-mcp `pl.json` | MIT |
| `Anniversary.Calendar.Russian.json` | Russia (Россия) | 366 | radoone/nameday-mcp `ru.json` | MIT |
| `Anniversary.Calendar.Slovak.json` | Slovakia (Slovensko) | 362 | radoone/nameday-mcp `sk.json` (public holidays without names omitted) | MIT |
| `Anniversary.Calendar.Latvian.json` | Latvia (Latvija) | 366 | Official list of the Latvian State Language Centre ([data.gov.lv](https://data.gov.lv/dati/eng/dataset/latviesu-tradicionalais-un-paplasinatais-kalendarvardu-saraksts), traditional list) | CC0-1.0 |

Days missing from a file (e.g. 29 February in some calendars) simply show no names. Each file's `locale` also makes it
the default calendar for that OS region (e.g. `cs-CZ`: Czechia); everywhere else the US calendar is the default
(spec 5.7).

## The US holiday calendar

Holidays are defined by law or long tradition, so there is no list to download. The entries are written in
`Build-Calendars.ps1`, each with its source:

- **Federal holidays** ([5 U.S.C. 6103(a)](https://www.law.cornell.edu/uscode/text/5/6103)): New Year's Day, Martin
  Luther King Jr. Day, Presidents' Day (Washington's Birthday in the law), Memorial Day, Juneteenth, Independence Day,
  Labor Day, Columbus Day, Veterans Day, Thanksgiving and Christmas Day. Floating ones use rule keys, e.g. `11-Thu-4`.
- **Observances designated by law:** Mother's Day (36 U.S.C. 117), Father's Day (36 U.S.C. 109), Flag Day
  (36 U.S.C. 110), Patriot Day (36 U.S.C. 144), and the start and end of daylight saving time (15 U.S.C. 260a).
- **Traditional dates:** Groundhog Day, Valentine's Day, Mardi Gras, St. Patrick's Day, April Fools' Day, Good Friday,
  Easter, Earth Day, Cinco de Mayo, Halloween, Black Friday, Christmas Eve and New Year's Eve.

The tests check the federal holidays of 2025–2027 against [OPM's schedules](https://www.opm.gov/policy-data-oversight/pay-leave/federal-holidays/).
Holidays are shown on their legal date. OPM lists the weekday off instead when a holiday falls on a weekend (e.g.
Friday 3 July 2026), and those observed days aren't listed.

## How the sources were checked

- **Is radoone reliable?** Its Czech file agrees with the Anniversary v4 Czech calendar on 359 of 366 days. The 7
  differences are spelling variants (Evženie/Evžénie, Magdeléna/Magdaléna) or public-holiday labels in v4 (Tři králové,
  Památka zesnulých, …).
- **Spot checks** on well-known dates: Slovak 07-05 Cyril, Metod; Polish 12-24 Adam, Ewa; Hungarian 08-20 István;
  Austrian 11-11 Martin; Bulgarian 05-06 Георги; Russian 12-19 Николай; Italian 10-04 Francesco; French 12-06 Nicolas.
- **Character encoding:** native letters are intact in all included files (no replacement characters).

## Not included (and why)

| Country | Reason |
|---|---|
| Croatia | The only open source found (radoone `hr.json`) has damaged entries: lost letters (`mucenici`, `Rodenje`) and fragments like `Christmas (Božić], hence …`. |
| Greece | The open source covers only 176 days, and many Greek name days move with Orthodox Easter, which the rule keys don't support yet (`easter` is Western Easter). |
| Sweden | The official list is the Swedish Academy's copyrighted *namnlängd*. |
| Finland | The official lists are copyrighted by the University of Helsinki Almanac Office. |
| Lithuania, Estonia, Denmark, Germany, Spain | No source with both a clear license and verifiable quality was found yet. They can be added later the same way: pinned source + license + validation in `Build-Calendars.ps1`. |
