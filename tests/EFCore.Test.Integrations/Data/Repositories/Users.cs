using Microsoft.EntityFrameworkCore;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

internal sealed class Users :RepositoryBase<User,Guid> ,IUsers
{
    public Users(DbContext context) : base(context)
    {
    }
}