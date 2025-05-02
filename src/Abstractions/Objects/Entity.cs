using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.Abstractions;

[SuppressMessage("SonarAnalyzer.CSharp", "S4035:Classes implementing 'IEquatable<T>' should be sealed",
    Justification = "Abstract base class with correct equality implementation")]
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    [Key]
    public TId Id { get; private set; } =  default!;

    public static bool operator ==(Entity<TId>? first, Entity<TId>? second) =>
        first is not null && second is not null && first.Equals(second);

    public static bool operator !=(Entity<TId>? first, Entity<TId>? second) =>
        !(first == second);

    public bool Equals(Entity<TId>? other)
        => ReferenceEquals(this, other) || (other is not null
                                            && EqualityComparer<TId>.Default.Equals(other.Id, Id));

    public override bool Equals(object? obj)
        => obj is Entity<TId> entity
           && EqualityComparer<TId>.Default.Equals(entity.Id, Id);

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id) * 41;

    public virtual bool IsTransient() => EqualityComparer<TId>.Default.Equals(Id, default!);
}