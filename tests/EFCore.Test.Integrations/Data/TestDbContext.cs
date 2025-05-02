using Microsoft.EntityFrameworkCore;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

public class TestDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContext).Assembly);
    }
}
