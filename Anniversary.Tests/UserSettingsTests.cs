using Anniversary.Model;

namespace Anniversary.Tests;

/// <summary>
/// Tests of <see cref="UserSettings"/> (spec 5.5).
/// </summary>
public sealed class UserSettingsTests : IDisposable
{
    private readonly TempFolder _folder = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _folder.Dispose();
    }

    [Fact]
    public void MissingFileGivesDefaults()
    {
        UserSettings settings = UserSettings.Load(_folder.PathOf("usersettings.json"));

        Assert.Equal(AppTheme.System, settings.Theme);
        Assert.Null(settings.Calendar);
        Assert.Equal(_folder.PathOf("usersettings.json"), settings.FilePath);
    }

    [Fact]
    public void SavedValuesLoadAgain()
    {
        var settings = new UserSettings
        {
            FilePath = _folder.PathOf("usersettings.json"),
            Theme = AppTheme.Dark,
            Calendar = "Anniversary.Calendar.Slovak.json"
        };
        settings.Save();

        UserSettings loaded = UserSettings.Load(settings.FilePath);

        Assert.Equal(AppTheme.Dark, loaded.Theme);
        Assert.Equal("Anniversary.Calendar.Slovak.json", loaded.Calendar);
    }

    [Fact]
    public void InvalidFileGivesDefaults()
    {
        string path = _folder.Write("usersettings.json", "{ \"theme\": 42");

        Assert.Equal(AppTheme.System, UserSettings.Load(path).Theme);
    }

    [Fact]
    public void ThemeIsReadCaseInsensitively()
    {
        string path = _folder.Write("usersettings.json", "{ \"theme\": \"light\" }");

        Assert.Equal(AppTheme.Light, UserSettings.Load(path).Theme);
    }

    [Fact]
    public void SaveCreatesMissingFolder()
    {
        var settings = new UserSettings { FilePath = _folder.PathOf(Path.Combine("new", "folder", "usersettings.json")) };

        settings.Save();

        Assert.True(File.Exists(settings.FilePath));
    }
}
