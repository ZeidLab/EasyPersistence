// ReSharper disable once CheckNamespace

namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Represents the score of a specific property in a fuzzy search or comparison operation.
/// </summary>
/// <example>
/// <code><![CDATA[
/// var score = new PropertyScore { Name = "FirstName", Score = 0.85 };
/// Console.WriteLine(score.Name);
/// ]]></code>
/// </example>
public sealed class PropertyScore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyScore"/> class.
    /// </summary>
    public PropertyScore()
    {
        Name = string.Empty;
    }

    /// <summary>
    /// Gets or sets the name of the property being scored.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the score for the property.
    /// </summary>
    public double Score { get; set; }
}