using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ZeidLab.ToolBox.EasyPersistence.TestHelpers;

public class TestHostedServiceManager
{
    private readonly IEnumerable<IHostedService> _hostedServices;

    public TestHostedServiceManager(IServiceProvider serviceProvider)
    {
        _hostedServices = serviceProvider.GetServices<IHostedService>();
    }

    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var service in _hostedServices)
        {
            await service.StartAsync(cancellationToken);
        }
    }

    public async Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var service in _hostedServices)
        {
            await service.StopAsync(cancellationToken);
        }
    }
}