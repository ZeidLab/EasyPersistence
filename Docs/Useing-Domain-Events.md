**EasyPersistence.EFCore** is a high-performance, modular extension for Entity Framework Core, providing advanced repository and unit of work patterns, fuzzy search, n-gram utilities, and SQL CLR integration. Designed for scalable, maintainable, and testable .NET data layers, it follows modern C# and Microsoft coding standards, with comprehensive XML documentation and NuGet-ready modularity.

---

## ✨ Features

- **Repository & Unit of Work Patterns:** Clean abstractions for data access
- **Fuzzy Search & N-Gram Extensions:** Advanced search capabilities for EF Core
- **SQL CLR Integration:** Efficient search via SQL CLR functions
- **High Performance:** Optimized for critical paths, minimal overhead

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class DomainEventPublishingInterceptor : SaveChangesInterceptor
{
    private readonly IEventBussService _eventBuss;

    public DomainEventPublishingInterceptor(IEventBussService eventBussService)
    {
        _eventBuss = eventBussService;
    }

    public override async ValueTask<InterceptionResult<int>> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return result;

        var entities = context.ChangeTracker.Entries<IHaveDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                await _eventBuss.PublishAsync(domainEvent, cancellationToken);
            }
            entity.DomainEvents.Clear();
        }

        return result;
    }
}
```


```csharp
services.AddScoped<DomainEventPublishingInterceptor>();

services.AddDbContext<MyDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<DomainEventPublishingInterceptor>();
    options.AddInterceptors(interceptor);
    // Other options...
});
```
Or, if configuring directly in your DbContext:

```csharp
services.AddScoped<DomainEventPublishingInterceptor>();

services.AddDbContext<MyDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<DomainEventPublishingInterceptor>();
    options.AddInterceptors(interceptor);
    // Other options...
});
```

