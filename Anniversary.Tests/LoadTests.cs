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
        TestFiles.WriteConfig(_appData);
        _exe.Write(TestFiles.DataName, TestFiles.People("""[{ "name": "From exe" }]"""));
        _appData.Write(TestFiles.DataName, TestFiles.People("""[{ "name": "From app-data" }]"""));

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath, _appData.FolderPath);

        Assert.Equal("From exe", Assert.Single(book.People).Name);
        Assert.Equal(_exe.PathOf(TestFiles.DataName), book.DataFilePath);
    }

    [Fact]
    public void FallsBackToSecondFolder()
    {
        TestFiles.WriteConfig(_appData);
        _appData.Write(TestFiles.CalendarName, TestFiles.Calendar("""{ "10-09": "App-data" }"""));
        _appData.Write(TestFiles.DataName, TestFiles.People("""[{ "name": "From app-data" }]"""));

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath, _appData.FolderPath);

        Assert.Equal("From app-data", Assert.Single(book.People).Name);
        Assert.Equal("App-data", book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void FallsBackToBuiltInDefaults()
    {
        AnniversaryBook book = TestFiles.LoadInCulture("en-US", null);

        Assert.Null(book.Config.CalendarFile);
        Assert.Equal("Anniversary.Calendar.US.json", book.CalendarFile);
        Assert.Equal("Independence Day", book.GetDay(new DateOnly(2026, 7, 4)).CalendarNames);
        Assert.Empty(book.People);
        Assert.Null(book.DataFilePath);
        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void AbsolutePathInConfigIsUsed()
    {
        string calendarPath = _appData.Write("Elsewhere.json", TestFiles.Calendar("""{ "10-09": "Absolute" }"""));
        TestFiles.WriteConfig(_exe, calendarFile: calendarPath);
        _exe.Write(TestFiles.DataName, TestFiles.People("[]"));

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath);

        Assert.Equal("Absolute", book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
    }

    [Fact]
    public void InvalidDataFileGivesWarningAndNoPeople()
    {
        TestFiles.WriteConfig(_exe);
        _exe.Write(TestFiles.CalendarName, TestFiles.Calendar("{}"));
        _exe.Write(TestFiles.DataName, "{ not json");

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath);

        Assert.StartsWith("Error loading data: ", Assert.Single(book.Warnings));
        Assert.Empty(book.People);
    }

    [Fact]
    public void InvalidCalendarFileGivesWarningAndEmptyCalendar()
    {
        TestFiles.WriteConfig(_exe);
        _exe.Write(TestFiles.CalendarName, "{ \"calendar\": [");
        _exe.Write(TestFiles.DataName, TestFiles.People("[]"));

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath);

        Assert.StartsWith("Error loading calendar: ", Assert.Single(book.Warnings));
        Assert.Equal(string.Empty, book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
    }

    [Fact]
    public void MissingNonDefaultFilesAreReported()
    {
        TestFiles.WriteConfig(_exe, calendarFile: "Missing.Calendar.json", dataFile: "Missing.Data.json");

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath);

        Assert.Equal(["Calendar file not found: Missing.Calendar.json", "Data file not found: Missing.Data.json"], book.Warnings);
    }

    [Fact]
    public void InvalidConfigUsesDefaultsAndWarns()
    {
        _exe.Write("appsettings.json", "{ \"numberOfDaysAhead\": \"many\" }");

        AnniversaryBook book = TestFiles.LoadInCulture("en-US", null, _exe.FolderPath);

        Assert.StartsWith(AnniversaryBook.ConfigurationWarningPrefix, Assert.Single(book.Warnings));
        Assert.Equal(7, book.Config.NumberOfDaysAhead);
        Assert.Equal("Independence Day", book.GetDay(new DateOnly(2026, 7, 4)).CalendarNames);
    }

    [Fact]
    public void ConfigAllowsCommentsAndTrailingCommas()
    {
        _exe.Write("appsettings.json", """
            {
              // Two days only
              "numberOfDaysAhead": 2,
            }
            """);

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath);

        Assert.Empty(book.Warnings);
        Assert.Equal(2, book.Config.NumberOfDaysAhead);
    }
}
