// ReSharper disable once CheckNamespace

namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

public sealed class ScoredRecord<TEntity>

{
    public required TEntity Entity { get; set; }

    public required double Score { get; set; }

    public IEnumerable<PropertyScore> Scores { get; set; } = Enumerable.Empty<PropertyScore>();
}

public sealed class PropertyScore
{
    public PropertyScore()
    {
        Name = string.Empty;
    }
    
    public required string Name { get; set; }
    public double Score { get; set; }
}