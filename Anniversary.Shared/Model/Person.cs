namespace Anniversary.Model;

/// <summary>
/// Represents a person with their name, birthday, nameday, and anniversaries.
/// </summary>
public class Person
{
    /// <summary>
    /// Gets or sets the person's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the person's birthdate (optional).
    /// </summary>
    public DateOnly? Birthdate { get; set; }

    /// <summary>
    /// Gets or sets the person's nameday in MM-dd format (optional).
    /// </summary>
    public string? Namedate { get; set; }

    /// <summary>
    /// Gets or sets the anniversaries, where the key is the anniversary date and the value its description (optional).
    /// </summary>
    public Dictionary<DateOnly, string>? Anniversaries { get; set; }
}
