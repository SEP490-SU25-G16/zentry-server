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

        return (Result<Guid>)Result.Success();
    }
}
