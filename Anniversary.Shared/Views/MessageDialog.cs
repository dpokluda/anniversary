using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Anniversary.Views;

/// <summary>
/// A minimal cross-platform replacement for <c>MessageBox.Show</c>.
/// </summary>
public static class MessageDialog
{
    /// <summary>
    /// Shows a modal message with an OK button.
    /// </summary>
    /// <param name="owner">The window that owns the message.</param>
    /// <param name="title">The window title.</param>
    /// <param name="message">The message text.</param>
    /// <returns>A task that completes when the message is closed.</returns>
    public static Task Show(Window owner, string title, string message)
    {
        var ok = new Button
        {
            Content = "OK",
            IsDefault = true,
            IsCancel = true,
            MinWidth = 75,
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        ok.Classes.Add("accent");

        var dialog = new Window
        {
            Title = title,
            Icon = owner.Icon,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(16),
                Spacing = 12,
                MinWidth = 240,
                MaxWidth = 400,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                    ok
                }
            }
        };
        ok.Click += (_, _) => dialog.Close();

        return dialog.ShowDialog(owner);
    }
}

