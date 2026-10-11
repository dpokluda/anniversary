namespace Anniversary.Tests;

/// <summary>
/// Tests of rule keys for dates that move from year to year (spec 5.1, 6.3) and of the shipped holiday entries (5.7).
/// </summary>
public sealed class CalendarRuleTests : IDisposable
{
    private readonly TempFolder _folder = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _folder.Dispose();
    }

    [Theory]
    [InlineData("11-Thu-4", 2026, 11, 26)]
    [InlineData("01-Mon-3", 2026, 1, 19)]
    [InlineData("09-Mon-1", 2026, 9, 7)]
    [InlineData("02-Sun-1", 2026, 2, 1)]
    [InlineData("05-Mon-last", 2026, 5, 25)]
    [InlineData("05-Mon-last", 2027, 5, 31)]
    [InlineData("02-Tue-last", 2028, 2, 29)]
    [InlineData("02-Tue-last", 2027, 2, 23)]
    public void WeekdayRuleFallsOnItsDayOnly(string key, int year, int month, int day)
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: $$"""{ "{{key}}": "Rule" }""");
        var date = new DateOnly(year, month, day);

        Assert.Equal("Rule", book.GetDay(date).CalendarNames);
        Assert.Equal(string.Empty, book.GetDay(date.AddDays(-7)).CalendarNames);
        Assert.Equal(string.Empty, book.GetDay(date.AddDays(7)).CalendarNames);
    }

    [Theory]
    [InlineData(1, 4, 1)]
    [InlineData(1818, 3, 22)]
    [InlineData(1943, 4, 25)]
    [InlineData(2000, 4, 23)]
    [InlineData(2008, 3, 23)]
    [InlineData(2024, 3, 31)]
    [InlineData(2025, 4, 20)]
    [InlineData(2026, 4, 5)]
    [InlineData(2027, 3, 28)]
    [InlineData(2038, 4, 25)]
    [InlineData(2285, 3, 22)]
    [InlineData(9999, 3, 28)]
    public void EasterFallsOnKnownDates(int year, int month, int day)
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "easter": "Easter" }""");

        Assert.Equal(new DateOnly(year, month, day), book.Find("Easter", new DateOnly(year, 1, 1), false, true));
    }

    [Fact]
    public void EasterIsASundayFrom22MarchTo25AprilInEveryYear()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "easter": "Easter" }""");

        for (int year = 1; year <= 9999; year++)
        {
            DateOnly? easter = book.Find("Easter", new DateOnly(year, 3, 22), false, true);

            Assert.True(easter is { DayOfWeek: DayOfWeek.Sunday } date && date <= new DateOnly(year, 4, 25),
                $"Easter after 22 March {year}: {easter}");
        }
    }

    [Theory]
    [InlineData("easter+1", 2026, 4, 6)]
    [InlineData("easter-2", 2026, 4, 3)]
    [InlineData("easter-47", 2026, 2, 17)]
    [InlineData("11-Thu-4+1", 2026, 11, 27)]
    [InlineData("12-Mon-last+7", 2027, 1, 4)]
    [InlineData("01-Mon-1-7", 2025, 12, 29)]
    public void OffsetMovesTheDateEvenIntoAnotherYear(string key, int year, int month, int day)
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: $$"""{ "{{key}}": "Rule" }""");
        var date = new DateOnly(year, month, day);

        Assert.Equal("Rule", book.GetDay(date).CalendarNames);
        Assert.Equal(string.Empty, book.GetDay(date.AddDays(-1)).CalendarNames);
        Assert.Equal(string.Empty, book.GetDay(date.AddDays(1)).CalendarNames);
    }

    [Fact]
    public void DayEntryComesFirstThenRulesInFileOrder()
    {
        // 6 April 2026 is both Easter Monday and the first Monday of April.
        AnniversaryBook book = TestFiles.Book(_folder,
            calendar: """{ "easter+1": "Velikonoční pondělí", "04-06": "Vendula", "04-Mon-1": "First Monday" }""");

        Assert.Equal("Vendula, Velikonoční pondělí, First Monday", book.GetDay(new DateOnly(2026, 4, 6)).CalendarNames);
    }

    [Fact]
    public void RuleKeysIgnoreCase()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "11-THU-4": "Thanksgiving", "05-mon-LAST": "Memorial Day", "Easter": "Easter" }""");

        Assert.Empty(book.Warnings);
        Assert.Equal("Thanksgiving", book.GetDay(new DateOnly(2026, 11, 26)).CalendarNames);
        Assert.Equal("Memorial Day", book.GetDay(new DateOnly(2026, 5, 25)).CalendarNames);
        Assert.Equal("Easter", book.GetDay(new DateOnly(2026, 4, 5)).CalendarNames);
    }

    [Fact]
    public void NextDaysAcrossNewYearUseTheNextYearsRules()
    {
        // The first Monday is 4 January in 2027, but was 5 January in 2026.
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "01-Mon-1": "First Monday" }""");

        Assert.Equal(["First Monday"], book.GetDay(new DateOnly(2026, 12, 30)).NextDaysNames);
        Assert.Empty(book.GetDay(new DateOnly(2026, 12, 27)).NextDaysNames);
    }

    [Fact]
    public void FindFindsARuleDateInTheFollowingYear()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "11-Thu-4": "Thanksgiving" }""");

        Assert.Equal(new DateOnly(2027, 11, 25), book.Find("thanksgiving", new DateOnly(2026, 11, 27), true, true));
    }

    [Fact]
    public void UnknownKeysAreReportedOnceAndTheOtherEntriesWork()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """
            { "13-01": "A", "11-Thx-4": "B", "easter+": "C", "11-Thu-5": "D", "easter+100": "E", "1-5": "F",
              "07-04": "Independence Day", "11-Thu-4": "Thanksgiving" }
            """);

        Assert.Equal($"Unknown dates in {TestFiles.CalendarName}: 13-01, 11-Thx-4, easter+, 11-Thu-5, easter+100 and 1 more",
            Assert.Single(book.Warnings));
        Assert.Equal("Independence Day", book.GetDay(new DateOnly(2026, 7, 4)).CalendarNames);
        Assert.Equal("Thanksgiving", book.GetDay(new DateOnly(2026, 11, 26)).CalendarNames);
    }

    [Fact]
    public void RulesAtTheEndsOfTheDateRangeDontFail()
    {
        // In 9999, "Late" would fall in the year 10000; in the year 1, "Early" would fall in the year 0.
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "12-Sun-last+7": "Late", "01-Sun-1-7": "Early" }""");

        Assert.Equal(string.Empty, book.GetDay(new DateOnly(9999, 12, 31)).CalendarNames);
        Assert.Equal(string.Empty, book.GetDay(new DateOnly(1, 1, 1)).CalendarNames);
        Assert.Null(book.Find("Late", new DateOnly(9999, 1, 4), false, true));
        Assert.Equal(new DateOnly(1, 12, 30), book.Find("Early", new DateOnly(1, 1, 1), false, true));
    }

    [Theory]
    // 5 U.S.C. 6103(a), on the dates in OPM's federal holiday schedules for 2025–2027. Fixed-date holidays are on their
    // legal date: when one falls on a weekend, OPM lists the weekday off instead (e.g. Friday 3 July 2026).
    [InlineData(2025, 1, 1, "New Year's Day")]
    [InlineData(2025, 1, 20, "Martin Luther King Jr. Day")]
    [InlineData(2025, 2, 17, "Presidents' Day")]
    [InlineData(2025, 5, 26, "Memorial Day")]
    [InlineData(2025, 6, 19, "Juneteenth")]
    [InlineData(2025, 7, 4, "Independence Day")]
    [InlineData(2025, 9, 1, "Labor Day")]
    [InlineData(2025, 10, 13, "Columbus Day")]
    [InlineData(2025, 11, 11, "Veterans Day")]
    [InlineData(2025, 11, 27, "Thanksgiving")]
    [InlineData(2025, 12, 25, "Christmas Day")]
    [InlineData(2026, 1, 1, "New Year's Day")]
    [InlineData(2026, 1, 19, "Martin Luther King Jr. Day")]
    [InlineData(2026, 2, 16, "Presidents' Day")]
    [InlineData(2026, 5, 25, "Memorial Day")]
    [InlineData(2026, 6, 19, "Juneteenth")]
    [InlineData(2026, 7, 4, "Independence Day")]
    [InlineData(2026, 9, 7, "Labor Day")]
    [InlineData(2026, 10, 12, "Columbus Day")]
    [InlineData(2026, 11, 11, "Veterans Day")]
    [InlineData(2026, 11, 26, "Thanksgiving")]
    [InlineData(2026, 12, 25, "Christmas Day")]
    [InlineData(2027, 1, 1, "New Year's Day")]
    [InlineData(2027, 1, 18, "Martin Luther King Jr. Day")]
    [InlineData(2027, 2, 15, "Presidents' Day")]
    [InlineData(2027, 5, 31, "Memorial Day")]
    [InlineData(2027, 6, 19, "Juneteenth")]
    [InlineData(2027, 7, 4, "Independence Day")]
    [InlineData(2027, 9, 6, "Labor Day")]
    [InlineData(2027, 10, 11, "Columbus Day")]
    [InlineData(2027, 11, 11, "Veterans Day")]
    [InlineData(2027, 11, 25, "Thanksgiving")]
    [InlineData(2027, 12, 25, "Christmas Day")]
    public void UsCalendarHasTheFederalHolidays(int year, int month, int day, string holiday)
    {
        AnniversaryBook book = AnniversaryBook.Load("Anniversary.Calendar.US.json");

        Assert.Equal(holiday, book.GetDay(new DateOnly(year, month, day)).CalendarNames);
    }

    [Theory]
    [InlineData(2026, 2, 17, "Mardi Gras")]
    [InlineData(2026, 3, 8, "Daylight Saving Time begins")]
    [InlineData(2026, 4, 3, "Good Friday")]
    [InlineData(2026, 4, 5, "Easter")]
    [InlineData(2026, 5, 10, "Mother's Day")]
    [InlineData(2026, 6, 21, "Father's Day")]
    [InlineData(2026, 11, 1, "Daylight Saving Time ends")]
    [InlineData(2026, 11, 27, "Black Friday")]
    public void UsCalendarHasTheFloatingObservances(int year, int month, int day, string observance)
    {
        AnniversaryBook book = AnniversaryBook.Load("Anniversary.Calendar.US.json");

        Assert.Equal(observance, book.GetDay(new DateOnly(year, month, day)).CalendarNames);
    }

    [Fact]
    public void CzechCalendarHasTheEasterHolidays()
    {
        AnniversaryBook book = AnniversaryBook.Load("Anniversary.Calendar.Czech.json");

        Assert.Empty(book.Warnings);
        Assert.Equal("Richard, Velký pátek", book.GetDay(new DateOnly(2026, 4, 3)).CalendarNames);
        Assert.Equal("Vendula, Velikonoční pondělí", book.GetDay(new DateOnly(2026, 4, 6)).CalendarNames);
        Assert.Equal(new DateOnly(2027, 3, 29), book.Find("Velikonoční pondělí", new DateOnly(2027, 1, 1), false, true));
    }
}
