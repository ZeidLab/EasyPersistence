// ReSharper disable once CheckNamespace

namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

public sealed class ScoredRecord<TEntity>

{
    public required TEntity Entity { get; set; }

    public required double Score { get; set; }

    public IDictionary<string, double>? Scores { get; set; }
}