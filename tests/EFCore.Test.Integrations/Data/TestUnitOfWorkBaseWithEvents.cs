using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

internal sealed class TestUnitOfWorkBaseWithEvents: 
    UnitOfWorkBase<TestDbContextWithEvents> 
    , ITestUnitOfWorkBaseWithEvents
{
    public IUsersRepository Users { get; }
    public IAppLogsRepository AppLogs { get; }


    public TestUnitOfWorkBaseWithEvents(TestDbContextWithEvents context): base(context)
    {
        Users = new UsersRepository(context);
        AppLogs = new AppLogsRepository(context);
    }
}