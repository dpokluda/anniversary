# Anniversary

Anniversary shows the name days, birthdays and anniversaries of the people you care about, for today or any other
day, together with the day's names or holidays from a calendar you choose. It runs on Windows, macOS and Linux.

## Starting it

Each download comes in two forms: a package that installs the app (a Windows setup, a macOS `.dmg` or a Linux
AppImage), or a plain archive (`.zip` or `.tar.gz`) with the single-file app in a folder you keep wherever you like.
The first time, Windows and macOS warn you about the app; "Opening it the first time" below shows what to click.

- **Windows:** run `Anniversary-…-win-x64-setup.exe` (`win-arm64` for Windows on Arm). It installs the app just for
  you, without admin rights, adds it to the Start menu and starts it. From the `.zip`, extract it and double-click
  `Anniversary.exe` instead.
- **macOS:** use `osx-arm64` on Apple Silicon Macs (M1 and newer) and `osx-x64` on Intel Macs. Open the `.dmg`, drag
  Anniversary onto Applications and start it from there. The `.tar.gz` holds the bare app instead: double-click
  `Anniversary` and leave the Terminal window that opens with it.
- **Linux:** the `.AppImage` is the whole app in one file. Make it executable (right-click it, **Properties**, turn on
  **Executable as Program**, or run `chmod +x Anniversary-*.AppImage`), then double-click it; tools such as Gear Lever
  add it to the applications menu. From the `.tar.gz`, run `./Anniversary` in the extracted folder. The app needs
  fontconfig, libX11, libICE and libSM, which desktop distributions already have.

## Opening it the first time

Anniversary isn't signed by Microsoft or Apple (that needs a paid certificate), so the first time you open it, Windows
or macOS warns you that it can't check the app. Allow it once as below; after that it opens normally. A new version
may need this again.

**On macOS:**

1. Open Anniversary. When macOS says "Anniversary.app" Not Opened, click **Done** (not Move to Trash).
2. Open **System Settings ▸ Privacy & Security** and scroll down to **Security**. Next to the message that Anniversary
   was blocked, click **Open Anyway** (**Open** on some macOS versions). The button is there for about an hour after
   step 1; if it's gone, open Anniversary again.
3. Confirm with **Open Anyway** and your password or Touch ID.

In Terminal, `xattr -dr com.apple.quarantine /Applications/Anniversary.app` does the same in one step (for the
`.tar.gz`, run `xattr -d com.apple.quarantine Anniversary` in its folder).

**On Windows:**

1. Your browser may say the file isn't commonly downloaded. Keep it; in Edge, choose **⋯ ▸ Keep ▸ Show more ▸ Keep
   anyway**.
2. When you run it, Windows may say "Windows protected your PC". Click **More info**, then **Run anyway**.

**On Linux** nothing asks; the AppImage only has to be executable (see above).

## Using it

The window shows the day, the calendar's names or holidays for it, your people's name days, their birthdays and
anniversaries (with the years), and everything coming up in the next 7 days.

- **Yesterday** and **Tomorrow** move by one day. Click the date to pick any day.
- The other commands are in the **⋯** button at the top right, or right-click anywhere in the window (on macOS they're
  also in the menu bar): Today, Change Date, Find, Find Next, Edit People, Data File, Calendar, Theme, Start at Login
  and About.
- **Find** looks for a name among your people and in the calendar.
- Shortcuts: Ctrl+F finds, F3 finds the next one, Ctrl+O opens a data file and Ctrl+Shift+S saves it under a new name
  (macOS: ⌘F, ⌘G, ⌘O and ⇧⌘S).
- **Theme** switches between light and dark, or follows your system (macOS: **View ▸ Appearance**).
- **Start at Login** starts the app whenever you log in; choose it again to stop that. On macOS it's called **Open at
  Login** (also in the **Anniversary** menu) and works for the app in Applications. Windows (Task Manager ▸ Startup
  apps) and macOS (System Settings ▸ General ▸ Login Items) list it too, and turning it off there works as well.

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

If you turned on **Start at Login**, turn it off first (the Windows uninstaller does that for you). Then, on Windows,
uninstall Anniversary in **Settings ▸ Apps**; on macOS, drag it from Applications to the Trash; on Linux, delete the
AppImage. From an archive, delete the app's folder. Your people and settings stay in the data folder above; delete it
too to remove everything.

## License

Anniversary is free and open source under the MIT License, Copyright (c) David Pokluda. The name-day calendars come from
other sources under their own licenses, listed in `THIRD-PARTY-NOTICES.md`, which comes with the app. Source code and
issues: https://github.com/dpokluda/anniversary
