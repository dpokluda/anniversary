using System.Globalization;
using System.Text.Json;
using Anniversary.Model;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Anniversary.Views;

/// <summary>
/// The main window: shows the day, the namedays, birthdays and anniversaries, and the next N days.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly FilePickerFileType DataFileType = new("Anniversary data")
    {
        Patterns = ["*.json"],
        AppleUniformTypeIdentifiers = ["public.json"],
        MimeTypes = ["application/json"]
    };

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
        OpenDataFileItem.InputGesture = OperatingSystem.IsMacOS()
            ? new KeyGesture(Key.O, KeyModifiers.Meta)
            : new KeyGesture(Key.O, KeyModifiers.Control);
        SaveDataFileAsItem.InputGesture = OperatingSystem.IsMacOS()
            ? new KeyGesture(Key.S, KeyModifiers.Meta | KeyModifiers.Shift)
            : new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Shift);

        // The macOS menu bar: native menu items have no routed Click and no generated fields,
        // so they are found by their header and wired here.
        NativeMenu menuBar = NativeMenu.GetMenu(this)!;
        NativeItem(menuBar, "File", "Edit People…").Click += (_, _) => EditPeople();
        NativeItem(menuBar, "File", "Open Data File…").Click += (_, _) => OpenDataFile();
        NativeItem(menuBar, "File", "Save Data File As…").Click += (_, _) => SaveDataFileAs();
        NativeItem(menuBar, "File", "Show Data File in Finder").Click += (_, _) => ShowDataFile();
        NativeItem(menuBar, "Go", "Yesterday").Click += (_, _) => ShowPreviousDay();
        NativeItem(menuBar, "Go", "Tomorrow").Click += (_, _) => ShowNextDay();
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
        Activated += OnActivated;
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
        else if (command && e.Key == Key.O)
        {
            OpenDataFile();
            e.Handled = true;
        }
        else if (command && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && e.Key == Key.S)
        {
            SaveDataFileAs();
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        QuitButton.Focus();
        await ShowWarnings();
    }

    private async void OnActivated(object? sender, EventArgs e)
    {
        // Picks up changes made elsewhere, e.g. on another computer through a cloud drive (spec 7.11).
        await ReloadPeopleIfChanged();
    }

    private async Task ReloadPeopleIfChanged()
    {
        if (_book.ReloadPeopleIfChanged())
        {
            Refresh();
            await ShowWarnings();
        }
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

    // Yesterday and Tomorrow do nothing at the ends of DateOnly's range: 1 January 0001 and 31 December 9999.
    private void ShowPreviousDay()
    {
        if (_day > DateOnly.MinValue)
        {
            ShowDay(_day.AddDays(-1));
        }
    }

    private void ShowNextDay()
    {
        if (_day < DateOnly.MaxValue)
        {
            ShowDay(_day.AddDays(1));
        }
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
            // No day follows 31 December 9999, so there is nothing to search.
            await PerformFind(_day < DateOnly.MaxValue ? _day.AddDays(1) : null);
        }
    }

    private async Task PerformFind(DateOnly? from)
    {
        if (string.IsNullOrWhiteSpace(_findName))
        {
            await MessageDialog.Show(this, "Find", "Please enter a name to search for.");
            return;
        }

        if (from is DateOnly start && _book.Find(_findName, start, _findWholeWords, _findCalendarOnly) is DateOnly found)
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
        await ReloadPeopleIfChanged();
        if (await new PeopleEditorWindow(_book).ShowDialog<bool>(this))
        {
            Refresh();
        }
    }

    private async void OpenDataFile()
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Data File",
            FileTypeFilter = [DataFileType, FilePickerFileTypes.All]
        });
        string? path = files.Count > 0 ? files[0].TryGetLocalPath() : null;
        if (path == null)
        {
            return;
        }

        try
        {
            _book.OpenDataFile(path);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            await MessageDialog.Show(this, "Open Data File", $"Could not open the data file: {ex.Message}");
            return;
        }
        UseDataFile(path);
    }

    private async void SaveDataFileAs()
    {
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Data File As",
            SuggestedFileName = Path.GetFileName(_book.SavePathFor()),
            DefaultExtension = "json",
            FileTypeChoices = [DataFileType],
            ShowOverwritePrompt = true
        });
        string? path = file?.TryGetLocalPath();
        if (path == null)
        {
            return;
        }

        try
        {
            _book.SavePeople(_book.People, path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await MessageDialog.Show(this, "Save Data File As", $"Could not save the data file: {ex.Message}");
            return;
        }
        UseDataFile(path);
    }

    private void UseDataFile(string path)
    {
        _settings.DataFile = path;
        _settings.Save();
        Refresh();
    }

    private async void ShowDataFile()
    {
        string? folder = Path.GetDirectoryName(_book.SavePathFor());
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            await MessageDialog.Show(this, "Data File", $"The data file's folder doesn't exist yet:{Environment.NewLine}{folder}");
            return;
        }
        await Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folder));
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
        ShowPreviousDay();
    }

    private void OnTomorrow(object? sender, RoutedEventArgs e)
    {
        ShowNextDay();
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

    private void OnOpenDataFile(object? sender, RoutedEventArgs e)
    {
        OpenDataFile();
    }

    private void OnSaveDataFileAs(object? sender, RoutedEventArgs e)
    {
        SaveDataFileAs();
    }

    private void OnShowDataFile(object? sender, RoutedEventArgs e)
    {
        ShowDataFile();
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
