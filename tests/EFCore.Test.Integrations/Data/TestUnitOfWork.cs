using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

internal sealed class TestUnitOfWork : UnitOfWorkBase<TestDbContext> ,ITestUnitOfWork
{
    public IUsersRepository Users { get; }
    public IUsersWithGuid7Repository UsersWithGuid7 { get; }

    public TestUnitOfWork(TestDbContext context): base(context)
    {
        Users = new UsersRepository(context);
        UsersWithGuid7 = new UsersWithGuid7Repository(context);
    }
}