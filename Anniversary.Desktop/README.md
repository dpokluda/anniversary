# Anniversary

Anniversary shows the name days, birthdays and anniversaries of the people you care about, for today or any other
day, together with the day's names or holidays from a calendar you choose. It runs on Windows, macOS and Linux.

## Starting it

The app is the single file `Anniversary` (`Anniversary.exe` on Windows). There's nothing to install: keep this folder
wherever you like and start the app from there.

- **Windows:** double-click `Anniversary.exe`. The app isn't signed, so the first time Windows may say "Windows
  protected your PC": click **More info**, then **Run anyway**.
- **macOS:** use the `osx-arm64` download on Apple Silicon Macs (M1 and newer) and `osx-x64` on Intel Macs. The app
  isn't signed by Apple, so macOS blocks it the first time: double-click `Anniversary`, close the warning, open
  **System Settings ▸ Privacy & Security**, click **Open Anyway** next to the message about Anniversary, and
  double-click it again. A Terminal window opens with it; leave it open while you use the app. (In Terminal, you can
  instead run `xattr -d com.apple.quarantine Anniversary` once in this folder, then `./Anniversary`.)
- **Linux:** in a terminal, in this folder, run `./Anniversary`. It needs fontconfig, libX11, libICE and libSM, which
  desktop distributions already have.

## Using it

The window shows the day, the calendar's names or holidays for it, your people's name days, their birthdays and
anniversaries (with the years), and everything coming up in the next 7 days.

- **Yesterday** and **Tomorrow** move by one day. Click the date to pick any day.
- The other commands are in the **⋯** button at the top right, or right-click anywhere in the window (on macOS they're
  also in the menu bar): Today, Change Date, Find, Find Next, Edit People, Data File, Calendar, Theme and About.
- **Find** looks for a name among your people and in the calendar.
- Shortcuts: Ctrl+F finds, F3 finds the next one, Ctrl+O opens a data file and Ctrl+Shift+S saves it under a new name
  (macOS: ⌘F, ⌘G, ⌘O and ⇧⌘S).
- **Theme** switches between light and dark, or follows your system (macOS: **View ▸ Appearance**).

## Your people

Choose **⋯ ▸ Edit People…** to add people with their name day, birthday and anniversaries (a wedding, for example).
**Save** writes them to your data file.

- **Sharing with family:** **⋯ ▸ Data File ▸ Show in Folder** opens the folder with your data file, so you can send the
  file to them. They save it somewhere permanent, such as Documents, and choose **⋯ ▸ Data File ▸ Open…** (macOS:
  **File ▸ Open Data File…**). The app then uses that file and saves changes into it.
- **The same people on several computers:** choose **⋯ ▸ Data File ▸ Save As…** and save the file into a cloud-drive
  folder (OneDrive, iCloud Drive, Dropbox, …), then choose **Open…** on each other computer. A change made on one
  computer shows on the others once the cloud has synced it and you switch back to the app. If the file was changed
  elsewhere while you were editing, the app asks before overwriting it.

Each computer remembers which data file it uses.

## Calendars

The line under the date shows the calendar's names for the day. The app starts with the calendar for your region (for
example Czech in Czechia), or the United States calendar of holidays and observances elsewhere. Choose another with
**⋯ ▸ Calendar** (macOS: **View ▸ Calendar**): name days for Austria, Bulgaria, Czechia, France, Hungary, Italy,
Latvia, Poland, Russia and Slovakia, the United States, or International Days (well-known UN days plus a few fun ones
such as Pi Day and International Beer Day).

## Files and settings

Your people and settings are in the app's data folder:

- Windows: `%APPDATA%\Anniversary`
- macOS: `~/Library/Application Support/Anniversary`
- Linux: `~/.config/Anniversary`

`Anniversary.Data.json` holds your people (unless you chose another data file), and `usersettings.json` your theme,
calendar and data file. For more settings, create `appsettings.json` there, for example to look 14 days ahead and hide
the years:

```json
{ "numberOfDaysAhead": 14, "displayYearsOnBirthdays": false }
```

## Removing it

Delete the app's folder. Your people and settings stay in the data folder above; delete it too to remove everything.

## License

Anniversary is free and open source under the MIT License, Copyright (c) David Pokluda. The name-day calendars come from
other sources under their own licenses, listed in `THIRD-PARTY-NOTICES.md` next to the app. Source code and issues:
https://github.com/dpokluda/anniversary
