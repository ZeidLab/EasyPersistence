namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

public sealed class UserWithGuid7 : EntityBase<Guid>, IAggregateRoot
{
    // EF Core requires a parameterless constructor for EF Core to create instances of the entity
    private UserWithGuid7() : base(Guid.CreateVersion7())
    {
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public DateTime DateOfBirth { get; private set; }

    public static UserWithGuid7 Create( string firstName, string lastName, string email, DateTime dateOfBirth)
    {
        ArgumentNullException.ThrowIfNull(firstName);

        ArgumentNullException.ThrowIfNull(lastName);

        ArgumentNullException.ThrowIfNull(email);

        return new UserWithGuid7
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfBirth = dateOfBirth
        };
    }
}