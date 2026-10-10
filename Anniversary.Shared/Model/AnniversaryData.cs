using System.Text.Json.Serialization;

namespace Anniversary.Model;

/// <summary>
/// Represents the people data file: a collection of people with their anniversary information.
/// </summary>
/// <remarks>
/// This was <c>Model.Anniversary</c> in v4; it was renamed so it doesn't clash with the <c>Anniversary</c> namespace.
/// </remarks>
public class AnniversaryData
{
    /// <summary>
    /// Gets or sets the collection of people.
    /// </summary>
    [JsonPropertyName("data")]
    public List<Person> People { get; set; } = new();
}
