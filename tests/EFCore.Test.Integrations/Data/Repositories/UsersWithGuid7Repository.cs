using Microsoft.EntityFrameworkCore;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

internal sealed class UsersWithGuid7Repository :RepositoryBase<UserWithGuid7,Guid> ,IUsersWithGuid7Repository
{
    public UsersWithGuid7Repository(DbContext context) : base(context)
    {
    }
}