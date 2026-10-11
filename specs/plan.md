# Anniversary v5 – Plan

Companion to [spec.md](spec.md), which is the detailed source of truth for the implementation. This file records the
approach, the decisions made while planning, and the work breakdown.

## Problem

Anniversary v4 (`Tools\csharp\Anniversary.v4` in the `pokluda` repository) is a small .NET 10 WinForms app. Its main window shows the date and the
calendar name day, then Namedays, Birthdays & Anniversaries, Next N days, and Yesterday/Tomorrow/Quit buttons. It also
has Change Date, Find and About dialogs. Extra commands live in the Win32 system menu (via P/Invoke), with Ctrl+F / F3
shortcuts. Data comes from JSON files next to the exe.

Goal: rewrite it as v5 (originally `Tools\csharp\Anniversary.v5` in `pokluda`, now the `anniversary` repository), running on Windows, macOS, Linux, iOS and Android, in that
priority order.

## Framework: Avalonia UI 12 on .NET 10

| Option | Win | macOS | Linux | iOS | Android | Fit |
|---|---|---|---|---|---|---|
| **Avalonia** | ✅ | ✅ native app | ✅ first-class | ✅ | ✅ | **Best**: desktop-first, matches the priority order |
| .NET MAUI | ✅ | ⚠️ Mac Catalyst | ❌ | ✅ | ✅ | Mobile-first; no Linux; weaker desktop |
| Uno Platform | ✅ | ✅ | ✅ | ✅ | ✅ | Viable, but heavier for a tiny app |
| Eto.Forms | ✅ | ✅ | ✅ (GTK) | ❌ | ❌ | No mobile |
| Flutter / Tauri / Compose MP | ✅ | ✅ | ✅ | ✅/⚠️ | ✅ | Leaves C#, so the model and logic can't be reused |

Why Avalonia: it stays in C#/.NET 10, so the v4 model and logic are reused almost verbatim. Its strongest platform is
the desktop (real windows, modal dialogs, the native macOS menu bar via `NativeMenu`). The same XAML views run on
iOS/Android later. It's MIT-licensed and mature.

Tooling: JetBrains Rider with the AvaloniaRider plugin is enough. Visual Studio isn't needed.

## Architecture

Three projects, code-behind views (no MVVM), and one plain logic class:

```
anniversary\  (repository root)
  Anniversary.slnx, Directory.Build.props, README.md
  .github\workflows\       ci.yml: build, test and publish on Windows, macOS and Linux
  specs\                   spec.md, plan.md, copilot\handover.md, screens\ (v5 preview mocks)
  Anniversary.Shared\      shared Avalonia library: Model\, AnniversaryBook, DayInfo, Views\, App.axaml, Assets\, Defaults\
  Anniversary.Desktop\     WinExe "Anniversary" for Windows/macOS/Linux (Program.cs)
  Anniversary.Tests\       xUnit, classic style
```

Later phases add `Anniversary.iOS` (and maybe `Anniversary.Android`) next to `Anniversary.Desktop`.

## Decisions (all confirmed with the user)

