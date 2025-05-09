using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Extensions;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

internal sealed class BackgroundEFCoreSqlClrInstallerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundEFCoreSqlClrInstallerService> _logger;

    public BackgroundEFCoreSqlClrInstallerService(
        IServiceProvider serviceProvider,
        ILogger<BackgroundEFCoreSqlClrInstallerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            const string assemblyName = "EFCoreSqlClr";
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, "EasyPersistence.EFCoreSqlClr.dll");

            _logger.LogInformation("Checking if SQL CLR assembly '{AssemblyName}' is registered...", assemblyName);

            // Check if the assembly is already registered
            var isRegistered = await dbContext.Database.ExecuteSqlRawAsync(
                $"SELECT 1 FROM sys.assemblies WHERE name = {assemblyName}", cancellationToken).ConfigureAwait(false) > 0;

            if (!isRegistered)
            {
                _logger.LogInformation("SQL CLR assembly '{AssemblyName}' is not registered. Registering now...", assemblyName);
                dbContext.DeploySqlClrAssembly(assemblyPath, assemblyName);
                _logger.LogInformation("SQL CLR assembly '{AssemblyName}' registered successfully.", assemblyName);
            }
            else
            {
                _logger.LogInformation("SQL CLR assembly '{AssemblyName}' is already registered.", assemblyName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while registering the SQL CLR assembly.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}