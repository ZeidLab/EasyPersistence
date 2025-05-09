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

    public static async Task InitializeSqlClrAsync<TContext>(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        const string assemblyName = "EFCoreSqlClr";
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, "EasyPersistence.EFCoreSqlClr.dll");

        if (!File.Exists(assemblyPath))
            throw new FileNotFoundException($"SQL CLR assembly not found at: {assemblyPath}");

        byte[] assemblyBytes;
        using (var fileStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            assemblyBytes = new byte[fileStream.Length];
            var bytesRead = await fileStream.ReadAsync(assemblyBytes, cancellationToken).ConfigureAwait(false);
            
            if (bytesRead != fileStream.Length)
                throw new IOException("Failed to read complete assembly file");
        }

        var assemblyHex = BitConverter.ToString(assemblyBytes).Replace("-", "");
        await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            IF NOT EXISTS (SELECT 1 FROM sys.assemblies WHERE name = {assemblyName})
            BEGIN
                DECLARE @assembly VARBINARY(MAX) = 0x{assemblyHex};
                CREATE ASSEMBLY [{assemblyName}]
                FROM @assembly
                WITH PERMISSION_SET = SAFE;
            END", cancellationToken).ConfigureAwait(false);
    }
}