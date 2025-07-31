using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.DeviceManagement.Features.AcceptDeviceChangeRequest;

public class AcceptDeviceChangeRequestCommand : ICommand<AcceptDeviceChangeRequestResponse>
{
    public Guid UserRequestId { get; set; } // ID của UserRequest cần chấp nhận
}

public class AcceptDeviceChangeRequestResponse
{
    public Guid UpdatedDeviceId { get; set; }
    public Guid DeactivatedDeviceId { get; set; }
    public Guid UserRequestId { get; set; }
    public string Message { get; set; } = string.Empty;
}
