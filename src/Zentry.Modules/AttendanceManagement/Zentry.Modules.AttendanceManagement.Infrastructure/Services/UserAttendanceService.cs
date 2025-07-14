using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Services;

public class UserAttendanceService(IUserQueryService userQueryService) : IUserAttendanceService
{
    public async Task<LecturerLookupDto?> GetLecturerByIdAsync(Guid lecturerId, CancellationToken cancellationToken)
    {
        return await userQueryService.GetLecturerByIdAsync(lecturerId, cancellationToken);
    }

    public async Task<UserLookupDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await userQueryService.GeUserByIdAsync(userId, cancellationToken);
    }
}
