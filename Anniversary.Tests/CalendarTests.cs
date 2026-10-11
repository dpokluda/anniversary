using Anniversary.Model;

namespace Anniversary.Tests;

/// <summary>
/// Tests of discovering and switching name-day calendars (spec 5.7).
/// </summary>
public sealed class CalendarTests : IDisposable
{
    private readonly TempFolder _exe = new();
    private readonly TempFolder _appData = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _exe.Dispose();
        _appData.Dispose();
    }

    [Fact]
    public void ShippedCalendarsAreDiscovered()
    {
        AnniversaryBook book = AnniversaryBook.Load(null);

        Assert.Equal(12, book.Calendars.Count);
        Assert.Contains(book.Calendars, c => c.FileName == "Anniversary.Calendar.US.json" && c.DisplayName == "United States");
        Assert.Contains(book.Calendars, c => c.FileName == "Anniversary.Calendar.Czech.json" && c.DisplayName == "Czechia (Česko)");
        Assert.Contains(book.Calendars, c => c.FileName == "Anniversary.Calendar.Latvian.json" && c.Locale == "lv-LV");
        Assert.Equal(
            book.Calendars.OrderBy(c => string.IsNullOrEmpty(c.Locale)).ThenBy(c => c.DisplayName, StringComparer.CurrentCulture),
            book.Calendars);
        Assert.Equal("International Days", book.Calendars[^1].DisplayName);
        Assert.Null(book.Calendars[^1].Locale);
    }

    [Fact]
    public void UserCalendarIsAddedToShippedOnes()
    {
        _appData.Write("Anniversary.Calendar.Test.json", TestFiles.Calendar("""{ "10-09": "Test" }""", name: "Test"));

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath, _appData.FolderPath);

        Assert.Equal(13, book.Calendars.Count);
        Assert.Equal(["International Days", "Test"], book.Calendars.TakeLast(2).Select(c => c.DisplayName));
    }

    [Fact]
    public void ExeFolderWinsOverAppDataWhichWinsOverEmbedded()
    {
        _appData.Write("Anniversary.Calendar.Czech.json", TestFiles.Calendar("""{ "10-09": "App-data" }""", name: "App-data Czech"));
        _exe.Write("Anniversary.Calendar.Czech.json", TestFiles.Calendar("""{ "10-09": "Exe" }""", name: "Exe Czech"));

        AnniversaryBook book = AnniversaryBook.Load("Anniversary.Calendar.Czech.json", _exe.FolderPath, _appData.FolderPath);

        Assert.Equal("Exe", book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
        Assert.Single(book.Calendars, c => c.FileName == "Anniversary.Calendar.Czech.json");
        Assert.Contains(book.Calendars, c => c.DisplayName == "Exe Czech");
    }

    [Fact]
    public void DisplayNameComesFromFileNameWhenMetadataIsMissing()
    {
        _appData.Write("Anniversary.Calendar.Klingon.json", """{ "calendar": { "01-01": "Worf" } }""");

        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);

        Assert.Contains(book.Calendars, c => c.FileName == "Anniversary.Calendar.Klingon.json" && c.DisplayName == "Klingon");
    }

    [Fact]
    public void PreferredCalendarIsUsedWhenAvailable()
    {
        AnniversaryBook book = AnniversaryBook.Load("Anniversary.Calendar.Slovak.json");

        Assert.Equal("Anniversary.Calendar.Slovak.json", book.CalendarFile);
        Assert.Equal("Dionýz", book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
    }

    [Fact]
    public void ConfiguredCalendarIsUsedWhenPreferredIsGone()
    {
        TestFiles.WriteConfig(_exe, calendarFile: "Anniversary.Calendar.Slovak.json");
        _exe.Write(TestFiles.DataName, TestFiles.People("[]"));

        AnniversaryBook book = AnniversaryBook.Load("Anniversary.Calendar.Removed.json", _exe.FolderPath);

        Assert.Equal("Anniversary.Calendar.Slovak.json", book.CalendarFile);
        Assert.Empty(book.Warnings);
    }

    [Theory]
    [InlineData("cs-CZ", "Anniversary.Calendar.Czech.json")]
    [InlineData("en-CZ", "Anniversary.Calendar.Czech.json")]
    [InlineData("sk-SK", "Anniversary.Calendar.Slovak.json")]
    [InlineData("de-AT", "Anniversary.Calendar.Austrian.json")]
    [InlineData("en-US", "Anniversary.Calendar.US.json")]
    [InlineData("en-GB", "Anniversary.Calendar.US.json")]
    [InlineData("de-DE", "Anniversary.Calendar.US.json")]
    [InlineData("cs", "Anniversary.Calendar.US.json")]
    [InlineData("", "Anniversary.Calendar.US.json")]
    public void CalendarForTheRegionIsUsedWhenNoneIsChosenOrConfigured(string culture, string expected)
    {
        AnniversaryBook book = TestFiles.LoadInCulture(culture, null);

        Assert.Equal(expected, book.CalendarFile);
        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void ChosenAndConfiguredCalendarsWinOverTheRegion()
    {
        TestFiles.WriteConfig(_exe, calendarFile: "Anniversary.Calendar.Slovak.json");
        _exe.Write(TestFiles.DataName, TestFiles.People("[]"));

        AnniversaryBook configured = TestFiles.LoadInCulture("cs-CZ", null, _exe.FolderPath);
        AnniversaryBook chosen = TestFiles.LoadInCulture("cs-CZ", "Anniversary.Calendar.Polish.json", _exe.FolderPath);

        Assert.Equal("Anniversary.Calendar.Slovak.json", configured.CalendarFile);
        Assert.Equal("Anniversary.Calendar.Polish.json", chosen.CalendarFile);
    }

    [Fact]
    public void UseCalendarSwitchesCalendarButKeepsPeople()
    {
        _appData.Write("Anniversary.Data.json", TestFiles.People("""[{ "name": "Jan", "namedate": "06-24" }]"""));
        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);

        book.UseCalendar("Anniversary.Calendar.Hungarian.json");

        Assert.Equal("Dénes, János", book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
        Assert.Equal(new DateOnly(2026, 8, 20), book.Find("István", new DateOnly(2026, 1, 1), true, true));
        Assert.Equal(["Jan"], book.GetDay(new DateOnly(2026, 6, 24)).Namedays);
    }

    [Fact]
    public void UseCalendarReplacesOldWarnings()
    {
        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);

        book.UseCalendar("Anniversary.Calendar.Missing.json");
        book.UseCalendar("Anniversary.Calendar.Czech.json");

        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void CalendarGivenByAbsolutePathIsListed()
    {
        string path = _appData.Write("My.Names.json", TestFiles.Calendar("""{ "10-09": "Mine" }""", name: "Mine"));
        TestFiles.WriteConfig(_exe, calendarFile: path);

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath);

        Assert.Contains(book.Calendars, c => c.FileName == path && c.DisplayName == "Mine");
    }

    [Fact]
    public void UserSettingsRemembersCalendar()
    {
        var settings = new UserSettings { FilePath = _appData.PathOf("usersettings.json"), Calendar = "Anniversary.Calendar.Polish.json" };
        settings.Save();

        Assert.Equal("Anniversary.Calendar.Polish.json", UserSettings.Load(settings.FilePath).Calendar);
    }
}
