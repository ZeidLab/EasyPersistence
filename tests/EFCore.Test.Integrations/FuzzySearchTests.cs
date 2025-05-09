using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;
using ZeidLab.ToolBox.EasyPersistence.TestHelpers;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations;

[SuppressMessage("ConfigureAwait", "ConfigureAwaitEnforcer:ConfigureAwaitEnforcer")]
[SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
public sealed class FuzzySearchTests: IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbGenerator _dbGenerator = TestDbGenerator.GenerateSqlServerOnly();

    public FuzzySearchTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<TestDbContext>(options =>
        {
            options.UseSqlServer(_dbGenerator.SqlServerConnectionString);
            options.EnableSensitiveDataLogging();
        });
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<ITestUnitOfWork, TestUnitOfWork>();
        services.AddEfCoreSqlClrMethods();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task FuzzySearch_ValidInput_ReturnsExpectedResults()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();

        // Arrange
        var searchTerm = "John";
        var expectedCount = 2;

        // Act
        var result = await usersRepository.FuzzySearchAsync(searchTerm, x => x.FirstName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCount, result.Count);
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