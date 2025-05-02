using ZeidLab.ToolBox.EasyPersistence.Abstractions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

public class UserProfile: Entity<Guid>
{
    // EF Core requires a parameterless constructor for EF Core to create instances of the entity
    private UserProfile()
    {
    }
    public Guid UserId { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}