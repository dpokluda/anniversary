# Contract: Rule keys in calendar files

The addition this feature made to the calendar file format, as built on 2026-10-10
(`Anniversary.Shared/AnniversaryBook.cs` reads the keys, `calendars/Build-Calendars.ps1` writes them). Rules and
states: [data-model.md](../data-model.md); system spec: [docs/spec.md](../../../docs/spec.md) 5.1, 5.4 and 6.3.

## Where they go

In the `calendar` object of any `Anniversary.Calendar.*.json` file, next to the `MM-dd` keys. Values are display text,
as before. An excerpt of `calendars/Anniversary.Calendar.US.json`, in the file's order:

```json
"calendar": {
  "05-Mon-last": "Memorial Day",
  "07-04": "Independence Day",
  "11-Thu-4": "Thanksgiving",
  "11-Thu-4+1": "Black Friday",
  "easter-2": "Good Friday"
}
```

## Grammar

```text
key      = date | rule
date     = MM "-" dd                    ; as before: a date that exists in a leap year, so 02-29 is valid
rule     = ( MM "-" weekday "-" nth | "easter" ) [ offset ]
MM       = "01" … "12"
weekday  = "Mon" | "Tue" | "Wed" | "Thu" | "Fri" | "Sat" | "Sun"
nth      = "1" | "2" | "3" | "4" | "last"
offset   = ( "+" | "-" ) n              ; n = 1 … 99, without a leading zero
```

Rules are case-insensitive: `11-THU-4`, `05-mon-LAST` and `Easter` work. The app recognises them with one regular
expression (`RuleKeyPattern`, `RegexOptions.IgnoreCase | RegexOptions.CultureInvariant`):

```text
^(?:(?<month>0[1-9]|1[0-2])-(?<weekday>mon|tue|wed|thu|fri|sat|sun)-(?<nth>[1-4]|last)|easter)(?<offset>[+-][1-9][0-9]?)?$
```

The build script's `Test-RuleKey` uses the same pattern without the group names.

## Meaning

| Key | The date in a year | Example (2026 unless stated) |
|---|---|---|
| `MM-Ddd-n` | the first `Ddd` on or after the 1st of `MM`, plus `7 × (n − 1)` days | `11-Thu-4`: 26 November (Thanksgiving); `01-Mon-3`: 19 January |
| `MM-Ddd-last` | the last `Ddd` on or before the last day of `MM` | `05-Mon-last`: 25 May, and 31 May 2027; `02-Tue-last`: 29 February 2028 |
| `easter` | Easter Sunday, Western (anonymous Gregorian algorithm) | 5 April; 28 March 2027 |
| a rule with `+n` or `-n` | the rule's date moved by n days | `easter+1`: 6 April; `easter-47`: 17 February; `11-Thu-4+1`: 27 November |

An offset may cross New Year: `12-Mon-last+7` falls on 4 January 2027, and `01-Mon-1-7` (for 2026) on 29 December
2025.

## Keys that are not accepted

| Key (from the tests) | Why |
|---|---|
| `13-01` | there is no month 13 |
| `11-Thx-4` | not a weekday |
| `easter+` | an offset without a number |
| `11-Thu-5` | n is 1–4; use `last` |
| `easter+100` | an offset over 99 |
| `1-5` | `MM-dd` needs two digits in each part |

By the same pattern, `easter+0` and `easter+05` aren't accepted either.

## Names for a day

- The day's `MM-dd` entry first, then each rule entry that falls on the day, in the order of the file, joined with
  `", "`. Entries with an empty text add nothing. Example (from the tests): `{ "easter+1": "Velikonoční pondělí",
  "04-06": "Vendula", "04-Mon-1": "First Monday" }` gives "Vendula, Velikonoční pondělí, First Monday" on 6 April 2026.
- A rule falls on a day if its date in the day's year, the year before or the year after is that day.
- These names are what the "Today is" line shows, what Next N days lists (split on "," and without duplicates) and what
  Find searches, also with **Search calendar only**.
- A rule date before 1 January 0001 or after 31 December 9999 is skipped, so nothing fails at the ends of the range:
  `12-Sun-last+7` has no date in 9999 (it would fall in the year 10000), and `01-Sun-1-7` none in the year 1 (it would
  fall in the year 0).

## Unknown keys

- Every key that is neither a valid `MM-dd` date nor a rule is ignored. One warning per load lists them:

  ```text
  Unknown dates in {name}: {key}, {key}, {key}, {key}, {key} and {n} more
  ```

  At most five keys are listed; ` and {n} more` is added only when there are more. `{name}` is the calendar's file
  name as chosen or configured. Example: `Unknown dates in Anniversary.Calendar.Test.json: 13-01, 11-Thx-4, easter+,
  11-Thu-5, easter+100 and 1 more`.
- The calendar's other entries load and work.
- The main window shows the warning in a message with **OK**, title **Data Error**: after the window opens at
  start-up, or right after the calendar is chosen in the menu.

## Compatibility

- Files with only `MM-dd` keys, including v4's, load and show the same names as before. Before this feature a key that
  wasn't `MM-dd` was kept but never shown; now it is reported.
- Rule keys are for calendar files only. A person's `namedate` stays `MM-dd`; the people editor still accepts only
  that.

## Build script (`calendars/Build-Calendars.ps1`)

- `Write-Calendar` throws `{file} : invalid day key '{key}'` for a key that is neither `MM-dd` (`Test-DayKey`) nor a
  rule (`Test-RuleKey`), and `{file} : empty value for '{key}'` for an empty text.
- Keys are written in ordinal order (`[StringComparer]::Ordinal`): by month, with the `MM-dd` keys of a month before
  its rule keys, and the `easter…` keys last. The files are the same whichever OS runs the script.
- The full-year check (at least 360 `MM-dd` keys) applies to name-day calendars only, not to calendars written with
  `-Holidays` ([holiday-calendars.md](holiday-calendars.md)).
