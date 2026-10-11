using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Anniversary;

/// <summary>
/// Starts this copy of the app when the user logs in (spec 7.12), the way each OS expects: a value under the
/// registry's Run key on Windows, a login item on macOS and an XDG autostart entry on Linux.
/// </summary>
internal static class StartAtLogin
{
    private const string Name = "Anniversary";
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string AutostartFileName = "anniversary.desktop";
    private const string ObjectiveC = "/usr/lib/libobjc.A.dylib";
    private const long MacEnabled = 1;
    private const long MacRequiresApproval = 2;

    /// <summary>
    /// Gets whether this copy can start at login: always on Windows and Linux; on macOS 13 and later, as an app bundle.
    /// </summary>
    public static bool IsSupported => OperatingSystem.IsWindows() || OperatingSystem.IsLinux()
        || (OperatingSystem.IsMacOSVersionAtLeast(13) && ProgramPath.Contains(".app/Contents/MacOS/", StringComparison.Ordinal));

    /// <summary>
    /// Gets the program that starts at login: the running executable, or the AppImage file it runs from.
    /// </summary>
    public static string ProgramPath
    {
        get
        {
            string program = Environment.ProcessPath ?? string.Empty;
            string? appImage = Environment.GetEnvironmentVariable("APPIMAGE");
            string? appDir = Environment.GetEnvironmentVariable("APPDIR");

            // Only when this executable runs from the AppImage: an AppImage app that started this one passes its own on.
            bool inAppImage = !string.IsNullOrEmpty(appImage) && !string.IsNullOrEmpty(appDir)
                && program.StartsWith(appDir.TrimEnd('/') + "/", StringComparison.Ordinal);
            return inAppImage ? appImage! : program;
        }
    }

