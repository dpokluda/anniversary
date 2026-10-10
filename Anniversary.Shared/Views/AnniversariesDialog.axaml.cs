using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Anniversary.Views;

/// <summary>
/// Editor for one person's anniversaries (date and description). Returns <c>true</c> from <c>ShowDialog</c> when confirmed.
/// </summary>
public partial class AnniversariesDialog : Window
{
    private readonly ObservableCollection<AnniversaryRow> _rows;
    private bool _validating;

    /// <summary>
    /// Initializes a new instance for the XAML previewer.
    /// </summary>
    public AnniversariesDialog() : this(string.Empty, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the dialog. It works on a copy of the anniversaries until OK.
    /// </summary>
    /// <param name="personName">The name of the person, shown in the title.</param>
    /// <param name="anniversaries">The anniversaries to edit, or <c>null</c>.</param>
    public AnniversariesDialog(string personName, IReadOnlyDictionary<DateOnly, string>? anniversaries)
    {
        InitializeComponent();

        Title = string.IsNullOrWhiteSpace(personName) ? "Anniversaries" : $"Anniversaries – {personName}";
        _rows = new ObservableCollection<AnniversaryRow>(AnniversaryRow.From(anniversaries));
        foreach (AnniversaryRow row in _rows)
        {
            row.PropertyChanged += OnRowChanged;
        }
        _rows.CollectionChanged += (_, _) => Validate();
        AnniversaryGrid.ItemsSource = _rows;

        Validate();
    }

    /// <summary>
    /// Gets the edited anniversaries, keyed by date. Valid after the dialog returns <c>true</c>.
    /// </summary>
    public Dictionary<DateOnly, string> Anniversaries { get; private set; } = new();

    private void Validate()
    {
        if (_validating)
        {
            return;
        }

        _validating = true;
        bool isValid = AnniversaryRow.Validate(_rows);
        _validating = false;

        OkButton.IsEnabled = isValid;
        ErrorText.IsVisible = !isValid;
        ErrorText.Text = "Every anniversary needs a date and a name, and each date can be used only once.";
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        Validate();
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        var row = new AnniversaryRow();
        row.PropertyChanged += OnRowChanged;
        _rows.Add(row);

        AnniversaryGrid.SelectedItem = row;
        AnniversaryGrid.ScrollIntoView(row, AnniversaryGrid.Columns[0]);
    }

    private void OnRemove(object? sender, RoutedEventArgs e)
    {
        foreach (AnniversaryRow row in AnniversaryGrid.SelectedItems.OfType<AnniversaryRow>().ToList())
        {
            row.PropertyChanged -= OnRowChanged;
            _rows.Remove(row);
        }
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        AnniversaryGrid.CommitEdit();
        if (!AnniversaryRow.Validate(_rows))
        {
            Validate();
            return;
        }

        Anniversaries = AnniversaryRow.ToDictionary(_rows);
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
