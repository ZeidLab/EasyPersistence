using Microsoft.EntityFrameworkCore;
using ZeidLab.ToolBox.EasyPersistence.EFCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

// ReSharper disable once InconsistentNaming
public static class EFCoreDependencyInjection
{
    // ReSharper disable once InconsistentNaming
    public static IServiceCollection AddEFCoreSqlClrMethods(
        this IServiceCollection services,
        bool useBackgroundService = true)
    {
        if (useBackgroundService)
        {
            services.AddHostedService<BackgroundEFCoreSqlClrInstallerService>();
        }

        return services;
    }

    public static ModelBuilder RegisterSqlClrMethods(
        this ModelBuilder modelBuilder )
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDbFunction(typeof(HelperMethods).GetMethod(nameof(HelperMethods.FuzzySearch),
                [typeof(string), typeof(string)]) ?? throw new InvalidOperationException())
            .HasName("FuzzySearch")
            .HasSchema("dbo");

        return modelBuilder;
    }
}