    // $XDG_CONFIG_HOME/autostart, or ~/.config/autostart: .NET resolves ApplicationData the same way on Linux.
    private static string AutostartFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "autostart");

    /// <summary>
    /// Tells whether this copy starts at login.
    /// </summary>
    /// <returns><see langword="true"/> if it does; <see langword="false"/> if it doesn't or that can't be read.</returns>
    public static bool IsOn()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return IsOnWindows(RunKey, ApprovedKey);
            }
            if (OperatingSystem.IsMacOS())
            {
                return MacStatus() == MacEnabled;
            }
            return IsOnLinux(AutostartFolder);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            return false;
        }
    }

    /// <summary>
    /// Turns starting this copy at login on or off.
    /// </summary>
    /// <param name="on">Whether this copy should start at login.</param>
    /// <exception cref="InvalidOperationException">It couldn't be changed; the message says why.</exception>
    public static void Set(bool on)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                SetWindows(on, RunKey, ApprovedKey);
            }
            else if (OperatingSystem.IsMacOS())
            {
                SetMac(on);
            }
            else
            {
                SetLinux(on, AutostartFolder);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Tells whether the Run value starts this copy and isn't turned off in Task Manager or Settings.
    /// </summary>
    /// <param name="runKey">The Run key under HKEY_CURRENT_USER (tests use a scratch key).</param>
    /// <param name="approvedKey">The key where Task Manager and Settings keep their flags.</param>
    /// <returns><see langword="true"/> if this copy starts at login.</returns>
    [SupportedOSPlatform("windows")]
    internal static bool IsOnWindows(string runKey, string approvedKey)
    {
        using RegistryKey? run = Registry.CurrentUser.OpenSubKey(runKey);
        if (!string.Equals(run?.GetValue(Name) as string, Quote(ProgramPath), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Turning it off in Task Manager or Settings keeps the Run value and writes flags: an odd first byte means off.
        using RegistryKey? approved = Registry.CurrentUser.OpenSubKey(approvedKey);
        return approved?.GetValue(Name) is not byte[] { Length: > 0 } flags || (flags[0] & 1) == 0;
    }

    /// <summary>
    /// Writes the Run value for this copy, or deletes it, and clears the flags of Task Manager and Settings.
    /// </summary>
    /// <param name="on">Whether this copy should start at login.</param>
    /// <param name="runKey">The Run key under HKEY_CURRENT_USER (tests use a scratch key).</param>
    /// <param name="approvedKey">The key where Task Manager and Settings keep their flags.</param>
    [SupportedOSPlatform("windows")]
    internal static void SetWindows(bool on, string runKey, string approvedKey)
    {
        using (RegistryKey? approved = Registry.CurrentUser.OpenSubKey(approvedKey, writable: true))
        {
            approved?.DeleteValue(Name, throwOnMissingValue: false);
        }

        if (on)
        {
            using RegistryKey run = Registry.CurrentUser.CreateSubKey(runKey);
            run.SetValue(Name, Quote(ProgramPath));
        }
        else
        {
            using RegistryKey? run = Registry.CurrentUser.OpenSubKey(runKey, writable: true);
            run?.DeleteValue(Name, throwOnMissingValue: false);
        }
    }

    /// <summary>
    /// Tells whether the autostart entry starts this copy and isn't turned off.
    /// </summary>
    /// <param name="autostartFolder">The autostart folder (tests use a temporary one).</param>
    /// <returns><see langword="true"/> if this copy starts at login.</returns>
    internal static bool IsOnLinux(string autostartFolder)
    {
        string file = Path.Combine(autostartFolder, AutostartFileName);
        if (!File.Exists(file))
        {
            return false;
        }

        string[] lines = File.ReadAllLines(file);
        return lines.Contains(ExecLine(ProgramPath))
            && !lines.Contains("Hidden=true")
            && !lines.Contains("X-GNOME-Autostart-enabled=false");
    }

    /// <summary>
    /// Writes the autostart entry for this copy, or deletes it.
    /// </summary>
    /// <param name="on">Whether this copy should start at login.</param>
    /// <param name="autostartFolder">The autostart folder (tests use a temporary one).</param>
    internal static void SetLinux(bool on, string autostartFolder)
    {
        string file = Path.Combine(autostartFolder, AutostartFileName);
        if (!on)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            return;
        }

        Directory.CreateDirectory(autostartFolder);
        File.WriteAllLines(file,
        [
            "[Desktop Entry]",
            "Type=Application",
            "Name=Anniversary",
            "Comment=Name days, birthdays and anniversaries for any day",
            ExecLine(ProgramPath),
            "Terminal=false"
        ]);
    }

    /// <summary>
    /// Returns the autostart entry's <c>Exec</c> line that starts a program.
    /// </summary>
    /// <param name="program">The program's full path.</param>
    /// <returns>The line, with the path quoted as the Desktop Entry specification requires.</returns>
    internal static string ExecLine(string program)
    {
        // Inside quotes, " ` $ and \ get a backslash and % is doubled; the file format then doubles every backslash.
        string quoted = Regex.Replace(program, @"[""`$\\]", @"\$0").Replace("%", "%%");
        return $"Exec=\"{quoted.Replace(@"\", @"\\")}\"";
    }

    private static string Quote(string path)
    {
        return $"\"{path}\"";
    }

    // SMAppService.mainApp's status: 0 not registered, 1 enabled, 2 requires approval, 3 not found.
    private static long MacStatus()
    {
        return SendMessage(MainAppService(), Selector("status"));
    }

    private static void SetMac(bool on)
    {
        // An app opened from Downloads or the disk image runs from a temporary copy, which would be gone by the next login.
        if (on && ProgramPath.Contains("/AppTranslocation/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Move Anniversary to the Applications folder, open it from there and try again.");
        }

        string selector = on ? "registerAndReturnError:" : "unregisterAndReturnError:";
        if (SendMessage(MainAppService(), Selector(selector), out IntPtr error) == 0)
        {
            IntPtr description = SendMessage(error, Selector("localizedDescription"));
            throw new InvalidOperationException(Marshal.PtrToStringUTF8(SendMessage(description, Selector("UTF8String"))));
        }
        if (on && MacStatus() == MacRequiresApproval)
        {
            throw new InvalidOperationException("Allow Anniversary in System Settings ▸ General ▸ Login Items.");
        }
    }

    // Apple's login-item API (macOS 13 and later) is in the ServiceManagement framework, which is loaded on first use.
    private static IntPtr MainAppService()
    {
        NativeLibrary.Load("/System/Library/Frameworks/ServiceManagement.framework/ServiceManagement");
        return SendMessage(GetClass("SMAppService"), Selector("mainAppService"));
    }

    [DllImport(ObjectiveC, EntryPoint = "objc_getClass")]
    private static extern IntPtr GetClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(ObjectiveC, EntryPoint = "sel_registerName")]
    private static extern IntPtr Selector([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static extern byte SendMessage(IntPtr receiver, IntPtr selector, out IntPtr error);
}
