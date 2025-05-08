// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, long TotalItems)
{
    public static readonly PagedResult<T> Empty = new([], 0);

    public IReadOnlyCollection<T> Items { get; } = Items;
    public long TotalItems { get; } = TotalItems;
}