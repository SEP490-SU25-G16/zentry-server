using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Services;

public class LecturerLookupService(IUserQueryService userQueryService) : ILecturerLookupService
{
    public async Task<LecturerLookupDto?> GetLecturerByIdAsync(Guid lecturerId, CancellationToken cancellationToken)
    {
        return await userQueryService.GetLecturerByIdAsync(lecturerId, cancellationToken);
    }
}
