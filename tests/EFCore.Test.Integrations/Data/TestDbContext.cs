using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

public class TestDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserWithGuid7> UsersWithGuid7 { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContext).Assembly);
        modelBuilder.RegisterSqlClrMethods();
    }
}
