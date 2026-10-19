# Contract: calendar file (`Anniversary.Calendar.Czech.json`)

v4's name-day calendar format with `MM-dd` keys, as this feature reads it, built on 2026-10-09. Later,
[003](../../003-calendar-chooser/spec.md) added optional metadata and more calendars, and
[006](../../006-holiday-calendar-rules/spec.md) added rule keys for dates that move. System spec:
[docs/spec.md](../../../docs/spec.md) §5.1, §5.3, §5.4, §6.1 and §6.2.

## Name and location

- The name is `calendarFile` in `appsettings.json` ([settings-files.md](settings-files.md)), by default
  `Anniversary.Calendar.Czech.json`. A full path is used as is.
- A file name is looked for next to the executable, then in the app-data folder, then among the built-in defaults, like
  the people file ([people-data-file.md](people-data-file.md)).
- Built in: v4's Czech calendar under its file name, with v4's values unchanged (today
  `calendars/Anniversary.Calendar.Czech.json`, which 003 generated with the same values plus metadata).

## Format

```json
{
  "calendar": {
    "01-01": "Nový rok",
    "01-02": "Karina",
    "02-29": "Horymír",
    "10-09": "Štefan/Sára",
    "12-24": "Adam a Eva/Štědrý den"
  }
}
```

| Key | Type | Meaning |
|---|---|---|
| `calendar` | object | the days of the year |
| each key | string `"MM-dd"` | the day, up to 366 keys including `02-29`; days without names may be missing |
| each value | string | the text for the day; it may contain `,` or `/` |

Property names may be in any letter case; comments and trailing commas are allowed.

## How the text is used

- **"Today is"**: the whole text for the shown day, as it is ("Štefan/Sára").
- **Next N days**: the texts of the next N days split at `,`, each piece trimmed, empty pieces skipped and each name
  listed once, in date order (a `/` doesn't split).
- **Find**: the whole text must contain the searched name; with "Whole words only" the text is split at spaces,
  commas, semicolons, tabs, slashes and hyphens, and one word must equal it.
- **`02-29`**: only in leap years.

## Errors

| Situation | Message (title "Data Error", after the main window opens) | Result |
|---|---|---|
| the file isn't found (another name than the default, or an absolute path) | `Calendar file not found: {name}` | empty calendar |
| invalid JSON, or the file can't be read | `Error loading calendar: {message}` | empty calendar |

v4 said "Data file not found" for a missing calendar too; v5 names the calendar (C3).
