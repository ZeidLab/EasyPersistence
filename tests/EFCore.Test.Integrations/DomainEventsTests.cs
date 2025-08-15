using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interceptors;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;
using ZeidLab.ToolBox.EasyPersistence.TestHelpers;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations;

public sealed class DomainEventsTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbGenerator _dbGenerator = TestDbGenerator.GenerateSqlServerOnly();

    public DomainEventsTests(ITestOutputHelper testOutput)
    {
        var services = new ServiceCollection();

        // Add logging services with Trace level for EventBuss
        services.AddLogging(builder =>
        {
            builder.AddFilter("ZeidLab.ToolBox.EventBuss", LogLevel.Trace)
                .AddXUnit(testOutput);
        });

        // Register ITestOutputHelper
        services.AddSingleton(testOutput);

        services.AddDbContext<TestDbContextWithEvents>((provider, options) =>
        {
            var eventPublishingInterceptor = provider.GetRequiredService<DomainEventPublishingInterceptor>();
            options.UseSqlServer(_dbGenerator.SqlServerConnectionString);
            options.EnableSensitiveDataLogging();
            options.AddInterceptors(eventPublishingInterceptor);
        });
        services.AddEventBuss(options =>
        {
            options.RegisterFromAssembly<DomainEventsTests>();
        });
        services.AddScoped<DomainEventPublishingInterceptor>();
        services.AddScoped<IAppLogsRepository, AppLogsRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<ITestUnitOfWorkBaseWithEvents, TestUnitOfWorkBaseWithEvents>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task EntityId_ShouldBeSetAndRetrievedCorrectly()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWorkBaseWithEvents>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContextWithEvents>();
        var hostedServiceManager = new TestHostedServiceManager(scope.ServiceProvider);

        // Start background services
        await hostedServiceManager.StartAllAsync();
        try
        {
            await dbContext.Database.EnsureCreatedAsync();

            // Arrange
            var user = User.Create("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));
            var profile = UserProfile.Create("123 Main St", "555-1234");
            user.WithProfile(profile);
  
            testUnitOfWork.Users.Add(user);
            await testUnitOfWork.SaveChangesAsync();
            

            // Wait for background processing to complete
            await Task.Delay(500);

            // Act
            var appLogs = await testUnitOfWork.AppLogs.GetAllAsync();

            // Assert
            appLogs.Should().NotBeEmpty();
            appLogs.Should().HaveCount(2);
        }
        finally
        {
            // Stop background services
            await hostedServiceManager.StopAllAsync();
        }
    }

    public async Task InitializeAsync()
    {
        await _dbGenerator.MakeSureIsRunningAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbGenerator.MakeSureIsStoppedAsync();
    }
}