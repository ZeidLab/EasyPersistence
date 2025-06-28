// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Marker interface to indicate an entity is an aggregate root in the domain model.
/// Used to enforce aggregate boundaries and repository constraints.
/// </summary>
/// <remarks>
/// Aggregate roots are the only entities that repositories should directly manage.
/// </remarks>
public interface IAggregateRoot;