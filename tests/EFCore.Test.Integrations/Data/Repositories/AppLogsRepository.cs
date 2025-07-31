using Microsoft.EntityFrameworkCore;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

internal sealed class AppLogsRepository : RepositoryBase<AppLog, Guid>, IAppLogsRepository
{
    public AppLogsRepository(DbContext context) : base(context)
    {
    }
}