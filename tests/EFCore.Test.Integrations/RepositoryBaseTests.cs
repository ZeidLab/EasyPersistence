using System.Diagnostics.CodeAnalysis;

using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.DTOs;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;
using ZeidLab.ToolBox.EasyPersistence.TestHelpers;

using FluentAssertions;

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
        {
            options.UseSqlServer(_dbGenerator.SqlServerConnectionString);
            options.EnableSensitiveDataLogging();
        });
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<ITestUnitOfWork, TestUnitOfWork>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GenerateUsersWithProfiles()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
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
        allUsers.Count.Should().Be(100);
    }

    [Fact]
    public async Task EntityId_ShouldBeSetAndRetrievedCorrectly()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
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
        retrievedUser.Should().NotBeNull();
        retrievedUser.Id.Should().Be(user.Id);
        retrievedUser.Profile.Id.Should().Be(user.Profile.Id);
    }

    [Fact]
    public async Task EntityId_ShouldBeSetAndRetrievedCorrectlyWhenIsSetFromConstructor()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user = UserWithGuid7.Create("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));

        testUnitOfWork.UsersWithGuid7.Add(user);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var allUsers = await testUnitOfWork.UsersWithGuid7.GetAllAsync();
        var retrievedUser = await testUnitOfWork.UsersWithGuid7.GetByIdAsync(user.Id);

        // Assert
        allUsers.Should().NotBeEmpty();
        retrievedUser.Should().NotBeNull();
        retrievedUser.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnMatchingEntities()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
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
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task FindFirstOrDefaultAsync_ShouldReturnFirstMatchingEntity()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
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
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Charlie");
    }

    [Fact]
    public async Task Remove_ShouldDeleteEntity()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
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
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedResultsAsync_ShouldReturnPagedData()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
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
        pagedResult.Items.Count.Should().Be(10);
        pagedResult.TotalItems.Should().Be(50);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingEntities()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user1 = User.Create("Emily", "Clark", "emily.clark@example.com", new DateTime(1992, 6, 10));
        var user2 = User.Create("Emma", "Clarkson", "emma.clarkson@example.com", new DateTime(1993, 8, 15));
        testUnitOfWork.Users.AddRange(new[] { user1, user2 });
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var result = await testUnitOfWork.Users.SearchAsync("Clark", 0, 10, nameof(User.LastName));

        // Assert
        result.Items.Count.Should().Be(2);
    }

    [Fact]
    public async Task AddRange_ShouldAddMultipleEntities()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
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
        allUsers.Count.Should().Be(2);
    }

    [Fact]
    public async Task UpdatePropertyAsync_WithSingleProperty_ShouldUpdateMatchingEntities()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var users = new List<User>
        {
            User.Create("John", "Smith", "john.smith@example.com", new DateTime(1990, 1, 1)),
            User.Create("Jane", "Smith", "jane.smith@example.com", new DateTime(1992, 3, 15)),
            User.Create("Mark", "Johnson", "mark.johnson@example.com", new DateTime(1985, 7, 22))
        };
        testUnitOfWork.Users.AddRange(users);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var updatedRows = await testUnitOfWork.Users.InDbUpdatePropertyAsync(
            u => u.LastName == "Smith",
            setter => setter.SetProperty(x => x.LastName, "Smith-Updated")
        );

        // Assert
        updatedRows.Should().Be(2); // Two rows should be updated

        // Verify update in database
        var updatedUsers = await testUnitOfWork.Users.FindAllAsync(u => u.LastName == "Smith-Updated");
        updatedUsers.Count.Should().Be(2);
        updatedUsers.All(u => u.LastName == "Smith-Updated").Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePropertyAsync_WithMultipleProperties_ShouldUpdateMatchingEntities()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var users = new List<User>
        {
            User.Create("John", "Smith", "john.smith@example.com", new DateTime(1990, 1, 1)),
            User.Create("Jane", "Smith", "jane.smith@example.com", new DateTime(1992, 3, 15)),
            User.Create("Mark", "Johnson", "mark.johnson@example.com", new DateTime(1985, 7, 22))
        };
        testUnitOfWork.Users.AddRange(users);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var updatedRows = await testUnitOfWork.Users.InDbUpdatePropertyAsync(
            u => u.LastName == "Smith", setters => setters
                .SetProperty(x => x.LastName, "Smith-Updated")
                .SetProperty(x => x.FirstName, "Updated-FirstName")
        );

        // Assert
        updatedRows.Should().Be(2); // Two rows should be updated

        // Verify update in database
        var updatedUsers = await testUnitOfWork.Users.FindAllAsync(u => u.LastName == "Updated-LastName");
        updatedUsers.Count.Should().Be(2);
        updatedUsers.All(u => u.FirstName == "Updated-FirstName" && u.LastName == "Updated-LastName").Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePropertyAsync_WithDifferentPropertyTypes_ShouldUpdateMatchingEntities()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var users = new List<User>
        {
            User.Create("John", "Smith", "john.smith@example.com", new DateTime(1990, 1, 1)),
            User.Create("Jane", "Smith", "jane.smith@example.com", new DateTime(1992, 3, 15))
        };
        testUnitOfWork.Users.AddRange(users);
        await testUnitOfWork.SaveChangesAsync();

        var newDate = new DateTime(2000, 1, 1);

        // Act
        var updatedRows = await testUnitOfWork.Users.InDbUpdatePropertyAsync(
            u => u.LastName == "Smith", setters
                => setters.SetProperty(u => u.DateOfBirth, newDate)
        );

        // Assert
        updatedRows.Should().Be(2); // Two rows should be updated

        // Verify update in database
        var updatedUsers = await testUnitOfWork.Users.FindAllAsync(u
            => u.LastName == "Smith");
        updatedUsers.Count.Should().Be(2);
        updatedUsers.All(u => u.DateOfBirth == newDate).Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePropertyAsync_NoMatchingEntities_ShouldReturnZero()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var users = new List<User>
        {
            User.Create("John", "Smith", "john.smith@example.com", new DateTime(1990, 1, 1)),
            User.Create("Jane", "Smith", "jane.smith@example.com", new DateTime(1992, 3, 15))
        };
        testUnitOfWork.Users.AddRange(users);
        await testUnitOfWork.SaveChangesAsync();

        // Act
        var updatedRows = await testUnitOfWork.Users.InDbUpdatePropertyAsync(
            u => u.LastName == "NonExistent", setters
                => setters.SetProperty(u => u.FirstName, "Updated")
        );

        // Assert
        updatedRows.Should().Be(0); // No rows should be updated
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldPersistChanges()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user = User.Create("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));

        // Act
        await using var transaction = await testUnitOfWork.BeginTransactionAsync();
        testUnitOfWork.Users.Add(user);
        await testUnitOfWork.SaveChangesAsync();
        await testUnitOfWork.CommitTransactionAsync();

        // Assert
        var retrievedUser = await testUnitOfWork.Users.GetByIdAsync(user.Id);
        retrievedUser.Should().NotBeNull();
        retrievedUser.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task RollbackTransactionAsync_ShouldDiscardChanges()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var testUnitOfWork = scope.ServiceProvider.GetRequiredService<ITestUnitOfWork>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Arrange
        var user = User.Create("Jane", "Doe", "jane.doe@example.com", new DateTime(1992, 3, 15));

        // Act
        await using var transaction = await testUnitOfWork.BeginTransactionAsync();
        testUnitOfWork.Users.Add(user);
        await testUnitOfWork.SaveChangesAsync();
        await testUnitOfWork.RollbackTransactionAsync();

        // Assert
        var retrievedUser = await testUnitOfWork.Users.GetByIdAsync(user.Id);
        retrievedUser.Should().BeNull();
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