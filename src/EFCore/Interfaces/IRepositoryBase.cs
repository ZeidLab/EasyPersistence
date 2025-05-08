using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

[SuppressMessage("Design", "MA0016:Prefer using collection abstraction instead of implementation")]
public interface IRepositoryBase<TEntity, in TEntityId>
    where TEntity : EntityBase<TEntityId>, IAggregateRoot
    where TEntityId : notnull
{
    Task<TEntity?> GetByIdAsync(TEntityId entityId);
    Task<List<TEntity>> GetAllAsync();
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    public Task<PagedResult<TEntity>> GetPagedResultsAsync(Expression<Func<TEntity, bool>> predicate, int page = 0,
        int pageSize = 10);

    public Task<PagedResult<TEntity>> GetPagedResultsAsync(int page, int pageSize);

    public Task<PagedResult<TEntity>> FuzzySearchAsync(string searchTerm,
        Expression<Func<TEntity, bool>> predicate, int page = 0, int pageSize = 10,
        params string[] fieldsToSearch);

    public Task<PagedResult<TEntity>> FuzzySearchAsync(string searchTerm,
        int page = 0, int pageSize = 10, params string[] fieldsToSearch);

    // Batch update properties without retrieving entities
    Task<int> InDbUpdatePropertyAsync(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setters);

    // Batch delete entities matching a predicate without retrieving them
    Task<int> InDbDeleteAsync(Expression<Func<TEntity, bool>> predicate);
}
