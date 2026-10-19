# Research: Cross-Platform Rewrite

Decisions made while planning and building phase 1 on 2026-10-09, plus one re-check on 2026-10-10. Sources: the
session of 2026-10-09 in which the spec, plan and mocks were written with the maintainer (and the macOS session that
night), the session of 2026-10-10, the "Framework" table and "Decisions (all confirmed with the user)" in
[docs/plan.md](../../docs/plan.md), [docs/spec.md](../../docs/spec.md) (§11 lists every deliberate change from v4),
`specs/copilot/handover-linux.md` and the code at the move to GitHub (`git show 67a4092:…`). The session excerpts
hold the requests and short answers, so alternatives are listed only where a source names them.

## Framework: Avalonia 12 on .NET 10

- **Decision**: Avalonia UI 12 (12.1.x; 12.1.4 was the latest when the plan was written) with the Fluent theme, on
  .NET 10.
- **Rationale**: It stays in C# and .NET 10, so v4's model and logic carry over almost verbatim. Its strongest platform
  is the desktop (real windows, modal dialogs, the native macOS menu bar through `NativeMenu`), which matches the
  priority order Windows → macOS → Linux → iOS → Android, and the same XAML views can run on iOS and Android later. It
  is MIT-licensed and mature.
- **Alternatives considered**: .NET MAUI (Mac Catalyst on macOS, no Linux, mobile-first with a weaker desktop); Uno
  Platform (works everywhere, but heavier for a tiny app); Eto.Forms (no mobile); Flutter, Tauri or Compose
  Multiplatform (they leave C#, so the model and logic can't be reused).
- **Source**: docs/plan.md, "Framework: Avalonia UI 12 on .NET 10"; session 2026-10-09 (the first request ended
  "First, recommend the right framework/UI library for the job").

## Desktop first

- **Decision**: Phase 1 is the desktop: Windows (x64), macOS (Apple Silicon and Intel) and Linux (x64). iOS is phase 2
  and Android phase 3 (optional); their projects will sit next to `Anniversary.Desktop` without restructuring.
- **Rationale**: The maintainer's priority order: "Windows, MacOS, Linux, iOS and finally Android".
- **Alternatives considered**: none recorded.
- **Source**: docs/plan.md, Decisions ("Scope"); docs/spec.md §1.

## Tools: Rider is enough

- **Decision**: JetBrains Rider with the AvaloniaRider plugin; Visual Studio isn't needed.
- **Rationale**: The plugin gives code completion and a live preview for Avalonia's XAML, and Avalonia apps debug
  normally in Rider. Rider also runs on macOS and Linux, and iOS builds need a Mac with Xcode either way.
- **Alternatives considered**: Visual Studio Community, which would only help with the WinForms designer (for v4) and
  .NET MAUI tooling (not used); it is Windows-only now that Visual Studio for Mac has been discontinued.
- **Source**: session 2026-10-09; docs/plan.md ("Tooling"); docs/spec.md §2 ("IDE").

## The maintainer's coding guidelines

- **Decision**: Honour v4's coding style (there is no `.editorconfig`, so v4 is the reference: Allman braces, braces on
  every `if`, `_camelCase` fields, `On…` handlers, file-scoped namespaces, nullable reference types); keep the code
  concise (no MVVM, service interfaces or extra layers, and a new type only if it clearly removes complexity); XML
  documentation on every class, struct, record, interface and enum and on every public member; classic (Detroit)
  tests with real objects and hand-written helpers, and no mocking library.
- **Rationale**: The maintainer's goal of a "concise code base that is easy to navigate, understand and maintain". The
  assistant checked for repository style settings before writing the rules down.
- **Alternatives considered**: mockist tests with a mocking library; the maintainer asked for the classic style
  described by Martin Fowler.
- **Source**: session 2026-10-09; docs/spec.md §3; docs/plan.md, "Coding guidelines (user-directed)". They are now
  Principles I–III of the constitution.

## Three projects, code-behind and one logic class

- **Decision**: `Anniversary.Shared` (all views and logic, no platform start-up), `Anniversary.Desktop` (the WinExe
  `Anniversary`) and `Anniversary.Tests`. Views use code-behind, as v4's forms did. All non-UI logic is in one plain
  class, `AnniversaryBook`, which returns a `DayInfo` record for a day.
- **Rationale**: The app is too small to benefit from MVVM. The shared assembly is named `Anniversary.Shared` because
  a library called `Anniversary` would clash with the executable, which must stay `Anniversary.exe` as in v4.
- **Alternatives considered**: an MVVM framework (CommunityToolkit.Mvvm, ReactiveUI) and a DI container, both ruled
  out by the spec; a shared project named `Anniversary` (the name clash).
