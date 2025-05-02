using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

internal sealed class TestUnitOfWork : ITestUnitOfWork
{
    public IUsersRepository Users { get; }
    public IUsersWithGuid7Repository UsersWithGuid7 { get; }

    private readonly TestDbContext _context;

    public TestUnitOfWork(TestDbContext context)
    {
        _context = context;
        Users = new UsersRepository(context);
        UsersWithGuid7 = new UsersWithGuid7Repository(context);
    }

    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}