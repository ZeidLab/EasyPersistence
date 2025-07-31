using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;
using ZeidLab.ToolBox.EventBuss;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Events;

internal readonly record struct UserIsRegistered(User UserInfo): IAppEvent, IDomainEvent;