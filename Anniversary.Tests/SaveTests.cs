using Anniversary.Model;

namespace Anniversary.Tests;

/// <summary>
/// Tests of saving the people file (spec 5.6).
/// </summary>
public sealed class SaveTests : IDisposable
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
    public void SavedPeopleLoadAgainUnchanged()
    {
        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath, _appData.FolderPath);
        Person[] people =
        [
            new Person
            {
                Name = "Kateřina Nováková",
                Namedate = "11-25",
                Birthdate = new DateOnly(1975, 2, 15),
                Anniversaries = new() { [new DateOnly(2008, 6, 21)] = "svatba", [new DateOnly(2004, 9, 1)] = "seznámení" }
            },
            new Person { Name = "Žofie" }
        ];

        string path = book.SavePathFor();
        book.SavePeople(people, path);
        AnniversaryBook reloaded = AnniversaryBook.Load(null, _exe.FolderPath, _appData.FolderPath);

        Assert.Equal(path, reloaded.DataFilePath);
        Assert.Collection(reloaded.People,
            p =>
            {
                Assert.Equal("Kateřina Nováková", p.Name);
                Assert.Equal("11-25", p.Namedate);
                Assert.Equal(new DateOnly(1975, 2, 15), p.Birthdate);
                Assert.Equal([new DateOnly(2004, 9, 1), new DateOnly(2008, 6, 21)], p.Anniversaries!.Keys);
                Assert.Equal("seznámení", p.Anniversaries[new DateOnly(2004, 9, 1)]);
            },
            p => Assert.Equal("Žofie", p.Name));
    }

    [Fact]
    public void FileUsesTheDocumentedFormat()
    {
        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);
        string path = book.SavePathFor();

        book.SavePeople([new Person { Name = "Žofie", Namedate = "  " }], path);
        string json = File.ReadAllText(path);

        Assert.Contains("\"data\"", json);
        Assert.Contains("\"name\": \"Žofie\"", json);
        Assert.DoesNotContain("namedate", json);
        Assert.DoesNotContain("birthdate", json);
        Assert.DoesNotContain("anniversaries", json);
        Assert.DoesNotContain("\\u", json);
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void DayReflectsSavedPeopleWithoutReloading()
    {
        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);

        book.SavePeople([new Person { Name = "Jan", Namedate = "06-24" }], book.SavePathFor());

        Assert.Equal(["Jan"], book.GetDay(new DateOnly(2026, 6, 24)).Namedays);
    }

    [Fact]
    public void SavePathIsAppDataForBuiltInDefault()
    {
        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath, _appData.FolderPath);

        Assert.Null(book.DataFilePath);
        Assert.Equal(_appData.PathOf("Anniversary.Data.json"), book.SavePathFor());
    }

    [Fact]
    public void SavePathIsTheLoadedFile()
    {
        string path = _exe.Write("Anniversary.Data.json", TestFiles.People("[]"));

        AnniversaryBook book = AnniversaryBook.Load(null, _exe.FolderPath, _appData.FolderPath);

        Assert.Equal(path, book.SavePathFor());
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("02-29", true)]
    [InlineData("12-31", true)]
    [InlineData("13-01", false)]
    [InlineData("02-30", false)]
    [InlineData("1-5", false)]
    [InlineData("abc", false)]
    public void NamedayValidation(string? value, bool expected)
    {
        Assert.Equal(expected, AnniversaryBook.IsValidNameday(value));
    }
}
