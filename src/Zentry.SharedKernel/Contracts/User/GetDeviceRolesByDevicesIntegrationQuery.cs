using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.User;

public record GetDeviceRolesByDevicesIntegrationQuery(List<Guid> DeviceIds) : IQuery<GetDeviceRolesByDevicesIntegrationResponse>;

// Response sẽ ánh xạ DeviceId tới Role
public record GetDeviceRolesByDevicesIntegrationResponse(
    Dictionary<Guid, string> DeviceRolesMap
);
