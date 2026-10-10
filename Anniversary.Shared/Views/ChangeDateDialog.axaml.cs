using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Anniversary.Views;

/// <summary>
/// Dialog for changing the displayed date. Returns <c>true</c> from <c>ShowDialog</c> when confirmed.
/// </summary>
public partial class ChangeDateDialog : Window
{
    private DateOnly _initialDate = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Initializes a new instance of the dialog.
    /// </summary>
    public ChangeDateDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the selected date. If the picker is cleared, the initial date is kept.
    /// </summary>
    public DateOnly SelectedDate
    {
        get => Picker.SelectedDate is DateTime date ? DateOnly.FromDateTime(date) : _initialDate;
        set
        {
            _initialDate = value;
            Picker.SelectedDate = value.ToDateTime(TimeOnly.MinValue);
        }
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
