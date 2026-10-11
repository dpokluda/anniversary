using Anniversary.Model;

namespace Anniversary.Tests;

/// <summary>
/// Tests of loading files and the lookup order (spec 5.3, 5.4).
/// </summary>
public sealed class LoadTests : IDisposable
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
    public void FirstFolderWinsOverSecond()
    {
        _exe.Write(TestFiles.DataName, TestFiles.People("""[{ "name": "From exe" }]"""));
        _appData.Write(TestFiles.DataName, TestFiles.People("""[{ "name": "From app-data" }]"""));

        AnniversaryBook book = AnniversaryBook.Load(TestFiles.Settings(), [_exe.FolderPath, _appData.FolderPath]);

        Assert.Equal("From exe", Assert.Single(book.People).Name);
        Assert.Equal(_exe.PathOf(TestFiles.DataName), book.DataFilePath);
    }

    [Fact]
    public void FallsBackToSecondFolder()
    {
        _appData.Write(TestFiles.CalendarName, TestFiles.Calendar("""{ "10-09": "App-data" }"""));
        _appData.Write(TestFiles.DataName, TestFiles.People("""[{ "name": "From app-data" }]"""));

        AnniversaryBook book = AnniversaryBook.Load(TestFiles.Settings(), [_exe.FolderPath, _appData.FolderPath]);

        Assert.Equal("From app-data", Assert.Single(book.People).Name);
        Assert.Equal("App-data", book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void FallsBackToBuiltInDefaults()
    {
        AnniversaryBook book = TestFiles.LoadInCulture("en-US", null);

        Assert.Equal("Anniversary.Calendar.US.json", book.CalendarFile);
        Assert.Equal("Independence Day", book.GetDay(new DateOnly(2026, 7, 4)).CalendarNames);
        Assert.Empty(book.People);
        Assert.Null(book.DataFilePath);
        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void DataFileNameInSettingsIsFoundInTheFolders()
    {
        _appData.Write("Family.Data.json", TestFiles.People("""[{ "name": "Family" }]"""));

        AnniversaryBook book = AnniversaryBook.Load(
            TestFiles.Settings(calendar: null, dataFile: "Family.Data.json"), [_exe.FolderPath, _appData.FolderPath]);

        Assert.Equal("Family", Assert.Single(book.People).Name);
        Assert.Equal(_appData.PathOf("Family.Data.json"), book.SavePathFor());
        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void MissingDataFileIsReportedAndSavedToTheAppDataFolder()
    {
        AnniversaryBook book = AnniversaryBook.Load(
            TestFiles.Settings(calendar: null, dataFile: "Missing.Data.json"), [_exe.FolderPath, _appData.FolderPath]);

        Assert.Equal(["Data file not found: Missing.Data.json"], book.Warnings);
        Assert.Empty(book.People);
        Assert.Equal(_appData.PathOf("Missing.Data.json"), book.SavePathFor());
    }

    [Fact]
    public void InvalidDataFileGivesWarningAndNoPeople()
    {
        _exe.Write(TestFiles.CalendarName, TestFiles.Calendar("{}"));
        _exe.Write(TestFiles.DataName, "{ not json");

        AnniversaryBook book = AnniversaryBook.Load(TestFiles.Settings(), [_exe.FolderPath]);

        Assert.StartsWith("Error loading data: ", Assert.Single(book.Warnings));
        Assert.Empty(book.People);
    }

    [Fact]
    public void SwitchingToAMissingOrInvalidCalendarIsReported()
    {
        _exe.Write(TestFiles.CalendarName, "{ \"calendar\": [");
        _exe.Write(TestFiles.DataName, TestFiles.People("[]"));
        AnniversaryBook book = AnniversaryBook.Load(TestFiles.Settings(calendar: null), [_exe.FolderPath]);

        book.UseCalendar("Anniversary.Calendar.Missing.json");

        Assert.Equal(["Calendar file not found: Anniversary.Calendar.Missing.json"], book.Warnings);

        book.UseCalendar(TestFiles.CalendarName);

        Assert.StartsWith("Error loading calendar: ", Assert.Single(book.Warnings));
        Assert.Equal(string.Empty, book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
    }

    [Fact]
    public void SettingsThatCouldNotBeReadAreReported()
    {
        string path = _appData.Write("usersettings.json", "{ \"theme\": \"Dark\", \"numberOfDaysAhead\": \"many\" }");

        AnniversaryBook book = AnniversaryBook.Load(UserSettings.Load(path), [_appData.FolderPath]);

        string warning = Assert.Single(book.Warnings);
        Assert.StartsWith($"{AnniversaryBook.ConfigurationWarningPrefix}: {path}: ", warning);
        Assert.EndsWith($"{Environment.NewLine}Using default settings.", warning);
        Assert.Equal(7, book.Settings.NumberOfDaysAhead);
        Assert.Equal(AppTheme.System, book.Settings.Theme);
    }
}
