using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

internal sealed class TestDbContextWithEvents: DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<AppLog> AppLogs { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public TestDbContextWithEvents(DbContextOptions<TestDbContextWithEvents> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContextWithEvents).Assembly);
        modelBuilder.RegisterFuzzySearchMethods();
    }
}