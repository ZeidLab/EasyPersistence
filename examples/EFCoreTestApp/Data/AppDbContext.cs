using Microsoft.EntityFrameworkCore;
using ZeidLab.ToolBox.EasyPersistence.EFCore;
namespace ZeidLab.ToolBox.EasyPersistence.EFCoreTestApp.Data
{
    public class TodoList : EntityBase<int> , IAggregateRoot
    {
        public string? Title { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TodoList> TodoLists { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.RegisterFuzzySearchMethods();
        }
    }
}

