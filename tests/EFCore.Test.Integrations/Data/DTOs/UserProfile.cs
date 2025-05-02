namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}