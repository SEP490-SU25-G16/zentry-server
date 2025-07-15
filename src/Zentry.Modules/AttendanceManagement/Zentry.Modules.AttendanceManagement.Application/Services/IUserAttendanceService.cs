using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IUserAttendanceService
{
    Task<GetUserByIdAndRoleIntegrationResponse?> GetUserByIdAndRoleAsync(string role, Guid userId,
        CancellationToken cancellationToken);
}
