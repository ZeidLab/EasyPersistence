using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

using ZeidLab.ToolBox.EventBuss;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interceptors;

public sealed class DomainEventPublishingInterceptor : SaveChangesInterceptor
{
    private readonly IEventBussService _eventBuss;
    private readonly ILogger<DomainEventPublishingInterceptor> _logger;
    
    public DomainEventPublishingInterceptor(
        IEventBussService eventBussService,
        ILogger<DomainEventPublishingInterceptor> logger)
    {
        _eventBuss = eventBussService ?? throw new ArgumentNullException(nameof(eventBussService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // Handle synchronous SaveChanges operations
    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        // Call the async version and wait to ensure consistent behavior
        PublishDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return result;
    }
    
    // Handle asynchronous SaveChanges operations
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishDomainEvents(eventData.Context, cancellationToken);
        return result;
    }
    
    private async Task PublishDomainEvents(DbContext context, CancellationToken cancellationToken = default)
    {
        if (context is null) return;
    
        var entitiesWithEvents = context.ChangeTracker
            .Entries<IHaveDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();
    
        if (!entitiesWithEvents.Any()) return;
    
        var events = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            //.Cast<IAppEvent>()
            .ToList();
    
        _logger.LogInformation("Publishing {EventCount} domain events from {EntityCount} entities", 
            events.Count, entitiesWithEvents.Count);
    
        try
        {
            foreach (var domainEvent in events)
            {
                try
                {
                    // very bad Idea
                    // Use reflection to call the generic Publish method with the actual event type
                    var eventType = domainEvent.GetType();
                    var method = typeof(IEventBussService).GetMethod("Publish")?.MakeGenericMethod(eventType);
                    method?.Invoke(_eventBuss, [domainEvent]);

                    //_eventBuss.Publish<>(domainEvent);
                    _logger.LogDebug("Published domain event {EventType}", domainEvent.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing domain event {EventType}", domainEvent.GetType().Name);
                    throw; // Rethrow to maintain awareness of the failure
                }
            }
    
            // Clear events only after successful publishing
            foreach (var entity in entitiesWithEvents)
            {
                entity.DomainEvents.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish domain events");
            throw; // Propagate the error
        }
    }
}