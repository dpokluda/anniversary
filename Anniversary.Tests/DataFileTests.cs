using System.Text.Json;
using Anniversary.Model;

namespace Anniversary.Tests;

/// <summary>
/// Tests of choosing, sharing and watching the data file (spec 5.3–5.6, 7.11).
/// </summary>
public sealed class DataFileTests : IDisposable
{
    private readonly TempFolder _appData = new();
    private readonly TempFolder _cloud = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _appData.Dispose();
        _cloud.Dispose();
    }

    [Fact]
    public void ChosenDataFileIsUsedInsteadOfTheDefaultOne()
    {
        _appData.Write("Anniversary.Data.json", TestFiles.People("""[{ "name": "Default" }]"""));
        string chosen = _cloud.Write("Family.json", TestFiles.People("""[{ "name": "Chosen" }]"""));

        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = chosen }, [_appData.FolderPath]);

        Assert.Equal("Chosen", Assert.Single(book.People).Name);
        Assert.Equal(chosen, book.DataFilePath);
        Assert.Equal(chosen, book.SavePathFor());
        Assert.False(book.DataFileLoadFailed);
        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void MissingChosenFileIsReportedAndKept()
    {
        string chosen = _cloud.PathOf("Family.json");

        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = chosen }, [_appData.FolderPath]);

        Assert.Equal([$"Data file not found: {chosen}"], book.Warnings);
        Assert.Empty(book.People);
        Assert.True(book.DataFileLoadFailed);
        Assert.Equal(chosen, book.SavePathFor());
        Assert.False(book.HasDataFileChanged());
    }

    [Fact]
    public void MissingFileIsLoadedOnceItAppears()
    {
        string chosen = _cloud.PathOf("Family.json");
        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = chosen }, [_appData.FolderPath]);

        _cloud.Write("Family.json", TestFiles.People("""[{ "name": "Synced" }]"""));

        Assert.True(book.HasDataFileChanged());
        Assert.True(book.ReloadPeopleIfChanged());
        Assert.Equal("Synced", Assert.Single(book.People).Name);
        Assert.False(book.DataFileLoadFailed);
        Assert.Empty(book.Warnings);
        Assert.False(book.HasDataFileChanged());
    }

    [Fact]
    public void UnreadableFileKeepsItsPath()
    {
        string chosen = _cloud.Write("Family.json", "{ not json");

        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = chosen }, [_appData.FolderPath]);

        Assert.StartsWith("Error loading data: ", Assert.Single(book.Warnings));
        Assert.Empty(book.People);
        Assert.True(book.DataFileLoadFailed);
        Assert.Equal(chosen, book.SavePathFor());
    }

    [Fact]
    public void FileChangedElsewhereIsReloaded()
    {
        string chosen = _cloud.Write("Family.json", TestFiles.People("""[{ "name": "Before" }]"""));
        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = chosen }, [_appData.FolderPath]);
        Assert.False(book.HasDataFileChanged());

        ChangeElsewhere(chosen, TestFiles.People("""[{ "name": "After", "namedate": "06-24" }]"""));

        Assert.True(book.HasDataFileChanged());
        Assert.True(book.ReloadPeopleIfChanged());
        Assert.Equal("After", Assert.Single(book.People).Name);
        Assert.Equal(["After"], book.GetDay(new DateOnly(2026, 6, 24)).Namedays);
        Assert.False(book.ReloadPeopleIfChanged());
    }

    [Fact]
    public void ReloadClearsOldWarnings()
    {
        _appData.Write(TestFiles.CalendarName, TestFiles.Calendar("""{ "13-45": "No such day" }"""));
        string chosen = _cloud.Write("Family.json", TestFiles.People("[]"));
        AnniversaryBook book = AnniversaryBook.Load(TestFiles.Settings(dataFile: chosen), [_appData.FolderPath]);
        Assert.NotEmpty(book.Warnings);

        ChangeElsewhere(chosen, TestFiles.People("""[{ "name": "New" }]"""));
        book.ReloadPeopleIfChanged();

        Assert.Empty(book.Warnings);
    }

    [Fact]
    public void SavingIsNotAChange()
    {
        string chosen = _cloud.Write("Family.json", TestFiles.People("""[{ "name": "Jan" }]"""));
        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = chosen }, [_appData.FolderPath]);

        book.SavePeople([new Person { Name = "Jan" }, new Person { Name = "Eva" }], book.SavePathFor());

        Assert.False(book.HasDataFileChanged());
        Assert.False(book.ReloadPeopleIfChanged());
        Assert.Equal(2, book.People.Count);
    }

    [Fact]
    public void DeletedFileIsNotAChange()
    {
        // A cloud app may remove a file for a moment while it replaces it.
        string chosen = _cloud.Write("Family.json", TestFiles.People("""[{ "name": "Jan" }]"""));
        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = chosen }, [_appData.FolderPath]);

        File.Delete(chosen);

        Assert.False(book.HasDataFileChanged());
        Assert.Equal("Jan", Assert.Single(book.People).Name);
    }

    [Fact]
    public void OpenDataFileUsesTheFileWhereItIs()
    {
        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);
        string shared = _cloud.Write("Family.json", TestFiles.People("""[{ "name": "Babička", "birthdate": "1950-03-14" }]"""));

        book.OpenDataFile(shared);
        book.SavePeople([.. book.People, new Person { Name = "Vnuk" }], book.SavePathFor());

        Assert.Equal(shared, book.DataFilePath);
        Assert.Equal(["Babička (76)"], book.GetDay(new DateOnly(2026, 3, 14)).Birthdays);
        Assert.Contains("Vnuk", File.ReadAllText(shared));
        Assert.False(File.Exists(_appData.PathOf("Anniversary.Data.json")));
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("""{ "calendar": { "10-09": "Štefan" } }""")]
    [InlineData("""{ "data": "nobody" }""")]
    [InlineData("[]")]
    public void OpenDataFileRejectsOtherFilesAndChangesNothing(string content)
    {
        string current = _cloud.Write("Current.json", TestFiles.People("""[{ "name": "Jan" }]"""));
        string other = _cloud.Write("Other.json", content);
        AnniversaryBook book = AnniversaryBook.Load(new UserSettings { DataFile = current }, [_appData.FolderPath]);

        Assert.ThrowsAny<JsonException>(() => book.OpenDataFile(other));
        Assert.Equal(current, book.DataFilePath);
        Assert.Equal("Jan", Assert.Single(book.People).Name);
    }

    [Fact]
    public void OpenDataFileReportsAMissingFile()
    {
        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);

        Assert.ThrowsAny<IOException>(() => book.OpenDataFile(_cloud.PathOf("Missing.json")));
        Assert.Null(book.DataFilePath);
    }

    [Fact]
    public void SavingToANewPathMakesItTheDataFile()
    {
        _appData.Write("Anniversary.Data.json", TestFiles.People("""[{ "name": "Jan" }]"""));
        AnniversaryBook book = AnniversaryBook.Load(null, _appData.FolderPath);
        string cloudPath = _cloud.PathOf("Anniversary.Data.json");

        book.SavePeople(book.People, cloudPath);

        Assert.Equal(cloudPath, book.DataFilePath);
        Assert.Equal(cloudPath, book.SavePathFor());
        Assert.Equal("Jan", Assert.Single(AnniversaryBook.Load(new UserSettings { DataFile = cloudPath }, [_appData.FolderPath]).People).Name);
    }

    [Fact]
    public void UserSettingsRemembersDataFile()
    {
        var settings = new UserSettings { FilePath = _appData.PathOf("usersettings.json"), DataFile = _cloud.PathOf("Family.json") };
        settings.Save();

        Assert.Equal(_cloud.PathOf("Family.json"), UserSettings.Load(settings.FilePath).DataFile);
    }

    // Another computer's save, synced by the cloud app: new content with a later time stamp.
    private static void ChangeElsewhere(string path, string content)
    {
        File.WriteAllText(path, content);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(1));
    }
}
