using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;
using ZeidLab.ToolBox.EasyPersistence.TestHelpers;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations;

[SuppressMessage("ConfigureAwait", "ConfigureAwaitEnforcer:ConfigureAwaitEnforcer")]
[SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
public sealed class FuzzySearchTests : IAsyncLifetime
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
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task FuzzySearch_ValidInput_ReturnsExpectedResults()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.InitializeSqlClrAsync();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        // var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();

        // Arrange
        var searchTerm = "John";
        var users = new List<User>
        {
            User.Create("John", "Smith", "john.smith@example.com", new DateTime(1990, 1, 1)),
            User.Create("Jonathan", "Doe", "jonathan.doe@example.com", new DateTime(1991, 2, 2)),
            User.Create("Jane", "Smith", "jane.smith@example.com", new DateTime(1992, 3, 15))
        };
        unitOfWork.Users.AddRange(users);
        await unitOfWork.SaveChangesAsync();

        // Act
        var result = await unitOfWork.Users
            .FuzzySearchAsync(searchTerm,
                x => x.FirstName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count); // Should return all records with scores

        // Convert to list for easier assertions
        var scoredUsers = result.OrderByDescending(x => x.Score).ToList();

        Assert.Equal("John", scoredUsers[0].Entity.FirstName);
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