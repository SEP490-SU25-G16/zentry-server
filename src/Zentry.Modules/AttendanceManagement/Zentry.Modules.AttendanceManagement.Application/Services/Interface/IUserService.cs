using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.AttendanceManagement.Application.Services.Interface;

public interface IUserService
{
    Task<GetUserByIdAndRoleIntegrationResponse?> GetUserByIdAndRoleAsync(Role role, Guid userId,
        CancellationToken cancellationToken);
}