| Topic | Decision |
|---|---|
| Framework | Avalonia UI 12 on .NET 10. |
| Scope | Phase 1 is desktop (Windows, macOS, Linux). iOS is phase 2, Android phase 3 (optional). |
| UI wiring | Code-behind, as in v4. No MVVM framework, because the app is too small to benefit. |
| Projects | 3. The shared project is named `Anniversary.Shared` so the exe can stay `Anniversary.exe` without an assembly name clash. |
| Commands (v4 system menu) | "⋯" button at the top-right of the "Today is" section, plus the same menu on right-click, plus the macOS menu bar. This follows the established "more" menu position in Windows 11, GNOME and Android apps. |
| Shortcuts | Ctrl/⌘+F (Find), F3 / ⌘G (Find Next); clicking the date opens Change Date. |
| Data files | Looked up next to the exe, then in the user app-data folder, then in built-in (embedded) defaults. The embedded people file is **empty** (user decision), so no personal data is compiled into the binaries. Phase 1 ships only the Czech name-day calendar; another calendar can be selected with `calendarFile`. |
| Name-day calendars | **⋯ ▸ Calendar ▸** (first called Nameday Calendar) lists the calendar files found (`Anniversary.Calendar.*.json` in the exe folder, app-data and the built-in calendars); the choice is remembered in `usersettings.json`. **10 calendars ship**: Czech (v4), Austrian, Bulgarian, French, Hungarian, Italian, Polish, Russian, Slovak (radoone/nameday-mcp, MIT) and Latvian (official list, CC0). They are generated reproducibly by `calendars\Build-Calendars.ps1` from pinned sources and validated; sources, checks and exclusions are in `calendars\README.md`. Left out: Croatian (damaged data), Greek (incomplete, partly tied to Easter), Swedish/Finnish (copyrighted), and others with no verified open source yet. |
| Holiday calendars | Calendar files may use **rule keys** for floating dates: `11-Thu-4`, `05-Mon-last` and `easter`, each with an optional ±n-day offset such as `easter+1` (spec 5.1, 6.3). A **US** calendar ships: the 11 federal holidays of 5 U.S.C. 6103(a) plus well-known observances. The **default calendar follows the OS region** (the region part of each calendar's `locale`, e.g. Czech in Czechia) and is **US** where no calendar matches; a menu choice or `calendarFile` wins. The US entries are written in `Build-Calendars.ps1`, each with its source, and tests check the federal holidays against OPM's schedules. The Czech calendar gets Velký pátek and Velikonoční pondělí. The menu was renamed from Nameday Calendar to **Calendar** (user decision), and calendars without a locale (International Days) come last, after a separator. Names on the same day are joined with ", ". An **International Days** calendar (56 well-known UN days plus five fun days, no locale) ships too. Not yet: observed days off, Orthodox Easter. |
| Sharing the data file | **⋯ ▸ Data File ▸ Open… / Save As… / Show in Folder** (macOS: File menu, ⌘O / ⇧⌘S). Open… uses the chosen file **where it is** (never copied; the user chose this over asking or a separate Import), so a family member opens the file you sent, and each of your computers opens the same file on a cloud drive. The choice is remembered per computer in `usersettings.json` and wins over `dataFile` in `appsettings.json`. A missing file keeps its link and is picked up when it appears; the file is reloaded when it changes elsewhere; the editor asks before overwriting such changes or saving over a file that couldn't be loaded. No merging. Double-click to open waits for the installers (needs its own file extension). |
| People editor | **Edit People…** dialog with a grid (Name, Nameday, Birthday, Anniversaries with an Edit… button) and a nested Anniversaries dialog (date → name). Saves the data file (to app-data when the data came from the empty default). Mocked and specified (spec 5.6, 7.8, 7.9); user-approved for phase 1; implemented after the core app. |
| Dates | `DateOnly` in all logic. The day is passed in; only the UI reads "today". No `TimeProvider`. |
| Window size and resizing | Default 312 × 468 device-independent pixels (312 × 492 on macOS): v4's 464 × 701, which v4 gave in physical pixels at 150 % scaling, so it matches v4 on screen. Resizing follows v4's anchors: only "Next N days" grows taller, and everything stretches to the width. |
| Change Date dialog | Compact, v4 size; the calendar drop-down extends below the dialog. |
| Look | Fluent theme, following the OS light/dark setting; layout mirrors v4. |
| Theme switching | **Theme ▸ System / Light / Dark** in the ⋯/right-click menu (macOS: View ▸ Appearance). System follows OS changes live, including scheduled ones (macOS Auto, PowerToys Light Switch). The choice is remembered in an app-written `usersettings.json` in the app-data folder; `appsettings.json` is never written. The persistence choice was made autonomously while the user was away. |

## Coding guidelines (user-directed)

- Honor the v4 coding style (Allman braces, braces on every `if`, `_camelCase` fields, `On…` handlers,
  file-scoped namespaces, nullable enabled).
- Keep the code concise: no unnecessary types, layers, interfaces or packages.
- XML documentation on every class, struct, record and interface, and on every public member.
- Tests use the classic (Detroit) style: real objects, real JSON files in a temp folder, hand-written helpers or fakes
  only where needed, and no mocking library.

## Preview mocks

`specs\screens\*.preview.png` are **approximate HTML mocks**, not real Avalonia renders. Each has a "PREVIEW MOCK"
banner. Their source is `specs\screens\mockup.html`.

## Work breakdown (phase 1)

Phase 1 is done and verified on Windows, macOS and Linux.

1. **spec** – Create `specs\` (spec, plan, mocks). ✅
2. **scaffold** – `Anniversary.slnx`, `Directory.Build.props`, the three projects. ✅
3. **core** – Model (including `UserSettings`), `AnniversaryBook`, `DayInfo`, embedded defaults. ✅
4. **core-tests** – Classic xUnit tests (spec section 9), including `UserSettingsTests`. ✅
5. **main-view** – `App.axaml` styles and `MainWindow` (code-behind, v4 resizing). ✅
6. **dialogs** – Change Date, Find, About, `MessageDialog`. ✅
7. **commands-menu** – "⋯" menu, right-click menu, Theme submenu, shortcuts, macOS `NativeMenu`. ✅
8. **assets** – Icon, default data/config files as embedded resources. ✅
9. **publish** – Self-contained publish for win-x64, osx-arm64, osx-x64, linux-x64; `README.md`. ✅
10. **people-editor** – People editor + Anniversaries dialog, saving, `SaveTests`. ✅
11. **calendar-chooser** – Calendar discovery, the Nameday Calendar menu (now **Calendar**), remembering the choice, `CalendarTests`, Czech metadata. ✅
12. **ci** – GitHub Actions: build and test on Windows, macOS and Linux, then publish every RID, now including
    `win-arm64` and `linux-arm64` (spec 10). ✅
13. **calendar-rules** – Rule keys for floating dates, the US holiday calendar, the default calendar chosen by the OS
    region (US otherwise), and the Czech Easter
    holidays (spec 5.1, 5.7, 6.3). ✅
14. **data-file** – Data File ▸ Open… / Save As… / Show in Folder, the chosen file remembered per computer, reloading
    when it changes elsewhere, and prompts before overwriting such changes or a file that couldn't be loaded
    (spec 5.5, 5.6, 7.8, 7.11). ✅
15. **international-days** – The International Days calendar: 56 well-known UN days plus five fun days (spec 5.7). ✅

Next (the user's plan, in this order):
1. **user-readme** ✅ – `Anniversary.Desktop/README.md`, the guide for users, shipped as `README.md` next to the
   executable in every archive and package; spec 10.
2. **packages** ✅ – besides the `.zip` and `.tar.gz`, built by `packaging/Build-Package.ps1` on each RID's own OS
   and attached by CI (spec 10): a macOS `.dmg` with an ad-hoc signed `Anniversary.app` (Apple Silicon and Intel;
   built and started on macOS), a Linux **AppImage** (user decision over an RPM: one file that runs on Fedora and other
   distributions, nothing to install) and an Inno Setup installer for Windows (per user, x64 and arm64). Checked by
   the user on Windows, macOS (Apple Silicon) and Fedora (acceptance criterion 15); the ARM and Intel-Mac builds
   haven't been run on matching machines.
3. **start-at-login** ✅ – **⋯ ▸ Start at Login** (macOS: **Open at Login**, also in the app menu), a check item in
   the app rather than an installer option (user decision), for the copy that's running: the Run key on Windows (chosen
   over a Startup-folder shortcut), Apple's login-item API on macOS (`SMAppService`, tested with the ad-hoc
   signed app on macOS 27), XDG autostart on Linux; the Windows uninstaller removes the Run value (spec 7.12). Checked
   by the user on Windows, macOS and Fedora (acceptance criterion 16).
4. **releases** – GitHub Releases from a version tag (5.1.0), with all packages and archives.

Signing (Apple Developer ID, a Windows code-signing certificate) isn't decided; unsigned builds need the first-launch
steps the user README explains.
Follow-ups: one task per additional name-day calendar (license checked, every date verified against its source).
Future phases: **ios** (phase 2), **android** (phase 3, optional).

## Holiday and observance calendars (done)

Countries without name days (the US first) show public holidays or observance days on the "Today is" line.

1. **calendar-rules** ✅ – Rule keys next to `MM-dd`, resolved by `AnniversaryBook` for the displayed year (so "Next N
   days" works across a year boundary): `11-Thu-4` (nth weekday), `05-Mon-last` (last weekday) and `easter`, each with
   an optional offset of up to 99 days (`easter+1` Easter Monday, `easter-2` Good Friday, `11-Thu-4+1` Black Friday).
   Specified in spec 5.1 and 6.3.
2. **us-holidays** ✅ – `Anniversary.Calendar.US.json`, the default where no calendar matches the OS region: the federal holidays (New Year's Day,
   Martin Luther King Jr. Day, Presidents' Day, Memorial Day, Juneteenth, Independence Day, Labor Day, Columbus Day,
   Veterans Day, Thanksgiving, Christmas Day) and well-known observances (Groundhog Day, Valentine's Day, Mardi Gras,
   St. Patrick's Day, Daylight Saving Time begins/ends, April Fools' Day, Good Friday, Easter, Earth Day, Cinco de
   Mayo, Mother's Day, Flag Day, Father's Day, Patriot Day, Halloween, Black Friday, Christmas Eve, New Year's Eve).
   The Czech calendar also got Velký pátek and Velikonoční pondělí.
3. **international-days** ✅ – `Anniversary.Calendar.International.json` ("International Days", no locale): 56
   well-known days chosen with the user from the UN list of international days (each with its UN resolution), plus the
   fun days Pi Day 3/14, Star Wars Day 5/4, World Emoji Day 7/17, Talk Like a Pirate Day 9/19 and International Beer
   Day (first Friday of August). The full UN list (239 days on 187 dates, many with long names) was too much for the
   small window. Compiled ourselves; no commercial "national day" sites (copyrighted).

Further ideas, not agreed yet:
- Observed days off: when a fixed federal holiday falls on a weekend, also show the Friday before or the Monday after
  (e.g. Friday 3 July 2026). New Year's Day on a Saturday is observed on 31 December of the year before.
- Orthodox Easter (e.g. `orthodox+1`) for Bulgarian, Russian and Greek holidays and name days.
- Overlay calendars: a holiday calendar shown together with the name-day calendar (e.g. Czech name days + US
  holidays).
- Saints' days (Spain, Portugal, Latin America); Greek name days (some Easter-relative).
- Swedish/Finnish name days: their official lists are copyrighted, so check licensing first.
- Astronomical days (solstices, equinoxes, full moons): computed, no licensing issue.

## Idea: installers and start at login (done)

Each platform has a package besides its archive (spec 10, **packages** above) and can start at login (spec 7.12,
**start-at-login** above).

| | Windows | macOS | Linux |
|---|---|---|---|
| **Install** (done) | Per-user Inno Setup installer into `%LOCALAPPDATA%\Programs\Anniversary`: Start-menu shortcut and uninstaller, no admin rights. MSIX is possible but needs a trusted signing certificate. | The `.app` bundle in a `.dmg`, dragged into Applications; macOS apps rarely use installers. | An AppImage (user decision over an RPM). A Flatpak later, if wanted. |
| **Start at login** (done) | The `Anniversary` value under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` | `SMAppService.mainApp` (macOS 13+), listed under Login Items ▸ Open at Login; a LaunchAgent would show up under "Allow in the Background" with a notification instead | `~/.config/autostart/anniversary.desktop` (XDG autostart) |

Suggestions (signing not decided yet):
- **No data file is installed** (done). The app already starts with the empty built-in people list and creates
  `Anniversary.Data.json` in the app-data folder on the first Save (spec 5.6). An installer must never put a data file
  next to the executable: Program Files is read-only for users, so saving would fail.
- **Start at login as an in-app setting** (done): per user, no admin rights, works for copies that weren't installed,
  and can be turned off again. The platform code is in the desktop project, with no packages, and the Windows
  uninstaller removes the entry.
- **Signing:** unsigned downloads get a SmartScreen warning on Windows and are blocked by Gatekeeper on macOS (Open
  Anyway, explained in the user README). Signing needs a Windows code-signing certificate and a paid Apple Developer
  account (Developer ID signing and notarization).
- **Releases:** CI builds each package on its own OS. Pushing a version tag (e.g. `v5.1.0`) could attach them,
  with the plain downloads, to a GitHub Release. The version is raised by hand in `Directory.Build.props` when
  something ships.
- **Uninstalling keeps the user's data** (the app-data folder; done for the Windows installer).