- **Source**: docs/plan.md, Decisions ("UI wiring", "Projects"); docs/spec.md §2–4.

## Dates: `DateOnly`, passed in

- **Decision**: All logic uses `DateOnly`, and `AnniversaryBook` never reads the clock: the caller passes the day in.
  Only the UI reads today, with `DateOnly.FromDateTime(DateTime.Today)`, at start-up and for the Today command.
- **Rationale**: The logic never needs a time of day, and no logic depends on the current time, so passing the date in
  keeps it simple to test (C11).
- **Alternatives considered**: v4's `DateTime` (replaced, C11); `TimeProvider` (deliberately not used).
- **Source**: docs/spec.md §6 ("Dates and today"), C11; session 2026-10-09 (the spec and plan were changed to
  `DateOnly` with the day passed in and no `TimeProvider`).

## Where the commands live: the ⋯ menu

- **Decision**: v4's system-menu commands go into one commands menu, opened by a ⋯ button at the top-right of the
  "Today is" section and by a right-click anywhere in the window; on macOS the menu bar has them too (Go, Find, View ▸
  Appearance, and About Anniversary in the app menu).
- **Rationale**: The Win32 system menu isn't portable (C1). Top-right is where Windows 11 apps (Snipping Tool, Sticky
  Notes, Photos, Edge) put "⋯ See more", GNOME apps put their main menu and Android puts ⋮; macOS apps use the menu
  bar instead.
- **Alternatives considered**: a ⋯ button at the bottom next to Quit, as the first mocks had it; the maintainer asked
  whether that was an established pattern ("We don't want to invent something completely new"), and it moved. On
  2026-10-10 the maintainer asked again, remembering hamburger menus at the top-left: a ☰ at the top-left usually opens
  a navigation drawer for pages, while this app has only commands, so the ⋯ stayed; a Linux-only ☰ would be a one-line
  change that the assistant didn't recommend.
- **Source**: session 2026-10-09 and session 2026-10-10; docs/plan.md, Decisions ("Commands"); docs/spec.md §7.2, C1.

## Shortcuts

- **Decision**: Ctrl+F for Find… and F3 for Find Next, as in v4; ⌘F and ⌘G on macOS. Clicking the date opens Change
  Date.
- **Rationale**: Platform conventions (C7).
- **Alternatives considered**: none recorded.
- **Source**: docs/plan.md, Decisions ("Shortcuts"); docs/spec.md §7.2, C7.

## Window size and resizing like v4

- **Decision**: The window opens centred at 312 × 468 device-independent pixels, can shrink to 280 × 420, and doesn't
  remember its size (as in v4). It is a five-row grid (`Auto, Auto, Auto, *, Auto`): every section stretches to the
  width, the Namedays and Birthdays boxes have a fixed height of 70, and only "Next N days" takes the extra height.
- **Rationale**: 312 × 468 is v4's 464 × 701 client size, which v4 gave in physical pixels at 150 % scaling, so on
  screen it matches the v4 window. The maintainer asked that the elements anchor "so that the resizing behavior is
  correct (as demonstrated on the v4 screenshot)"; the minimum height keeps about three lines visible in "Next N days".
- **Alternatives considered**: none recorded. (docs/plan.md first said "464 × 700 (as v4)"; the docs update of
  2026-10-10 corrected it to 312 × 468.)
