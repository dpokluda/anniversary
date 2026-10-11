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
/// All the settings (spec 5.5), stored in <c>usersettings.json</c> in the user app-data folder. The app writes the file
/// when a choice changes in its menus; people may also edit it, with the app closed.
/// </summary>
public class UserSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
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
    /// Gets or sets the data file: a full path chosen with Open or Save As, or a file name found in the lookup folders
    /// (spec 5.3); <c>null</c> for <c>Anniversary.Data.json</c>.
    /// </summary>
    public string? DataFile { get; set; }

    /// <summary>
    /// Gets or sets the number of days shown in the "Next N days" section.
    /// </summary>
    public int NumberOfDaysAhead { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether birthdays and anniversaries show the number of years.
    /// </summary>
    public bool DisplayYearsOnBirthdays { get; set; } = true;

    /// <summary>
    /// Gets or sets the path of the file the settings are loaded from and saved to.
    /// </summary>
    [JsonIgnore]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets why the file couldn't be read, or <c>null</c>. While it's set, <see cref="Save"/> doesn't write the file.
    /// </summary>
    [JsonIgnore]
    public string? LoadError { get; private set; }

    /// <summary>
    /// Loads the settings from the given file. Returns default settings if the file is missing, and also if it can't be
    /// read, with <see cref="LoadError"/> set.
    /// </summary>
    /// <param name="filePath">The path of the settings file.</param>
    /// <returns>The loaded settings, with <see cref="FilePath"/> set.</returns>
    public static UserSettings Load(string filePath)
    {
        UserSettings? settings = null;
        string? error = null;
        try
        {
            if (File.Exists(filePath))
            {
                settings = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(filePath), JsonOptions);
            }
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            error = ex.Message;
        }

        settings ??= new UserSettings();
        settings.FilePath = filePath;
        settings.LoadError = error;
        return settings;
    }

    /// <summary>
    /// Saves the settings to <see cref="FilePath"/>, creating the folder if needed. Does nothing while
    /// <see cref="LoadError"/> is set; failures are ignored.
    /// </summary>
    public void Save()
    {
        if (LoadError != null)
        {
            // The file couldn't be read, perhaps because of a typo made by hand; writing would lose what's in it.
            return;
        }

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
