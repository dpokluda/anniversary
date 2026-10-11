using Microsoft.Win32;

namespace Anniversary.Tests;

/// <summary>
/// Tests of <see cref="StartAtLogin"/> (spec 7.12): the Linux autostart entry in a temporary folder on every OS, and
/// the Windows Run value under a scratch registry key on Windows. macOS's login items are checked by hand.
/// </summary>
public sealed class StartAtLoginTests : IDisposable
{
    private const string ScratchKey = @"Software\Anniversary.Tests";
    private const string RunKey = ScratchKey + @"\Run";
    private const string ApprovedKey = ScratchKey + @"\StartupApproved\Run";

    private readonly TempFolder _folder = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _folder.Dispose();
        if (OperatingSystem.IsWindows())
        {
            Registry.CurrentUser.DeleteSubKeyTree(ScratchKey, throwOnMissingSubKey: false);
        }
    }

    [Fact]
    public void LinuxEntryStartsThisCopy()
    {
        StartAtLogin.SetLinux(true, _folder.FolderPath);

        string[] lines = File.ReadAllLines(_folder.PathOf("anniversary.desktop"));
        Assert.Equal("[Desktop Entry]", lines[0]);
        Assert.Contains("Type=Application", lines);
        Assert.Contains("Name=Anniversary", lines);
        Assert.Contains(StartAtLogin.ExecLine(StartAtLogin.ProgramPath), lines);
        Assert.True(StartAtLogin.IsOnLinux(_folder.FolderPath));
    }

    [Fact]
    public void LinuxEntryIsDeletedWhenTurnedOff()
    {
        Assert.False(StartAtLogin.IsOnLinux(_folder.FolderPath));

        StartAtLogin.SetLinux(true, _folder.FolderPath);
        StartAtLogin.SetLinux(false, _folder.FolderPath);

        Assert.False(File.Exists(_folder.PathOf("anniversary.desktop")));
        Assert.False(StartAtLogin.IsOnLinux(_folder.FolderPath));
    }

    [Fact]
    public void TurningOffWithoutAnEntryIsFine()
    {
        StartAtLogin.SetLinux(false, _folder.PathOf("missing"));

        Assert.False(StartAtLogin.IsOnLinux(_folder.PathOf("missing")));
    }

    [Fact]
    public void LinuxEntryForAnotherCopyIsOff()
    {
        _folder.Write("anniversary.desktop", "[Desktop Entry]\nType=Application\nExec=\"/opt/Other/Anniversary.AppImage\"\n");

        Assert.False(StartAtLogin.IsOnLinux(_folder.FolderPath));
    }

    [Theory]
    [InlineData("Hidden=true")]
    [InlineData("X-GNOME-Autostart-enabled=false")]
    public void LinuxEntryTurnedOffByTheDesktopIsOff(string flag)
    {
        _folder.Write("anniversary.desktop", $"[Desktop Entry]\n{StartAtLogin.ExecLine(StartAtLogin.ProgramPath)}\n{flag}\n");

        Assert.False(StartAtLogin.IsOnLinux(_folder.FolderPath));
    }

    [Theory]
    [InlineData("/home/david/Apps/Anniversary.AppImage", @"Exec=""/home/david/Apps/Anniversary.AppImage""")]
    [InlineData("/home/david/My Apps/Anniversary", @"Exec=""/home/david/My Apps/Anniversary""")]
    [InlineData(@"/tmp/a""b`c$d", @"Exec=""/tmp/a\\""b\\`c\\$d""")]
    [InlineData(@"/tmp/back\slash", @"Exec=""/tmp/back\\\\slash""")]
    [InlineData("/tmp/100%", @"Exec=""/tmp/100%%""")]
    public void ExecLineQuotesThePath(string program, string expected)
    {
        Assert.Equal(expected, StartAtLogin.ExecLine(program));
    }

    [Fact]
    public void WindowsRunValueStartsThisCopy()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("The registry exists only on Windows.");
            return;
        }

        StartAtLogin.SetWindows(true, RunKey, ApprovedKey);

        using (RegistryKey run = Registry.CurrentUser.OpenSubKey(RunKey)!)
        {
            Assert.Equal($"\"{StartAtLogin.ProgramPath}\"", run.GetValue("Anniversary"));
        }
        Assert.True(StartAtLogin.IsOnWindows(RunKey, ApprovedKey));

        StartAtLogin.SetWindows(false, RunKey, ApprovedKey);

        using (RegistryKey run = Registry.CurrentUser.OpenSubKey(RunKey)!)
        {
            Assert.Null(run.GetValue("Anniversary"));
        }
        Assert.False(StartAtLogin.IsOnWindows(RunKey, ApprovedKey));
    }

    [Fact]
    public void WindowsRunValueForAnotherCopyIsOff()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("The registry exists only on Windows.");
            return;
        }

        using (RegistryKey run = Registry.CurrentUser.CreateSubKey(RunKey))
        {
            run.SetValue("Anniversary", @"""C:\Other\Anniversary.exe""");
        }

        Assert.False(StartAtLogin.IsOnWindows(RunKey, ApprovedKey));
    }

    [Fact]
    public void WindowsTaskManagerFlagsAreFollowedAndClearedWhenTurnedOn()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("The registry exists only on Windows.");
            return;
        }

        StartAtLogin.SetWindows(true, RunKey, ApprovedKey);
        using (RegistryKey approved = Registry.CurrentUser.CreateSubKey(ApprovedKey))
        {
            approved.SetValue("Anniversary", new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, RegistryValueKind.Binary);
        }
        Assert.True(StartAtLogin.IsOnWindows(RunKey, ApprovedKey));

        // What Task Manager writes when the app is turned off there: 3, then the time it was turned off.
        using (RegistryKey approved = Registry.CurrentUser.CreateSubKey(ApprovedKey))
        {
            approved.SetValue("Anniversary", new byte[] { 3, 0, 0, 0, 0x2E, 0x55, 0x8A, 0x3C, 0x4F, 0x1D, 0xDB, 0x01 }, RegistryValueKind.Binary);
        }
        Assert.False(StartAtLogin.IsOnWindows(RunKey, ApprovedKey));

        StartAtLogin.SetWindows(true, RunKey, ApprovedKey);

        Assert.True(StartAtLogin.IsOnWindows(RunKey, ApprovedKey));
        using (RegistryKey approved = Registry.CurrentUser.OpenSubKey(ApprovedKey)!)
        {
            Assert.Null(approved.GetValue("Anniversary"));
        }
    }
}
