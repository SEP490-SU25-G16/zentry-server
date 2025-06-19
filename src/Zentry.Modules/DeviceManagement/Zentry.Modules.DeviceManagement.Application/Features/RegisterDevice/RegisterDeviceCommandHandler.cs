using MediatR;
using Zentry.Modules.DeviceManagement.Application.Abstractions;
using Zentry.Modules.DeviceManagement.Domain.Entities;
using Zentry.Modules.DeviceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.SharedKernel.Common;

namespace Zentry.Modules.DeviceManagement.Application.Features.RegisterDevice;

public class RegisterDeviceCommandHandler(
    IDeviceRepository deviceRepository,
    // IUserService userService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterDeviceCommand, Result<Guid>>
{
    // private readonly IUserService _userService;

    // _userService = userService;

    public async Task<Result<Guid>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        // var user = await _userServce.GetUserByIdAsync(request.AccountId, cancellationToken);
        dynamic user = null;
        if (user == null)
            return (Result<Guid>)Result.Failure("User not found.");

        var device = Device.Register(request.AccountId, DeviceName.Create(request.DeviceName), DeviceToken.Create());

        await deviceRepository.AddAsync(device, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return (Result<Guid>)Result.Success();
    }
}
