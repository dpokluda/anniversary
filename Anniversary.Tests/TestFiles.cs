using System.Globalization;
using Anniversary.Model;

namespace Anniversary.Tests;

/// <summary>
/// Builds the JSON files the tests load (calendar and people) and the settings that use them.
/// </summary>
internal static class TestFiles
{
    /// <summary>
    /// The calendar file name used by most tests.
    /// </summary>
    public const string CalendarName = "Anniversary.Calendar.Test.json";

    /// <summary>
    /// The people file name used by most tests.
    /// </summary>
    public const string DataName = "Test.Data.json";

    /// <summary>
    /// Writes a calendar and people file into the folder and loads a book from it, with settings that use them.
    /// </summary>
    /// <param name="folder">The folder to write into.</param>
    /// <param name="people">The JSON array of people, e.g. <c>[{ "name": "Jan" }]</c>.</param>
    /// <param name="calendar">The JSON object of calendar days, e.g. <c>{ "10-09": "Štefan" }</c>.</param>
    /// <param name="daysAhead">The number of days ahead.</param>
    /// <param name="displayYears">Whether to display years on birthdays.</param>
    /// <returns>The loaded book.</returns>
    public static AnniversaryBook Book(TempFolder folder, string people = "[]", string calendar = "{}",
        int daysAhead = 7, bool displayYears = true)
    {
        folder.Write(CalendarName, Calendar(calendar));
        folder.Write(DataName, People(people));
        return AnniversaryBook.Load(Settings(daysAhead: daysAhead, displayYears: displayYears), [folder.FolderPath]);
    }

    /// <summary>
    /// Returns settings that choose the given calendar and data file (by default the test ones).
    /// </summary>
    /// <param name="calendar">The calendar file name, or <c>null</c> for the region's calendar.</param>
    /// <param name="dataFile">The data file name or path, or <c>null</c> for <c>Anniversary.Data.json</c>.</param>
    /// <param name="daysAhead">The number of days ahead.</param>
    /// <param name="displayYears">Whether to display years on birthdays.</param>
    /// <returns>The settings.</returns>
    public static UserSettings Settings(string? calendar = CalendarName, string? dataFile = DataName, int daysAhead = 7,
        bool displayYears = true)
    {
        return new UserSettings
        {
            Calendar = calendar,
            DataFile = dataFile,
            NumberOfDaysAhead = daysAhead,
            DisplayYearsOnBirthdays = displayYears
        };
    }

    /// <summary>
    /// Loads a book while the current culture is the given one, the way the app runs with the OS culture (spec 5.7).
    /// </summary>
    /// <param name="culture">The culture name, e.g. <c>"cs-CZ"</c>; <c>""</c> is the invariant culture.</param>
    /// <param name="preferredCalendar">The calendar chosen by the user, or <c>null</c>.</param>
    /// <param name="folders">The folders to look in.</param>
    /// <returns>The loaded book.</returns>
    public static AnniversaryBook LoadInCulture(string culture, string? preferredCalendar, params string[] folders)
    {
        CultureInfo saved = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        try
        {
            return AnniversaryBook.Load(preferredCalendar, folders);
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
    }

    /// <summary>
    /// Wraps calendar days into a calendar file.
    /// </summary>
    /// <param name="days">The JSON object of calendar days.</param>
    /// <param name="name">The optional display name.</param>
    /// <returns>The calendar file content.</returns>
    public static string Calendar(string days, string? name = null)
    {
        string nameProperty = name != null ? $"\"name\": \"{name}\", " : string.Empty;
        return $$"""{ {{nameProperty}}"calendar": {{days}} }""";
    }

    /// <summary>
    /// Wraps a JSON array of people into a people file.
    /// </summary>
    /// <param name="people">The JSON array of people.</param>
    /// <returns>The people file content.</returns>
    public static string People(string people)
    {
        return $$"""{ "data": {{people}} }""";
    }
}
