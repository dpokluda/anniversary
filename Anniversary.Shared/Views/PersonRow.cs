using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Anniversary.Model;

namespace Anniversary.Views;

/// <summary>
/// An editable row of the people editor. Reports changes and validation errors to the data grid.
/// </summary>
public sealed class PersonRow : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private string _name = string.Empty;
    private string _nameday = string.Empty;
    private DateTime? _birthday;
    private Dictionary<DateOnly, string> _anniversaries = new();

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the validation errors of a property change.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Gets or sets the person's name. Required.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value ?? string.Empty;
            OnChanged(nameof(Name));
        }
    }

    /// <summary>
    /// Gets or sets the nameday in MM-dd format, or an empty string.
    /// </summary>
    public string Nameday
    {
        get => _nameday;
        set
        {
            _nameday = value ?? string.Empty;
            OnChanged(nameof(Nameday));
            OnChanged(nameof(NamedayDate));
        }
    }

    /// <summary>
    /// Gets the current culture's month-and-day format, e.g. "M/d" (en-US) or "d. M." (cs-CZ).
    /// </summary>
    public static string MonthDayFormat => MonthDayPattern(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);

    /// <summary>
    /// Gets or sets the nameday as a date for the date picker. Only the month and day are kept; the year is the current
    /// one (or the nearest earlier leap year for 29 February) and is ignored when set.
    /// </summary>
    public DateTime? NamedayDate
    {
        get
        {
            string nameday = _nameday.Trim();
            if (nameday.Length == 0 || !AnniversaryBook.IsValidNameday(nameday))
            {
                return null;
            }

            int month = int.Parse(nameday[..2], CultureInfo.InvariantCulture);
            int day = int.Parse(nameday[3..], CultureInfo.InvariantCulture);
            int year = DateTime.Today.Year;
            while (day > DateTime.DaysInMonth(year, month))
            {
                year--;
            }
            return new DateTime(year, month, day);
        }
        set => Nameday = value?.ToString("MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// Derives a month-and-day format from a short date format by removing the year, e.g. "M/d/yyyy" → "M/d",
    /// "d. M. yyyy" → "d. M.", "yyyy-MM-dd" → "MM-dd".
    /// </summary>
    /// <param name="shortDatePattern">A short date format pattern.</param>
    /// <returns>The pattern without the year.</returns>
    public static string MonthDayPattern(string shortDatePattern)
    {
        // A leading year takes the separator after it; otherwise the year takes the separator before it.
        string pattern = Regex.Replace(shortDatePattern, @"^y+[^dM]*|[^dM.]*y+[^dM]*$|y+[^dM]*", string.Empty);
        return pattern.Trim();
    }

    /// <summary>
    /// Gets or sets the birthday (the date picker works with <see cref="DateTime"/>).
    /// </summary>
    public DateTime? Birthday
    {
        get => _birthday;
        set
        {
            _birthday = value?.Date;
            OnChanged(nameof(Birthday));
        }
    }

    /// <summary>
    /// Gets or sets the anniversaries, where the key is the date and the value its description.
    /// </summary>
    public Dictionary<DateOnly, string> Anniversaries
    {
        get => _anniversaries;
        set
        {
            _anniversaries = value ?? new Dictionary<DateOnly, string>();
            OnChanged(nameof(Anniversaries));
            OnChanged(nameof(AnniversariesSummary));
            OnChanged(nameof(HasAnniversaries));
        }
    }

    /// <summary>
    /// Gets a one-line summary of the anniversaries, ordered by date, e.g. "svatba (6/21/2008)".
    /// </summary>
    public string AnniversariesSummary =>
        string.Join(", ", _anniversaries.OrderBy(a => a.Key)
            .Select(a => $"{a.Value} ({a.Key.ToString("d", CultureInfo.CurrentCulture)})"));

    /// <summary>
    /// Gets whether the person has any anniversaries.
    /// </summary>
    public bool HasAnniversaries => _anniversaries.Count > 0;

    /// <summary>
    /// Gets whether the row has validation errors.
    /// </summary>
    public bool HasErrors => NameError != null || NamedayError != null;

    private string? NameError => string.IsNullOrWhiteSpace(_name) ? "Name is required." : null;

    private string? NamedayError => AnniversaryBook.IsValidNameday(_nameday.Trim()) ? null : "Use MM-dd, e.g. 06-24.";

    /// <summary>
    /// Creates a row from a person.
    /// </summary>
    /// <param name="person">The person to edit.</param>
    /// <returns>A new row with copied values.</returns>
    public static PersonRow From(Person person)
    {
        return new PersonRow
        {
            _name = person.Name,
            _nameday = person.Namedate ?? string.Empty,
            _birthday = person.Birthdate?.ToDateTime(TimeOnly.MinValue),
            _anniversaries = person.Anniversaries != null
                ? new Dictionary<DateOnly, string>(person.Anniversaries)
                : new Dictionary<DateOnly, string>()
        };
    }

    /// <summary>
    /// Creates a person from the row.
    /// </summary>
    /// <returns>A new person with the edited values.</returns>
    public Person ToPerson()
    {
        string nameday = _nameday.Trim();
        return new Person
        {
            Name = _name.Trim(),
            Namedate = nameday.Length > 0 ? nameday : null,
            Birthdate = _birthday is DateTime birthday ? DateOnly.FromDateTime(birthday) : null,
            Anniversaries = _anniversaries.Count > 0 ? new Dictionary<DateOnly, string>(_anniversaries) : null
        };
    }

    /// <summary>
    /// Gets the validation errors of a property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The error messages, or an empty sequence.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        string? error = propertyName switch
        {
            nameof(Name) => NameError,
            nameof(Nameday) or nameof(NamedayDate) => NamedayError,
            _ => null
        };
        return error != null ? new[] { error } : Array.Empty<string>();
    }

    private void OnChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
    }
}
