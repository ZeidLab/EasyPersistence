using System.Diagnostics.CodeAnalysis;

using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeidLab.ToolBox.EasyPersistence.Abstractions;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.DTOs;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;
using ZeidLab.ToolBox.EasyPersistence.TestHelpers;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations;

[SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
[SuppressMessage("ConfigureAwait", "ConfigureAwaitEnforcer:ConfigureAwaitEnforcer")]
public class RepositoryBaseTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbGenerator _dbGenerator = TestDbGenerator.GenerateSqlServerOnly();

    public RepositoryBaseTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer(_dbGenerator.SqlServerConnectionString));

        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<ITestUnitOfWork, TestUnitOfWork>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GenerateUsersWithProfiles()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var profileFaker = new Faker<UserProfileDto>()
            .RuleFor(p => p.Address, f => f.Address.FullAddress())
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber("###-###-####"));

        var userFaker = new Faker<UserDto>()
            .UseSeed(125487)
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.DateOfBirth, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
            .RuleFor(u => u.Profile, _ => profileFaker.Generate());

        var users = userFaker.Generate(100);
        var entries = users.Select(x =>
        {
            var user = User.Create(x.FirstName, x.LastName, x.Email, x.DateOfBirth);
            return user.WithProfile(UserProfile.Create(x.Profile.Address, x.Profile.PhoneNumber));
        });
        testUnitOfWork.Users.AddRange(entries);
        await testUnitOfWork.SaveChangesAsync();
        var allUsers = await testUnitOfWork.Users.GetAllAsync();
        Assert.Equal(100, allUsers.Count);
    }

    [Fact]
    public async Task EntityId_ShouldBeSetAndRetrievedCorrectly()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user = User.Create("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));
        var profile = UserProfile.Create("123 Main St", "555-1234");
        user.WithProfile(profile);

        testUnitOfWork.Users.Add(user);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var retrievedUser = await testUnitOfWork.Users.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Profile.Id, retrievedUser.Profile.Id);
    }
    [Fact]
    public async Task EntityId_ShouldBeSetAndRetrievedCorrectlyWhenIsSetFromConstructor()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user = UserWithGuid7.Create("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));

        testUnitOfWork.UsersWithGuid7.Add(user);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var allUsers = await testUnitOfWork.Users.GetAllAsync();
        var retrievedUser = await testUnitOfWork.Users.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(allUsers);
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnMatchingEntities()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user1 = User.Create("Alice", "Smith", "alice.smith@example.com", new DateTime(1995, 5, 15));
        var user2 = User.Create("Bob", "Smith", "bob.smith@example.com", new DateTime(1990, 3, 10));
        testUnitOfWork.Users.AddRange(new[] { user1, user2 });
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var result = await testUnitOfWork.Users.FindAllAsync(u => u.LastName == "Smith");

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FindFirstOrDefaultAsync_ShouldReturnFirstMatchingEntity()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user = User.Create("Charlie", "Brown", "charlie.brown@example.com", new DateTime(1985, 7, 20));
        testUnitOfWork.Users.Add(user);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var result = await testUnitOfWork.Users.FindFirstOrDefaultAsync(u => u.FirstName == "Charlie");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Charlie", result.FirstName);
    }

    [Fact]
    public async Task Remove_ShouldDeleteEntity()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user = User.Create("David", "Johnson", "david.johnson@example.com", new DateTime(1980, 12, 25));
        testUnitOfWork.Users.Add(user);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        testUnitOfWork.Users.Remove(user);
        await testUnitOfWork.SaveChangesAsync();

        // Assert
        var result = await testUnitOfWork.Users.GetByIdAsync(user.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPagedResultsAsync_ShouldReturnPagedData()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var users = Enumerable.Range(1, 50).Select(i =>
            User.Create($"User{i}", "Test", $"user{i}@example.com", new DateTime(1990, 1, 1).AddDays(i))).ToList();
        testUnitOfWork.Users.AddRange(users);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var pagedResult = await testUnitOfWork.Users.GetPagedResultsAsync(1, 10);

        // Assert
        Assert.Equal(10, pagedResult.Items.Count);
        Assert.Equal(50, pagedResult.TotalItems);
    }

    [Fact]
    public async Task FuzzySearchAsync_ShouldReturnMatchingEntities()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user1 = User.Create("Emily", "Clark", "emily.clark@example.com", new DateTime(1992, 6, 10));
        var user2 = User.Create("Emma", "Clarkson", "emma.clarkson@example.com", new DateTime(1993, 8, 15));
        testUnitOfWork.Users.AddRange(new[] { user1, user2 });
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var result = await testUnitOfWork.Users.FuzzySearchAsync("Clark", u => true, 0, 10, nameof(User.LastName));

        // Assert
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task AddRange_ShouldAddMultipleEntities()
    {
        using var scope = _serviceProvider.CreateScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var users = new[]
        {
            User.Create("George", "Miller", "george.miller@example.com", new DateTime(1985, 4, 10)),
            User.Create("Grace", "Miller", "grace.miller@example.com", new DateTime(1987, 9, 20))
        };

        // Act
        testUnitOfWork.Users.AddRange(users);
        await testUnitOfWork.SaveChangesAsync();

        // Assert
        var allUsers = await testUnitOfWork.Users.GetAllAsync();
        Assert.Equal(2, allUsers.Count);
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
