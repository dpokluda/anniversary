namespace Anniversary.Tests;

/// <summary>
/// Tests of <see cref="AnniversaryBook.GetDay"/> (spec 6.1).
/// </summary>
public sealed class DayTests : IDisposable
{
    private readonly TempFolder _folder = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _folder.Dispose();
    }

    [Fact]
    public void CalendarNamesAreShownForTheDay()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "10-09": "Štefan/Sára" }""");

        Assert.Equal("Štefan/Sára", book.GetDay(new DateOnly(2026, 10, 9)).CalendarNames);
    }

    [Fact]
    public void NamedayIsListed()
    {
        AnniversaryBook book = TestFiles.Book(_folder, people: """[{ "name": "Jan Novák", "namedate": "06-24" }]""");

        Assert.Equal(["Jan Novák"], book.GetDay(new DateOnly(2026, 6, 24)).Namedays);
    }

    [Fact]
    public void BirthdayShowsYears()
    {
        AnniversaryBook book = TestFiles.Book(_folder, people: """[{ "name": "Eva", "birthdate": "1982-09-30" }]""");

        Assert.Equal(["Eva (44)"], book.GetDay(new DateOnly(2026, 9, 30)).Birthdays);
    }

    [Fact]
    public void BirthdayWithoutYearsWhenConfigTurnsItOff()
    {
        AnniversaryBook book = TestFiles.Book(_folder, people: """[{ "name": "Eva", "birthdate": "1982-09-30" }]""",
            displayYears: false);

        Assert.Equal(["Eva"], book.GetDay(new DateOnly(2026, 9, 30)).Birthdays);
    }

    [Fact]
    public void AnniversaryLineHasNameDescriptionAndYears()
    {
        AnniversaryBook book = TestFiles.Book(_folder,
            people: """[{ "name": "Jan", "anniversaries": { "2008-06-21": "svatba" } }]""");

        Assert.Equal(["Jan - svatba (18)"], book.GetDay(new DateOnly(2026, 6, 21)).Birthdays);
    }

    [Fact]
    public void PersonWithSeveralEventsOnOneDayIsListedOnceInEachList()
    {
        AnniversaryBook book = TestFiles.Book(_folder,
            people: """[{ "name": "Jan", "namedate": "06-24", "birthdate": "1980-06-24" }]""");

        DayInfo day = book.GetDay(new DateOnly(2026, 6, 24));

        Assert.Equal(["Jan"], day.Namedays);
        Assert.Equal(["Jan (46)"], day.Birthdays);
    }

    [Fact]
    public void NextDaysRespectsTheConfiguredNumberOfDays()
    {
        AnniversaryBook book = TestFiles.Book(_folder, daysAhead: 2,
            people: """[{ "name": "A", "namedate": "01-02" }, { "name": "B", "namedate": "01-03" }, { "name": "C", "namedate": "01-04" }]""",
            calendar: """{ "01-02": "X", "01-03": "Y", "01-04": "Z" }""");

        DayInfo day = book.GetDay(new DateOnly(2026, 1, 1));

        Assert.Equal(["A", "B"], day.NextDaysPeople);
        Assert.Equal(["X", "Y"], day.NextDaysNames);
    }

    [Fact]
    public void NextDaysAreInDateOrderWithoutDuplicatesAndCalendarEntriesAreSplit()
    {
        AnniversaryBook book = TestFiles.Book(_folder,
            people: """[{ "name": "Later", "namedate": "01-03" }, { "name": "Sooner", "namedate": "01-02", "birthdate": "1990-01-03" }]""",
            calendar: """{ "01-02": "Adam, Eva", "01-03": " Eva ,Ivo,," }""");

        DayInfo day = book.GetDay(new DateOnly(2026, 1, 1));

        Assert.Equal(["Sooner", "Later"], day.NextDaysPeople);
        Assert.Equal(["Adam", "Eva", "Ivo"], day.NextDaysNames);
    }

    [Fact]
    public void NextDaysStopOnLastDayOfYear9999()
    {
        // 31 December 9999 is the last day DateOnly can hold; the 01-01 entries would be in year 10000.
        AnniversaryBook book = TestFiles.Book(_folder,
            people: """[{ "name": "Silvestr", "namedate": "12-31" }, { "name": "Nový", "namedate": "01-01" }]""",
            calendar: """{ "12-30": "David", "12-31": "Silvestr", "01-01": "Nový rok" }""");

        DayInfo day = book.GetDay(new DateOnly(9999, 12, 29));
        DayInfo lastDay = book.GetDay(new DateOnly(9999, 12, 31));

        Assert.Equal(["Silvestr"], day.NextDaysPeople);
        Assert.Equal(["David", "Silvestr"], day.NextDaysNames);
        Assert.Equal("Silvestr", lastDay.CalendarNames);
        Assert.Empty(lastDay.NextDaysPeople);
        Assert.Empty(lastDay.NextDaysNames);
    }

    [Fact]
    public void UnknownYearShowsNoYears()
    {
        // v4 data uses 9999 for an unknown year; browsing to a year before an event behaves the same.
        AnniversaryBook book = TestFiles.Book(_folder,
            people: """[{ "name": "Alena", "birthdate": "9999-06-22", "anniversaries": { "2030-06-22": "future" } }]""");

        Assert.Equal(["Alena", "Alena - future"], book.GetDay(new DateOnly(2026, 6, 22)).Birthdays);
    }

    [Fact]
    public void TwentyNinthOfFebruaryMatchesOnlyInLeapYears()
    {
        AnniversaryBook book = TestFiles.Book(_folder, daysAhead: 0,
            people: """[{ "name": "Leap", "birthdate": "2000-02-29" }]""");

        Assert.Equal(["Leap (24)"], book.GetDay(new DateOnly(2024, 2, 29)).Birthdays);
        Assert.Empty(book.GetDay(new DateOnly(2025, 2, 28)).Birthdays);
        Assert.Empty(book.GetDay(new DateOnly(2025, 3, 1)).Birthdays);
    }

    [Fact]
    public void DayWithoutDataIsEmpty()
    {
        AnniversaryBook book = TestFiles.Book(_folder);

        DayInfo day = book.GetDay(new DateOnly(2026, 10, 9));

        Assert.Equal(string.Empty, day.CalendarNames);
        Assert.Empty(day.Namedays);
        Assert.Empty(day.Birthdays);
        Assert.Empty(day.NextDaysPeople);
        Assert.Empty(day.NextDaysNames);
    }
}
