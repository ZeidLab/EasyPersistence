// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Represents a paged result set with items and total item count.
/// </summary>
/// <typeparam name="T">The type of the items in the result set.</typeparam>
/// <param name="Items">The collection of items for the current page.</param>
/// <param name="TotalItems">The total number of items across all pages.</param>
/// <example>
/// <code><![CDATA[
/// var paged = new PagedResult<Person>(new[] { person1, person2 }, 100);
/// var count = paged.TotalItems;
/// var items = paged.Items;
/// ]]></code>
/// </example>
public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, long TotalItems)
{
    /// <summary>
    /// Gets an empty <see cref="PagedResult{T}"/> instance.
    /// </summary>
    public static readonly PagedResult<T> Empty = new([], 0);

    /// <summary>
    /// Gets the collection of items for the current page.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; } = Items;

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public long TotalItems { get; } = TotalItems;
}