using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Anniversary.Views;

/// <summary>
/// Dialog for entering a name to find. Returns <c>true</c> from <c>ShowDialog</c> when confirmed.
/// </summary>
public partial class FindDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the dialog.
    /// </summary>
    public FindDialog()
    {
        InitializeComponent();
        Opened += OnOpened;
    }

    /// <summary>
    /// Gets or sets the name to search for.
    /// </summary>
    public string FindName
    {
        get => NameBox.Text ?? string.Empty;
        set => NameBox.Text = value;
    }

    /// <summary>
    /// Gets or sets whether to search from the current date (otherwise from 1 January).
    /// </summary>
    public bool FromCurrentDate
    {
        get => FromCurrentDateBox.IsChecked == true;
        set => FromCurrentDateBox.IsChecked = value;
    }

    /// <summary>
    /// Gets or sets whether only whole words match.
    /// </summary>
    public bool WholeWordsOnly
    {
        get => WholeWordsBox.IsChecked == true;
        set => WholeWordsBox.IsChecked = value;
    }

    /// <summary>
    /// Gets or sets whether to search the name-day calendar only (not people).
    /// </summary>
    public bool SearchCalendarOnly
    {
        get => CalendarOnlyBox.IsChecked == true;
        set => CalendarOnlyBox.IsChecked = value;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        NameBox.Focus();
        NameBox.SelectAll();
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
