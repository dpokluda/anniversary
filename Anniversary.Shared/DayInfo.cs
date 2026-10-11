namespace Anniversary;

/// <summary>
/// Everything the main window shows for one day.
/// </summary>
/// <param name="CalendarNames">The calendar text for the day (names or holidays), or an empty string.</param>
/// <param name="Namedays">People who have their nameday on the day, one entry per line.</param>
/// <param name="Birthdays">Birthdays and anniversaries on the day, one entry per line.</param>
/// <param name="NextDaysPeople">People with an event in the next N days.</param>
/// <param name="NextDaysNames">Calendar names in the next N days.</param>
public sealed record DayInfo(
    string CalendarNames,
    IReadOnlyList<string> Namedays,
    IReadOnlyList<string> Birthdays,
    IReadOnlyList<string> NextDaysPeople,
    IReadOnlyList<string> NextDaysNames);
