# Anniversary v5

[![CI](https://github.com/dpokluda/anniversary/actions/workflows/ci.yml/badge.svg)](https://github.com/dpokluda/anniversary/actions/workflows/ci.yml)

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

CI ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)) builds with warnings as errors and runs the tests on
Windows, macOS and Linux for every push to `main` and every pull request, then publishes all the builds below.

## Publish

Each platform gets a single self-contained executable plus `THIRD-PARTY-NOTICES.md` (required by the license of the
bundled name-day calendars; keep it next to the executable).

```powershell
dotnet publish Anniversary.Desktop -c Release -r win-x64     --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-x64
dotnet publish Anniversary.Desktop -c Release -r win-arm64   --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-arm64
dotnet publish Anniversary.Desktop -c Release -r osx-arm64   --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\osx-arm64
dotnet publish Anniversary.Desktop -c Release -r osx-x64     --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\osx-x64
dotnet publish Anniversary.Desktop -c Release -r linux-x64   --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\linux-x64
dotnet publish Anniversary.Desktop -c Release -r linux-arm64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\linux-arm64
```

All six can be built on Windows. The `arm64` builds are for ARM machines: `win-arm64` for Windows on ARM (e.g.
Snapdragon laptops), `osx-arm64` for Apple Silicon Macs and `linux-arm64` for ARM Linux (e.g. a Linux virtual machine on
an Apple Silicon Mac). On macOS and Linux, mark the file executable (`chmod +x Anniversary`). The macOS output is a
plain executable, not a signed `.app` bundle.

### Downloading a build

Every CI run that passes ([Actions](https://github.com/dpokluda/anniversary/actions/workflows/ci.yml)) offers one
download per platform, kept for 30 days: `Anniversary-{rid}.zip` for Windows and `Anniversary-{rid}.tar.gz` for macOS
and Linux (downloading needs a GitHub sign-in). Each holds a `{rid}` folder with the executable and
`THIRD-PARTY-NOTICES.md`. The `.tar.gz` keeps the executable flag, so `tar -xzf Anniversary-linux-x64.tar.gz` gives a
folder that's ready to run.

### Running on macOS

The macOS builds are a single ad-hoc signed executable, not a notarized `.app`. Use `osx-arm64` for Apple Silicon (M1 and
newer) and `osx-x64` for Intel Macs. Copy the `publish/osx-…` folder to the Mac (or extract a CI download), then in
Terminal:

```bash
cd path/to/osx-arm64
chmod +x Anniversary
xattr -d com.apple.quarantine Anniversary   # only if it was downloaded (Gatekeeper would block it)
./Anniversary
```

On macOS the commands are also in the menu bar: **File** (Edit People…, Open Data File… ⌘O, Save Data File As… ⇧⌘S,
Show Data File in Finder), **View** (Nameday Calendar, Appearance),
**Go** (Yesterday, Tomorrow, Today, Change Date…), **Find** (Find… ⌘F, Find Next ⌘G), and **About Anniversary** in the
app menu.

### Running on Linux

Use `linux-x64` on PCs and `linux-arm64` on ARM machines. Copy the `publish/linux-…` folder (or extract a CI download),
then:

```bash
cd path/to/linux-x64
chmod +x Anniversary   # not needed for a CI download
./Anniversary
```

The app needs fontconfig, libX11, libICE and libSM, which desktop distributions such as Fedora Workstation already have.

## Your data

The app ships with an empty people list and 11 calendars: the US holiday calendar and 10 name-day calendars. It looks
for each file in this order, and the first one found wins:

1. next to the executable,
2. the user app-data folder:
   - Windows: `%APPDATA%\Anniversary`
   - macOS: `~/Library/Application Support/Anniversary`
   - Linux: `~/.config/Anniversary`
3. the built-in defaults.

| File | What it is | Written by the app? |
|---|---|---|
| `Anniversary.Data.json` | the people (edit them with **⋯ ▸ Edit People…**); any file chosen with **⋯ ▸ Data File ▸ Open…** works too | yes, when you save in the editor |
| `appsettings.json` | configuration (`calendarFile`, `dataFile`, `displayYearsOnBirthdays`, `numberOfDaysAhead`) | never |
| `usersettings.json` | the chosen theme, calendar and data file | yes |
| `Anniversary.Calendar.*.json` | extra or replacement calendars | never |

### Sharing your data

**With family:** send them your data file. **⋯ ▸ Data File ▸ Show in Folder** (macOS: **File ▸ Show Data File in
Finder**) opens the folder it's in, so you can attach it to an email. They save it somewhere permanent, such as
Documents, and choose **⋯ ▸ Data File ▸ Open…** (macOS: **File ▸ Open Data File…**, ⌘O). The app uses the file where
it is, saves changes into it, and remembers it.

**On several computers:** on the first one, choose **⋯ ▸ Data File ▸ Save As…** (⇧⌘S on macOS) and save into a
cloud-drive folder (OneDrive, iCloud Drive, Dropbox, …). On each other computer, choose **Open…** and pick that file.
A change saved on one computer appears on the others the next time their app window becomes active, once the cloud
app has synced it. If the file was changed elsewhere while you were editing, the editor asks before overwriting it. If
the file is missing when the app starts (for example, not synced yet), the app says so and keeps waiting for it, and
the editor asks before creating a new one. Each computer remembers its own path to the file.

**Moving from v4:** open your v4 `Anniversary.Data.json` with **⋯ ▸ Data File ▸ Open…**, or copy it (and
`appsettings.json`, if you changed it) from the v4 folder into the app-data folder above. The format is unchanged. A
birth year of `9999` still means "year unknown". If your OS region isn't Czechia, choose **Czechia (Česko)** in the
calendar menu once (a copied v4 `appsettings.json` already selects it).

## Calendars

The default calendar is the one for your OS region, for example Czech in Czechia or Slovak in Slovakia. For any other
region it's the **United States** calendar: the federal holidays plus well-known observances such as Mother's Day and
Halloween. The name-day calendars list names instead. To use another one, choose it with **⋯ ▸ Nameday Calendar**
(macOS: **View ▸ Nameday Calendar**); the choice is remembered. A `calendarFile` in your own `appsettings.json` also
works. To add a calendar, put an `Anniversary.Calendar.{Name}.json` file into the app-data folder; it appears in the
menu the next time it opens.

Besides `MM-dd` dates, a calendar file can list days that move from year to year: `11-Thu-4` (the fourth Thursday of
November), `05-Mon-last` (the last Monday of May) and `easter`, each optionally moved by up to 99 days (`easter+1`,
`11-Thu-4+1`). The format is in [`specs/spec.md`](specs/spec.md) (5.1). The shipped calendars, their sources and
licenses are described in [`calendars/README.md`](calendars/README.md).

## Projects

| Project | Contents |
|---|---|
| `Anniversary.Shared` | everything shared by all platforms: model, `AnniversaryBook` (all logic), windows and dialogs, embedded defaults |
| `Anniversary.Desktop` | the Windows/macOS/Linux executable (`Program.cs`) |
| `Anniversary.Tests` | xUnit tests in the classic style: real objects and real files, no mocks |
| `icon/` | the app icon and the script that draws it (see [`icon/README.md`](icon/README.md)) |
| `calendars/` | calendars (name days and holidays) and the script that builds them |

## License

[MIT](LICENSE). The bundled name-day calendars come from third-party sources under their own licenses (MIT and
CC0); see [`calendars/README.md`](calendars/README.md) and
[`calendars/THIRD-PARTY-NOTICES.md`](calendars/THIRD-PARTY-NOTICES.md).
