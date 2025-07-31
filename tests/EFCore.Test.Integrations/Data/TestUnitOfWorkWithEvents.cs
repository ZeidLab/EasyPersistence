using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

internal sealed class TestUnitOfWorkWithEvents: 
    UnitOfWorkBase<TestDbContextWithEvents> 
    , ITestUnitOfWorkWithEvents
{
    public IUsersRepository Users { get; }
    public IAppLogsRepository AppLogs { get; }


    public TestUnitOfWorkWithEvents(TestDbContextWithEvents context): base(context)
    {
        Users = new UsersRepository(context);
        AppLogs = new AppLogsRepository(context);
    }
}