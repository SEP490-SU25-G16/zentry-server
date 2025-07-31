using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Device;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.DeviceManagement.Features.AcceptDeviceChangeRequest;

public class AcceptDeviceChangeRequestHandler(
    IDeviceRepository deviceRepository,
    ILogger<AcceptDeviceChangeRequestHandler> logger,
    IMediator mediator
) : ICommandHandler<AcceptDeviceChangeRequestCommand, AcceptDeviceChangeRequestResponse>
{
    public async Task<AcceptDeviceChangeRequestResponse> Handle(AcceptDeviceChangeRequestCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Admin is accepting device change request for UserRequestId: {UserRequestId}.",
            command.UserRequestId);

        var response = await mediator.Send(new UpdateUserRequestStatusIntegrationCommand(command.UserRequestId),
            cancellationToken);

        var userId = response.UserId;

        var currentActiveDevice = await deviceRepository.GetActiveDeviceForUserAsync(userId, cancellationToken);
        var deactivatedDeviceId = Guid.Empty;

        if (currentActiveDevice is not null)
        {
            currentActiveDevice.Update(
                currentActiveDevice.DeviceName,
                DeviceStatus.Inactive,
                currentActiveDevice.MacAddress,
                currentActiveDevice.Platform,
                currentActiveDevice.OsVersion,
                currentActiveDevice.Model,
                currentActiveDevice.Manufacturer,
                currentActiveDevice.AppVersion,
                currentActiveDevice.PushNotificationToken
            );
            await deviceRepository.UpdateAsync(currentActiveDevice, cancellationToken);
            deactivatedDeviceId = currentActiveDevice.Id;
            logger.LogInformation("Deactivated old device {DeviceId} for user {UserId}.", currentActiveDevice.Id,
                userId);
        }
        else
        {
            logger.LogWarning(
                "No active device found for user {UserId} when accepting request {UserRequestId}. Proceeding with new device activation.",
                userId, command.UserRequestId);
        }

        var newPendingDevice =
            await deviceRepository.GetPendingDeviceForUserAsync(userId, response.RelatedEntityId, cancellationToken);
        if (newPendingDevice is null)
        {
            logger.LogError(
                "Acceptance failed: New pending device with ID {NewDeviceId} for user {UserId} not found or not in Pending status.",
                response.RelatedEntityId, userId);
            throw new NotFoundException("NewDevice",
                $"Thiết bị mới đang chờ duyệt với ID '{response.RelatedEntityId}' không tìm thấy hoặc không ở trạng thái chờ.");
        }

        newPendingDevice.Update(
            newPendingDevice.DeviceName,
            DeviceStatus.Active,
            newPendingDevice.MacAddress,
            newPendingDevice.Platform,
            newPendingDevice.OsVersion,
            newPendingDevice.Model,
            newPendingDevice.Manufacturer,
            newPendingDevice.AppVersion,
            newPendingDevice.PushNotificationToken
        );
        await deviceRepository.UpdateAsync(newPendingDevice, cancellationToken);
        logger.LogInformation("Activated new device {DeviceId} for user {UserId}.", newPendingDevice.Id, userId);


        logger.LogInformation("UserRequest {UserRequestId} approved successfully.", command.UserRequestId);


        return new AcceptDeviceChangeRequestResponse
        {
            UpdatedDeviceId = newPendingDevice.Id,
            DeactivatedDeviceId = deactivatedDeviceId,
            UserRequestId = response.UserId,
            Message = "Yêu cầu thay đổi thiết bị đã được chấp nhận thành công."
        };
    }
}
