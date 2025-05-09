using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        const int maxRetries = 3;
        const int retryDelayMs = 1000;
        var currentTry = 0;

        while (currentTry < maxRetries)
        {
            try
            {
                currentTry++;
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

                const string assemblyName = "EFCoreSqlClr";
                var assemblyPath = Path.Combine(AppContext.BaseDirectory, "EasyPersistence.EFCoreSqlClr.dll");

                if (!File.Exists(assemblyPath))
                {
                    _logger.LogWarning("SQL CLR assembly file not found at '{AssemblyPath}'. Retrying in {Delay}ms...", 
                        assemblyPath, retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                _logger.LogInformation("Checking if SQL CLR assembly '{AssemblyName}' is registered...", assemblyName);

                // Check if the assembly is already registered
                var isRegistered = await dbContext.Database
                    .SqlQuery<int>($"SELECT COUNT(1) FROM sys.assemblies WHERE name = '{assemblyName}'")
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (isRegistered == 0)
                {
                    try
                    {
                        _logger.LogInformation("SQL CLR assembly '{AssemblyName}' is not registered. Registering now...", assemblyName);
                        
                        // Read file with FileShare.ReadWrite to allow other processes to access it
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

                        _logger.LogInformation("SQL CLR assembly '{AssemblyName}' registered successfully.", assemblyName);
                        return;
                    }
                    catch (IOException ex) when (currentTry < maxRetries)
                    {
                        _logger.LogWarning(ex, "Failed to access SQL CLR assembly file. Retrying in {Delay}ms...", retryDelayMs);
                        await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                }
                else
                {
                    _logger.LogInformation("SQL CLR assembly '{AssemblyName}' is already registered.", assemblyName);
                    return;
                }
            }
            catch (Exception ex)
            {
                if (currentTry >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to register SQL CLR assembly after {Retries} attempts.", maxRetries);
                    throw;
                }

                _logger.LogWarning(ex, "An error occurred while registering SQL CLR assembly. Retrying in {Delay}ms...", retryDelayMs);
                await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}