# Contract: `calendars/Build-Calendars.ps1`

The generator of the shipped calendars, as written on 2026-10-09 and committed at the move to GitHub (`67a4092`). Output
format: [calendar-file.md](calendar-file.md); sources, checks and exclusions:
[calendars/README.md](../../../calendars/README.md); system spec: [docs/spec.md](../../../docs/spec.md) 5.7.

## Invocation

```powershell
pwsh -File calendars/Build-Calendars.ps1 [-OutputFolder <folder>] [-CzechSource <file>]
```

- Needs PowerShell 7 (`#Requires -Version 7.0`) and internet access. Building the app doesn't need it: the generated
  files are kept with the code.
- `-OutputFolder`: where the files are written; default: the script's folder (`calendars/`).
- `-CzechSource`: v4's Czech calendar. At the move to GitHub the default became the repository's own
  `calendars/Anniversary.Calendar.Czech.json` ("the generated Czech file keeps its values unchanged, so it can be its
  own source"); before, in the old repository, the script read v4's copy.
- Stops at the first error (`$ErrorActionPreference = 'Stop'`), including a failed download.

## Inputs

| Calendars | Input | Pinned by |
|---|---|---|
| Czech | the `calendar` object of `-CzechSource` | v4's names, unchanged |
| Austrian, Bulgarian, French, Hungarian, Italian, Polish, Russian, Slovak | `https://raw.githubusercontent.com/radoone/nameday-mcp/{commit}/src/data/{code}.json` for the codes `at`, `bg`, `fr`, `hu`, `it`, `pl`, `ru`, `sk` | the commit `5d3f301f0796b2fca8048db8c092e9bd09504dfe` |
| Latvian | the CSV of the data.gov.lv dataset "Latviešu tradicionālais un paplašinātais kalendārvārdu saraksts" (traditional list) | the resource's download URL in the script |

## Conversion

| Source format | Rule |
|---|---|
| radoone: `{ "MM-dd": [ "Name", … ] }` | the names trimmed, empty ones dropped, the rest joined with ", "; a day with no names is left out |
| Latvian CSV: columns `datums` (`dd.MM.`) and `vardadienas` | the key becomes `MM-dd`; the value is trimmed; rows without names are skipped |
| Czech: a calendar file | its `calendar` object as it is |

The metadata written for each calendar (display name, locale, source, license) is in
[calendar-file.md](calendar-file.md#the-ten-shipped-calendars).

## Checks (`Write-Calendar`)

Each calendar is checked before and after it is written; a failure throws and stops the script:

| Check | Message |
|---|---|
| every key is `MM-dd` and a real date in the leap year 2000 (`Test-DayKey`) | `{file} : invalid day key '{key}'` |
| no value is empty or blank | `{file} : empty value for '{key}'` |
| at least 360 days | `{file} : only {n} days; expected a full year` |
| the written file parses back with the same number of days | `{file} : round-trip check failed` |

## Output

- `Anniversary.Calendar.{File}.json` in `-OutputFolder` for Czech, Austrian, Bulgarian, French, Hungarian, Italian,
  Polish, Russian, Slovak and Latvian.
- UTF-8 without a byte-order mark; the keys `name`, `locale`, `source`, `license` and `calendar` in that order, indented
  by two spaces; the days sorted by key, one per line. Only `\` and `"` are escaped, so letters such as č, ő or д are
  written as they are.
- One console line per calendar: the file, the number of days and the display name, for example
  `Czech        366 days  Czechia (Česko)`.
- Running the script twice gives identical files, and the Czech names are identical to v4's (both checked when the
  calendars were built).

## Adding a country

"To add a country, extend the script with a pinned source plus its license and validation" (spec 5.7); in the words of
`calendars/README.md`, "pinned source + license + validation in `Build-Calendars.ps1`". Name days are never typed in by
hand.

## Later

[006](../../006-holiday-calendar-rules/spec.md) and [008](../../008-international-days/spec.md) extended the script with
holiday entries written in it (the US calendar, two Czech Easter holidays, International Days), so today's script writes
twelve files.
