using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ZeidLab.ToolBox.EasyPersistence.EFCore;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

// ReSharper disable once InconsistentNaming
public static class EFCoreDependencyInjection
{
    public static ModelBuilder RegisterFuzzySearchMethods(
        this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDbFunction(typeof(FuzzySearchExtensions).GetMethod(nameof(FuzzySearchExtensions.FuzzySearch),
                [typeof(string), typeof(string)]) ?? throw new InvalidOperationException())
            .HasName("FuzzySearch")
            .HasSchema("dbo");

        return modelBuilder;
    }

    /// <summary>
    /// Registers SQL CLR assembly for the specified DbContext.
    /// if the assembly is not already registered, it will be deployed to the database.
    /// if the assembly is already registered, it will ignore the request.
    /// If there is multiple DbContext, you only need to call this method once with one of the DbContexts.
    /// If there is multiple SqlServer instances, you need to call this for each instance only once.
    /// </summary>
    /// <param name="app"></param>
    /// <typeparam name="TDbContext"></typeparam>
    [SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
    [SuppressMessage("ConfigureAwait", "ConfigureAwaitEnforcer:ConfigureAwaitEnforcer")]
    public async static Task RegisterFuzzySearchAssemblyAsync<TDbContext>(this IApplicationBuilder app)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        await using var scope = app.ApplicationServices.CreateAsyncScope();

        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EasyPersistence");

        try
        {
            var applicationLifetime = services.GetRequiredService<IHostApplicationLifetime>();
            if (applicationLifetime.ApplicationStarted.IsCancellationRequested)
            {
                logger.LogWarning("Application has already stopped, skipping SQL CLR assembly registration.");
                return;
            }
            var dbContext = services.GetRequiredService<TDbContext>();
            var strategy = dbContext.Database.CreateExecutionStrategy();
            // ReSharper disable once HeapView.CanAvoidClosure
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(CancellationToken.None);
                await dbContext.InitializeSqlClrAsync(cancellationToken: applicationLifetime.ApplicationStarted);

                await transaction.CommitAsync(CancellationToken.None);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register SQL CLR assembly.");
        }
    }
}