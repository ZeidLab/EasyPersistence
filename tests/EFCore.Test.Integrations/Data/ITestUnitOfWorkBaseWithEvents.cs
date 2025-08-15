using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

public interface ITestUnitOfWorkBaseWithEvents : IUnitOfWorkBase
{
    IUsersRepository Users { get; }
    IAppLogsRepository AppLogs { get; }
}