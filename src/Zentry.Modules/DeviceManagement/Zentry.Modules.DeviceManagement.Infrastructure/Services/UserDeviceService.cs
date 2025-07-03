using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.UserManagement.Interfaces;

namespace Zentry.Modules.DeviceManagement.Infrastructure.Services;

public class UserDeviceService(IUserQueryService userQueryService) : IUserDeviceService
{
    public async Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await userQueryService.CheckUserExistsAsync(userId, cancellationToken);
    }
}