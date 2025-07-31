// ReSharper disable once CheckNamespace
    namespace ZeidLab.ToolBox.EasyPersistence.EFCore;
    
    /// <summary>
    /// Defines an entity that supports domain events, enabling event-driven architecture patterns
    /// within the domain model.
    /// </summary>
    /// <remarks>
    /// Implement this interface on entities that need to raise domain events during their lifecycle.
    /// Domain events are typically collected during entity operations and dispatched after the entity
    /// is successfully persisted to the database.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// public class Order : Entity, IHaveDomainEvents
    /// {
    ///     public IList<IDomainEvent> DomainEvents { get; } = new List<IDomainEvent>();
    ///     
    ///     public void Place()
    ///     {
    ///         Status = OrderStatus.Placed;
    ///         DomainEvents.Add(new OrderPlacedEvent(this.Id));
    ///     }
    /// }
    /// 
    /// // After persistence in repository
    /// await dbContext.SaveChangesAsync();
    /// foreach (var domainEvent in order.DomainEvents)
    /// {
    ///     await eventDispatcher.DispatchAsync(domainEvent);
    /// }
    /// order.DomainEvents.Clear();
    /// ]]></code>
    /// </example>
    public interface IHaveDomainEvents
    {
        /// <summary>
        /// Gets the collection of domain events associated with this entity.
        /// </summary>
        /// <remarks>
        /// Use this collection to track domain events that should be published after persistence.
        /// Domain events represent meaningful business occurrences that other components
        /// might need to react to.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// // Adding events during business operations
        /// entity.DomainEvents.Add(new EntityCreatedEvent(entity.Id));
        /// 
        /// // Publishing events after persistence
        /// await repository.SaveAsync(entity);
        /// foreach (var domainEvent in entity.DomainEvents)
        /// {
        ///     await mediator.PublishAsync(domainEvent);
        /// }
        /// entity.DomainEvents.Clear();
        /// ]]></code>
        /// </example>
        IList<IDomainEvent> DomainEvents { get; }
    }