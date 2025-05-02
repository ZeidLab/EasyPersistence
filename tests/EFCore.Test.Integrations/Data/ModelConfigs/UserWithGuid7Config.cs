using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.ModelConfigs;

public class UserWithGuid7Config : IEntityTypeConfiguration<UserWithGuid7>
{
    public void Configure(EntityTypeBuilder<UserWithGuid7> builder)
    {
        builder.HasKey(u => u.Id);
    }
}