- **Source**: session 2026-10-09; docs/spec.md §7.2 ("Resizing" table); docs/plan.md, Decisions ("Window size and
  resizing").

## A compact Change Date dialog and a smaller calendar drop-down

- **Decision**: The Change Date dialog keeps about v4's size, and the date picker's calendar is a popup that extends
  below the dialog's edge. The calendar itself is scaled down from Fluent's touch size (about 300 × 320) to about
  200 × 210: the app's font size, 28 × 26 day cells without gaps and smaller arrows, plus a `TemplateApplied` handler
  for the sizes fixed inside the calendar's template.
- **Rationale**: The maintainer: "I can imagine the actual dialog to be similar size as before with the drop down
  going down". Fluent's calendar is sized for touch (14 px text, 40 px day cells, a 294 × 290 minimum).
- **Alternatives considered**: the first mocks' dialog, tall enough to hold the calendar.
- **Source**: session 2026-10-09; docs/spec.md §7.1 ("Calendar drop-down") and §7.3; docs/plan.md, Decisions ("Change
  Date dialog").

## A native look on each OS

- **Decision**: Fluent with `DensityStyle="Compact"`; the platform's standard UI font size set in `App.Initialize` as
  `AppFontSize`: 12 px on Windows (Segoe UI 9 pt, as in v4) and 13 px on macOS and Linux, with the names one pixel
  larger (`AppNamesFontSize`) and the date four (`AppDateFontSize`), mirroring v4's 10 pt and 12 pt; the system font
  (no bundled font); theme-aware brushes for the sections, value boxes, captions and subtle 1 px separators; menu items
  and separators with the same inset, and a smaller submenu arrow.
- **Rationale**: The app should blend with each OS and keep v4's compact layout; Fluent's 14 px default isn't used
  anywhere. The macOS-specific values came later the same night with [004](../004-macos-native-look/spec.md), after
  the first macOS run looked heavier than Windows.
- **Alternatives considered**: Fluent's default sizes; bundling the Inter font (the spec says not to).
- **Source**: docs/spec.md §7.1; docs/plan.md, Decisions ("Look"); the handover's "Decisions already made".

## File lookup: next to the executable, the app-data folder, then built in

- **Decision**: Each file (`appsettings.json`, the calendar, the people file) is resolved on its own: an absolute path
  is used as is; otherwise the first copy in the executable's folder, then in the per-user app-data folder
  (`%APPDATA%\Anniversary`, `~/Library/Application Support/Anniversary`, `~/.config/Anniversary`), then the embedded
  default with that file name. The build doesn't copy JSON files next to the executable.
- **Rationale**: It works with macOS app bundles and mobile, and it fixes v4's dependence on the current directory
  (C2). The real file names became the defaults (C5).
- **Alternatives considered**: v4's lookup in the current directory, with `calendar.txt` and `data.txt` as defaults.
- **Source**: docs/spec.md §5.3, C2, C5; docs/plan.md, Decisions ("Data files").

## An empty built-in people list

- **Decision**: The built-in `Anniversary.Data.json` is `{ "data": [] }`; when there are no people, the Birthdays and
  Anniversaries box shows a hint instead of nothing. The user's v4 file goes into the app-data folder (or next to the
  executable), where the lookup prefers it.
- **Rationale**: No personal names or birth dates are compiled into the binaries (C13). The maintainer: "With respect
  to default dataset. I would think it should be empty." (user decision).
- **Alternatives considered**: the made-up sample people the spec had before that answer.
- **Source**: session 2026-10-09; docs/plan.md, Decisions ("Data files"); docs/spec.md §5.1, C13.

## Load problems become warnings

- **Decision**: Loading never throws and never shows UI. Problems are collected in `AnniversaryBook.Warnings`, and the
  main window shows each one in a message after it opens, titled "Configuration Error" for the configuration and
  "Data Error" otherwise. The app continues with v4's default configuration, an empty calendar or no people.
- **Rationale**: Loading stays UI-free and testable (C6). A missing calendar now says "Calendar file not found" (C3).
- **Alternatives considered**: v4's behaviour, which C6 changes; v4 also said "Data file not found" for both files (C3).
- **Source**: docs/spec.md §5.4, C3, C6.

## Theme switching and `usersettings.json`

- **Decision**: ⋯ ▸ Theme ▸ System / Light / Dark (macOS: View ▸ Appearance) sets the application's theme variant;
  System is `ThemeVariant.Default`, which follows the OS live with no OS-specific code. The choice is kept in
  `usersettings.json` in the app-data folder, a file only the app writes, and applied before the main window is
  created, so it opens without a flash of the wrong theme.
- **Rationale**: The maintainer asked for a menu choice where System also follows scheduled changes (macOS, or Windows
  with PowerToys) (C12). `appsettings.json` stays the user's file, never written by the app (C9).
- **Alternatives considered**: writing the choice into `appsettings.json` (v4 had an unused `SaveConfiguration`, which
  was dropped). The persistence choice was made by the assistant while the maintainer was away.
- **Source**: session 2026-10-09; docs/plan.md, Decisions ("Theme switching"); docs/spec.md §5.5, §7.7, C9, C12.

## v4's formats and models

- **Decision**: v4's people, calendar and configuration formats are read unchanged (case-insensitive names; comments
  and trailing commas allowed). v4's models are ported with XML docs; `Model.Calendar` becomes `CalendarData` and
  `Model.Anniversary` becomes `AnniversaryData`. A person is listed once per day (C4), and the years are hidden when the
  date's year is after the shown day (C16).
- **Rationale**: The renames avoid clashes with Avalonia's `Calendar` control and the `Anniversary` namespace (C8). v4
  could list a person twice (C4), and v4 data uses `9999` for "year unknown", which would print a negative age such as
  "Alena (-7973)"; that was found while testing with the real v4 data (C16).
