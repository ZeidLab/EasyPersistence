// ReSharper disable once CheckNamespace

namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

public sealed class PropertyScore
{
    public PropertyScore()
    {
        Name = string.Empty;
    }

    public required string Name { get; set; }
    public double Score { get; set; }
}