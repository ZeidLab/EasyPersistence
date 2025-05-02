using ZeidLab.ToolBox.EasyPersistence.Abstractions;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;

public interface IUsers : IRepositoryBase<User,Guid>;