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
        Assert.Null(settings.DataFile);
        Assert.Equal(7, settings.NumberOfDaysAhead);
        Assert.True(settings.DisplayYearsOnBirthdays);
        Assert.Null(settings.LoadError);
        Assert.Equal(_folder.PathOf("usersettings.json"), settings.FilePath);
    }

    [Fact]
    public void SavedValuesLoadAgain()
    {
        var settings = new UserSettings
        {
            FilePath = _folder.PathOf("usersettings.json"),
            Theme = AppTheme.Dark,
            Calendar = "Anniversary.Calendar.Slovak.json",
            DataFile = _folder.PathOf("Family.json"),
            NumberOfDaysAhead = 14,
            DisplayYearsOnBirthdays = false
        };
        settings.Save();

        UserSettings loaded = UserSettings.Load(settings.FilePath);

        Assert.Equal(AppTheme.Dark, loaded.Theme);
        Assert.Equal("Anniversary.Calendar.Slovak.json", loaded.Calendar);
        Assert.Equal(_folder.PathOf("Family.json"), loaded.DataFile);
        Assert.Equal(14, loaded.NumberOfDaysAhead);
        Assert.False(loaded.DisplayYearsOnBirthdays);
        Assert.Null(loaded.LoadError);
    }

    [Fact]
    public void SaveWritesEveryKey()
    {
        var settings = new UserSettings { FilePath = _folder.PathOf("usersettings.json") };

        settings.Save();

        string json = File.ReadAllText(settings.FilePath);
        foreach (string key in new[] { "theme", "calendar", "dataFile", "numberOfDaysAhead", "displayYearsOnBirthdays" })
        {
            Assert.Contains($"\"{key}\":", json);
        }
        Assert.Contains("\"numberOfDaysAhead\": 7", json);
    }

    [Fact]
    public void InvalidFileGivesDefaults()
    {
        string path = _folder.Write("usersettings.json", "{ \"theme\": 42");

        UserSettings settings = UserSettings.Load(path);

        Assert.Equal(AppTheme.System, settings.Theme);
        Assert.NotNull(settings.LoadError);
    }

    [Fact]
    public void CommentsAndTrailingCommasAreAllowed()
    {
        string path = _folder.Write("usersettings.json", """
            {
              // Two weeks ahead
              "numberOfDaysAhead": 14,
              "displayYearsOnBirthdays": false,
            }
            """);

        UserSettings settings = UserSettings.Load(path);

        Assert.Null(settings.LoadError);
        Assert.Equal(14, settings.NumberOfDaysAhead);
        Assert.False(settings.DisplayYearsOnBirthdays);
    }

    [Fact]
    public void FileThatCouldNotBeReadIsNotSavedOver()
    {
        const string Typo = """{ "dataFile": "/Users/jan/Family.json", "numberOfDaysAhead": "14" }""";
        string path = _folder.Write("usersettings.json", Typo);
        UserSettings settings = UserSettings.Load(path);

        settings.Theme = AppTheme.Dark;
        settings.Save();

        Assert.Contains("numberOfDaysAhead", settings.LoadError);
        Assert.Null(settings.DataFile);
        Assert.Equal(Typo, File.ReadAllText(path));
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
