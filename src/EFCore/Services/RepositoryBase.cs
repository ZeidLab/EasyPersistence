using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Provides a base implementation of <see cref="IRepositoryBase{TEntity, TEntityId}"/> using EF Core.
/// </summary>
/// <typeparam name="TEntity">The type of the entity managed by the repository.</typeparam>
/// <typeparam name="TEntityId">The type of the entity's unique identifier.</typeparam>
/// <remarks>
/// Inherit from this class to implement custom repository logic for your entities.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// public sealed class PersonRepository : RepositoryBase<Person, int>
/// {
///     public PersonRepository(DbContext context) : base(context) { }
/// }
/// 
/// var repo = new PersonRepository(context);
/// var person = await repo.GetByIdAsync(1);
/// ]]></code>
/// </example>
[SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
public abstract class RepositoryBase<TEntity, TEntityId> : IRepositoryBase<TEntity, TEntityId>
    where TEntity : EntityBase<TEntityId>, IAggregateRoot
    where TEntityId : notnull
{
    /// <summary>
    /// Gets the EF Core database context used by this repository for data access operations.
    /// </summary>
    protected readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity, TEntityId}"/> class.
    /// </summary>
    /// <param name="context">The EF Core <see cref="DbContext"/> to use.</param>
    protected RepositoryBase(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public Task<TEntity?> GetByIdAsync(TEntityId entityId)
    {
        return _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(entityId));
    }

    /// <inheritdoc/>
    public Task<List<TEntity>> GetAllAsync()
    {
        return _context.Set<TEntity>().ToListAsync();
    }

    /// <inheritdoc/>
    public Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    /// <inheritdoc/>
    public Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    /// <inheritdoc/>
    public void Add(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    /// <inheritdoc/>
    public void AddRange(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().AddRange(entities);
    }

    /// <inheritdoc/>
    public void Remove(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc/>
    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().RemoveRange(entities);
    }

    /// <inheritdoc/>
    public Task<PagedResult<TEntity>> GetPagedResultsAsync(Expression<Func<TEntity, bool>> predicate,
        int page = 0, int pageSize = 10)
    {
        return _context.Set<TEntity>()
            .Where(predicate)
            .GetPagedResultsAsync(page, pageSize);
    }

    /// <inheritdoc/>
    public Task<PagedResult<TEntity>> GetPagedResultsAsync(int page, int pageSize)
    {
        return _context.Set<TEntity>()
            .GetPagedResultsAsync(page, pageSize);
    }

    /// <inheritdoc/>
    public Task<PagedResult<TEntity>> SearchAsync(string searchTerm,
        Expression<Func<TEntity, bool>> predicate, int page = 0, int pageSize = 10,
        params string[] fieldsToSearch)
    {
        return _context.Set<TEntity>().Where(predicate)
            .ApplySearch(searchTerm, fieldsToSearch)
            .GetPagedResultsAsync(page, pageSize);
    }

    /// <inheritdoc/>
    public Task<PagedResult<TEntity>> SearchAsync(string searchTerm, int page = 0, int pageSize = 10,
        params string[] fieldsToSearch)
    {
        return _context.Set<TEntity>()
            .ApplySearch(searchTerm, fieldsToSearch)
            .GetPagedResultsAsync(page, pageSize);
    }

    /// <inheritdoc/>
    public Task<List<ScoredRecord<TEntity>>> FuzzySearchAsync(string searchTerm, params Expression<Func<TEntity, string>>[] fieldsToSearch)
    {
        // Example of validating property expressions
        return fieldsToSearch == null || fieldsToSearch.Any(expr => expr is null)
            ? throw new ArgumentNullException(nameof(fieldsToSearch), "One or more property expressions are null.")
            : _context.Set<TEntity>()
                .ApplyFuzzySearch(searchTerm, fieldsToSearch)
                .ToListAsync();
    }

    /// <inheritdoc/>
    public Task<int> InDbUpdatePropertyAsync(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setters)
    {
        // Execute the update with the dynamically built expression
        return _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteUpdateAsync(setters);
    }

    /// <inheritdoc/>
    public Task<int> InDbDeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteDeleteAsync();
    }
}