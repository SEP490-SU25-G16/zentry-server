using MediatR;
using Zentry.SharedKernel.Common;

namespace Zentry.Modules.DeviceManagement.Application.Features.RegisterDevice;

public record RegisterDeviceCommand(
    Guid AccountId,
    string DeviceName,
    string DeviceToken
) : IRequest<Result<Guid>>;

