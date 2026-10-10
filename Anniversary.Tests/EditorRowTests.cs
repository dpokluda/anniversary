using System.Globalization;
using Anniversary.Model;
using Anniversary.Views;

namespace Anniversary.Tests;

/// <summary>
/// Tests of the people editor rows (spec 7.8, 7.9).
/// </summary>
public sealed class EditorRowTests
{
    [Fact]
    public void BlankNameIsAnErrorUntilFixed()
    {
        var row = new PersonRow { Name = "  " };

        Assert.True(row.HasErrors);
        Assert.Single(row.GetErrors(nameof(PersonRow.Name)));

        row.Name = "Jan";

        Assert.False(row.HasErrors);
        Assert.Empty(row.GetErrors(nameof(PersonRow.Name)));
    }

    [Fact]
    public void InvalidNamedayIsAnErrorUntilFixed()
    {
        var row = new PersonRow { Name = "Jan", Nameday = "24-06" };

        Assert.True(row.HasErrors);
        Assert.Single(row.GetErrors(nameof(PersonRow.Nameday)));

        row.Nameday = "06-24";

        Assert.False(row.HasErrors);
    }

    [Fact]
    public void ErrorsChangedIsRaisedWhenAValueChanges()
    {
        var row = new PersonRow();
        var changed = new List<string?>();
        row.ErrorsChanged += (_, e) => changed.Add(e.PropertyName);

        row.Name = "Jan";

        Assert.Contains(nameof(PersonRow.Name), changed);
    }

    [Fact]
    public void PersonRoundTripsThroughRow()
    {
        var person = new Person
        {
            Name = "Eva",
            Namedate = "12-24",
            Birthdate = new DateOnly(1982, 9, 30),
            Anniversaries = new() { [new DateOnly(2008, 6, 21)] = "svatba" }
        };

        Person copy = PersonRow.From(person).ToPerson();

        Assert.Equal("Eva", copy.Name);
        Assert.Equal("12-24", copy.Namedate);
        Assert.Equal(new DateOnly(1982, 9, 30), copy.Birthdate);
        Assert.Equal("svatba", copy.Anniversaries![new DateOnly(2008, 6, 21)]);
        Assert.NotSame(person.Anniversaries, copy.Anniversaries);
    }

    [Fact]
    public void EmptyNamedayBecomesNull()
    {
        Person person = new PersonRow { Name = " Jan ", Nameday = " " }.ToPerson();

        Assert.Equal("Jan", person.Name);
        Assert.Null(person.Namedate);
        Assert.Null(person.Anniversaries);
    }

    [Fact]
    public void AnniversariesSummaryIsOrderedByDate()
    {
        var row = new PersonRow
        {
            Anniversaries = new() { [new DateOnly(2008, 6, 21)] = "svatba", [new DateOnly(2004, 9, 1)] = "seznámení" }
        };

        string expected = $"seznámení ({new DateOnly(2004, 9, 1).ToString("d", CultureInfo.CurrentCulture)}), " +
            $"svatba ({new DateOnly(2008, 6, 21).ToString("d", CultureInfo.CurrentCulture)})";
        Assert.Equal(expected, row.AnniversariesSummary);
        Assert.True(row.HasAnniversaries);
    }

    [Fact]
    public void NamedayDateKeepsOnlyMonthAndDay()
    {
        var row = new PersonRow { Name = "Jan" };

        row.NamedayDate = new DateTime(1999, 6, 24);

        Assert.Equal("06-24", row.Nameday);
        Assert.Equal(new DateTime(DateTime.Today.Year, 6, 24), row.NamedayDate);
    }

    [Fact]
    public void TwentyNinthOfFebruaryNamedayUsesALeapYear()
    {
        var row = new PersonRow { Name = "Horymír", Nameday = "02-29" };

        DateTime date = row.NamedayDate!.Value;

        Assert.Equal((2, 29), (date.Month, date.Day));
        Assert.True(DateTime.IsLeapYear(date.Year));
    }

    [Fact]
    public void ClearedOrInvalidNamedayHasNoDate()
    {
        var row = new PersonRow { Name = "Jan", Nameday = "1-5" };

        Assert.Null(row.NamedayDate);
        Assert.Single(row.GetErrors(nameof(PersonRow.NamedayDate)));

        row.NamedayDate = null;

        Assert.Equal(string.Empty, row.Nameday);
        Assert.False(row.HasErrors);
    }

    [Theory]
    [InlineData("M/d/yyyy", "M/d")]
    [InlineData("d. M. yyyy", "d. M.")]
    [InlineData("dd.MM.yyyy", "dd.MM.")]
    [InlineData("yyyy-MM-dd", "MM-dd")]
    [InlineData("yyyy/MM/dd", "MM/dd")]
    [InlineData("dd/MM/yy", "dd/MM")]
    public void MonthDayPatternRemovesTheYear(string shortDatePattern, string expected)
    {
        Assert.Equal(expected, PersonRow.MonthDayPattern(shortDatePattern));
    }

    [Fact]
    public void DuplicateAnniversaryDatesAreInvalid()
    {
        var rows = new List<AnniversaryRow>
        {
            new() { Date = new DateTime(2008, 6, 21), Name = "svatba" },
            new() { Date = new DateTime(2008, 6, 21), Name = "again" }
        };

        Assert.False(AnniversaryRow.Validate(rows));
        Assert.Single(rows[1].GetErrors(nameof(AnniversaryRow.Date)));

        rows[1].Date = new DateTime(2004, 9, 1);

        Assert.True(AnniversaryRow.Validate(rows));
        Assert.Equal(2, AnniversaryRow.ToDictionary(rows).Count);
    }

    [Fact]
    public void AnniversaryNeedsDateAndName()
    {
        var rows = new List<AnniversaryRow> { new() };

        Assert.False(AnniversaryRow.Validate(rows));
        Assert.Single(rows[0].GetErrors(nameof(AnniversaryRow.Date)));
        Assert.Single(rows[0].GetErrors(nameof(AnniversaryRow.Name)));
    }
}
