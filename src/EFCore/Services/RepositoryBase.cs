using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

[SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
public abstract class RepositoryBase<TEntity, TEntityId> : IRepositoryBase<TEntity, TEntityId>
    where TEntity : EntityBase<TEntityId>, IAggregateRoot
    where TEntityId : notnull
{
    private protected readonly DbContext _context;

    protected RepositoryBase(DbContext context)
    {
        _context = context;
    }

    // Define your methods here
    public Task<TEntity?> GetByIdAsync(TEntityId entityId)
    {
        return _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(entityId));
    }

    public Task<List<TEntity>> GetAllAsync()
    {
        return _context.Set<TEntity>().ToListAsync();
    }

    public Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public void Add(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().AddRange(entities);
    }

    public void Remove(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().RemoveRange(entities);
    }

    public Task<PagedResult<TEntity>> GetPagedResultsAsync(Expression<Func<TEntity, bool>> predicate,
        int page = 0, int pageSize = 10)
    {
        return _context.Set<TEntity>()
            .Where(predicate)
            .GetPagedResultsAsync(_context,page, pageSize);
    }

    public Task<PagedResult<TEntity>> GetPagedResultsAsync(int page, int pageSize)
    {
        return _context.Set<TEntity>()
            .GetPagedResultsAsync(_context,page, pageSize);
    }

    public Task<PagedResult<TEntity>> SearchAsync(string searchTerm,
        Expression<Func<TEntity, bool>> predicate, int page = 0, int pageSize = 10,
        params string[] fieldsToSearch)
    {
        return _context.Set<TEntity>().Where(predicate)
            .ApplySearch(searchTerm, fieldsToSearch)
            .GetPagedResultsAsync(_context,page, pageSize);
    }

    public Task<PagedResult<TEntity>> SearchAsync(string searchTerm, int page = 0, int pageSize = 10,
        params string[] fieldsToSearch)
    {
        return _context.Set<TEntity>()
            .ApplySearch(searchTerm, fieldsToSearch)
            .GetPagedResultsAsync(_context,page, pageSize);
    }

    public Task<List<ScoredRecord<TEntity>>> FuzzySearchAsync(string searchTerm, params Expression<Func<TEntity, string>>[] fieldsToSearch)
    {
        // Example of validating property expressions
        return fieldsToSearch == null || fieldsToSearch.Any(expr => expr is null)
            ? throw new ArgumentNullException(nameof(fieldsToSearch), "One or more property expressions are null.")
            : _context.Set<TEntity>()
                .ApplyFuzzySearch(searchTerm, fieldsToSearch)
                .ToListAsync();
    }

    public Task<int> InDbUpdatePropertyAsync(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setters)
    {
        // Execute the update with the dynamically built expression
        return _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteUpdateAsync(setters);
    }

    public Task<int> InDbDeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteDeleteAsync();
    }
}