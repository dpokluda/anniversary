# Contract: User guide outline

The user guide as written on 2026-10-10 (`Anniversary.Desktop/README.md` at commit `5868c42`, 81 lines), shipped as
`README.md` in every download ([publish-folder.md](publish-folder.md)). Behaviour: [spec.md](../spec.md); the rule for
it: [docs/spec.md](../../../docs/spec.md) §10. Later features changed some sections ([below](#later-changes)).

## Form

- **Audience**: the people who use the app. The repository's `README.md` is for developers and links to the guide.
- **Language**: English, like the app.
- **Markdown that reads well as plain text**: one `#` title, `##` sections, short paragraphs and `-` lists, bold for the
  names users see, backticks for files, folders and commands, one fenced `json` example. No tables, images or links;
  the web address is written out.
- **Length**: concise, about 80 lines.
- **Wording**: the app's own names (**⋯**, **Edit People…**, **⋯ ▸ Data File ▸ Open…**), menu paths with ▸, the OS's
  own texts in quotes ("Windows protected your PC"), and the macOS names and shortcuts in parentheses, for example
  "(macOS: **View ▸ Calendar**)".

## Sections, in this order

### Title and opening paragraph

- Heading: `# Anniversary`.
- Must say what the app shows (the name days, birthdays and anniversaries of the people you care about), for which day
  (today or any other day), that the day's names or holidays come from a calendar you choose, and that it runs on
  Windows, macOS and Linux.

### `## Starting it`

- The app is the single file `Anniversary` (`Anniversary.exe` on Windows); there's nothing to install; keep the folder
  wherever you like and start the app from there.
- **Windows**: double-click `Anniversary.exe`. It isn't signed, so the first time Windows may say "Windows protected
  your PC": click **More info**, then **Run anyway**.
- **macOS**: the `osx-arm64` download for Apple Silicon Macs (M1 and newer), `osx-x64` for Intel Macs. It isn't signed
  by Apple, so macOS blocks it the first time: double-click `Anniversary`, close the warning, open **System Settings ▸
  Privacy & Security**, click **Open Anyway** next to the message about Anniversary, and double-click it again. A
  Terminal window opens with it; leave it open while you use the app. Alternative: in Terminal, in this folder,
  `xattr -d com.apple.quarantine Anniversary` once, then `./Anniversary`.
- **Linux**: in a terminal, in this folder, `./Anniversary`. It needs fontconfig, libX11, libICE and libSM, which
  desktop distributions already have.
- It covers the plain archives, the only downloads at the time; each package step updates this section.

### `## Using it`

- What the window shows: the day, the calendar's names or holidays for it, your people's name days, their birthdays and
  anniversaries (with the years), and everything coming up in the next 7 days.
- **Yesterday** and **Tomorrow** move by one day; click the date to pick any day.
- The other commands are in the **⋯** button at the top right, or on right-click anywhere in the window (on macOS also
  in the menu bar): Today, Change Date, Find, Find Next, Edit People, Data File, Calendar, Theme and About.
- **Find** looks for a name among your people and in the calendar.
- Shortcuts: Ctrl+F finds, F3 finds the next one, Ctrl+O opens a data file, Ctrl+Shift+S saves it under a new name
  (macOS: ⌘F, ⌘G, ⌘O and ⇧⌘S).
- **Theme** switches between light and dark, or follows your system (macOS: **View ▸ Appearance**).

### `## Your people`

- **⋯ ▸ Edit People…** adds people with their name day, birthday and anniversaries (a wedding, for example); **Save**
  writes them to your data file.
- **Sharing with family**: **⋯ ▸ Data File ▸ Show in Folder** opens the folder with your data file, to send it. They
  save it somewhere permanent, such as Documents, and choose **⋯ ▸ Data File ▸ Open…** (macOS: **File ▸ Open Data
  File…**); the app then uses that file and saves changes into it.
- **The same people on several computers**: **⋯ ▸ Data File ▸ Save As…** into a cloud-drive folder (OneDrive, iCloud
  Drive, Dropbox, …), then **Open…** on each other computer. A change made on one computer shows on the others once the
  cloud has synced it and you switch back to the app. If the file was changed elsewhere while you were editing, the app
  asks before overwriting it.
- Each computer remembers which data file it uses.

### `## Calendars`

- The line under the date shows the calendar's names for the day.
- The app starts with the calendar for your region (for example Czech in Czechia), or the United States calendar of
  holidays and observances elsewhere.
- **⋯ ▸ Calendar** (macOS: **View ▸ Calendar**) offers name days for Austria, Bulgaria, Czechia, France, Hungary, Italy,
  Latvia, Poland, Russia and Slovakia, the United States, or International Days (well-known UN days plus a few fun ones
  such as Pi Day and International Beer Day). The countries are named in English; the menu shows most of them with
  their own name too, for example "Czechia (Česko)".

### `## Files and settings`

- The data folder: `%APPDATA%\Anniversary` (Windows), `~/Library/Application Support/Anniversary` (macOS),
  `~/.config/Anniversary` (Linux).
- `Anniversary.Data.json` holds your people (unless you chose another data file); `usersettings.json` your theme,
  calendar and data file.
- More settings: create `appsettings.json` there. The example looks 14 days ahead and hides the years:

  ```json
  { "numberOfDaysAhead": 14, "displayYearsOnBirthdays": false }
  ```

### `## Removing it`

- Delete the app's folder. Your people and settings stay in the data folder; delete it too to remove everything.

### `## License`

- Free and open source under the MIT License, Copyright (c) David Pokluda.
- The name-day calendars come from other sources under their own licenses, listed in `THIRD-PARTY-NOTICES.md` next to
  the app.
- Source code and issues: https://github.com/dpokluda/anniversary

## Rules for changes

- Every change users can see updates the guide in the same change (docs/spec.md §10).
- Each package step updates "Starting it" (the handover's next steps at the time).

## Later changes

- [010](../../010-install-packages/spec.md): "Starting it" covers the installer, the `.dmg` and the AppImage as well
  as the archives; a new section "Opening it the first time", after "Starting it", gives numbered steps through the
  macOS and Windows warnings, naming the buttons as the OS shows them; "Removing it" covers each package; "License"
  says that `THIRD-PARTY-NOTICES.md` "comes with the app".
- [011](../../011-start-at-login/spec.md): "Using it" lists and explains Start at Login (Open at Login on macOS);
  "Removing it" says to turn it off first.
- [012](../../012-single-settings-file/spec.md): "Files and settings" says that `usersettings.json` holds all settings,
  shows the two file-only ones in it and what happens if the app can't read the file; `appsettings.json` is no longer
  mentioned.
