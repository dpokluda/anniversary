# Anniversary v5

Shows name days, birthdays and anniversaries for a day. It's a cross-platform rewrite of the WinForms
Anniversary v4 (kept in a separate repository) using [Avalonia UI](https://avaloniaui.net/) on .NET 10, and runs on Windows, macOS
and Linux. The design is in [`specs/spec.md`](specs/spec.md) and [`specs/plan.md`](specs/plan.md).

## Build, test, run

Requires the .NET 10 SDK.

```powershell
dotnet build Anniversary.slnx
dotnet test --project Anniversary.Tests
dotnet run --project Anniversary.Desktop
```

`global.json` opts `dotnet test` into Microsoft.Testing.Platform, which the .NET 10 SDK needs for xUnit v3.
JetBrains Rider with the AvaloniaRider plugin gives a live XAML preview.

## Publish

Each platform gets a single self-contained executable plus `THIRD-PARTY-NOTICES.md` (required by the license of the
bundled name-day calendars; keep it next to the executable).

```powershell
dotnet publish Anniversary.Desktop -c Release -r win-x64   --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-x64
dotnet publish Anniversary.Desktop -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\osx-arm64
dotnet publish Anniversary.Desktop -c Release -r osx-x64   --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\osx-x64
dotnet publish Anniversary.Desktop -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\linux-x64
```

All four can be built on Windows. On macOS and Linux, mark the file executable (`chmod +x Anniversary`). The macOS
output is a plain executable, not a signed `.app` bundle.

### Running on macOS

The macOS builds are a single ad-hoc signed executable, not a notarized `.app`. Use `osx-arm64` for Apple Silicon (M1 and
newer) and `osx-x64` for Intel Macs. Copy the `publish/osx-…` folder to the Mac, then in Terminal:

```bash
cd path/to/osx-arm64
chmod +x Anniversary
xattr -d com.apple.quarantine Anniversary   # only if it was downloaded (Gatekeeper would block it)
./Anniversary
```

Things to check on a Mac (spec acceptance criteria 2, 4, 7, 9):

- The window opens with the macOS title bar and the system font, and the layout matches Windows.
- The menu bar has **View** (Nameday Calendar, Appearance), **Go**, **Find** (⌘F, ⌘G), and **About Anniversary** in the
  app menu. ⌘F and ⌘G work.
- With Appearance set to **System**, switching macOS between Light and Dark (or Auto) re-themes the running app.
- Data is read from `~/Library/Application Support/Anniversary`. Copy `Anniversary.Data.json` there and restart.
- Edit People saves, and the calendar drop-downs open and close correctly.

## Your data

The app ships with an empty people list, the Czech name-day calendar as the default and 9 more calendars to choose from.
It looks for each file in this order, and the first one found wins:

1. next to the executable,
2. the user app-data folder:
   - Windows: `%APPDATA%\Anniversary`
   - macOS: `~/Library/Application Support/Anniversary`
   - Linux: `~/.config/Anniversary`
3. the built-in defaults.

| File | What it is | Written by the app? |
|---|---|---|
| `Anniversary.Data.json` | the people (edit them with **⋯ ▸ Edit People…**) | yes, when you save in the editor |
| `appsettings.json` | configuration (`calendarFile`, `dataFile`, `displayYearsOnBirthdays`, `numberOfDaysAhead`) | never |
| `usersettings.json` | the chosen theme and name-day calendar | yes |
| `Anniversary.Calendar.*.json` | extra or replacement name-day calendars | never |

**Moving from v4:** copy your `Anniversary.Data.json` (and `appsettings.json`, if you changed it) from the v4 folder into
the app-data folder above. The format is unchanged. A birth year of `9999` still means "year unknown".

## Name-day calendars

Choose one with **⋯ ▸ Nameday Calendar** (macOS: **View ▸ Nameday Calendar**). The choice is remembered. To add a
calendar, put an `Anniversary.Calendar.{Name}.json` file into the app-data folder; it appears in the menu the next time
it opens. The shipped calendars, their sources and licenses are described in [`calendars/README.md`](calendars/README.md).

## Projects

| Project | Contents |
|---|---|
| `Anniversary.Shared` | everything shared by all platforms: model, `AnniversaryBook` (all logic), windows and dialogs, embedded defaults |
| `Anniversary.Desktop` | the Windows/macOS/Linux executable (`Program.cs`) |
| `Anniversary.Tests` | xUnit tests in the classic style: real objects and real files, no mocks |
| `icon/` | the app icon and the script that draws it (see [`icon/README.md`](icon/README.md)) |
| `calendars/` | name-day calendars and the script that builds them |

## License

[MIT](LICENSE). The bundled name-day calendars come from third-party sources under their own licenses (MIT and
CC0); see [`calendars/README.md`](calendars/README.md) and
[`calendars/THIRD-PARTY-NOTICES.md`](calendars/THIRD-PARTY-NOTICES.md).
