using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

using ZeidLab.ToolBox.EasyPersistence.EFCore;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

// ReSharper disable once InconsistentNaming
public static class EFCoreDependencyInjection
{
    // ReSharper disable once InconsistentNaming
    public static IServiceCollection AddEFCoreSqlClrMethods(
        this IServiceCollection services)
    {


        return services;
    }

    public static ModelBuilder RegisterSqlClrMethods(
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
    public async static Task RegisterSqlClrAssemblyAsync<TDbContext>(this WebApplication app)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        await using var scope = app.Services.CreateAsyncScope();

        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EasyPersistence");

        try
        {
            var dbContext = services.GetRequiredService<TDbContext>();
            var strategy = dbContext.Database.CreateExecutionStrategy();
            // ReSharper disable once HeapView.CanAvoidClosure
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(CancellationToken.None);
                await dbContext.InitializeSqlClrAsync(cancellationToken: app.Lifetime.ApplicationStarted);

                await transaction.CommitAsync(CancellationToken.None);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register SQL CLR assembly.");
        }
    }
}