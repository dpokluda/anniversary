using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anniversary.Model;

/// <summary>
/// The application theme chosen by the user.
/// </summary>
public enum AppTheme
{
    /// <summary>
    /// Follow the operating system light/dark setting, including changes while the app runs.
    /// </summary>
    System,

    /// <summary>
    /// Always use the light theme.
    /// </summary>
    Light,

    /// <summary>
    /// Always use the dark theme.
    /// </summary>
    Dark
}

/// <summary>
/// User preferences that only the app writes, stored in <c>usersettings.json</c> in the user app-data folder.
/// </summary>
public class UserSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Gets or sets the chosen theme.
    /// </summary>
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>
    /// Gets or sets the file name of the chosen name-day calendar, or <c>null</c> if none was chosen yet.
    /// </summary>
    public string? Calendar { get; set; }

    /// <summary>
    /// Gets or sets the path of the file the settings are loaded from and saved to.
    /// </summary>
    [JsonIgnore]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Loads the settings from the given file. Returns default settings if the file is missing or invalid.
    /// </summary>
    /// <param name="filePath">The path of the settings file.</param>
    /// <returns>The loaded settings, with <see cref="FilePath"/> set.</returns>
    public static UserSettings Load(string filePath)
    {
        UserSettings? settings = null;
        try
        {
            if (File.Exists(filePath))
            {
                settings = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(filePath), JsonOptions);
            }
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // The file is app-written; if it's unusable, start over with defaults.
        }

        settings ??= new UserSettings();
        settings.FilePath = filePath;
        return settings;
    }

    /// <summary>
    /// Saves the settings to <see cref="FilePath"/>, creating the folder if needed. Failures are ignored.
    /// </summary>
    public void Save()
    {
        try
        {
            string? folder = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The choice still applies for the current session.
        }
    }
}
