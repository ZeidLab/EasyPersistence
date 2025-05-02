using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ZeidLab.ToolBox.EasyPersistence.Abstractions;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;
[CLSCompliant(false)] // Mark the class as non-CLS-compliant
[SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
public abstract class RepositoryBase<TEntity, TEntityId> : IRepositoryBase<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>, IAggregateRoot
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

    public async Task<PagedResult<TEntity>> GetPagedResultsAsync(Expression<Func<TEntity, bool>> predicate,
        int page = 0, int pageSize = 10)
    {
        var query = _context.Set<TEntity>().Where(predicate);
        var itemsCount = query.AsNoTracking().CountAsync();
        var items = query.Skip(page * pageSize).Take(pageSize).ToListAsync();
        await Task.WhenAll(itemsCount, items).ConfigureAwait(false);
        return new PagedResult<TEntity>(await items.ConfigureAwait(false), await itemsCount.ConfigureAwait(false));
    }

    public async Task<PagedResult<TEntity>> GetPagedResultsAsync(int page, int pageSize)
    {
        var itemsCount = _context.Set<TEntity>().AsNoTracking().CountAsync();
        var items = _context.Set<TEntity>().Skip(page * pageSize).Take(pageSize).ToListAsync();
        await Task.WhenAll(itemsCount, items).ConfigureAwait(false);
        return new PagedResult<TEntity>(await items.ConfigureAwait(false), await itemsCount.ConfigureAwait(false));
    }


    public async Task<PagedResult<TEntity>> FuzzySearchAsync(string searchTerm,
        Expression<Func<TEntity, bool>> predicate, int page = 0, int pageSize = 10,
        params string[] fieldsToSearch)
    {
        var query = _context.Set<TEntity>().Where(predicate)
            .Where(x => fieldsToSearch.Any(fieldToSearch
                => EF.Functions.Like(fieldToSearch, $"%{searchTerm}%")));
        var itemsCount = query.AsNoTracking().CountAsync();
        var items = query.Skip(page * pageSize).Take(pageSize).ToListAsync();
        await Task.WhenAll(itemsCount, items).ConfigureAwait(false);
        return new PagedResult<TEntity>(await items.ConfigureAwait(false), await itemsCount.ConfigureAwait(false));
    }

    public async Task<PagedResult<TEntity>> FuzzySearchAsync(string searchTerm, int page = 0, int pageSize = 10,
        params string[] fieldsToSearch)
    {
        var query = _context.Set<TEntity>()
            .Where(x => fieldsToSearch.Any(fieldToSearch
                => EF.Functions.Like(fieldToSearch, $"%{searchTerm}%")));
        var itemsCount = query.AsNoTracking().CountAsync();
        var items = query.Skip(page * pageSize).Take(pageSize).ToListAsync();
        await Task.WhenAll(itemsCount, items).ConfigureAwait(false);
        return new PagedResult<TEntity>(await items.ConfigureAwait(false), await itemsCount.ConfigureAwait(false));
    }
}