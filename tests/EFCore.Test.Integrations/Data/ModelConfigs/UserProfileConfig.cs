using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.ModelConfigs;

public class UserProfileConfig : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Address)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(15)
            .IsRequired(false);

        builder.HasOne(u => u.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(u => u.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
