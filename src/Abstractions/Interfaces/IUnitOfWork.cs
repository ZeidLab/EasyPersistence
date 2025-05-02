// ReSharper disable once CheckNamespace
namespace ZeidLab.ToolBox.EasyPersistence.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}