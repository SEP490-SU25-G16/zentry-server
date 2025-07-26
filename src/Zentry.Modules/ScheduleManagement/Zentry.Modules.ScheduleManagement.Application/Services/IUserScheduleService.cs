using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.ScheduleManagement.Application.Services;

public interface IUserScheduleService
{
    Task<GetUserByIdAndRoleIntegrationResponse?> GetUserByIdAndRoleAsync(Role role, Guid userId,
        CancellationToken cancellationToken);
}
