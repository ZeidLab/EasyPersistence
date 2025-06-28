// ReSharper disable once CheckNamespace

namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Represents an entity with an associated fuzzy search score and per-property scores.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being scored.</typeparam>
/// <example>
/// <code><![CDATA[
/// var record = new ScoredRecord<Person>
/// {
///     Entity = person,
///     Score = 0.92,
///     Scores = new[] { new PropertyScore { Name = "FirstName", Score = 0.95 } }
/// };
/// Console.WriteLine(record.Score);
/// ]]></code>
/// </example>
public sealed class ScoredRecord<TEntity>
{
    /// <summary>
    /// Gets or sets the entity being scored.
    /// </summary>
    public required TEntity Entity { get; set; }

    /// <summary>
    /// Gets or sets the overall score for the entity.
    /// </summary>
    public required double Score { get; set; }

    /// <summary>
    /// Gets or sets the collection of per-property scores.
    /// </summary>
    public IEnumerable<PropertyScore> Scores { get; set; } = Enumerable.Empty<PropertyScore>();
}