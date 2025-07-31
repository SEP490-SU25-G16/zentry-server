using MediatR;
using Zentry.Modules.DeviceManagement.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.DeviceManagement.Features.GetDeviceDetails;

public class GetDeviceDetailsQueryHandler(
    IDeviceRepository deviceRepository,
    IMediator mediator
) : IQueryHandler<GetDeviceDetailsQuery, GetDeviceDetailsResponse>
{
    public async Task<GetDeviceDetailsResponse> Handle(GetDeviceDetailsQuery request, CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken);

        if (device is null)
        {
            throw new NotFoundException(nameof(GetDeviceDetailsQueryHandler), request.DeviceId);
        }

        // too fucking lazy to create a new integration handler
        var usersResponse = await mediator.Send(new GetUsersByIdsIntegrationQuery([device.UserId]), cancellationToken);
        var userInfo = usersResponse.Users.FirstOrDefault();

        return new GetDeviceDetailsResponse
        {
            DeviceId = device.Id,
            UserId = device.UserId,
            UserFullName = userInfo?.FullName,
            UserEmail = userInfo?.Email,
            DeviceName = device.DeviceName.Value,
            MacAddress = device.MacAddress.Value,
            DeviceToken = device.DeviceToken.Value,
            Platform = device.Platform,
            OsVersion = device.OsVersion,
            Model = device.Model,
            Manufacturer = device.Manufacturer,
            AppVersion = device.AppVersion,
            PushNotificationToken = device.PushNotificationToken,
            CreatedAt = device.CreatedAt,
            UpdatedAt = device.UpdatedAt,
            LastVerifiedAt = device.LastVerifiedAt,
            Status = device.Status
        };
    }
}
