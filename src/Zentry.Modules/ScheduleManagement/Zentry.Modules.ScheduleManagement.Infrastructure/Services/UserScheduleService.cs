using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Services;

public class UserScheduleService(IUserQueryService userQueryService) : IUserScheduleService
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
