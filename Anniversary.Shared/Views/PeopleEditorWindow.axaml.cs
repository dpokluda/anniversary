using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Anniversary.Views;

/// <summary>
/// Editor for the people data: a grid with one row per person. Returns <c>true</c> from <c>ShowDialog</c> when saved.
/// </summary>
public partial class PeopleEditorWindow : Window
{
    private readonly AnniversaryBook _book;
    private readonly string _savePath;
    private readonly ObservableCollection<PersonRow> _rows;
    private readonly DataGridCollectionView _view;
    private string _appliedFilter = string.Empty;

    /// <summary>
    /// Initializes a new instance for the XAML previewer, using the built-in defaults.
    /// </summary>
    public PeopleEditorWindow() : this(AnniversaryBook.Load(null))
    {
    }

    /// <summary>
    /// Initializes a new instance of the editor. It works on a copy of the people until Save.
    /// </summary>
    /// <param name="book">The book whose people are edited and saved.</param>
    public PeopleEditorWindow(AnniversaryBook book)
    {
        InitializeComponent();

        _book = book;
        _savePath = book.SavePathFor();
        _rows = new ObservableCollection<PersonRow>(book.People.Select(PersonRow.From));
        foreach (PersonRow row in _rows)
        {
            row.PropertyChanged += OnRowChanged;
        }
        _rows.CollectionChanged += OnRowsChanged;

        _view = new DataGridCollectionView(_rows) { Filter = MatchesFilter };
        _view.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(PersonRow.Name), ListSortDirection.Ascending));
        PeopleGrid.ItemsSource = _view;

        UpdateState();
    }

    private bool MatchesFilter(object item)
    {
        return string.IsNullOrWhiteSpace(_appliedFilter) ||
            ((PersonRow)item).Name.Contains(_appliedFilter.Trim(), StringComparison.CurrentCultureIgnoreCase);
    }

    private void UpdateState()
    {
        SaveButton.IsEnabled = _rows.All(r => !r.HasErrors);
        FooterText.Text = $"{_rows.Count} people · {_savePath}";
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateState();
    }

    private void OnRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateState();
    }

    private void OnFilterChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        // TextChanged arrives through the dispatcher, so only refresh when the filter really changed;
        // the view can't refresh while a row is being edited.
        string filter = FilterBox.Text ?? string.Empty;
        if (filter == _appliedFilter)
        {
            return;
        }
        _appliedFilter = filter;
        PeopleGrid.CommitEdit();
        _view.Refresh();
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        PeopleGrid.CommitEdit();
        FilterBox.Text = string.Empty;
        ApplyFilter();

        var row = new PersonRow();
        row.PropertyChanged += OnRowChanged;
        _rows.Add(row);

        PeopleGrid.SelectedItem = row;
        PeopleGrid.ScrollIntoView(row, PeopleGrid.Columns[0]);
        PeopleGrid.CurrentColumn = PeopleGrid.Columns[0];
        PeopleGrid.BeginEdit();
    }

    private void OnRemove(object? sender, RoutedEventArgs e)
    {
        foreach (PersonRow row in PeopleGrid.SelectedItems.OfType<PersonRow>().ToList())
        {
            row.PropertyChanged -= OnRowChanged;
            _rows.Remove(row);
        }
    }

    private async void OnEditAnniversaries(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: PersonRow row })
        {
            var dialog = new AnniversariesDialog(row.Name, row.Anniversaries);
            if (await dialog.ShowDialog<bool>(this))
            {
                row.Anniversaries = dialog.Anniversaries;
            }
        }
    }

    private async void OnSave(object? sender, RoutedEventArgs e)
    {
        PeopleGrid.CommitEdit();
        if (_rows.Any(r => r.HasErrors))
        {
            UpdateState();
            return;
        }

        try
        {
            _book.SavePeople(_rows.Select(r => r.ToPerson()).ToList(), _savePath);
            Close(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await MessageDialog.Show(this, "Edit People", $"Could not save the data file: {ex.Message}");
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
