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

    public async Task InitializeAsync()
    {
        await _dbGenerator.MakeSureIsRunningAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbGenerator.MakeSureIsStoppedAsync();
    }
}
