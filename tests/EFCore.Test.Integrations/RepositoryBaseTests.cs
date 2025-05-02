using Bogus;
using Microsoft.EntityFrameworkCore;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations;

public class RepositoryBaseTests
{
    public RepositoryBaseTests()
    {
        
    }
    [Fact]
    public async Task GenerateUsersWithProfiles()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(databaseName: "TestDatabase")
            .Options;

        using var context = new TestDbContext(options);

        var profileFaker = new Faker<UserProfile>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid()) // GUID v7
            .RuleFor(p => p.Address, f => f.Address.FullAddress())
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber());

        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid()) // GUID v7
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.DateOfBirth, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
            .RuleFor(u => u.Profile, _ => profileFaker.Generate());

        var users = userFaker.Generate(100);

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        Assert.Equal(100, await context.Users.CountAsync());
    }
}
