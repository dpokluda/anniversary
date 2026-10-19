# Contract: Choosing the default calendar

Which calendar is active when the user hasn't chosen one, as built on 2026-10-10 (`AnniversaryBook.Load` in
`Anniversary.Shared/AnniversaryBook.cs`). Later, [012](../../012-single-settings-file/spec.md) stopped reading
`appsettings.json`, which removed step 2. States: [data-model.md](../data-model.md#choosing-the-active-calendar);
system spec: [docs/spec.md](../../../docs/spec.md) 5.7 and 6.

## API

```csharp
public static AnniversaryBook Load(string? preferredCalendar, params string[] folders); // signature unchanged
public string CalendarFile { get; }                   // the active calendar's file name (003)
public IReadOnlyList<CalendarInfo> Calendars { get; } // the discovered calendars, sorted by display name (003)
public AppConfig Config { get; }                      // Config.CalendarFile is now string?, null unless set
```

- `preferredCalendar`: "The calendar chosen by the user, or `null` to use the configured one. When none is configured,
  the calendar for the region of `CultureInfo.CurrentCulture` is used, otherwise the US one (spec 5.7)."
- The desktop app passes the remembered choice, unchanged since 003:
  `AnniversaryBook.Load(settings.Calendar, AppContext.BaseDirectory, appData)` in `Anniversary.Shared/App.axaml.cs`.
- `Load` never throws; problems go to `Warnings` as before (spec 5.4).

## Order

The first that applies:

1. **The menu choice**: `preferredCalendar` (`calendar` in `usersettings.json`), if that file is among the discovered
   calendars.
2. **`calendarFile`** from an `appsettings.json` next to the executable or in the app-data folder, if it isn't blank.
   A copied v4 file names `Anniversary.Calendar.Czech.json`. The built-in `appsettings.json` no longer has the key. A
   missing file gives the warning `Calendar file not found: {name}` and an empty calendar, as before.
3. **The OS region**: the first calendar, by display name, whose `locale` has the region of
   `CultureInfo.CurrentCulture`.
4. **`Anniversary.Calendar.US.json`**.

Before this feature, a start without a menu choice always used `calendarFile`, which defaulted to
`Anniversary.Calendar.Czech.json` both in `AppConfig` and in the built-in `appsettings.json`.

## The OS region

- The current culture is the one the OS gives .NET, and the app leaves it as it is: the Regional format on Windows
  (not "Country or region"), Language & Region on macOS, and `LANG`/`LC_ALL` on Linux.
- A region is the culture's two-letter ISO region (`RegionInfo.TwoLetterISORegionName`). A neutral culture (`cs`), the
  invariant culture and an unknown culture name have none.
- A calendar's region comes from its `locale` the same way. The shipped calendars have AT, BG, CZ, FR, HU, IT, LV, PL,
  RU, SK and US.

| Current culture (from the tests) | Region | Active calendar |
|---|---|---|
| `cs-CZ`, `en-CZ` | CZ | `Anniversary.Calendar.Czech.json` |
| `sk-SK` | SK | `Anniversary.Calendar.Slovak.json` |
| `de-AT` | AT | `Anniversary.Calendar.Austrian.json` |
| `en-US` | US | `Anniversary.Calendar.US.json` |
| `en-GB`, `de-DE` | GB, DE: no calendar | `Anniversary.Calendar.US.json` |
| `cs` (neutral), `""` (invariant) | none | `Anniversary.Calendar.US.json` |

With `cs-CZ`, a configured Slovak calendar (step 2) and a chosen Polish one (step 1) still win.

## The calendar menu

- No new item. **⋯ ▸ Nameday Calendar ▸** (on macOS also **View ▸ Nameday Calendar**) lists the United States
  calendar as one more radio item among the 11 shipped calendars, sorted by display name, so it comes last after
  Slovakia (Slovensko). The active calendar is checked, whichever step chose it. The menu keeps its name.
- Choosing a calendar saves it as `calendar` in `usersettings.json` (003), so step 1 applies from the next start.

## What users see

| User | Active calendar at start-up |
|---|---|
| chose a calendar in the menu before | that calendar, as before |
| copied v4's `appsettings.json` into the app-data folder | Czech (its `calendarFile`) |
| fresh install, OS region with a shipped calendar | that region's calendar |
| fresh install, any other region or none | United States |
| Czech user on Windows with an English (United States) Regional format | United States; choosing Czechia (Česko) once is remembered |

## Tests

`CalendarForTheRegionIsUsedWhenNoneIsChosenOrConfigured` (the nine cultures above) and
`ChosenAndConfiguredCalendarsWinOverTheRegion` in `Anniversary.Tests/CalendarTests.cs`; `FallsBackToBuiltInDefaults`
in `Anniversary.Tests/LoadTests.cs` (in `en-US`: no `calendarFile`, the US calendar, Independence Day on 4 July 2026).
All of them load through `TestFiles.LoadInCulture`, which sets `CultureInfo.CurrentCulture` for the load.
