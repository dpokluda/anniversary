# Contract: Nameday Calendar menu

The user interface this feature added, as built on 2026-10-09 and committed at the move to GitHub (`67a4092`):
`Anniversary.Shared/Views/MainWindow.axaml` and `MainWindow.axaml.cs`. Behaviour: [spec.md](../spec.md); the API it
calls: [calendar-api.md](calendar-api.md); system spec: [docs/spec.md](../../../docs/spec.md) 7.2 and 7.10;
[mock](../../../docs/screens/main-window-calendar-menu.light.preview.png).

## Menu items

| Menu | Item | Content |
|---|---|---|
| ⋯ button and right-click menu, after **Edit People…** and a separator, before **Theme** | **Nameday Calendar ▸** | one radio item per available calendar (its display name), the active one checked |
| macOS menu bar ▸ **View**, above **Appearance** | **Nameday Calendar ▸** | the same list |

The ⋯ menu then reads: Today, Change Date…, ─, Find…, Find Next, ─, Edit People…, ─, **Nameday Calendar ▸**, Theme ▸, ─,
About Anniversary…. The macOS View menu reads: **Nameday Calendar ▸**, Appearance ▸. Neither item has a shortcut.

## The list

- **Entries**: `AnniversaryBook.Calendars`, in that order: sorted by display name (current culture). With the shipped
  calendars only: Austria (Österreich), Bulgaria (България), Czechia (Česko), France, Hungary (Magyarország), Italy
  (Italia), Latvia (Latvija), Poland (Polska), Russia (Россия), Slovakia (Slovensko).
- **Check mark**: on the entry whose file name equals `AnniversaryBook.CalendarFile` (compared case-insensitively). In
  the ⋯ menu the items are `MenuItem`s with `ToggleType` Radio and `GroupName` "Calendar"; on macOS they are
  `NativeMenuItem`s with `ToggleType` Radio.
- **When it is built**: at start-up; each time the ⋯ or right-click menu opens (after `RefreshCalendars()`, so a file
  added while the app runs is listed); and after every choice. The macOS View list is rebuilt at the same moments.
- **Not listed**: files whose names don't follow `Anniversary.Calendar.*.json` (except the active calendar given by a
  path in `calendarFile`), and calendar files that can't be read.

## Choosing a calendar

1. `AnniversaryBook.UseCalendar(fileName)`: clears the old warnings and loads the calendar.
2. `UserSettings.Calendar = fileName`, then `Save()`: `calendar` in `usersettings.json`
   ([calendar-api.md](calendar-api.md#usersettingscalendar-calendar-in-usersettingsjson)).
3. Both lists are rebuilt, so the check moves.
4. The main window is refreshed for the same day: the calendar names under the date and Next N days change, and Find
   uses the new calendar from then on. The people and the displayed day don't change.
5. Each new warning is shown in its own message (below).

Choosing the calendar that is already active does the same.

## Warnings

| Situation | Title | Text |
|---|---|---|
| The calendar file isn't found | **Data Error** | `Calendar file not found: {name}` |
| The calendar file can't be read | **Data Error** | `Error loading calendar: {message}` |

At start-up these appear after the main window opens, together with any other load warnings (001). After a choice only
the warnings of that choice appear. In both cases the calendar names stay empty until a calendar loads.
