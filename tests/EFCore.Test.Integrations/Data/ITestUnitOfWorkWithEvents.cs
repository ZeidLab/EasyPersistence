using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

public interface ITestUnitOfWorkWithEvents : IUnitOfWork
{
    IUsersRepository Users { get; }
    IAppLogsRepository AppLogs { get; }
}