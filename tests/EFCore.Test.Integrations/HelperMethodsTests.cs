using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeidLab.ToolBox.EasyPersistence.Abstractions;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Helpers;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;
using ZeidLab.ToolBox.EasyPersistence.TestHelpers;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations;

[SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
[SuppressMessage("ConfigureAwait", "ConfigureAwaitEnforcer:ConfigureAwaitEnforcer")]
public sealed class HelperMethodsTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbGenerator _dbGenerator = TestDbGenerator.GenerateSqlServerOnly();
    //TODO: WriteDown test methods to see the generated expression and SQL queries
    public HelperMethodsTests()
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
    public async Task GetPagedResultsAsync_EmptyData_ShouldReturnEmptyPage()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Act
        var query = dbContext.Users.AsQueryable();
        var result = await query.GetPagedResultsAsync(0, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task GetPagedResultsAsync_SinglePage_ShouldReturnAllItems()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var users = CreateTestUsers(5);
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        // Act
        var query = dbContext.Users.AsQueryable();
        var result = await query.GetPagedResultsAsync(0, 10);

        // Assert
        result.Items.Count.Should().Be(5);
        result.TotalItems.Should().Be(5);
    }

    [Fact]
    public async Task GetPagedResultsAsync_MultiplePages_ShouldReturnCorrectPage()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var users = CreateTestUsers(25);
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        // Act - get second page with page size 10
        var query = dbContext.Users.AsQueryable();
        var result = await query.GetPagedResultsAsync(1, 10);

        // Assert
        result.Items.Count.Should().Be(10);
        result.TotalItems.Should().Be(25);
    }

    [Fact]
    public async Task GetPagedResultsAsync_PartialLastPage_ShouldReturnRemainingItems()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var users = CreateTestUsers(25);
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        // Act - get last page with page size 10
        var query = dbContext.Users.AsQueryable();
        var result = await query.GetPagedResultsAsync(2, 10);

        // Assert
        result.Items.Count.Should().Be(5);
        result.TotalItems.Should().Be(25);
    }

    [Fact]
    public async Task ApplyFuzzySearch_StringProperties_ShouldReturnMatchingItems()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.Users.AddRangeAsync(new[]
        {
            User.Create("Alice", "Johnson", "alice@example.com", new DateTime(1990, 1, 1)),
            User.Create("Bob", "Smith", "bob@example.com", new DateTime(1991, 2, 2)),
            User.Create("Charlie", "Johnson", "charlie@example.com", new DateTime(1992, 3, 3))
        });
        await dbContext.SaveChangesAsync();

        // Act
        var query = dbContext.Users.AsQueryable();
        var result = await query.ApplyFuzzySearch("John", nameof(User.LastName))
            .ToListAsync();

        // Assert
        result.Count.Should().Be(2);
        result.All(u => u.LastName.Contains("John")).Should().BeTrue();
    }

    [Fact]
    public async Task ApplyFuzzySearch_MultipleProperties_ShouldSearchAcrossAllFields()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.Users.AddRangeAsync(new[]
        {
            User.Create("Alice", "Johnson", "alice@example.com", new DateTime(1990, 1, 1)),
            User.Create("Bob", "Smith", "bob@example.com", new DateTime(1991, 2, 2)),
            User.Create("John", "Doe", "john@example.com", new DateTime(1992, 3, 3))
        });
        await dbContext.SaveChangesAsync();

        // Act
        var query = dbContext.Users.AsQueryable();
        var result = await query.ApplyFuzzySearch("John", nameof(User.FirstName), nameof(User.LastName))
            .ToListAsync();

        // Assert
        result.Count.Should().Be(2); // Should find both "John" in FirstName and "Johnson" in LastName
    }

    [Fact]
    public async Task ApplyFuzzySearch_ExpressionBased_ShouldReturnMatchingItems()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.Users.AddRangeAsync(new[]
        {
            User.Create("Alice", "Johnson", "alice@example.com", new DateTime(1990, 1, 1)),
            User.Create("Bob", "Smith", "bob@example.com", new DateTime(1991, 2, 2)),
            User.Create("Charlie", "Johnson", "charlie@example.com", new DateTime(1992, 3, 3))
        });
        await dbContext.SaveChangesAsync();

        // Act
        var query = dbContext.Users.AsQueryable();
        var result = await query.ApplyFuzzySearch("John", u => u.LastName)
            .ToListAsync();

        // Assert
        result.Count.Should().Be(2);
        result.All(u => u.LastName.Contains("John")).Should().BeTrue();
    }

    [Fact]
    public async Task ApplyFuzzySearch_MultipleExpressions_ShouldSearchAcrossAllFields()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.Users.AddRangeAsync(new[]
        {
            User.Create("Alice", "Johnson", "alice@example.com", new DateTime(1990, 1, 1)),
            User.Create("Bob", "Smith", "bob@example.com", new DateTime(1991, 2, 2)),
            User.Create("John", "Doe", "john@example.com", new DateTime(1992, 3, 3))
        });
        await dbContext.SaveChangesAsync();

        // Act
        var query = dbContext.Users.AsQueryable();
        var result = await query.ApplyFuzzySearch("John", u => u.FirstName, u => u.LastName)
            .ToListAsync();

        // Assert
        result.Count.Should().Be(2); // Should find both "John" in FirstName and "Johnson" in LastName
    }

    [Fact]
    public async Task ApplyFuzzySearch_EmptySearchTerm_ShouldReturnAllItems()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var users = CreateTestUsers(5);
        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        // Act
        var query = dbContext.Users.AsQueryable();
        var result = await query.ApplyFuzzySearch("", u => u.FirstName)
            .ToListAsync();

        // Assert
        result.Count.Should().Be(5); // Should return all records
    }

    [Fact]
    public async Task CombinedMethods_ShouldProduceCorrectResults()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var users = Enumerable.Range(1, 20).Select(i =>
            User.Create($"User{i}", i % 2 == 0 ? "Smith" : "Johnson", $"user{i}@example.com",
                new DateTime(1990, 1, 1).AddDays(i))).ToList();

        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        // Act - fuzzy search + paging
        var query = dbContext.Users.AsQueryable();
        var result = await query
            .ApplyFuzzySearch("Smith", nameof(User.LastName))
            .GetPagedResultsAsync(0, 5);

        // Assert
        result.Items.Count.Should().Be(5);
        result.TotalItems.Should().Be(10); // Half of 20 users have LastName="Smith"
        result.Items.All(u => u.LastName == "Smith").Should().BeTrue();
    }

    private static List<User> CreateTestUsers(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => User.Create(
                $"User{i}",
                $"LastName{i}",
                $"user{i}@example.com",
                new DateTime(1990, 1, 1).AddDays(i)))
            .ToList();
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