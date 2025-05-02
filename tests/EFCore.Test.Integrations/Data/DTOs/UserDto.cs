namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.DTOs;

public class UserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public UserProfileDto Profile { get; set; }
}