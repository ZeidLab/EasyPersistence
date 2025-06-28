using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Serves as a base class for entities with a strongly-typed identifier and value-based equality.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
/// Implements equality and hash code logic based on the <see cref="Id"/> property.
/// </remarks>
[SuppressMessage("SonarAnalyzer.CSharp", "S4035:Classes implementing 'IEquatable<T>' should be sealed",
    Justification = "Abstract base class with correct equality implementation")]
public abstract class EntityBase<TId> : IEquatable<EntityBase<TId>>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityBase{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <example>
    /// <code><![CDATA[
    /// var entity = new MyEntity(123);
    /// var id = entity.Id;
    /// ]]></code>
    /// </example>
    // ReSharper disable once NullableWarningSuppressionIsUsed
    protected EntityBase(TId id = default!)
    {
       Id = id;
    }

    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    [Key]
    public TId Id { get; private set; }

    /// <summary>
    /// Determines whether two entities are equal by comparing their identifiers.
    /// </summary>
    /// <param name="first">The first entity to compare.</param>
    /// <param name="second">The second entity to compare.</param>
    /// <returns><c>true</c> if both entities are not null and have the same identifier; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var a = new MyEntity(1);
    /// var b = new MyEntity(1);
    /// bool areEqual = a == b; // true
    /// ]]></code>
    /// </example>
    public static bool operator ==(EntityBase<TId>? first, EntityBase<TId>? second) =>
        first is not null && second is not null && first.Equals(second);

    /// <summary>
    /// Determines whether two entities are not equal by comparing their identifiers.
    /// </summary>
    /// <param name="first">The first entity to compare.</param>
    /// <param name="second">The second entity to compare.</param>
    /// <returns><c>true</c> if the entities are not equal; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var a = new MyEntity(1);
    /// var b = new MyEntity(2);
    /// bool notEqual = a != b; // true
    /// ]]></code>
    /// </example>
    public static bool operator !=(EntityBase<TId>? first, EntityBase<TId>? second) =>
        !(first == second);

    /// <inheritdoc/>
    public bool Equals(EntityBase<TId>? other)
        => ReferenceEquals(this, other) || (other is not null
                                            && EqualityComparer<TId>.Default.Equals(other.Id, Id));

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is EntityBase<TId> entity
           && EqualityComparer<TId>.Default.Equals(entity.Id, Id);

    /// <inheritdoc/>
    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id) * 41;

    /// <summary>
    /// Determines whether the entity is transient (not persisted) by checking if <see cref="Id"/> is the default value.
    /// </summary>
    /// <returns><c>true</c> if the entity is transient; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var entity = new MyEntity();
    /// bool isTransient = entity.IsTransient();
    /// ]]></code>
    /// </example>
    public virtual bool IsTransient() => EqualityComparer<TId>.Default.Equals(Id, default!);
}