using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ZeidLab.ToolBox.EasyPersistence.EFCore;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class EFCoreDependencyInjection
{
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