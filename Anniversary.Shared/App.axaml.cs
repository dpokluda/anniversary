using Anniversary.Model;
using Anniversary.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace Anniversary;

/// <summary>
/// The Avalonia application: loads the data and the user settings and opens the main window.
/// </summary>
public partial class App : Application
{
    private const string AppFolderName = "Anniversary";
    private const string UserSettingsFileName = "usersettings.json";

    /// <summary>
    /// Gets the user app-data folder: <c>%APPDATA%\Anniversary</c>, <c>~/Library/Application Support/Anniversary</c>
    /// or <c>~/.config/Anniversary</c>.
    /// </summary>
    public static string AppDataFolder
    {
        get
        {
            Environment.SpecialFolder folder = OperatingSystem.IsMacOS()
                ? Environment.SpecialFolder.LocalApplicationData
                : Environment.SpecialFolder.ApplicationData;
            return Path.Combine(Environment.GetFolderPath(folder), AppFolderName);
        }
    }

    /// <summary>
    /// Gets or sets Start at Login (spec 7.12): <c>IsOn</c> tells whether this copy starts when the user logs in, and
    /// <c>Set</c> turns that on or off, throwing <see cref="InvalidOperationException"/> with the reason when it can't.
    /// The desktop project sets it where the OS supports it; <see langword="null"/> hides the menu items.
    /// </summary>
    public static (Func<bool> IsOn, Action<bool> Set)? StartAtLogin { get; set; }

    /// <summary>
    /// Applies a theme to the whole application.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public static void ApplyTheme(AppTheme theme)
    {
        if (Current != null)
        {
            Current.RequestedThemeVariant = theme switch
            {
                AppTheme.Light => ThemeVariant.Light,
                AppTheme.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        }
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // The platform's standard UI font size: Segoe UI 9 pt on Windows (as in v4), 13 pt system font on macOS
        // and the usual desktop font size on Linux. Names and the date are slightly larger, as in v4.
        double fontSize = OperatingSystem.IsWindows() ? 12 : 13;
        Resources["AppFontSize"] = fontSize;
        Resources["AppNamesFontSize"] = fontSize + 1;
        Resources["AppDateFontSize"] = fontSize + 4;
        Resources["AppCaptionFontSize"] = fontSize;
        Resources["AppStrongFontWeight"] = FontWeight.SemiBold;
        Resources["AppLineHeight"] = double.NaN;
        Resources["AppNamesLineHeight"] = double.NaN;
        Resources["AppDateLineHeight"] = double.NaN;
        Resources["AppCaptionLineHeight"] = double.NaN;
        Resources["AppWindowMargin"] = new Thickness(8);
        Resources["AppWindowHeight"] = 468.0;
        Resources["CalendarFontSize"] = fontSize;
        Resources["CalendarDayButtonFontSize"] = fontSize;

        if (OperatingSystem.IsMacOS())
        {
            // Skia's default macOS font is Helvetica. The system font (SF) is a variable font and Avalonia takes glyph
            // advances from its regular instance, so bold SF text is drawn cramped. Helvetica Neue is static and
            // renders correctly. It has no semibold (which falls back to bold), so "strong" text uses Medium, and
            // section captions are a size smaller, like macOS group box titles.
            Resources["ContentControlThemeFontFamily"] = new FontFamily("Helvetica Neue");
            Resources["AppCaptionFontSize"] = fontSize - 1;
            Resources["AppStrongFontWeight"] = FontWeight.Medium;

            // Helvetica Neue's lines are tight (about 1.17 em, so accents on capitals nearly touch the line above).
            // Text gets Segoe UI's 4/3 line height instead; Avalonia centres the glyphs in the taller line.
            const double LineHeightFactor = 4.0 / 3.0;
            Resources["AppLineHeight"] = Math.Round(fontSize * LineHeightFactor);
            Resources["AppNamesLineHeight"] = Math.Round((fontSize + 1) * LineHeightFactor);
            Resources["AppDateLineHeight"] = Math.Round((fontSize + 4) * LineHeightFactor);
            Resources["AppCaptionLineHeight"] = Math.Round((fontSize - 1) * LineHeightFactor);

            // macOS windows have large rounded corners, which crowd controls placed 8 px from the edge. Native
            // controls are also rounder, and the taller lines get a taller window so "Next N days" keeps its room.
            Resources["AppWindowMargin"] = new Thickness(12);
            Resources["ControlCornerRadius"] = new CornerRadius(6);
            Resources["AppWindowHeight"] = 492.0;
        }

        // Values set inside the calendar's template outrank styles, so its fixed sizes (294 x 290 minimum,
        // 40 px header, 38 px day-name row) are shrunk when the template is applied.
        TemplatedControl.TemplateAppliedEvent.AddClassHandler<CalendarItem>(OnCalendarTemplateApplied);

        // The submenu arrow is sized for touch (8 x 16) inside the menu item's template.
        TemplatedControl.TemplateAppliedEvent.AddClassHandler<MenuItem>(OnMenuItemTemplateApplied);
    }

    private static void OnMenuItemTemplateApplied(MenuItem item, TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find<Avalonia.Controls.Shapes.Path>("PART_ChevronPath") is Avalonia.Controls.Shapes.Path chevron)
        {
            chevron.Width = 5;
            chevron.Height = 9;
        }
    }

    private static void OnCalendarTemplateApplied(CalendarItem calendar, TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find<Grid>("PART_MonthView") is Grid monthView && monthView.Parent is Grid outer)
        {
            outer.MinWidth = 0;
            outer.RowDefinitions[0].Height = new GridLength(30);
            monthView.MinHeight = 0;
            monthView.RowDefinitions[0].Height = new GridLength(24);
        }
        if (e.NameScope.Find<Grid>("PART_YearView") is Grid yearView)
        {
            yearView.MinHeight = 0;
        }
        if (e.NameScope.Find<Border>("BackgroundLayer") is Border backgroundLayer)
        {
            backgroundLayer.Margin = new Thickness(0, 24, 0, 0);
        }
        if (e.NameScope.Find<Button>("PART_HeaderButton") is Button header)
        {
            header.Padding = new Thickness(8, 0, 0, 0);
        }
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string appData = AppDataFolder;
            UserSettings settings = UserSettings.Load(Path.Combine(appData, UserSettingsFileName));
            if (!File.Exists(settings.FilePath))
            {
                // Every setting is then in the file, ready to edit (spec 5.5).
                settings.Save();
            }
            ApplyTheme(settings.Theme);

            AnniversaryBook book = AnniversaryBook.Load(settings, [AppContext.BaseDirectory, appData]);
            desktop.MainWindow = new MainWindow(book, DateOnly.FromDateTime(DateTime.Today), settings);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void OnAbout(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: Window owner })
        {
            await new AboutDialog().ShowDialog(owner);
        }
    }
}
