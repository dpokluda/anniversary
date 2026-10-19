# Contract: people data file (`Anniversary.Data.json`)

The people file as this feature reads it, built on 2026-10-09: v4's format, unchanged. Writing it came with
[002](../../002-people-editor/spec.md). System spec: [docs/spec.md](../../../docs/spec.md) §5.1, §5.3 and §5.4.

## Name and location

- The name is `dataFile` in `appsettings.json` ([settings-files.md](settings-files.md)), by default
  `Anniversary.Data.json`. A full path is used as is.
- A file name is looked for next to the executable, then in the app-data folder (`%APPDATA%\Anniversary` on Windows,
  `~/Library/Application Support/Anniversary` on macOS, `~/.config/Anniversary` on Linux), then among the built-in
  defaults. The first copy found wins.
- Built in: `Anniversary.Shared/Defaults/Anniversary.Data.json`, `{ "data": [] }`: no people, so no personal data in
  the app (C13). Users copy their v4 file into the app-data folder (or next to the executable).

## Format

```json
{
  "data": [
    { "name": "Jan Novák", "namedate": "06-24", "birthdate": "1980-03-14",
      "anniversaries": { "2008-06-21": "svatba", "2004-09-01": "seznámení" } }
  ]
}
```

| Key | Type | Required | Meaning |
|---|---|---|---|
| `data` | array of people | yes | the people, in display order |
| `name` | string | yes | the name shown in the lists |
| `namedate` | string `"MM-dd"` | no | the name day |
| `birthdate` | string `"yyyy-MM-dd"` | no | the birth date; the year `9999` means "year unknown" |
| `anniversaries` | object, `"yyyy-MM-dd"` → string | no | each anniversary's date and description |

- Property names may be in any letter case; comments and trailing commas are allowed.
- v4 files load unchanged; v5 adds nothing to this format.

## Errors

| Situation | Message (title "Data Error", after the main window opens) | Result |
|---|---|---|
| the file isn't found (another name than the default, or an absolute path) | `Data file not found: {name}` | no people |
| invalid JSON, or the file can't be read | `Error loading data: {message}` | no people |

With no people, the Birthdays and Anniversaries box shows the hint "No people yet. Use ⋯ ▸ Edit People… to add some."
([main-window.md](main-window.md)).

## How the people are shown

- Namedays: the people whose `namedate` is the shown day.
- Birthdays and Anniversaries: `"{name} ({years})"` and `"{name} - {description} ({years})"`; no years when
  `displayYearsOnBirthdays` is `false` or the year is after the shown day's year (so `9999` shows just the name).
- Next N days: each person with any event in the next N days, once.
- Find: a person's `name` matches unless "Search calendar only" is checked.

The exact rules are in [anniversarybook-api.md](anniversarybook-api.md).
