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

/// <summary>
/// Provides extension methods for integrating EasyPersistence EFCore features into dependency injection and model configuration.
/// </summary>
/// <remarks>
/// Use these methods to register FuzzySearch SQL CLR and EF Core model extensions in your application startup.
/// </remarks>
// ReSharper disable once InconsistentNaming
public static class EFCoreDependencyInjection
{
    /// <summary>
    /// Registers the FuzzySearch SQL function with the EF Core model builder.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance to extend.</param>
    /// <returns>The same <see cref="ModelBuilder"/> instance for chaining.</returns>
    /// <example>
    /// <code><![CDATA[
    /// modelBuilder.RegisterFuzzySearchMethods();
    /// ]]></code>
    /// </example>
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

    /// <inheritdoc cref="RegisterFuzzySearchAssemblyAsync{TDbContext}(IHost)"/>
    /// <remarks>
    /// This method should be called during application startup to ensure the SQL CLR assembly is registered.
    /// </remarks>
    /// <param name="app"></param>
    /// <typeparam name="TDbContext"></typeparam>
    [SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
    [SuppressMessage("ConfigureAwait", "ConfigureAwaitEnforcer:ConfigureAwaitEnforcer")]
    public async static Task RegisterFuzzySearchAssemblyAsync<TDbContext>(this IHost app)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        await using var scope = app.Services.CreateAsyncScope();

        var services = scope.ServiceProvider.CreateScope().ServiceProvider;
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
            await dbContext.InitializeSqlClrAsync(cancellationToken: applicationLifetime.ApplicationStarted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register SQL CLR assembly.");
        }
    }
}