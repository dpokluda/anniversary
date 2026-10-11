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
        Button ok = CreateButton("OK", isDefault: true);
        Window dialog = CreateWindow(owner, title, message, ok);
        ok.Click += (_, _) => dialog.Close();

        return dialog.ShowDialog(owner);
    }

    /// <summary>
    /// Asks a question with a confirm button and a Cancel button; Cancel is the default (spec 7.6).
    /// </summary>
    /// <param name="owner">The window that owns the question.</param>
    /// <param name="title">The window title.</param>
    /// <param name="message">The question text.</param>
    /// <param name="confirmText">The text of the confirm button, e.g. "Overwrite".</param>
    /// <returns><c>true</c> if the confirm button was clicked.</returns>
    public static Task<bool> Confirm(Window owner, string title, string message, string confirmText)
    {
        Button confirm = CreateButton(confirmText, isDefault: false);
        Button cancel = CreateButton("Cancel", isDefault: true);
        Window dialog = CreateWindow(owner, title, message, confirm, cancel);
        confirm.Click += (_, _) => dialog.Close(true);
        cancel.Click += (_, _) => dialog.Close(false);

        return dialog.ShowDialog<bool>(owner);
    }

    private static Button CreateButton(string text, bool isDefault)
    {
        var button = new Button
        {
            Content = text,
            IsDefault = isDefault,
            IsCancel = isDefault,
            MinWidth = 75,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        if (isDefault)
        {
            button.Classes.Add("accent");
        }
        return button;
    }

    private static Window CreateWindow(Window owner, string title, string message, params Button[] buttons)
    {
        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        buttonRow.Children.AddRange(buttons);

        return new Window
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
                    buttonRow
                }
            }
        };
    }
}
