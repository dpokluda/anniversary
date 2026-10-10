using System.Text.Json.Serialization;

namespace Anniversary.Model;

/// <summary>
/// Represents a name-day calendar file: names for each day of the year plus optional metadata.
/// </summary>
/// <remarks>
/// Days are keyed in MM-dd format (e.g. "01-15" for January 15th). The value is the display text for that day;
/// multiple names are separated by commas. This was <c>Calendar</c> in v4; it was renamed so it doesn't clash with
/// Avalonia's <c>Calendar</c> control.
/// </remarks>
public class CalendarData
{
    /// <summary>
    /// Gets or sets the display name of the calendar (optional).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the culture code of the calendar, e.g. "cs-CZ" (optional).
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets where the calendar data came from (optional).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the license of the calendar data (optional).
    /// </summary>
    public string? License { get; set; }

    /// <summary>
    /// Gets or sets the calendar data, where the key is the date in MM-dd format and the value the names for that day.
    /// </summary>
    [JsonPropertyName("calendar")]
    public Dictionary<string, string> Data { get; set; } = new();
}
