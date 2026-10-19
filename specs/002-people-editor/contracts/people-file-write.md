# Contract: The people data file, as the editor writes it

How Save in the editor writes the people, as built on 2026-10-09 (`SavePathFor` and `SavePeople` in
`Anniversary.Shared/AnniversaryBook.cs`). Reading the file and finding it are 001's
([docs/spec.md](../../../docs/spec.md) 5.1, 5.3 and 5.4). The members are described in [editing-api.md](editing-api.md);
later, [007](../../007-data-file-sharing/contracts/data-file-api.md) made the target the data file in use and added
Save As….

## Where

The target is chosen when the editor opens and shown in its footer:

1. the file the people were loaded from (next to the executable, in the app-data folder, or at an absolute `dataFile`);
2. otherwise, when `dataFile` in `appsettings.json` is an absolute path, that path;
3. otherwise `{app-data folder}/{dataFile}`, where `dataFile` defaults to `Anniversary.Data.json`.

Step 2 or 3 applies when the people came from the built-in empty list, or when the configured file wasn't found or
couldn't be read at start-up. The app-data folder is `%APPDATA%\Anniversary` (Windows),
`~/Library/Application Support/Anniversary` (macOS) or `~/.config/Anniversary` (Linux); it is created if needed.

## Format

The same format as v4's data file (docs/spec.md 5.1). For example (the made-up people of the test
`SavedPeopleLoadAgainUnchanged`):

```json
{
  "data": [
    {
      "name": "Kateřina Nováková",
      "birthdate": "1975-02-15",
      "namedate": "11-25",
      "anniversaries": {
        "2004-09-01": "seznámení",
        "2008-06-21": "svatba"
      }
    },
    {
      "name": "Žofie"
    }
  ]
}
```

| Item | Written as |
|---|---|
| Root | an object with `data`, the list of people (`[]` when the list is empty) |
| Names of the keys | camelCase, in the order of the model: `name`, `birthdate`, `namedate`, `anniversaries` |
| `name` | trimmed |
| `birthdate` | `yyyy-MM-dd`; left out when there is none |
| `namedate` | `MM-dd`, trimmed; left out when blank |
| `anniversaries` | an object `{ "yyyy-MM-dd": "description" }`, ordered by date; left out when there are none |
| Order of the people | the list order: the order they were loaded in, with new people at the end; sorting or filtering the grid doesn't change it |
| Text | indented UTF-8 without a byte order mark; Czech and other non-ASCII letters written as they are, not as `\u` escapes (`JavaScriptEncoder.UnsafeRelaxedJsonEscaping`) |

The app reads the file back as before (names in any letter case, comments and trailing commas allowed), so a saved file
loads with the same people, in the same order. A birth year of `9999` ("year unknown" in v4 data) is written back as it
was read.

## Writing

1. The people are normalized (trimmed name and nameday, blank nameday and empty anniversaries left out, anniversaries
   ordered by date) and serialized.
2. The target's folder is created if needed.
3. The text is written to `{target}.tmp`, which is then moved over the target, replacing it.
4. Only then do the book's `People` and `DataFilePath` change and the day index rebuild, so the main window and Find
   show the saved people without reading the file again.

After a successful save no `.tmp` file is left behind.

## Errors

`IOException` and `UnauthorizedAccessException` (for example for a read-only folder next to the executable) reach the
editor, which shows `Could not save the data file: {message}` (title **Edit People**) and stays open. The target is
left as it was, and nothing in the book changes.

## Scope

- Save writes only this file. The app never writes `appsettings.json`; its only other file is `usersettings.json`
  (spec C9).
- Save doesn't check whether the file was changed elsewhere since it was loaded and doesn't ask before replacing it;
  that came with [007](../../007-data-file-sharing/contracts/data-file-menu.md).
