using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Events;
using ZeidLab.ToolBox.EventBuss;
using ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.Models; // Add this for test output

namespace ZeidLab.ToolBox.EasyPersistence.EFCore.Test.Integrations.Data.EventHandlers;

internal sealed class UserIsRegisteredHandler : IAppEventHandler<UserIsRegistered>
{

    private readonly ITestUnitOfWorkBaseWithEvents _unitOfWorkBase;


    public UserIsRegisteredHandler( ITestUnitOfWorkBaseWithEvents unitOfWorkBase)
    {
       
        _unitOfWorkBase = unitOfWorkBase;
    }
    
    public async Task HandleAsync(UserIsRegistered appEvent, CancellationToken cancellationToken = new CancellationToken())
    {
        var log1 = AppLog.Create($"Handling event: {appEvent.GetType().Name}");
        var log2 = AppLog.Create($"User: {appEvent.UserInfo.FirstName}, Email: {appEvent.UserInfo.Email}");

        _unitOfWorkBase.AppLogs.Add(log1);
        _unitOfWorkBase.AppLogs.Add(log2);
        
        // Your implementation here
        await _unitOfWorkBase.SaveChangesAsync(cancellationToken);
    }
}