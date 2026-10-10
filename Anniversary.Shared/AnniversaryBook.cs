using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Anniversary.Model;

namespace Anniversary;

/// <summary>
/// Holds the configuration, the name-day calendar and the people, and answers everything the UI asks about a day.
/// </summary>
/// <remarks>
/// This is the only non-UI logic class. It never reads the clock and never shows UI: the caller passes the day in,
/// and loading problems are collected in <see cref="Warnings"/>.
/// </remarks>
public sealed class AnniversaryBook
{
    /// <summary>
    /// The configuration file name.
    /// </summary>
    public const string ConfigFileName = "appsettings.json";

    /// <summary>
    /// The prefix of warnings about the configuration file (the UI shows them with a different title).
    /// </summary>
    public const string ConfigurationWarningPrefix = "Error loading configuration";

    private const string CalendarFilePrefix = "Anniversary.Calendar.";
    private const string CalendarFileSuffix = ".json";

    private static readonly char[] WordDelimiters = [' ', ',', ';', '\t', '/', '-'];

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
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
    private List<Person> _people = new();
    private List<CalendarInfo> _calendars = new();

    private AnniversaryBook(string[] folders)
    {
        _folders = folders.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
        AppDataFolder = _folders.Length > 0 ? _folders[^1] : string.Empty;
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public AppConfig Config { get; private set; } = new();

    /// <summary>
    /// Gets the problems found by the last load operation.
    /// </summary>
    public IReadOnlyList<string> Warnings => _warnings;

    /// <summary>
    /// Gets the people in data-file order.
    /// </summary>
    public IReadOnlyList<Person> People => _people;

    /// <summary>
    /// Gets the path of the people file, or <c>null</c> when the people came from the built-in default.
    /// </summary>
    public string? DataFilePath { get; private set; }

    /// <summary>
    /// Gets the user app-data folder (the last folder passed to <see cref="Load"/>), or an empty string.
    /// </summary>
    public string AppDataFolder { get; }

    /// <summary>
    /// Gets the available name-day calendars, sorted by display name.
    /// </summary>
    public IReadOnlyList<CalendarInfo> Calendars => _calendars;

    /// <summary>
    /// Gets the file name of the active name-day calendar.
    /// </summary>
    public string CalendarFile { get; private set; } = string.Empty;

    /// <summary>
    /// Loads the configuration, the name-day calendar and the people.
    /// </summary>
    /// <param name="preferredCalendar">The calendar chosen by the user, or <c>null</c> to use the configured one.</param>
    /// <param name="folders">The folders to look in, in order of precedence; the last one is the user app-data folder.</param>
    /// <returns>The loaded book. Problems are reported in <see cref="Warnings"/>.</returns>
    public static AnniversaryBook Load(string? preferredCalendar, params string[] folders)
    {
        var book = new AnniversaryBook(folders);
        book.LoadConfig();
        book.RefreshCalendars();

        string calendar = book.Config.CalendarFile;
        if (preferredCalendar != null && book._calendars.Any(c => IsSameFile(c.FileName, preferredCalendar)))
        {
            calendar = preferredCalendar;
        }
        book.LoadCalendar(calendar);
        book.LoadPeople();
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
        for (int i = 1; i <= Config.NumberOfDaysAhead; i++)
        {
            string nextKey = ToKey(day.AddDays(i));

            if (_calendar.TryGetValue(nextKey, out string? names))
            {
                foreach (string name in names.Split(',').Select(n => n.Trim()))
                {
                    if (name.Length > 0 && !nextNames.Contains(name))
                    {
                        nextNames.Add(name);
                    }
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

        return new DayInfo(_calendar.GetValueOrDefault(key) ?? string.Empty, namedays, birthdays, nextPeople, nextNames);
    }

    /// <summary>
    /// Finds the first day, starting at <paramref name="from"/>, whose calendar names or people match the name.
    /// The search ends on 31 December of the following year.
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

        var end = new DateOnly(from.Year + 1, 12, 31);
        for (DateOnly day = from; day <= end; day = day.AddDays(1))
        {
            string key = ToKey(day);
            if (_calendar.TryGetValue(key, out string? names) && Matches(names, name, wholeWordsOnly))
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
        return Path.IsPathRooted(Config.DataFile) ? Config.DataFile : Path.Combine(AppDataFolder, Config.DataFile);
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
        IndexPeople();
    }

    private bool ShowYears(DateOnly date, DateOnly day)
    {
        // A year after the shown day means "year unknown" (v4 data uses 9999 for that), so no negative years.
        return Config.DisplayYearsOnBirthdays && date.Year <= day.Year;
    }

    private void LoadConfig()
    {
        try
        {
            string? json = ReadFile(ConfigFileName, out _);
            if (json != null)
            {
                Config = JsonSerializer.Deserialize<AppConfig>(json, ReadOptions) ?? new AppConfig();
            }
        }
        catch (Exception ex) when (IsLoadException(ex))
        {
            _warnings.Add($"{ConfigurationWarningPrefix}: {ex.Message}{Environment.NewLine}Using default settings.");
            Config = new AppConfig();
        }
    }

    private void LoadCalendar(string fileName)
    {
        _calendar = new Dictionary<string, string>();
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
                _calendar = data?.Data ?? new Dictionary<string, string>();
            }
        }
        catch (Exception ex) when (IsLoadException(ex))
        {
            _warnings.Add($"Error loading calendar: {ex.Message}");
        }

        CalendarFile = fileName;
        AddActiveCalendarIfMissing(data);
    }

    private void LoadPeople()
    {
        _people = new List<Person>();
        DataFilePath = null;
        try
        {
            string? json = ReadFile(Config.DataFile, out string? path);
            if (json == null)
            {
                _warnings.Add($"Data file not found: {Config.DataFile}");
            }
            else
            {
                _people = JsonSerializer.Deserialize<AnniversaryData>(json, ReadOptions)?.People ?? new List<Person>();
                DataFilePath = path;
            }
        }
        catch (Exception ex) when (IsLoadException(ex))
        {
            _warnings.Add($"Error loading data: {ex.Message}");
        }
        IndexPeople();
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
        _calendars.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCulture));
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
