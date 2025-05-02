using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using ZeidLab.ToolBox.EasyPersistence.Abstractions;

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Helpers;

public static class HelperMethods
{
    public static async Task<PagedResult<TEntity>> GetPagedResultsAsync<TEntity>(this IQueryable<TEntity> query,
        int page, int pageSize) where TEntity : class
    {
        var itemsCount = await query.AsNoTracking().LongCountAsync().ConfigureAwait(false);
        var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
        return new PagedResult<TEntity>(items, itemsCount);
    }

    public static IQueryable<TEntity> ApplyFuzzySearch<TEntity>(this IQueryable<TEntity> query, string searchTerm,
        params string[] propertyNames)
    {
        return query.Where(entity => propertyNames.Any(propertyName
            => EF.Property<string>(entity!, propertyName).Contains(searchTerm)));
    }

    public static IQueryable<TEntity> ApplyFuzzySearch<TEntity>(
        this IQueryable<TEntity> query,
        string searchTerm,
        params Expression<Func<TEntity, object>>[] propertyExpressions)
    {
        if (string.IsNullOrEmpty(searchTerm) || propertyExpressions.Length == 0)
            return query;

        // Extract property names from the expressions
        var propertyNames = propertyExpressions.Select(GetPropertyPath).ToArray();

        // Use the string-based overload to perform the search
        return query.Where(entity => propertyNames.Any(propertyName =>
            EF.Property<string>(entity!, propertyName).Contains(searchTerm)));
    }

    private static string GetPropertyPath<TEntity>(Expression<Func<TEntity, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (expression.Body is UnaryExpression unaryExpression &&
                 unaryExpression.Operand is MemberExpression memberExp)
        {
            return memberExp.Member.Name;
        }

        throw new ArgumentException("Expression does not refer to a property.", nameof(expression));
    }
}