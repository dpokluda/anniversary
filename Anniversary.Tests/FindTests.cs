namespace Anniversary.Tests;

/// <summary>
/// Tests of <see cref="AnniversaryBook.Find"/> (spec 6.2).
/// </summary>
public sealed class FindTests : IDisposable
{
    private readonly TempFolder _folder = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _folder.Dispose();
    }

    [Fact]
    public void SubstringMatchIgnoresCase()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "10-09": "Štefan/Sára" }""");

        Assert.Equal(new DateOnly(2026, 10, 9), book.Find("sÁRa", new DateOnly(2026, 1, 1), false, false));
    }

    [Theory]
    [InlineData("Ana Bob")]
    [InlineData("Ana,Bob")]
    [InlineData("Ana;Bob")]
    [InlineData("Ana\tBob")]
    [InlineData("Ana/Bob")]
    [InlineData("Ana-Bob")]
    public void WholeWordsSplitOnEveryDelimiter(string names)
    {
        AnniversaryBook book = TestFiles.Book(_folder,
            calendar: $$"""{ "03-05": {{System.Text.Json.JsonSerializer.Serialize(names)}} }""");

        Assert.Equal(new DateOnly(2026, 3, 5), book.Find("bob", new DateOnly(2026, 1, 1), true, false));
    }

    [Fact]
    public void WholeWordsDoNotMatchPartOfAWord()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "03-05": "Bobby" }""");

        Assert.Null(book.Find("Bob", new DateOnly(2026, 1, 1), true, false));
        Assert.Equal(new DateOnly(2026, 3, 5), book.Find("Bob", new DateOnly(2026, 1, 1), false, false));
    }

    [Fact]
    public void CalendarOnlyIgnoresPeople()
    {
        AnniversaryBook book = TestFiles.Book(_folder, people: """[{ "name": "Jan Novák", "birthdate": "1980-03-14" }]""");

        Assert.Equal(new DateOnly(2026, 3, 14), book.Find("Novák", new DateOnly(2026, 1, 1), false, false));
        Assert.Null(book.Find("Novák", new DateOnly(2026, 1, 1), false, true));
    }

    [Fact]
    public void StartDayIsIncluded()
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "10-09": "Štefan" }""");

        Assert.Equal(new DateOnly(2026, 10, 9), book.Find("Štefan", new DateOnly(2026, 10, 9), false, false));
        Assert.Equal(new DateOnly(2027, 10, 9), book.Find("Štefan", new DateOnly(2026, 10, 10), false, false));
    }

    [Fact]
    public void SearchEndsOnLastDayOfFollowingYear()
    {
        // The next 29 February after 1 March 2025 is in 2028, beyond 31 December 2026.
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "02-29": "Horymír" }""");

        Assert.Null(book.Find("Horymír", new DateOnly(2025, 3, 1), false, false));
        Assert.Equal(new DateOnly(2028, 2, 29), book.Find("Horymír", new DateOnly(2027, 1, 1), false, false));
    }

    [Fact]
    public void SearchStopsOnLastDayOfYear9999()
    {
        // 31 December 9999 is the last day DateOnly can hold, so it ends searches from 9998 and 9999.
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "06-01": "Laura", "12-31": "Silvestr" }""");

        Assert.Equal(new DateOnly(9999, 12, 31), book.Find("Silvestr", new DateOnly(9999, 6, 2), false, false));
        Assert.Equal(new DateOnly(9999, 12, 31), book.Find("Silvestr", new DateOnly(9999, 12, 31), false, false));
        Assert.Null(book.Find("Laura", new DateOnly(9999, 6, 2), false, false));
        Assert.Null(book.Find("Nobody", new DateOnly(9998, 1, 1), false, false));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankNameFindsNothing(string name)
    {
        AnniversaryBook book = TestFiles.Book(_folder, calendar: """{ "01-01": " " }""");

        Assert.Null(book.Find(name, new DateOnly(2026, 1, 1), false, false));
    }
}
