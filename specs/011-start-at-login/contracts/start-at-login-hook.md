# Contract: the `App.StartAtLogin` hook and the `StartAtLogin` class

How the shared UI reaches the platform code, as built on 2026-10-10. The shared library (`Anniversary.Shared`) has no
platform code; the desktop executable (`Anniversary.Desktop`) supplies Start at Login through one property. System spec:
[docs/spec.md](../../../docs/spec.md) 7.12 and 8; formats: [os-registration.md](os-registration.md).

## `App.StartAtLogin` (`Anniversary.Shared/App.axaml.cs`)

```csharp
public static (Func<bool> IsOn, Action<bool> Set)? StartAtLogin { get; set; }
```

| Member | Meaning |
|---|---|
| `IsOn()` | `true` if this copy starts when the user logs in (and the OS's settings haven't turned it off); `false` otherwise or if that can't be read. Never throws for I/O, access or security errors. |
| `Set(on)` | turns starting this copy at login on or off; throws `InvalidOperationException` whose message is the reason shown to the user |
| `null` (the property) | the OS can't do it for this copy: the menu items (and the macOS app menu's separator before Open at Login) are hidden |

- **Set by** the desktop executable's `Program.Main`, before Avalonia starts. Nothing else writes it.
- **Used by** `MainWindow` only: for the items' visibility (once, in the constructor), for their check marks
  (`UpdateStartAtLoginChecks`) and for a click (`ToggleStartAtLogin`, which calls `Set(!IsOn())`).
- **Future heads** (the mobile apps) leave it `null`.

## `Program.Main` (`Anniversary.Desktop/Program.cs`)

```csharp
if (StartAtLogin.IsSupported)
{
    App.StartAtLogin = (StartAtLogin.IsOn, StartAtLogin.Set);
}
return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
```

## `StartAtLogin` (`Anniversary.Desktop/StartAtLogin.cs`)

`internal static class StartAtLogin`, visible to `Anniversary.Tests` through `InternalsVisibleTo` in
`Anniversary.Desktop/Anniversary.Desktop.csproj`.

```csharp
public static bool IsSupported { get; }      // Windows, Linux; macOS 13+ inside an .app (".app/Contents/MacOS/")
public static string ProgramPath { get; }    // the running executable, or $APPIMAGE inside an AppImage
public static bool IsOn();                   // false on IOException, UnauthorizedAccessException, SecurityException
public static void Set(bool on);             // those exceptions become InvalidOperationException(ex.Message, ex)

// Test seams: the folder and keys are parameters, so the tests use scratch ones
[SupportedOSPlatform("windows")]
internal static bool IsOnWindows(string runKey, string approvedKey);
[SupportedOSPlatform("windows")]
internal static void SetWindows(bool on, string runKey, string approvedKey);
internal static bool IsOnLinux(string autostartFolder);
internal static void SetLinux(bool on, string autostartFolder);
internal static string ExecLine(string program);
```

- `IsOn` and `Set` pick the platform with `OperatingSystem.IsWindows()` and `IsMacOS()`; anything else uses the Linux
  autostart entry.
- Windows calls use `Software\Microsoft\Windows\CurrentVersion\Run` and
  `Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run` under `HKEY_CURRENT_USER`.
- Linux calls use `{ApplicationData}/autostart`.
- macOS has no seam: `MacStatus`, `SetMac`, `MainAppService` and the four P/Invokes into the Objective-C runtime are
  private, and the login item is checked by hand.

## Tests

`Anniversary.Tests/StartAtLoginTests.cs` (14 cases) calls the seams with a `TempFolder` and the scratch keys
`Software\Anniversary.Tests\Run` and `Software\Anniversary.Tests\StartupApproved\Run`; the list is in
[tasks.md](../tasks.md) and docs/spec.md §9. The test project references `Anniversary.Desktop` for this.
