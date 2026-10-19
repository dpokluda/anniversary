# Contract: `dataFile` in `usersettings.json`

The setting this feature added, as built on 2026-10-10 (`Anniversary.Shared/Model/UserSettings.cs`). Later,
[012](../../012-single-settings-file/spec.md) made `usersettings.json` the only settings file.

## File

- `usersettings.json` in the app-data folder: `%APPDATA%\Anniversary` (Windows),
  `~/Library/Application Support/Anniversary` (macOS), `~/.config/Anniversary` (Linux).
- Written only by the app; one file per user account and computer.

## Key

| Key | Type | Default | Meaning |
|---|---|---|---|
| `dataFile` | string or `null` | `null` (or missing) | the full path of the data file chosen with Open… or Save As… on this computer |

The other keys at the time were `theme` and `calendar`. Example (docs/spec.md 5.5 at the time):

```json
{ "theme": "System", "calendar": "Anniversary.Calendar.Czech.json", "dataFile": "C:\\Users\\Jan\\OneDrive\\Anniversary.Data.json" }
```

## Behaviour

- **Written** when Open… or Save As… succeeds: the app sets the key and saves the whole file (camelCase, indented; a
  key that was never chosen is written as `null`). If saving fails, the choice still applies until the app closes.
- **Read** at start-up, with case-insensitive names. A missing or invalid file gives the defaults, without a warning.
- **Used** as is, even when the file is missing: the warning names it, and the app keeps the link and loads the file
  once it appears ([data-model.md](../data-model.md)).
- **Precedence**: `dataFile` here, when not blank; otherwise `dataFile` in `appsettings.json` (default
  `Anniversary.Data.json`). A full path there is used as is; a file name is found next to the executable, then in the
  app-data folder, then among the built-in defaults (docs/spec.md 5.3).
- **Per computer**: each computer keeps its own path, so the same shared file can have a different path on each one
  (for example on Windows and on macOS).
- **Compatibility**: a `usersettings.json` written before this feature has no `dataFile` and means "nothing chosen".
  The app still never writes `appsettings.json`.
