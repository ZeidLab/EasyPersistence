using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;
// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;
/// <summary>
/// Provides extension methods for performing fuzzy search operations on <see cref="IQueryable{TEntity}"/> sources.
/// </summary>
/// <remarks>
/// These methods enable scoring and ranking of entities based on similarity to a search term, using SQL CLR or in-memory logic.
/// </remarks>
public static class FuzzySearchExtensions
{
    /// <summary>
    /// Applies fuzzy search scoring to the specified query using the provided search term and string properties.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
    /// <param name="query">The source query to apply fuzzy search to.</param>
    /// <param name="searchTerm">The term to compare against entity properties.</param>
    /// <param name="propertyExpressions">One or more expressions selecting string properties to search.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="ScoredRecord{TEntity}"/> with entities and their fuzzy search scores.
    /// </returns>
    /// <example>
    /// <code><![CDATA[
    /// var results = dbContext.People
    ///     .ApplyFuzzySearch("john", x => x.FirstName, x => x.LastName)
    ///     .OrderByDescending(x => x.Score)
    ///     .ToList();
    /// ]]></code>
    /// </example>
    public static IQueryable<ScoredRecord<TEntity>> ApplyFuzzySearch<TEntity>(
        this IQueryable<TEntity> query,
        string searchTerm,
        params Expression<Func<TEntity, string>>[] propertyExpressions)
    {
        // Example of validating property expressions
        if (propertyExpressions == null || propertyExpressions.Any(expr => expr == null))
        {
            throw new ArgumentNullException(nameof(propertyExpressions), "One or more property expressions are null.");
        }

        if (string.IsNullOrEmpty(searchTerm)
            || propertyExpressions.Length == 0
            || (searchTerm.Build3GramString() is var normalizedSearchTerm && normalizedSearchTerm.Length < 3))
            return query.Select(x =>
                new ScoredRecord<TEntity> { Entity = x, Score = 0, Scores = Enumerable.Empty<PropertyScore>() });

        // Create the parameter for our entity
        var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
        var searchTermConstant = Expression.Constant(normalizedSearchTerm);

        // Build up expressions for each property
        var propertyScores = propertyExpressions.Select(propExpr =>
        {
            var propertyPath = GetPropertyPath(propExpr);
            var visitor = new SearchExtensions.ParameterReplacer(propExpr.Parameters[0], entityParameter);
            var propertyAccess = visitor.Visit(propExpr.Body);

            // For the property score, use property name and fuzzy search score
            return Expression.MemberInit(
                Expression.New(typeof(PropertyScore)),
                Expression.Bind(
                    typeof(PropertyScore).GetProperty(nameof(PropertyScore.Name))!,
                    Expression.Constant(propertyPath)
                ),
                Expression.Bind(
                    typeof(PropertyScore).GetProperty(nameof(PropertyScore.Score))!,
                    Expression.Call(
                        null,
                        typeof(FuzzySearchExtensions).GetMethod(nameof(FuzzySearch),
                            [typeof(string), typeof(string)]) ??
                        throw new InvalidOperationException("The FuzzySearch method can not be found"),
                        searchTermConstant,
                        Expression.Coalesce(propertyAccess, Expression.Constant(string.Empty))
                    )
                )
            );
        }).ToArray();

        // Create array of property scores
        var scoresArrayExpr = Expression.NewArrayInit(
            typeof(PropertyScore),
            propertyScores
        );

        // Create a direct calculation for the average score
        // We'll calculate it manually since we don't have direct access to Enumerable.Average in a way
        // that EF Core can translate to SQL
        Expression avgScoreExpr;

        if (propertyScores.Length == 1)
        {
            // If there's only one property, the score is just that property's score
            avgScoreExpr = Expression.PropertyOrField(propertyScores[0], nameof(PropertyScore.Score));
        }
        else if (propertyScores.Length > 1)
        {
            // For multiple properties, we'll add all scores and divide by the count
            // First map out all the individual score expressions
            var scoreExpressions = propertyScores.Select(p =>
                Expression.PropertyOrField(p, nameof(PropertyScore.Score))).ToArray();

            // Sum them up
            Expression sumExpr = scoreExpressions[0];
            for (int i = 1; i < scoreExpressions.Length; i++)
            {
                sumExpr = Expression.Add(sumExpr, scoreExpressions[i]);
            }

            // Divide by count to get average
            avgScoreExpr = Expression.Divide(
                sumExpr,
                Expression.Constant((double)propertyScores.Length)
            );
        }
        else
        {
            // Fallback case - shouldn't happen given our initial check
            avgScoreExpr = Expression.Constant(0.0);
        }

        // Create final select expression
        var finalSelectExpr = Expression.MemberInit(
            Expression.New(typeof(ScoredRecord<TEntity>)),
            Expression.Bind(
                typeof(ScoredRecord<TEntity>).GetProperty(nameof(ScoredRecord<TEntity>.Entity))!,
                entityParameter
            ),
            Expression.Bind(
                typeof(ScoredRecord<TEntity>).GetProperty(nameof(ScoredRecord<TEntity>.Score))!,
                avgScoreExpr
            ),
            Expression.Bind(
                typeof(ScoredRecord<TEntity>).GetProperty(nameof(ScoredRecord<TEntity>.Scores))!,
                scoresArrayExpr
            )
        );

        // Create the final lambda expression
        var lambda = Expression.Lambda<Func<TEntity, ScoredRecord<TEntity>>>(
            finalSelectExpr,
            entityParameter
        );

        return query.Select(lambda);
    }

    /// <summary>
    /// Computes a fuzzy similarity score between a search term and a string for use in SQL queries.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="comparedString">The string to compare against the search term.</param>
    /// <returns>A similarity score between 0 and 1, where higher is more similar.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var score = FuzzySearchExtensions.FuzzySearch("john", "jonathan");
    /// // Used in LINQ-to-Entities queries for scoring
    /// ]]></code>
    /// </example>
    [DbFunction("FuzzySearch", schema: "dbo")]
    public static double FuzzySearch(string searchTerm, string comparedString)
    {
        // This is a placeholder for the actual implementation
        // You would typically use this in a LINQ query or similar
        return 0;
    }

    // Get property path from expression like x => x.Property or x => x.NestedObject.Property
    private static string GetPropertyPath<TEntity>(Expression<Func<TEntity, string>> propertyExpression)
    {
        var memberExpression = propertyExpression.Body as MemberExpression;
        var path = new List<string>();

        while (memberExpression != null)
        {
            path.Insert(0, memberExpression.Member.Name);
            memberExpression = memberExpression.Expression as MemberExpression;
        }

        return string.Join(".", path);
    }
}