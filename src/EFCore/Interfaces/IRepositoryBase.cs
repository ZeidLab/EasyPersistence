using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Defines a generic repository interface for managing entities in a data store.
/// Provides methods for querying, adding, removing, searching, and batch operations on entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity managed by the repository. Must inherit from <see cref="EntityBase{TEntityId}"/> and implement <see cref="IAggregateRoot"/>.</typeparam>
/// <typeparam name="TEntityId">The type of the entity's unique identifier.</typeparam>
/// <example>
/// Basic usage:
/// <code><![CDATA[
/// var repository = new MyEntityRepository(dbContext);
/// var entity = await repository.GetByIdAsync(1);
/// if (entity != null)
/// {
///     repository.Remove(entity);
///     await unitOfWork.SaveChangesAsync();
/// }
/// ]]></code>
/// 
/// Querying entities:
/// <code><![CDATA[
/// var all = await repository.GetAllAsync();
/// var filtered = await repository.FindAllAsync(x => x.IsActive);
/// ]]></code>
/// 
/// Paging and search:
/// <code><![CDATA[
/// var paged = await repository.GetPagedResultsAsync(page: 0, pageSize: 10);
/// var search = await repository.SearchAsync("keyword", page: 0, pageSize: 5, "Name");
/// ]]></code>
/// </example>
[SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
public interface IRepositoryBase<TEntity, in TEntityId>
    where TEntity : EntityBase<TEntityId>, IAggregateRoot
    where TEntityId : notnull
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    Task<TEntity?> GetByIdAsync(TEntityId entityId);

    /// <summary>
    /// Retrieves all entities from the data store.
    /// </summary>
    /// <returns>A list of all entities.</returns>
    Task<List<TEntity>> GetAllAsync();

    /// <summary>
    /// Finds all entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter expression to apply.</param>
    /// <returns>A list of matching entities.</returns>
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Finds the first entity matching the specified predicate, or <c>null</c> if none found.
    /// </summary>
    /// <param name="predicate">The filter expression to apply.</param>
    /// <returns>The first matching entity, or <c>null</c>.</returns>
    Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void Add(TEntity entity);

    /// <summary>
    /// Adds a range of entities to the data store.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Removes an entity from the data store.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Removes a range of entities from the data store.
    /// </summary>
    /// <param name="entities">The entities to remove.</param>
    void RemoveRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Retrieves a paged result of entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter expression to apply.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paged result containing entities and paging information.</returns>
    Task<PagedResult<TEntity>> GetPagedResultsAsync(Expression<Func<TEntity, bool>> predicate, int page = 0, int pageSize = 10);

    /// <summary>
    /// Retrieves a paged result of all entities.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paged result containing entities and paging information.</returns>
    Task<PagedResult<TEntity>> GetPagedResultsAsync(int page, int pageSize);

    /// <summary>
    /// Searches for entities matching the search term and predicate, with paging and field selection.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="predicate">The filter expression to apply.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="fieldsToSearch">The fields to search within.</param>
    /// <returns>A paged result of matching entities.</returns>
    Task<PagedResult<TEntity>> SearchAsync(string searchTerm, Expression<Func<TEntity, bool>> predicate, int page = 0, int pageSize = 10, params string[] fieldsToSearch);

    /// <summary>
    /// Searches for entities matching the search term, with paging and field selection.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="fieldsToSearch">The fields to search within.</param>
    /// <returns>A paged result of matching entities.</returns>
    Task<PagedResult<TEntity>> SearchAsync(string searchTerm, int page = 0, int pageSize = 10, params string[] fieldsToSearch);

    /// <summary>
    /// Performs a fuzzy search on the specified fields and returns scored results.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="fieldsToSearch">The fields to search within.</param>
    /// <returns>A list of scored records matching the search term.</returns>
    Task<List<ScoredRecord<TEntity>>> FuzzySearchAsync(string searchTerm, params Expression<Func<TEntity, string>>[] fieldsToSearch);

    /// <summary>
    /// Updates properties of entities matching the predicate directly in the database without retrieving them.
    /// </summary>
    /// <param name="predicate">The filter expression to select entities.</param>
    /// <param name="setters">The property setters to apply.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> InDbUpdatePropertyAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setters);

    /// <summary>
    /// Deletes entities matching the predicate directly in the database without retrieving them.
    /// </summary>
    /// <param name="predicate">The filter expression to select entities.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> InDbDeleteAsync(Expression<Func<TEntity, bool>> predicate);
}