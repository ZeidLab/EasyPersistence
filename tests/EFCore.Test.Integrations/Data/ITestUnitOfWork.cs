using ZeidLab.ToolBox.EasyPersistence.Abstractions;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data;

public interface ITestUnitOfWork : IUnitOfWork
{
    IUsersRepository Users { get; }
    IUsersWithGuid7Repository UsersWithGuid7 { get; }
}