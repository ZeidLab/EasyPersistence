using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

public static class HelperExtensions
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> queryable, bool condition,
        Expression<Func<T, bool>> predicate)
        where T : notnull
    {
        return condition ? queryable.Where(predicate) : queryable;
    }

    public static async Task<PagedResult<TEntity>> GetPagedResultsAsync<TEntity>(this IQueryable<TEntity> query,
        int page, int pageSize) where TEntity : class
    {
        var itemsCount = await query.AsNoTracking().LongCountAsync().ConfigureAwait(false);
        var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
        return new PagedResult<TEntity>(items, itemsCount);
    }
}