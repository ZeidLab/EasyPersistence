using Microsoft.EntityFrameworkCore;

using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Interfaces;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Repositories;

internal sealed class UsersRepository :RepositoryBase<User,Guid> ,IUsersRepository
{
    public UsersRepository(DbContext context) : base(context)
    {
    }
}