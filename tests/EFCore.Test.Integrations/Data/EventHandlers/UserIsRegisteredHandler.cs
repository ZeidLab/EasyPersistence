using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Events;
using ZeidLab.ToolBox.EventBuss;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models; // Add this for test output

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.EventHandlers;

internal sealed class UserIsRegisteredHandler : IAppEventHandler<UserIsRegistered>
{

    private readonly ITestUnitOfWorkWithEvents _unitOfWork;


    public UserIsRegisteredHandler( ITestUnitOfWorkWithEvents unitOfWork)
    {
       
        _unitOfWork = unitOfWork;
    }
    
    public async Task HandleAsync(UserIsRegistered appEvent, CancellationToken cancellationToken = new CancellationToken())
    {
        var log1 = AppLog.Create($"Handling event: {appEvent.GetType().Name}");
        var log2 = AppLog.Create($"User: {appEvent.UserInfo.FirstName}, Email: {appEvent.UserInfo.Email}");

        _unitOfWork.AppLogs.Add(log1);
        _unitOfWork.AppLogs.Add(log2);
        
        // Your implementation here
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}