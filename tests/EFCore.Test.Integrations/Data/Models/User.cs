using ZeidLab.ToolBox.EasyPersistence.Abstractions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

public class User : Entity<Guid>, IAggregateRoot
{
    // EF Core requires a parameterless constructor for EF Core to create instances of the entity
    private User()
    {
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public UserProfile Profile { get; set; }
}