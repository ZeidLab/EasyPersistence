using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Provides helper extension methods for query composition and paging in EF Core.
/// </summary>
/// <remarks>
/// Includes conditional filtering and efficient paging utilities for <see cref="IQueryable{T}"/> sources.
/// </remarks>
public static class HelperExtensions
{
    /// <summary>
    /// Conditionally applies a filter to the queryable source.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source.</typeparam>
    /// <param name="queryable">The source queryable.</param>
    /// <param name="condition">If <c>true</c>, applies the filter; otherwise, returns the original queryable.</param>
    /// <param name="predicate">The filter expression to apply.</param>
    /// <returns>The filtered or original queryable, depending on <paramref name="condition"/>.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var filtered = dbContext.People.WhereIf(applyFilter, x => x.IsActive);
    /// ]]></code>
    /// </example>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> queryable, bool condition,
        Expression<Func<T, bool>> predicate)
        where T : notnull
    {
        return condition ? queryable.Where(predicate) : queryable;
    }

    /// <summary>
    /// Returns a paged result for the specified query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A <see cref="PagedResult{TEntity}"/> containing the items and total count.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var paged = await dbContext.People.GetPagedResultsAsync(0, 10);
    /// ]]></code>
    /// </example>
    public static async Task<PagedResult<TEntity>> GetPagedResultsAsync<TEntity>(this IQueryable<TEntity> query,
        int page, int pageSize) where TEntity : class
    {
        var itemsCount = await query.AsNoTracking().LongCountAsync().ConfigureAwait(false);
        var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
        return new PagedResult<TEntity>(items, itemsCount);
    }

#pragma warning disable S125
    // /// <summary>
    // /// Returns a paged result for the specified query using a custom SQL query for total count.
    // /// </summary>
    // /// <typeparam name="TEntity">The type of the entity.</typeparam>
    // /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
    // /// <param name="query">The source query.</param>
    // /// <param name="dbContext">The database context to execute the query.</param>
    // /// <param name="page">The zero-based page index.</param>
    // /// <param name="pageSize">The number of items per page.</param>
    // /// <returns>A <see cref="PagedResult{TEntity}"/> containing the items and total count.</returns>
    // /// <example>
    // /// <code><![CDATA[
    // /// var paged = await dbContext.People.GetPagedResultsAsync(dbContext, 0, 10);
    // /// ]]></code>
    // /// </example>
    // public static async Task<PagedResult<TEntity>> GetPagedResultsAsync<TEntity, TDbContext>(
    //     this IQueryable<TEntity> query,
    //     TDbContext dbContext, int page, int pageSize) where TEntity : class where TDbContext : DbContext
    // {
    //     // Generate SQL that includes COUNT(*) OVER() to get total count in single query
    //     var sql = query.ToQueryString();
    //     var countedSql = $"SELECT *, COUNT(*) OVER() AS TotalCount FROM ({sql}) AS CountedQuery " +
    //                      $"ORDER BY (SELECT NULL) OFFSET {(page * pageSize).ToString(CultureInfo.InvariantCulture)}" +
    //                      $" ROWS FETCH NEXT {pageSize.ToString(CultureInfo.InvariantCulture)} ROWS ONLY";
    //
    //     // Execute the query
    //     var result = await dbContext.Set<TEntity>()
    //         .FromSqlRaw(countedSql)
    //         .AsNoTracking()
    //         .Select(x =>
    //             new { Item = x, TotalCount = EF.Property<int>(x, "TotalCount") })
    //         .ToListAsync()
    //         .ConfigureAwait(false);
    //
    //     // Get total count from the first row (or 0 if no results)
    //     long totalCount = result.Count != 0 ? result[0].TotalCount : 0;
    //     return new PagedResult<TEntity>(result.ConvertAll(x => x.Item), totalCount);
    // }
#pragma warning restore S125
}
