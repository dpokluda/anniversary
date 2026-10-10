using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Anniversary.Views;

/// <summary>
/// Dialog displaying information about the application.
/// </summary>
public partial class AboutDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the dialog.
    /// </summary>
    public AboutDialog()
    {
        InitializeComponent();

        Version? version = typeof(AboutDialog).Assembly.GetName().Version;
        VersionText.Text = version != null ? $"Version {version.Major}.{version.Minor}" : "Version 5.0";
        CopyrightText.Text = $"Copyright © 1997-{DateTime.Now.Year} David Pokluda · MIT License";
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
