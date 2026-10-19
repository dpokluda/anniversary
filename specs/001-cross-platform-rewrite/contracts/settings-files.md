# Contract: settings files (`appsettings.json` and `usersettings.json`)

The two settings files as built on 2026-10-09. [012](../../012-single-settings-file/spec.md) later replaced them with
one `usersettings.json` that holds every setting, and stopped reading `appsettings.json`. System spec at the time:
docs/spec.md §5.1, §5.4, §5.5 and C9 (then `specs/spec.md`; the current [docs/spec.md](../../../docs/spec.md) describes
the single file).

## `appsettings.json`: v4's configuration, read only

- Found like the other files: next to the executable, then in the app-data folder, then the built-in copy
  (`Anniversary.Shared/Defaults/appsettings.json`, v4's file; historical, removed by 012). The name can't be changed.
- Edited by hand; the app never writes it (C9: v4's unused `SaveConfiguration` was dropped).

```json
{
  "calendarFile": "Anniversary.Calendar.Czech.json",
  "dataFile": "Anniversary.Data.json",
  "displayYearsOnBirthdays": true,
  "numberOfDaysAhead": 7
}
```

| Key | Type | Default (if missing) | Used for |
|---|---|---|---|
| `calendarFile` | file name or absolute path | `Anniversary.Calendar.Czech.json` | the calendar ([calendar-file.md](calendar-file.md)) |
| `dataFile` | file name or absolute path | `Anniversary.Data.json` | the people ([people-data-file.md](people-data-file.md)) |
| `displayYearsOnBirthdays` | bool | `true` | the years after birthdays and anniversaries |
| `numberOfDaysAhead` | int (≥ 0) | `7` | N in "Next N days" |

- Property names may be in any letter case; comments and trailing commas are allowed.
- If the file can't be read (invalid JSON, a value of the wrong type, an I/O error), the app uses the defaults and,
  after the main window opens, shows `Error loading configuration: {message}`, a line break and `Using default
  settings.`, titled "Configuration Error".

## `usersettings.json`: the app's own settings

- Location: `{app-data folder}/usersettings.json`, for example `%APPDATA%\Anniversary\usersettings.json`,
  `~/Library/Application Support/Anniversary/usersettings.json` or `~/.config/Anniversary/usersettings.json`. One file
  per user account.
- Written only by the app, when the theme is chosen in its menu.

```json
{
  "theme": "Dark"
}
```

| Key | Type | Default | Meaning |
|---|---|---|---|
| `theme` | `"System"`, `"Light"` or `"Dark"` | `"System"` | the chosen theme ([commands-and-dialogs.md](commands-and-dialogs.md#theme)) |

- **Read** at start-up, before the main window is created, with names and values in any letter case (`"light"` loads
  as Light). A missing file, invalid JSON or an I/O or access error gives the defaults, without a message.
- **Written** as camelCase, indented JSON with the theme as a string; the folder is created if needed. If writing
  fails, the choice still applies until the app closes.
- The same file got the chosen calendar (`calendar`) with 003 and the chosen data file (`dataFile`) with
  [007](../../007-data-file-sharing/spec.md).
