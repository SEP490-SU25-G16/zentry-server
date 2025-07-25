using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.Modules.DeviceManagement.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Device;

namespace Zentry.Modules.DeviceManagement.Integration;

/// <summary>
/// Handler for getting push notification tokens for a specific user
/// Used by NotificationService to get device tokens for push notifications
/// </summary>
public class GetDevicePushTokensQueryHandler(IDeviceRepository repository)
    : IQueryHandler<GetDevicePushTokensIntegrationQuery, GetDevicePushTokensIntegrationResponse>
{
    public async Task<GetDevicePushTokensIntegrationResponse> Handle(
        GetDevicePushTokensIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        // Get all active devices for the user
        var activeDevices = await repository.GetActiveDevicesByUserIdsAsync(
            new List<Guid> { request.UserId }, 
            cancellationToken);

        // Extract push notification tokens, filtering out null/empty values
        var pushTokens = activeDevices
            .Where(device => !string.IsNullOrWhiteSpace(device.PushNotificationToken))
            .Select(device => device.PushNotificationToken!)
            .Distinct()
            .ToList();

        return new GetDevicePushTokensIntegrationResponse(pushTokens.AsReadOnly());
    }
} 