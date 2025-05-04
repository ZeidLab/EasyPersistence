using ZeidLab.ToolBox.EasyPersistence.Abstractions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Units.Models;

public sealed class User : Entity<Guid>, IAggregateRoot
{
    // EF Core requires a parameterless constructor for EF Core to create instances of the entity
    private User() 
    {
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public UserProfile? Profile { get; private set; }

    public static User Create( string firstName, string lastName, string email, DateTime dateOfBirth)
    {
        ArgumentNullException.ThrowIfNull(firstName);

        ArgumentNullException.ThrowIfNull(lastName);

        ArgumentNullException.ThrowIfNull(email);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfBirth = dateOfBirth
        };
    }

    public User WithProfile(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        Profile = profile;
        return this;
    }
    public void Update(string firstName, string lastName, string email, DateTime dateOfBirth)
    {
        ArgumentNullException.ThrowIfNull(firstName);

        ArgumentNullException.ThrowIfNull(lastName);

        ArgumentNullException.ThrowIfNull(email);

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DateOfBirth = dateOfBirth;
    }
}