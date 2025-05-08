namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units.Models;

public sealed class UserProfile : EntityBase<Guid>
{
    // EF Core requires a parameterless constructor for EF Core to create instances of the entity
    private UserProfile()
    {
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public string? Address { get; private set; }
    public string? PhoneNumber { get; private set; }

    public static UserProfile Create(string? address, string? phoneNumber)
    {
        return new UserProfile { Address = address, PhoneNumber = phoneNumber };
    }
}