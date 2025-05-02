// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.Abstractions;

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalItems)
{
    public static readonly PagedResult<T> Empty = new([], 0);

    public IReadOnlyCollection<T> Items { get; } = Items;
    public int TotalItems { get; } = TotalItems;
}