using System.Globalization;
using Anniversary.Model;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Anniversary.Views;

/// <summary>
/// The main window: shows the day, the namedays, birthdays and anniversaries, and the next N days.
/// </summary>
public partial class MainWindow : Window
{
    private readonly AnniversaryBook _book;
    private readonly UserSettings _settings;
    private readonly NativeMenu _nativeCalendarMenu;
    private readonly Dictionary<AppTheme, NativeMenuItem> _nativeThemeItems;
    private DateOnly _day;

    // Find state, kept between Find and Find Next (as in v4)
    private string _findName = string.Empty;
    private bool _findFromCurrentDate;
    private bool _findWholeWords;
    private bool _findCalendarOnly;

    /// <summary>
    /// Initializes a new instance for the XAML previewer, using the built-in defaults.
    /// </summary>
    public MainWindow() : this(AnniversaryBook.Load(null), DateOnly.FromDateTime(DateTime.Today), new UserSettings())
    {
    }

    /// <summary>
    /// Initializes a new instance of the main window.
    /// </summary>
    /// <param name="book">The loaded data.</param>
    /// <param name="day">The day to show first.</param>
    /// <param name="settings">The user settings (theme and calendar), saved when they change.</param>
    public MainWindow(AnniversaryBook book, DateOnly day, UserSettings settings)
    {
        InitializeComponent();

        _book = book;
        _settings = settings;
        _day = day;

        ContextFlyout = (MenuFlyout)Resources["CommandsMenu"]!;
        FindMenuItem.InputGesture = OperatingSystem.IsMacOS()
            ? new KeyGesture(Key.F, KeyModifiers.Meta)
            : new KeyGesture(Key.F, KeyModifiers.Control);
        FindNextMenuItem.InputGesture = OperatingSystem.IsMacOS()
            ? new KeyGesture(Key.G, KeyModifiers.Meta)
            : new KeyGesture(Key.F3);

        // The macOS menu bar: native menu items have no routed Click and no generated fields,
        // so they are found by their header and wired here.
        NativeMenu menuBar = NativeMenu.GetMenu(this)!;
        NativeItem(menuBar, "File", "Edit People…").Click += (_, _) => EditPeople();
        NativeItem(menuBar, "Go", "Yesterday").Click += (_, _) => ShowDay(_day.AddDays(-1));
        NativeItem(menuBar, "Go", "Tomorrow").Click += (_, _) => ShowDay(_day.AddDays(1));
        NativeItem(menuBar, "Go", "Today").Click += (_, _) => ShowToday();
        NativeItem(menuBar, "Go", "Change Date…").Click += (_, _) => ChangeDate();
        NativeItem(menuBar, "Find", "Find…").Click += (_, _) => Find();
        NativeItem(menuBar, "Find", "Find Next").Click += (_, _) => FindNext();
        _nativeCalendarMenu = NativeItem(menuBar, "View", "Nameday Calendar").Menu!;
        _nativeThemeItems = new Dictionary<AppTheme, NativeMenuItem>
        {
            [AppTheme.System] = NativeItem(menuBar, "View", "Appearance", "System"),
            [AppTheme.Light] = NativeItem(menuBar, "View", "Appearance", "Light"),
            [AppTheme.Dark] = NativeItem(menuBar, "View", "Appearance", "Dark")
        };
        foreach (KeyValuePair<AppTheme, NativeMenuItem> item in _nativeThemeItems)
        {
            AppTheme theme = item.Key;
            item.Value.Click += (_, _) => ChooseTheme(theme);
        }

        NextDaysCaption.Text = $"Next {book.Config.NumberOfDaysAhead} days";
        UpdateThemeChecks();
        UpdateCalendarMenus();
        Refresh();

        Opened += OnOpened;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        bool command = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        if (command && e.Key == Key.F)
        {
            Find();
            e.Handled = true;
        }
        else if (e.Key == Key.F3 || (command && e.Key == Key.G))
        {
            FindNext();
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        QuitButton.Focus();
        await ShowWarnings();
    }

    private void Refresh()
    {
        DayInfo info = _book.GetDay(_day);

        DateText.Text = _day.ToString("D", CultureInfo.CurrentCulture);
        CalendarText.Text = info.CalendarNames;
        NamedaysText.Text = string.Join(Environment.NewLine, info.Namedays);

        bool noPeople = _book.People.Count == 0;
        BirthdaysText.Classes.Set("hint", noPeople);
        BirthdaysText.Text = noPeople
            ? "No people yet. Use ⋯ ▸ Edit People… to add some."
            : string.Join(Environment.NewLine, info.Birthdays);

        var nextDays = new List<string>();
        if (info.NextDaysPeople.Count > 0)
        {
            nextDays.Add(string.Join(", ", info.NextDaysPeople));
        }
        if (info.NextDaysNames.Count > 0)
        {
            nextDays.Add(string.Join(", ", info.NextDaysNames));
        }
        NextDaysText.Text = string.Join(Environment.NewLine, nextDays);
    }

    private async Task ShowWarnings()
    {
        foreach (string warning in _book.Warnings.ToList())
        {
            string title = warning.StartsWith(AnniversaryBook.ConfigurationWarningPrefix, StringComparison.Ordinal)
                ? "Configuration Error"
                : "Data Error";
            await MessageDialog.Show(this, title, warning);
        }
    }

    private void ShowDay(DateOnly day)
    {
        _day = day;
        Refresh();
    }

    private void ShowToday()
    {
        ShowDay(DateOnly.FromDateTime(DateTime.Today));
    }

    private async void ChangeDate()
    {
        var dialog = new ChangeDateDialog { SelectedDate = _day };
        if (await dialog.ShowDialog<bool>(this))
        {
            ShowDay(dialog.SelectedDate);
        }
    }

    private async void Find()
    {
        var dialog = new FindDialog
        {
            FindName = _findName,
            FromCurrentDate = _findFromCurrentDate,
            WholeWordsOnly = _findWholeWords,
            SearchCalendarOnly = _findCalendarOnly
        };
        if (!await dialog.ShowDialog<bool>(this))
        {
            return;
        }

        _findName = dialog.FindName;
        _findFromCurrentDate = dialog.FromCurrentDate;
        _findWholeWords = dialog.WholeWordsOnly;
        _findCalendarOnly = dialog.SearchCalendarOnly;

        await PerformFind(_findFromCurrentDate ? _day : new DateOnly(_day.Year, 1, 1));
    }

    private async void FindNext()
    {
        if (string.IsNullOrWhiteSpace(_findName))
        {
            Find();
        }
        else
        {
            await PerformFind(_day.AddDays(1));
        }
    }

    private async Task PerformFind(DateOnly from)
    {
        if (string.IsNullOrWhiteSpace(_findName))
        {
            await MessageDialog.Show(this, "Find", "Please enter a name to search for.");
            return;
        }

        if (_book.Find(_findName, from, _findWholeWords, _findCalendarOnly) is DateOnly found)
        {
            ShowDay(found);
        }
        else
        {
            await MessageDialog.Show(this, "Find", $"'{_findName}' not found.");
        }
    }

    private async void EditPeople()
    {
        if (await new PeopleEditorWindow(_book).ShowDialog<bool>(this))
        {
            Refresh();
        }
    }

    private void ChooseTheme(AppTheme theme)
    {
        _settings.Theme = theme;
        App.ApplyTheme(theme);
        _settings.Save();
        UpdateThemeChecks();
    }

    private void UpdateThemeChecks()
    {
        ThemeSystemItem.IsChecked = _settings.Theme == AppTheme.System;
        ThemeLightItem.IsChecked = _settings.Theme == AppTheme.Light;
        ThemeDarkItem.IsChecked = _settings.Theme == AppTheme.Dark;
        foreach (KeyValuePair<AppTheme, NativeMenuItem> item in _nativeThemeItems)
        {
            item.Value.IsChecked = _settings.Theme == item.Key;
        }
    }

    private void UpdateCalendarMenus()
    {
        CalendarMenu.Items.Clear();
        _nativeCalendarMenu.Items.Clear();

        foreach (CalendarInfo calendar in _book.Calendars)
        {
            bool isActive = string.Equals(calendar.FileName, _book.CalendarFile, StringComparison.OrdinalIgnoreCase);
            string fileName = calendar.FileName;

            var item = new MenuItem
            {
                Header = calendar.DisplayName,
                ToggleType = MenuItemToggleType.Radio,
                GroupName = "Calendar",
                IsChecked = isActive
            };
            item.Click += (_, _) => ChooseCalendar(fileName);
            CalendarMenu.Items.Add(item);

            var nativeItem = new NativeMenuItem
            {
                Header = calendar.DisplayName,
                ToggleType = MenuItemToggleType.Radio,
                IsChecked = isActive
            };
            nativeItem.Click += (_, _) => ChooseCalendar(fileName);
            _nativeCalendarMenu.Items.Add(nativeItem);
        }
    }

    private async void ChooseCalendar(string fileName)
    {
        _book.UseCalendar(fileName);
        _settings.Calendar = fileName;
        _settings.Save();
        UpdateCalendarMenus();
        Refresh();
        await ShowWarnings();
    }

    private void OnYesterday(object? sender, RoutedEventArgs e)
    {
        ShowDay(_day.AddDays(-1));
    }

    private void OnTomorrow(object? sender, RoutedEventArgs e)
    {
        ShowDay(_day.AddDays(1));
    }

    private void OnToday(object? sender, RoutedEventArgs e)
    {
        ShowToday();
    }

    private void OnQuit(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnDatePressed(object? sender, PointerPressedEventArgs e)
    {
        ChangeDate();
    }

    private void OnChangeDate(object? sender, RoutedEventArgs e)
    {
        ChangeDate();
    }

    private void OnFind(object? sender, RoutedEventArgs e)
    {
        Find();
    }

    private void OnFindNext(object? sender, RoutedEventArgs e)
    {
        FindNext();
    }

    private void OnEditPeople(object? sender, RoutedEventArgs e)
    {
        EditPeople();
    }

    private void OnThemeSystem(object? sender, RoutedEventArgs e)
    {
        ChooseTheme(AppTheme.System);
    }

    private void OnThemeLight(object? sender, RoutedEventArgs e)
    {
        ChooseTheme(AppTheme.Light);
    }

    private void OnThemeDark(object? sender, RoutedEventArgs e)
    {
        ChooseTheme(AppTheme.Dark);
    }

    private async void OnAbout(object? sender, RoutedEventArgs e)
    {
        await new AboutDialog().ShowDialog(this);
    }

    private void OnCommandsMenuOpening(object? sender, EventArgs e)
    {
        // Picks up calendar files added while the app runs (spec 7.10).
        _book.RefreshCalendars();
        UpdateCalendarMenus();
    }

    private static NativeMenuItem NativeItem(NativeMenu menu, params string[] headers)
    {
        NativeMenuItem item = menu.Items.OfType<NativeMenuItem>().First(i => i.Header == headers[0]);
        for (int i = 1; i < headers.Length; i++)
        {
            item = item.Menu!.Items.OfType<NativeMenuItem>().First(m => m.Header == headers[i]);
        }
        return item;
    }
}
