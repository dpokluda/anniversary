namespace Anniversary;

/// <summary>
/// Describes an available name-day calendar.
/// </summary>
/// <param name="FileName">The calendar file name, e.g. "Anniversary.Calendar.Czech.json".</param>
/// <param name="DisplayName">The name shown in the calendar menu.</param>
/// <param name="Locale">The culture code of the calendar, if known.</param>
public sealed record CalendarInfo(string FileName, string DisplayName, string? Locale);
