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
  specs\                   spec.md, plan.md, screens\ (v5 preview mocks)
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
| Name-day calendars | **⋯ ▸ Nameday Calendar ▸** lists the calendar files found (`Anniversary.Calendar.*.json` in the exe folder, app-data and the built-in calendars); the choice is remembered in `usersettings.json`. **10 calendars ship**: Czech (v4), Austrian, Bulgarian, French, Hungarian, Italian, Polish, Russian, Slovak (radoone/nameday-mcp, MIT) and Latvian (official list, CC0). They are generated reproducibly by `calendars\Build-Calendars.ps1` from pinned sources and validated; sources, checks and exclusions are in `calendars\README.md`. Left out: Croatian (damaged data), Greek (incomplete, partly tied to Easter), Swedish/Finnish (copyrighted), and others with no verified open source yet. |
| People editor | **Edit People…** dialog with a grid (Name, Nameday, Birthday, Anniversaries with an Edit… button) and a nested Anniversaries dialog (date → name). Saves the data file (to app-data when the data came from the empty default). Mocked and specified (spec 5.6, 7.8, 7.9); user-approved for phase 1; implemented after the core app. |
| Dates | `DateOnly` in all logic. The day is passed in; only the UI reads "today". No `TimeProvider`. |
| Window size and resizing | Default 464 × 700 (as v4). Resizing follows v4's anchors: only "Next N days" grows taller, and everything stretches to the width. |
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

1. **spec** – Create `specs\` (spec, plan, mocks). ✅
2. **scaffold** – `Anniversary.slnx`, `Directory.Build.props`, the three projects.
3. **core** – Model (including `UserSettings`), `AnniversaryBook`, `DayInfo`, embedded defaults.
4. **core-tests** – Classic xUnit tests (spec section 9), including `UserSettingsTests`.
5. **main-view** – `App.axaml` styles and `MainWindow` (code-behind, v4 resizing).
6. **dialogs** – Change Date, Find, About, `MessageDialog`.
7. **commands-menu** – "⋯" menu, right-click menu, Theme submenu, shortcuts, macOS `NativeMenu`.
8. **assets** – Icon, default data/config files as embedded resources.
9. **publish** – Self-contained publish for win-x64, osx-arm64, osx-x64, linux-x64; `README.md`.
10. **people-editor** – People editor + Anniversaries dialog, saving, `SaveTests`.
11. **calendar-chooser** – Calendar discovery, the Nameday Calendar menu, remembering the choice, `CalendarTests`, Czech metadata.

Follow-ups: one task per additional name-day calendar (license checked, every date verified against its source).
Future phases: **ios** (phase 2), **android** (phase 3, optional).

## Idea: holiday and observance calendars (agreed direction, not started)

Countries without name days (the US first) could show public holidays or observance days on the "Today is" line.
Calendar keys are fixed `MM-dd` today, so floating dates can't be expressed yet.

1. **calendar-rules** – Extend the calendar format with rule keys next to `MM-dd`, resolved by `AnniversaryBook` for
   the displayed year (so "Next N days" works across a year boundary):
   - nth weekday of a month, e.g. `11-Thu-4` (Thanksgiving), `01-Mon-3` (MLK Day);
   - last weekday of a month, e.g. `05-Mon-last` (Memorial Day);
   - Easter-relative, e.g. `easter+1` (Easter Monday), `easter-2` (Good Friday).
   Update the spec's calendar file format, add tests (including leap years and duplicate dates).
2. **us-holidays** – `Anniversary.Calendar.US.json`: federal holidays, fixed (New Year's Day, Juneteenth, Independence
   Day, Veterans Day, Christmas) and floating (MLK, Presidents', Memorial, Labor, Columbus, Thanksgiving).
3. **international-days** – An "International Days" calendar: the UN list of international days plus well-known fun
   days (Pi Day 3/14, Star Wars Day 5/4, World Emoji Day 7/17, Talk Like a Pirate Day 9/19, International Beer Day =
   first Friday of August). Compile it ourselves; don't copy commercial "national day" sites (copyrighted).

Further ideas, not agreed yet:
- Overlay calendars: a holiday calendar shown together with the name-day calendar (e.g. Czech name days + Czech public
  holidays, which need `easter` rules for Good Friday and Easter Monday).
- Saints' days (Spain, Portugal, Latin America); Greek name days (some Easter-relative).
- Swedish/Finnish name days: their official lists are copyrighted, so check licensing first.
- Astronomical days (solstices, equinoxes, full moons): computed, no licensing issue.


