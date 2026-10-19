# Contract: `usersettings.json`

The one settings file, as built on 2026-10-11 (`Anniversary.Shared/Model/UserSettings.cs`; docs/spec.md 5.4 and 5.5).
States: [data-model.md](../data-model.md); API: [settings-api.md](settings-api.md).

## File

- `usersettings.json` in the app-data folder: `%APPDATA%\Anniversary` (Windows),
  `~/Library/Application Support/Anniversary` (macOS), `~/.config/Anniversary` (Linux). Only there: unlike the people
  and calendar files, it isn't looked for next to the executable.
- One file per user account and computer, so a shared data file can have its own path on each computer (007).
- Written by the app; may be edited by hand with the app closed.

## Keys

| Key | Type | Default (when missing) | Set by | Meaning |
|---|---|---|---|---|
| `theme` | string: `System`, `Light` or `Dark` (any letter case) | `System` | ⋯ ▸ Theme (macOS: View ▸ Appearance) | the theme |
| `calendar` | string or `null` | `null`: the calendar for the OS region, otherwise the US one | ⋯ ▸ Calendar (macOS: View ▸ Calendar) | the file name of the chosen calendar, e.g. `Anniversary.Calendar.Czech.json` |
| `dataFile` | string or `null` | `null`: `Anniversary.Data.json` | Data File ▸ Open… or Save As… (a full path); by hand, a full path or a file name | the people's data file |
| `numberOfDaysAhead` | whole number (≥ 0) | `7` | by hand only | how many days the "Next N days" section covers |
| `displayYearsOnBirthdays` | `true` or `false` | `true` | by hand only | whether birthdays and anniversaries show the number of years |

As the app writes it at the first start (every key, camelCase, indented):

```json
{
  "theme": "System",
  "calendar": null,
  "dataFile": null,
  "numberOfDaysAhead": 7,
  "displayYearsOnBirthdays": true
}
```

The example in docs/spec.md 5.5:

```json
{
  "theme": "System",
  "calendar": "Anniversary.Calendar.Czech.json",
  "dataFile": "C:\\Users\\Jan\\OneDrive\\Anniversary.Data.json",
  "numberOfDaysAhead": 7,
  "displayYearsOnBirthdays": true
}
```

## Reading

- Once, at start-up, before the main window is created.
- Key names in any letter case; comments and trailing commas are allowed.
- Missing keys take their defaults; unknown keys are ignored.
- `calendar` applies only when it is the file name of an available calendar (docs/spec.md 5.7); otherwise the calendar
  for the OS region is used, otherwise the US one, without a message.
- `dataFile`: a full path is used as is, even when the file is missing (the warning `Data file not found: {path}`, and
  the app keeps the link, 007). A file name is looked for next to the executable, then in the app-data folder, then
  among the built-in files (only `Anniversary.Data.json` is built in, with no people). `null` or blank means
  `Anniversary.Data.json`.

## Writing

- At start-up when the file doesn't exist (the folder is created if needed), and whenever a menu choice changes: the
  theme, the calendar, or the data file after a successful Data File ▸ Open… or Save As….
- Always the whole file: every key, camelCase, indented, with `null` for a calendar or data file not chosen. Keys the
  app doesn't know, comments and the user's layout aren't kept.
- The values written are those in memory: the ones read at start-up plus the menu choices since. A hand edit made
  while the app runs is therefore replaced at the next menu choice.
- If writing fails (an I/O or access error), the failure is ignored and the choice applies until the app closes.
- Never while the file couldn't be read (below).

## Errors

| Situation | Message | Result |
|---|---|---|
| The file doesn't exist | none | the defaults; the app creates the file |
| Invalid JSON, a value of the wrong type (e.g. `"numberOfDaysAhead": "14"`), or an I/O or access error while reading | after the main window opens, titled **Configuration Error**: `Error loading configuration: {full path of the file}: {reason}`, a new line, then `Using default settings.` (for a value of the wrong type, the reason names the key) | the defaults for all five settings; the file isn't written until it is fixed and the app started again; menu choices apply until the app closes |

## Compatibility

- A `usersettings.json` written before this feature (`theme`, `calendar`, `dataFile`) reads as before; the two options
  take their defaults, and their keys are added at the next menu choice.
- Before this feature the file was read case-insensitively but without comments or trailing commas, and a file the app
  couldn't read gave the defaults without a message and was replaced at the next menu choice.
- `appsettings.json` (v4's file, or one made for an earlier v5) is no longer read anywhere: not next to the
  executable, not in the app-data folder, and there is no built-in one. Its keys map as follows:

| `appsettings.json` | In this version |
|---|---|
| `numberOfDaysAhead` | `numberOfDaysAhead` in `usersettings.json` (copied by hand) |
| `displayYearsOnBirthdays` | `displayYearsOnBirthdays` in `usersettings.json` (copied by hand) |
| `dataFile` | `dataFile` in `usersettings.json`, or Data File ▸ Open… |
| `calendarFile` | `calendar` in `usersettings.json`: choose the calendar in the menu |
