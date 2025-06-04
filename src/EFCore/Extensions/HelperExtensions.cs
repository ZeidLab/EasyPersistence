using System.Diagnostics.Contracts;
using System.Globalization;
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

    public static async Task<PagedResult<TEntity>> GetPagedResultsAsync<TEntity, TDbContext>(
        this IQueryable<TEntity> query,
        TDbContext dbContext, int page, int pageSize) where TEntity : class where TDbContext : DbContext
    {
        // Generate SQL that includes COUNT(*) OVER() to get total count in single query
        var sql = query.ToQueryString();
        var countedSql = $"SELECT *, COUNT(*) OVER() AS TotalCount FROM ({sql}) AS CountedQuery " +
                         $"ORDER BY (SELECT NULL) OFFSET {(page * pageSize).ToString(CultureInfo.InvariantCulture)}" +
                         $" ROWS FETCH NEXT {pageSize.ToString(CultureInfo.InvariantCulture)} ROWS ONLY";

        // Execute the query
        var result = await dbContext.Set<TEntity>()
            .FromSqlRaw(countedSql)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        // Get total count from the first row (or 0 if no results)
        var totalCount = result.Any()
            ? (long)(result[0].GetType().GetProperty("TotalCount")?.GetValue(result[0], null) ?? 0)
            : 0;
        return new PagedResult<TEntity>(result, totalCount);
    }
}