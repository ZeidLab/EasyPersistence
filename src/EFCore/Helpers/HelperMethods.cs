using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

public static class HelperMethods
{
    public static async Task<PagedResult<TEntity>> GetPagedResultsAsync<TEntity>(this IQueryable<TEntity> query,
        int page, int pageSize) where TEntity : class
    {
        var itemsCount = await query.AsNoTracking().LongCountAsync().ConfigureAwait(false);
        var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
        return new PagedResult<TEntity>(items, itemsCount);
    }

    public static IQueryable<TEntity> ApplySearch<TEntity>(
        this IQueryable<TEntity> query,
        string searchTerm,
        params string[] propertyNames)
    {
        if (string.IsNullOrEmpty(searchTerm) || propertyNames.Length == 0)
            return query;

        // Create a parameter for the entity
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression? combinedExpression = null;

        // Build an OR condition for each property
        foreach (var propertyName in propertyNames)
        {
            var property = typeof(TEntity).GetProperty(propertyName);

            // Skip properties that don't exist or aren't string type
            if (property == null || property.PropertyType != typeof(string))
                continue;

            // Create property access: x.PropertyName
            var propertyAccess = Expression.Property(parameter, property);

            // Add null check: x.PropertyName != null
            var notNullExpression = Expression.NotEqual(
                propertyAccess,
                Expression.Constant(null, typeof(string))
            );

            // Create condition: x.PropertyName.Contains(searchTerm)
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
            var containsExpression = Expression.Call(
                propertyAccess,
                containsMethod,
                Expression.Constant(searchTerm)
            );

            // Combine not null and contains: x.PropertyName != null && x.PropertyName.Contains(searchTerm)
            var safeContainsExpression = Expression.AndAlso(notNullExpression, containsExpression);

            // Add this condition to the OR expression
            combinedExpression = combinedExpression == null
                ? safeContainsExpression
                : Expression.OrElse(combinedExpression, safeContainsExpression);
        }

        // If no properties matched, return the original query
        if (combinedExpression == null)
            return query;

        // Create the lambda expression: x => combined conditions
        var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);

        // Apply the where clause
        return query.Where(lambda);
    }

    public static IQueryable<TEntity> ApplySearch<TEntity>(
        this IQueryable<TEntity> query,
        string searchTerm,
        params Expression<Func<TEntity, string>>[] propertyExpressions)
    {
        if (string.IsNullOrEmpty(searchTerm) || propertyExpressions.Length == 0)
            return query;

        // Start with a predicate that's always false
        Expression<Func<TEntity, bool>> predicate = x => false;

        // Add each property check to our predicate
        foreach (var propertyExpression in propertyExpressions)
        {
            // Create a contains expression for this property
            var containsPredicate = BuildContainsExpression(propertyExpression, searchTerm);

            // Combine with the main predicate using OR
            predicate = CombineWithOr(predicate, containsPredicate);
        }

        return query.Where(predicate);
    }

    private static Expression<Func<TEntity, bool>> BuildContainsExpression<TEntity>(
        Expression<Func<TEntity, string>> propertyExpression,
        string searchTerm)
    {
        // Create the Contains expression
        var body = propertyExpression.Body;
        var parameter = propertyExpression.Parameters[0];

        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        var searchTermExpr = Expression.Constant(searchTerm);
        var containsCall = Expression.Call(body, containsMethod, searchTermExpr);

        // Add null check to avoid NullReferenceException
        var propNotNull = Expression.NotEqual(body, Expression.Constant(null, typeof(string)));
        var safeContains = Expression.AndAlso(propNotNull, containsCall);

        return Expression.Lambda<Func<TEntity, bool>>(safeContains, parameter);
    }

    private static Expression<Func<T, bool>> CombineWithOr<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        // Create a new parameter
        var parameter = Expression.Parameter(typeof(T), "x");

        // Replace the parameters in both expressions
        var visitor1 = new ParameterReplacer(expr1.Parameters[0], parameter);
        var visitor2 = new ParameterReplacer(expr2.Parameters[0], parameter);

        var left = visitor1.Visit(expr1.Body);
        var right = visitor2.Visit(expr2.Body);

        // Combine with OR
        var body = Expression.OrElse(left, right);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    internal sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly ParameterExpression _new;

        public ParameterReplacer(ParameterExpression old, ParameterExpression @new)
        {
            _old = old;
            _new = @new;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return ReferenceEquals(node, _old) ? _new : base.VisitParameter(node);
        }
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> queryable, bool condition,
        Expression<Func<T, bool>> predicate)
        where T : notnull
    {
        return condition ? queryable.Where(predicate) : queryable;
    }
}