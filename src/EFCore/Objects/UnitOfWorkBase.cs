using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

public class UnitOfWorkBase<TContext> : IUnitOfWork, IAsyncDisposable
    where TContext : DbContext
{
    // ReSharper disable once MemberCanBePrivate.Global
    private protected TContext Context { get; }

    protected UnitOfWorkBase(TContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Context.Database.BeginTransactionAsync(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Context.Database.RollbackTransactionAsync(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Context.Database.CommitTransactionAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return Context.DisposeAsync();
    }
}