using Avalonia;

namespace Anniversary;

/// <summary>
/// The desktop (Windows, macOS, Linux) entry point.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The exit code.</returns>
    [STAThread]
    public static int Main(string[] args)
    {
        if (StartAtLogin.IsSupported)
        {
            App.StartAtLogin = (StartAtLogin.IsOn, StartAtLogin.Set);
        }
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Configures the Avalonia application; also used by the XAML previewer.
    /// </summary>
    /// <returns>The application builder.</returns>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
