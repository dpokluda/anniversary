using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Anniversary.Model;

namespace Anniversary;

/// <summary>
/// Holds the settings, the name-day calendar and the people, and answers everything the UI asks about a day.
/// </summary>
/// <remarks>
/// This is the only non-UI logic class. It never reads the clock and never shows UI: the caller passes the day in,
/// and loading problems are collected in <see cref="Warnings"/>.
/// </remarks>
public sealed class AnniversaryBook
{
    /// <summary>
    /// The prefix of warnings about the settings file (the UI shows them with a different title).
    /// </summary>
    public const string ConfigurationWarningPrefix = "Error loading configuration";

    private const string CalendarFilePrefix = "Anniversary.Calendar.";
    private const string CalendarFileSuffix = ".json";
    private const string FallbackCalendarFile = "Anniversary.Calendar.US.json";
    private const string DefaultDataFile = "Anniversary.Data.json";
    private const int MaxReportedKeys = 5;

    private static readonly char[] WordDelimiters = [' ', ',', ';', '\t', '/', '-'];

    // Rule keys for floating dates (spec 5.1): "11-Thu-4", "05-Mon-last" or "easter", optionally followed by "+n"/"-n".
    private static readonly Regex RuleKeyPattern = new(
        "^(?:(?<month>0[1-9]|1[0-2])-(?<weekday>mon|tue|wed|thu|fri|sat|sun)-(?<nth>[1-4]|last)|easter)(?<offset>[+-][1-9][0-9]?)?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // Indexed by DayOfWeek.
    private static readonly string[] WeekdayNames = ["sun", "mon", "tue", "wed", "thu", "fri", "sat"];

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly string[] _folders;
    private readonly List<string> _warnings = new();
    private readonly Dictionary<string, List<Person>> _peopleByDate = new();
    private Dictionary<string, string> _calendar = new();
    private List<(Match Rule, string Names)> _calendarRules = new();
    private List<Person> _people = new();
    private List<CalendarInfo> _calendars = new();

    // The data file's last-write time and size when the people were loaded or saved (spec 5.6); null if there was none.
    private (DateTime WriteTime, long Length)? _dataFileStamp;

    private AnniversaryBook(UserSettings settings, IEnumerable<string> folders)
    {
        Settings = settings;
        _folders = folders.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
        AppDataFolder = _folders.Length > 0 ? _folders[^1] : string.Empty;
    }

    /// <summary>
    /// Gets the settings the book was loaded with (spec 5.5); the main window shares the same object.
    /// </summary>
    public UserSettings Settings { get; }

    /// <summary>
    /// Gets the problems found by the last load operation.
    /// </summary>
    public IReadOnlyList<string> Warnings => _warnings;

    /// <summary>
    /// Gets the people in data-file order.
    /// </summary>
    public IReadOnlyList<Person> People => _people;

    /// <summary>
    /// Gets the data file in use: the file the people were loaded from or saved to, or a chosen or absolute file that
    /// was missing or couldn't be read. <c>null</c> when the people came from the built-in default.
    /// </summary>
    public string? DataFilePath { get; private set; }

    /// <summary>
    /// Gets whether the data file wasn't found or couldn't be read when the people were loaded (spec 5.4).
    /// </summary>
    public bool DataFileLoadFailed { get; private set; }

    /// <summary>
    /// Gets the user app-data folder (the last folder passed to <see cref="Load(UserSettings, IReadOnlyList{string})"/>),
    /// or an empty string.
    /// </summary>
    public string AppDataFolder { get; }

    /// <summary>
    /// Gets the available calendars: those with a locale (a country) by display name, then those without one
    /// (e.g. International Days) by display name.
    /// </summary>
    public IReadOnlyList<CalendarInfo> Calendars => _calendars;

    /// <summary>
    /// Gets the file name of the active name-day calendar.
    /// </summary>
    public string CalendarFile { get; private set; } = string.Empty;

    /// <summary>
    /// Loads the name-day calendar and the people with default settings.
    /// </summary>
    /// <param name="preferredCalendar">
    /// The calendar chosen by the user, or <c>null</c> for the calendar for the region of
    /// <see cref="CultureInfo.CurrentCulture"/>, otherwise the US one (spec 5.7).
    /// </param>
    /// <param name="folders">The folders to look in, in order of precedence; the last one is the user app-data folder.</param>
    /// <returns>The loaded book. Problems are reported in <see cref="Warnings"/>.</returns>
    public static AnniversaryBook Load(string? preferredCalendar, params string[] folders)
    {
        return Load(new UserSettings { Calendar = preferredCalendar }, folders);
    }

    /// <summary>
    /// Loads the name-day calendar and the people as the settings say (spec 5.3, 5.5, 5.7).
    /// </summary>
    /// <param name="settings">
    /// The settings, kept as <see cref="Settings"/>. If they couldn't be read (<see cref="UserSettings.LoadError"/>),
    /// that is reported in <see cref="Warnings"/>.
    /// </param>
    /// <param name="folders">The folders to look in, in order of precedence; the last one is the user app-data folder.</param>
    /// <returns>The loaded book. Problems are reported in <see cref="Warnings"/>.</returns>
    public static AnniversaryBook Load(UserSettings settings, IReadOnlyList<string> folders)
    {
        var book = new AnniversaryBook(settings, folders);
        if (settings.LoadError != null)
        {
            book._warnings.Add(
                $"{ConfigurationWarningPrefix}: {settings.FilePath}: {settings.LoadError}{Environment.NewLine}Using default settings.");
        }
        book.RefreshCalendars();

        string? chosen = settings.Calendar;
        string calendar = chosen != null && book._calendars.Any(c => IsSameFile(c.FileName, chosen))
            ? chosen
            : book.CalendarForRegion(CultureInfo.CurrentCulture);
        book.LoadCalendar(calendar);
        book.LoadPeople(book.DataFileName);
        return book;
    }

    /// <summary>
    /// Returns whether the value is a valid nameday: empty, or "MM-dd" that exists in a leap year.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if the value is empty or a valid MM-dd date.</returns>
    public static bool IsValidNameday(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }
        return value.Length == 5 &&
            DateOnly.TryParseExact("2000-" + value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    /// <summary>
    /// Gets everything shown for the given day.
    /// </summary>
    /// <param name="day">The day.</param>
    /// <returns>The calendar names, namedays, birthdays and anniversaries, and the next N days.</returns>
    public DayInfo GetDay(DateOnly day)
    {
        string key = ToKey(day);
        var namedays = new List<string>();
        var birthdays = new List<string>();

        foreach (Person person in PeopleOn(key))
        {
            if (person.Namedate == key)
            {
                namedays.Add(person.Name);
            }

            if (person.Birthdate is DateOnly birthdate && ToKey(birthdate) == key)
            {
                birthdays.Add(ShowYears(birthdate, day)
                    ? $"{person.Name} ({day.Year - birthdate.Year})"
                    : person.Name);
            }

            if (person.Anniversaries != null)
            {
                foreach (KeyValuePair<DateOnly, string> anniversary in person.Anniversaries)
                {
                    if (ToKey(anniversary.Key) == key)
                    {
                        birthdays.Add(ShowYears(anniversary.Key, day)
                            ? $"{person.Name} - {anniversary.Value} ({day.Year - anniversary.Key.Year})"
                            : $"{person.Name} - {anniversary.Value}");
                    }
                }
            }
        }

        var nextPeople = new List<string>();
        var nextNames = new List<string>();

        // The days after 31 December 9999, the last day DateOnly can hold, don't exist.
        int daysAhead = Math.Min(Settings.NumberOfDaysAhead, DateOnly.MaxValue.DayNumber - day.DayNumber);
        for (int i = 1; i <= daysAhead; i++)
        {
            DateOnly next = day.AddDays(i);
            string nextKey = ToKey(next);

            foreach (string name in CalendarNamesOn(next).Split(',').Select(n => n.Trim()))
            {
                if (name.Length > 0 && !nextNames.Contains(name))
                {
                    nextNames.Add(name);
                }
            }

            foreach (Person person in PeopleOn(nextKey))
            {
                if (!nextPeople.Contains(person.Name))
                {
                    nextPeople.Add(person.Name);
                }
            }
        }

        return new DayInfo(CalendarNamesOn(day), namedays, birthdays, nextPeople, nextNames);
    }

    /// <summary>
    /// Finds the first day, starting at <paramref name="from"/>, whose calendar names or people match the name.
    /// The search ends on 31 December of the following year, or on 31 December 9999, the last day
    /// <see cref="DateOnly"/> can hold.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="from">The first day to check.</param>
    /// <param name="wholeWordsOnly">Whether only whole words match.</param>
    /// <param name="calendarOnly">Whether to search only the name-day calendar (not people).</param>
    /// <returns>The first matching day, or <c>null</c>.</returns>
    public DateOnly? Find(string name, DateOnly from, bool wholeWordsOnly, bool calendarOnly)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        DateOnly end = from.Year < DateOnly.MaxValue.Year ? new DateOnly(from.Year + 1, 12, 31) : DateOnly.MaxValue;

        // Counting day numbers, unlike AddDays, can step past 31 December 9999 to end the loop.
        for (int dayNumber = from.DayNumber; dayNumber <= end.DayNumber; dayNumber++)
        {
            DateOnly day = DateOnly.FromDayNumber(dayNumber);
            string key = ToKey(day);
            if (Matches(CalendarNamesOn(day), name, wholeWordsOnly))
            {
                return day;
            }
            if (!calendarOnly && PeopleOn(key).Any(p => Matches(p.Name, name, wholeWordsOnly)))
            {
                return day;
            }
        }
        return null;
    }

    /// <summary>
    /// Discovers the available name-day calendars again, e.g. to pick up a file added while the app runs.
    /// </summary>
    public void RefreshCalendars()
    {
        var found = new Dictionary<string, CalendarInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (string folder in _folders)
        {
            if (!Directory.Exists(folder))
            {
                continue;
            }
            foreach (string path in Directory.EnumerateFiles(folder, CalendarFilePrefix + "*" + CalendarFileSuffix))
            {
                string fileName = Path.GetFileName(path);
                if (!found.ContainsKey(fileName) && TryReadCalendarInfo(fileName, () => File.ReadAllText(path)) is CalendarInfo info)
                {
                    found[fileName] = info;
                }
            }
        }

        foreach (string resource in typeof(AnniversaryBook).Assembly.GetManifestResourceNames())
        {
            if (IsCalendarFileName(resource) && !found.ContainsKey(resource) &&
                TryReadCalendarInfo(resource, () => ReadResource(resource)!) is CalendarInfo info)
            {
                found[resource] = info;
            }
        }

        _calendars = found.Values.ToList();
        AddActiveCalendarIfMissing(null);
    }

    /// <summary>
    /// Switches to another name-day calendar. Clears <see cref="Warnings"/> first.
    /// </summary>
    /// <param name="fileName">The calendar file name (or absolute path).</param>
    public void UseCalendar(string fileName)
    {
        _warnings.Clear();
        LoadCalendar(fileName);
    }

    /// <summary>
    /// Gets the path the people file is saved to: the file it was loaded from, or the app-data folder for the built-in default.
    /// </summary>
    /// <returns>The full path of the people file to write.</returns>
    public string SavePathFor()
    {
        if (DataFilePath != null)
        {
            return DataFilePath;
        }
        return Path.IsPathRooted(DataFileName) ? DataFileName : Path.Combine(AppDataFolder, DataFileName);
    }

    /// <summary>
    /// Writes the people to the file and makes them the current people.
    /// </summary>
    /// <param name="people">The people, in the order to keep.</param>
    /// <param name="path">The file to write.</param>
    /// <exception cref="IOException">The file couldn't be written.</exception>
    /// <exception cref="UnauthorizedAccessException">The file or folder isn't writable.</exception>
    public void SavePeople(IReadOnlyList<Person> people, string path)
    {
        var data = new AnniversaryData { People = people.Select(Normalize).ToList() };
        string json = JsonSerializer.Serialize(data, WriteOptions);

        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
        File.Move(temporaryPath, path, overwrite: true);

        _people = data.People;
        DataFilePath = path;
        DataFileLoadFailed = false;
        _dataFileStamp = StampOf(path);
        IndexPeople();
    }

    /// <summary>
    /// Reads a data file and uses it where it is from now on (spec 7.11). Nothing changes if it can't be read.
    /// </summary>
    /// <param name="path">The full path of the data file.</param>
    /// <exception cref="JsonException">The file isn't valid JSON or has no "data" list of people.</exception>
    /// <exception cref="IOException">The file couldn't be read.</exception>
    /// <exception cref="UnauthorizedAccessException">The file isn't readable.</exception>
    public void OpenDataFile(string path)
    {
        string json = File.ReadAllText(path);
        using (JsonDocument document = JsonDocument.Parse(json, DocumentOptions))
        {
            // A calendar or any other JSON file would otherwise open as an empty list, and saving would replace it.
            bool hasPeople = document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.EnumerateObject().Any(p =>
                    p.Name.Equals("data", StringComparison.OrdinalIgnoreCase) && p.Value.ValueKind == JsonValueKind.Array);
            if (!hasPeople)
            {
                throw new JsonException("This isn't an Anniversary data file (it has no \"data\" list).");
            }
        }

        _people = JsonSerializer.Deserialize<AnniversaryData>(json, ReadOptions)?.People ?? new List<Person>();
        DataFilePath = path;
        DataFileLoadFailed = false;
        _dataFileStamp = StampOf(path);
        IndexPeople();
    }

    /// <summary>
    /// Returns whether the data file exists and was changed since the people were loaded or saved, e.g. on another
    /// computer through a cloud drive, or appeared after being missing (spec 5.6).
    /// </summary>
    /// <returns><c>true</c> if the file's last-write time or size differs from when it was loaded or saved.</returns>
    public bool HasDataFileChanged()
    {
        (DateTime WriteTime, long Length)? stamp = StampOf(DataFilePath);
        return stamp != null && stamp != _dataFileStamp;
    }

    /// <summary>
    /// Loads the people again if the data file changed (see <see cref="HasDataFileChanged"/>). Clears
    /// <see cref="Warnings"/> first; loading problems are reported there as at start-up (spec 5.4).
    /// </summary>
    /// <returns><c>true</c> if the people were loaded again.</returns>
    public bool ReloadPeopleIfChanged()
    {
        if (!HasDataFileChanged())
        {
            return false;
        }
        _warnings.Clear();
        LoadPeople(DataFilePath!);
        return true;
    }

    // The data file named in the settings, or the default one (spec 5.3).
    private string DataFileName => string.IsNullOrWhiteSpace(Settings.DataFile) ? DefaultDataFile : Settings.DataFile;

    private bool ShowYears(DateOnly date, DateOnly day)
    {
        // A year after the shown day means "year unknown" (v4 data uses 9999 for that), so no negative years.
        return Settings.DisplayYearsOnBirthdays && date.Year <= day.Year;
    }

    private void LoadCalendar(string fileName)
    {
        _calendar = new Dictionary<string, string>();
        _calendarRules = new List<(Match Rule, string Names)>();
        CalendarData? data = null;
        try
        {
            string? json = ReadFile(fileName, out _);
            if (json == null)
            {
                _warnings.Add($"Calendar file not found: {fileName}");
            }
            else
            {
                data = JsonSerializer.Deserialize<CalendarData>(json, ReadOptions);
                AddCalendarEntries(fileName, data?.Data ?? new Dictionary<string, string>());
            }
        }
        catch (Exception ex) when (IsLoadException(ex))
        {
            _warnings.Add($"Error loading calendar: {ex.Message}");
        }

        CalendarFile = fileName;
        AddActiveCalendarIfMissing(data);
    }

    private void AddCalendarEntries(string fileName, Dictionary<string, string> entries)
    {
        var unknownKeys = new List<string>();
        foreach (KeyValuePair<string, string> entry in entries)
        {
            if (entry.Key.Length > 0 && IsValidNameday(entry.Key))
            {
                _calendar[entry.Key] = entry.Value;
            }
            else if (RuleKeyPattern.Match(entry.Key) is { Success: true } rule)
            {
                _calendarRules.Add((rule, entry.Value));
            }
            else
            {
                unknownKeys.Add(entry.Key);
            }
        }

        if (unknownKeys.Count > 0)
        {
            string more = unknownKeys.Count > MaxReportedKeys ? $" and {unknownKeys.Count - MaxReportedKeys} more" : string.Empty;
            _warnings.Add($"Unknown dates in {fileName}: {string.Join(", ", unknownKeys.Take(MaxReportedKeys))}{more}");
        }
    }

    // The day's MM-dd entry, then the rules that fall on the day, in file order (spec 6.3).
    private string CalendarNamesOn(DateOnly day)
    {
        var names = new List<string>();
        if (_calendar.TryGetValue(ToKey(day), out string? dayNames) && !string.IsNullOrEmpty(dayNames))
        {
            names.Add(dayNames);
        }

        foreach ((Match rule, string ruleNames) in _calendarRules)
        {
            // An offset can move a rule's date into the previous or next year.
            bool fallsOnDay = Enumerable.Range(day.Year - 1, 3)
                .Any(year => TryResolveRule(rule, year, out DateOnly date) && date == day);
            if (fallsOnDay && !string.IsNullOrEmpty(ruleNames))
            {
                names.Add(ruleNames);
            }
        }

        return string.Join(", ", names);
    }

    // The date of a parsed rule key (spec 5.1) in the year; false if it falls outside DateOnly's range.
    private static bool TryResolveRule(Match rule, int year, out DateOnly date)
    {
        date = default;
        if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
        {
            return false;
        }

        DateOnly ruleDate;
        if (!rule.Groups["month"].Success)
        {
            ruleDate = EasterSunday(year);
        }
        else
        {
            int month = int.Parse(rule.Groups["month"].Value, CultureInfo.InvariantCulture);
            var weekday = (DayOfWeek)Array.IndexOf(WeekdayNames, rule.Groups["weekday"].Value.ToLowerInvariant());
            string nth = rule.Groups["nth"].Value;
            if (nth.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                var lastDay = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
                ruleDate = lastDay.AddDays(-(((int)lastDay.DayOfWeek - (int)weekday + 7) % 7));
            }
            else
            {
                var firstDay = new DateOnly(year, month, 1);
                int toWeekday = ((int)weekday - (int)firstDay.DayOfWeek + 7) % 7;
                ruleDate = firstDay.AddDays(toWeekday + 7 * (int.Parse(nth, CultureInfo.InvariantCulture) - 1));
            }
        }

        int dayNumber = ruleDate.DayNumber;
        if (rule.Groups["offset"].Success)
        {
            dayNumber += int.Parse(rule.Groups["offset"].Value, CultureInfo.InvariantCulture);
        }
        if (dayNumber < DateOnly.MinValue.DayNumber || dayNumber > DateOnly.MaxValue.DayNumber)
        {
            return false;
        }

        date = DateOnly.FromDayNumber(dayNumber);
        return true;
    }

    // Western Easter by the anonymous Gregorian algorithm (Meeus, Jones and Butcher).
    private static DateOnly EasterSunday(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int monthAndDay = h + l - 7 * m + 114;
        return new DateOnly(year, monthAndDay / 31, monthAndDay % 31 + 1);
    }

    private void LoadPeople(string name)
    {
        _people = new List<Person>();
        DataFileLoadFailed = false;
        string? path = null;
        try
        {
            string? json = ReadFile(name, out path);
            if (json == null)
            {
                _warnings.Add($"Data file not found: {name}");
                DataFileLoadFailed = true;

                // A chosen or absolute file stays the data file, so it's saved there and picked up when it appears.
                path = Path.IsPathRooted(name) ? name : null;
            }
            else
            {
                _people = JsonSerializer.Deserialize<AnniversaryData>(json, ReadOptions)?.People ?? new List<Person>();
            }
        }
        catch (Exception ex) when (IsLoadException(ex))
        {
            _warnings.Add($"Error loading data: {ex.Message}");
            DataFileLoadFailed = true;
        }

        DataFilePath = path;
        _dataFileStamp = StampOf(path);
        IndexPeople();
    }

    private static (DateTime WriteTime, long Length)? StampOf(string? path)
    {
        if (path == null)
        {
            return null;
        }
        var info = new FileInfo(path);
        return info.Exists ? (info.LastWriteTimeUtc, info.Length) : null;
    }

    private void IndexPeople()
    {
        _peopleByDate.Clear();
        foreach (Person person in _people)
        {
            if (person.Birthdate is DateOnly birthdate)
            {
                AddPersonToDate(ToKey(birthdate), person);
            }
            if (!string.IsNullOrEmpty(person.Namedate))
            {
                AddPersonToDate(person.Namedate, person);
            }
            if (person.Anniversaries != null)
            {
                foreach (DateOnly date in person.Anniversaries.Keys)
                {
                    AddPersonToDate(ToKey(date), person);
                }
            }
        }
    }

    private void AddPersonToDate(string key, Person person)
    {
        if (!_peopleByDate.TryGetValue(key, out List<Person>? people))
        {
            people = new List<Person>();
            _peopleByDate[key] = people;
        }
        if (!people.Contains(person))
        {
            people.Add(person);
        }
    }

    private IEnumerable<Person> PeopleOn(string key)
    {
        return _peopleByDate.TryGetValue(key, out List<Person>? people) ? people : Enumerable.Empty<Person>();
    }

    private void AddActiveCalendarIfMissing(CalendarData? data)
    {
        if (CalendarFile.Length > 0 && !_calendars.Any(c => IsSameFile(c.FileName, CalendarFile)))
        {
            _calendars.Add(new CalendarInfo(CalendarFile, data?.Name ?? DisplayNameFromFile(CalendarFile), data?.Locale));
        }

        // Calendars for a region (with a locale) first, then the others, e.g. International Days (spec 5.7).
        _calendars = _calendars
            .OrderBy(c => string.IsNullOrEmpty(c.Locale))
            .ThenBy(c => c.DisplayName, StringComparer.CurrentCulture)
            .ToList();
    }

    // The first calendar (by display name) whose locale has the culture's region, e.g. "en-CZ" -> CZ -> Czech;
    // otherwise the US one (spec 5.7).
    private string CalendarForRegion(CultureInfo culture)
    {
        string? region = RegionOf(culture.Name);
        CalendarInfo? match = region == null
            ? null
            : _calendars.FirstOrDefault(c => c.Locale != null && RegionOf(c.Locale) == region);
        return match?.FileName ?? FallbackCalendarFile;
    }

    // The two-letter region of a culture name, or null for a neutral or invariant culture or an unknown name.
    private static string? RegionOf(string cultureName)
    {
        try
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
            return culture.IsNeutralCulture || culture.Name.Length == 0
                ? null
                : new RegionInfo(culture.Name).TwoLetterISORegionName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private string? ReadFile(string name, out string? path)
    {
        path = null;
        if (Path.IsPathRooted(name))
        {
            if (!File.Exists(name))
            {
                return null;
            }
            path = name;
            return File.ReadAllText(name);
        }

        foreach (string folder in _folders)
        {
            string candidate = Path.Combine(folder, name);
            if (File.Exists(candidate))
            {
                path = candidate;
                return File.ReadAllText(candidate);
            }
        }

        return ReadResource(name);
    }

    private static string? ReadResource(string name)
    {
        using Stream? stream = typeof(AnniversaryBook).Assembly.GetManifestResourceStream(name);
        if (stream == null)
        {
            return null;
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static CalendarInfo? TryReadCalendarInfo(string fileName, Func<string> readJson)
    {
        try
        {
            CalendarData? data = JsonSerializer.Deserialize<CalendarData>(readJson(), ReadOptions);
            return new CalendarInfo(fileName, data?.Name ?? DisplayNameFromFile(fileName), data?.Locale);
        }
        catch (Exception ex) when (IsLoadException(ex))
        {
            // An unreadable file isn't offered; choosing it by name still reports the error.
            return null;
        }
    }

    private static string DisplayNameFromFile(string fileName)
    {
        string name = Path.GetFileName(fileName);
        if (IsCalendarFileName(name))
        {
            return name[CalendarFilePrefix.Length..^CalendarFileSuffix.Length];
        }
        return Path.GetFileNameWithoutExtension(name);
    }

    private static bool IsCalendarFileName(string name)
    {
        return name.StartsWith(CalendarFilePrefix, StringComparison.OrdinalIgnoreCase) &&
            name.EndsWith(CalendarFileSuffix, StringComparison.OrdinalIgnoreCase) &&
            name.Length > CalendarFilePrefix.Length + CalendarFileSuffix.Length;
    }

    private static bool IsSameFile(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLoadException(Exception ex)
    {
        return ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException;
    }

    private static Person Normalize(Person person)
    {
        return new Person
        {
            Name = person.Name.Trim(),
            Namedate = string.IsNullOrWhiteSpace(person.Namedate) ? null : person.Namedate.Trim(),
            Birthdate = person.Birthdate,
            Anniversaries = person.Anniversaries is { Count: > 0 }
                ? person.Anniversaries.OrderBy(a => a.Key).ToDictionary(a => a.Key, a => a.Value)
                : null
        };
    }

    private static bool Matches(string text, string search, bool wholeWordsOnly)
    {
        if (wholeWordsOnly)
        {
            return text.Split(WordDelimiters, StringSplitOptions.RemoveEmptyEntries)
                .Any(w => w.Equals(search, StringComparison.OrdinalIgnoreCase));
        }
        return text.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToKey(DateOnly date)
    {
        return date.ToString("MM-dd", CultureInfo.InvariantCulture);
    }
}