- **Alternatives considered**: none recorded.
- **Source**: docs/spec.md §5.1, §5.2, §6.1, C4, C8, C16.

## Messages without WinForms

- **Decision**: `MessageDialog.Show(owner, title, message)` builds a small modal window in code: wrapped text and an
  OK button that is both default and cancel. The dialogs size to their content, aren't resizable, stay out of the
  taskbar and open centred on the main window.
- **Rationale**: It replaces WinForms' `MessageBox.Show`, without XAML or an extra package.
- **Alternatives considered**: none recorded.
- **Source**: docs/spec.md §7.3–7.6.

## Preview mocks instead of real renders

- **Decision**: The look was agreed on approximate HTML mocks (`docs/screens/mockup.html`, exported as
  `*.preview.png` with a "PREVIEW MOCK" banner); the sizes in the spec win over the mocks.
- **Rationale**: The assistant first set up a throwaway Avalonia prototype in the session folder to render
  screenshots, then switched to an HTML and CSS mockup that needs no build. Because that prototype was never compiled,
  the spec's implementation order starts with a build of an empty main window using the Avalonia 12 APIs the spec
  relies on, to be adjusted in the spec if an API differs.
- **Alternatives considered**: real renders from the prototype.
- **Source**: session 2026-10-09; docs/spec.md §7 and §13 (step 1); docs/plan.md, "Preview mocks".

## What the build found in Avalonia 12

- **Decision**: The macOS menu bar's items are found by their header and wired in code; radio items use the same
  `MenuItemToggleType` in both kinds of menu; sizes fixed inside control templates (the calendar's, the submenu
  arrow's) are changed in `TemplateApplied` class handlers in `App`; `global.json` opts `dotnet test` into
  Microsoft.Testing.Platform.
- **Rationale**: `NativeMenuItem` has no routed `Click` event and gets no generated fields; values set inside a
  template can't be overridden by styles; xUnit v3 on the .NET 10 SDK needs Microsoft.Testing.Platform, so tests run
  with `dotnet test --project Anniversary.Tests`.
- **Alternatives considered**: none recorded.
- **Source**: `specs/copilot/handover-linux.md`, "Avalonia 12 gotchas learned on Windows"; docs/spec.md §4, §7.1 and
  §7.2.

## Publishing one self-contained file per OS

- **Decision**: `dotnet publish Anniversary.Desktop -c Release -r <rid> --self-contained -p:PublishSingleFile=true
  -p:IncludeNativeLibrariesForSelfExtract=true` for `win-x64`, `osx-arm64`, `osx-x64` and `linux-x64`. Release builds
  leave out the `.pdb` and `.xml` files and compress the single file. The executable is called `Anniversary`, and on
  macOS it is a plain executable, not an `.app`.
- **Rationale**: One file per platform that runs without .NET installed; all four can be built on Windows, but running
  them needs the target OS.
- **Alternatives considered**: installers and MSIX, a macOS `.app` bundle with signing and notarization, and trimming
  or AOT, all non-goals of phase 1 (packages came with [010](../010-install-packages/spec.md)).
- **Source**: docs/spec.md §1 and §10; `README.md` and `Anniversary.Desktop/Anniversary.Desktop.csproj` at `67a4092`.

## App icon

- **Decision**: v4's tear-off calendar (yellow binding, red "24", blue lines, stacked pages), redrawn as vector shapes
  by `icon/Build-Icon.ps1` (PowerShell 7 with System.Drawing, so Windows only) into `App.ico` (16–256 px), `App.png`
  (256 px, for About), `App.icns` (up to 1024 px) and `App-1024.png`. Small sizes drop details, and 16 px is drawn as
  crisp pixels with a 3 × 5 pixel font.
- **Rationale**: Sharp on high-DPI and Retina screens (C17).
- **Alternatives considered**: v4's icon, 16 colours at 16 and 32 px only.
- **Source**: `icon/README.md` at `67a4092`; docs/spec.md C17.

## Verification approach

- **Decision**: Tests with real files for the logic (38 cases); a live run on Windows (v4's size, native font sizes,
  light and dark); a first run on macOS by the maintainer the same evening; the Linux builds published from the Mac the
  next morning, and the app checked by the maintainer on macOS and Fedora that day.
- **Rationale**: Windows was the development machine and the first priority; the other platforms need their own OS to
  run.
- **Alternatives considered**: none recorded.
- **Source**: `specs/copilot/handover-linux.md` ("Where things stand"); session of 2026-10-09 on macOS (the
  maintainer: "The program works fine but it seems that the fonts, line spacing is better on Windows"); session of
  2026-10-10 (the maintainer validated the code on macOS and Fedora).
