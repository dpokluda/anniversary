namespace Anniversary.Model;

/// <summary>
/// Application configuration settings read from <c>appsettings.json</c>.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Gets or sets the name (or absolute path) of the calendar file (name days or holidays), or <c>null</c> to choose
    /// it by the OS region (spec 5.7).
    /// </summary>
    public string? CalendarFile { get; set; }

    /// <summary>
    /// Gets or sets the name (or absolute path) of the people data file.
    /// </summary>
    public string DataFile { get; set; } = "Anniversary.Data.json";

    /// <summary>
    /// Gets or sets whether to display the number of years next to birthdays and anniversaries.
    /// </summary>
    public bool DisplayYearsOnBirthdays { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of days ahead shown in the "Next N days" section.
    /// </summary>
    public int NumberOfDaysAhead { get; set; } = 7;
}
