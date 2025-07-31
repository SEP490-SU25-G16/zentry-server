using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.User;

public record GetUserRolesByDevicesIntegrationQuery(List<Guid> DeviceIds)
    : IQuery<GetUserRolesByDevicesIntegrationResponse>;

// Response sẽ ánh xạ DeviceId tới Role
public record GetUserRolesByDevicesIntegrationResponse(
    Dictionary<Guid, string> DeviceRolesMap
);
