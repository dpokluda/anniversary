using System.Collections;
using System.ComponentModel;

namespace Anniversary.Views;

/// <summary>
/// An editable row of the anniversaries dialog. Reports changes and validation errors to the data grid.
/// </summary>
public sealed class AnniversaryRow : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private DateTime? _date;
    private string _name = string.Empty;
    private bool _isDuplicate;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the validation errors of a property change.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Gets or sets the anniversary date (the date picker works with <see cref="DateTime"/>). Required and unique.
    /// </summary>
    public DateTime? Date
    {
        get => _date;
        set
        {
            _date = value?.Date;
            OnChanged(nameof(Date));
        }
    }

    /// <summary>
    /// Gets or sets the anniversary description. Required.
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
    /// Gets whether the row has validation errors.
    /// </summary>
    public bool HasErrors => DateError != null || NameError != null;

    private string? DateError => _date == null ? "Date is required." : _isDuplicate ? "This date is used twice." : null;

    private string? NameError => string.IsNullOrWhiteSpace(_name) ? "Anniversary is required." : null;

    /// <summary>
    /// Creates rows from a person's anniversaries, ordered by date.
    /// </summary>
    /// <param name="anniversaries">The anniversaries, or <c>null</c>.</param>
    /// <returns>The rows.</returns>
    public static List<AnniversaryRow> From(IReadOnlyDictionary<DateOnly, string>? anniversaries)
    {
        if (anniversaries == null)
        {
            return new List<AnniversaryRow>();
        }
        return anniversaries.OrderBy(a => a.Key)
            .Select(a => new AnniversaryRow { _date = a.Key.ToDateTime(TimeOnly.MinValue), _name = a.Value })
            .ToList();
    }

    /// <summary>
    /// Marks rows that share a date and returns whether all rows are valid.
    /// </summary>
    /// <param name="rows">All rows of the dialog.</param>
    /// <returns><c>true</c> if every row is valid.</returns>
    public static bool Validate(IReadOnlyList<AnniversaryRow> rows)
    {
        foreach (AnniversaryRow row in rows)
        {
            bool isDuplicate = row._date != null && rows.Count(r => r._date == row._date) > 1;
            if (row._isDuplicate != isDuplicate)
            {
                row._isDuplicate = isDuplicate;
                row.OnChanged(nameof(Date));
            }
        }
        return rows.All(r => !r.HasErrors);
    }

    /// <summary>
    /// Converts valid rows to the anniversaries dictionary.
    /// </summary>
    /// <param name="rows">The rows; they must be valid (see <see cref="Validate"/>).</param>
    /// <returns>The anniversaries, keyed by date.</returns>
    public static Dictionary<DateOnly, string> ToDictionary(IEnumerable<AnniversaryRow> rows)
    {
        return rows.Where(r => r._date != null)
            .ToDictionary(r => DateOnly.FromDateTime(r._date!.Value), r => r._name.Trim());
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
            nameof(Date) => DateError,
            nameof(Name) => NameError,
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
