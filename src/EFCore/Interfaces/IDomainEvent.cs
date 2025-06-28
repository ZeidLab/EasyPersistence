// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Marker interface for domain events in the domain-driven design pattern.
/// </summary>
/// <remarks>
/// Implement this interface to represent a domain event that can be tracked and published by entities.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// public sealed class EntityCreatedEvent : IDomainEvent
/// {
///     // Event-specific properties and logic
/// }
/// ]]></code>
/// </example>
public interface IDomainEvent;
