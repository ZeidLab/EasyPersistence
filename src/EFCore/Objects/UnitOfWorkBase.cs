using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Provides a base implementation of <see cref="IUnitOfWork"/> for EF Core <see cref="DbContext"/>.
/// </summary>
/// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
/// <remarks>
/// Inherit from this class to implement custom unit of work logic for your context.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// public sealed class MyUnitOfWork : UnitOfWorkBase<MyDbContext>
/// {
///     public MyUnitOfWork(MyDbContext context) : base(context) { }
/// }
/// 
/// await myUnitOfWork.BeginTransactionAsync();
/// await myUnitOfWork.SaveChangesAsync();
/// await myUnitOfWork.CommitTransactionAsync();
/// ]]></code>
/// </example>
public abstract class UnitOfWorkBase<TContext> : IUnitOfWork, IAsyncDisposable
    where TContext : DbContext
{
    /// <summary>
    /// Gets the underlying <see cref="DbContext"/> instance.
    /// </summary>
    private protected TContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkBase{TContext}"/> class.
    /// </summary>
    /// <param name="context">The database context to use for the unit of work.</param>
    protected UnitOfWorkBase(TContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Context.Database.RollbackTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Context.Database.CommitTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return Context.DisposeAsync();
    }
}