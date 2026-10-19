# Contract: Calendar menu and the order of calendars

The menu this feature renamed and ordered, as built on 2026-10-10 (`Anniversary.Shared/Views/MainWindow.axaml`,
`MainWindow.axaml.cs`), and the list behind it (`AnniversaryBook.Calendars` in `Anniversary.Shared/AnniversaryBook.cs`).
Behaviour: [spec.md](../spec.md); data model: [data-model.md](../data-model.md); system spec:
[docs/spec.md](../../../docs/spec.md) 5.7, 7.2 and 7.10.

## Where it is

| Menu | Item (before → after) | Next to it |
|---|---|---|
| ⋯ button and right-click menu, after Data File ▸ and a separator | **Nameday Calendar ▸** → **Calendar ▸** | Theme ▸ below it |
| macOS menu bar ▸ **View** | **Nameday Calendar ▸** → **Calendar ▸** | Appearance ▸ below it |

The ⋯ menu then reads: Today, Change Date…, ─, Find…, Find Next, ─, Edit People…, Data File ▸, ─, **Calendar ▸**,
Theme ▸, ─, About Anniversary…. The macOS View menu reads: **Calendar ▸**, Appearance ▸. The main window's
**Namedays** box keeps its name. The code finds the macOS item by its header, so it now looks for "Calendar".

## Items

- One radio item per available calendar, showing its display name; the active calendar is checked (003).
- **Order** (both menus): the calendars with a `locale`, sorted by display name, then a **separator**, then the
  calendars without a `locale` (missing or empty), sorted by display name. The ⋯ submenu gets a `Separator`, the macOS
  submenu a `NativeMenuItemSeparator`. There is at most one separator, and none when either group is empty.
- With the shipped calendars, in English: Austria (Österreich), Bulgaria (България), Czechia (Česko), France, Hungary
  (Magyarország), Italy (Italia), Latvia (Latvija), Poland (Polska), Russia (Россия), Slovakia (Slovensko), United
  States, ─, International Days.
- A calendar file a user adds without a `locale` (for example `Anniversary.Calendar.Test.json` with `"name": "Test"`)
  is listed after the separator: International Days, Test. A v4-style file without metadata goes there too, under its
  file name.
- Unchanged from 003: the submenu is rebuilt each time the ⋯ menu opens, after the calendar files are discovered again,
  so a file added while the app runs appears without a restart; it is also rebuilt after a calendar is chosen.
  Choosing an item switches the calendar (`UseCalendar`), refreshes the main window, saves `calendar` in
  `usersettings.json` and shows any warnings.

## `AnniversaryBook.Calendars`

```csharp
// Those with a locale (a country) by display name, then those without one (e.g. International Days) by display name.
public IReadOnlyList<CalendarInfo> Calendars { get; }

public sealed record CalendarInfo(string FileName, string DisplayName, string? Locale); // unchanged (003)
```

- Sort keys: first `string.IsNullOrEmpty(Locale)` (`false` first), then `DisplayName` with
  `StringComparer.CurrentCulture`. Before this feature the list was sorted by display name only.
- The list is sorted whenever it is rebuilt (`RefreshCalendars()`) and whenever a calendar is loaded (the active
  calendar is added first if it isn't a discovered file, for example a calendar given by an absolute path).
- The starting calendar for an OS region is still the first calendar in this list whose `locale` has the region of the
  current culture (006); since the countries come first, sorted by name, that is unchanged, and a calendar without a
  `locale` is never picked.
