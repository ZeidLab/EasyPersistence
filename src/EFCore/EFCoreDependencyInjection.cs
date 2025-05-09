using ZeidLab.ToolBox.EasyPersistence.EFCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class EFCoreDependencyInjection
{
    public static IServiceCollection AddEfCoreSqlClrMethods(this IServiceCollection services)
    {
        services.AddHostedService<BackgroundEFCoreSqlClrInstallerService>();
        return services;
    }
}