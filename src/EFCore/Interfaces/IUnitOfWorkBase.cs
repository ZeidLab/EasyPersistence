using Microsoft.EntityFrameworkCore.Storage;
// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.EFCore;

/// <summary>
/// Defines a unit of work contract for managing database transactions and saving changes.
/// </summary>
/// <example>
/// Basic usage:
/// <code><![CDATA[
/// await unitOfWork.BeginTransactionAsync();
/// try
/// {
///     // Perform data operations
///     await unitOfWork.SaveChangesAsync();
///     await unitOfWork.CommitTransactionAsync();
/// }
/// catch
/// {
///     await unitOfWork.RollbackTransactionAsync();
///     throw;
/// }
/// ]]></code>
/// </example>
public interface IUnitOfWorkBase
{
    /// <summary>
    /// Saves all changes made in the current context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The started database transaction.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}