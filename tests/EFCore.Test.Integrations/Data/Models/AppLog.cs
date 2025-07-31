namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models;

public sealed class AppLog : EntityBase<Guid>, IAggregateRoot
{
    // EF Core requires a parameterless constructor for EF Core to create instances of the entity
    private AppLog()
    {
    }

    public string Message { get; private set; } = string.Empty;
    
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public static AppLog Create(string message)
    {
        return new AppLog { Message = message };
    }
}