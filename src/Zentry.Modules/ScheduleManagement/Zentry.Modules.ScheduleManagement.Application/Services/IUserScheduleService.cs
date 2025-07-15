using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IUserScheduleService
{
    Task<GetUserByIdAndRoleIntegrationResponse?> GetUserByIdAndRoleAsync(string role, Guid userId,
        CancellationToken cancellationToken);
}
