using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IUserScheduleService
{
    Task<LecturerLookupDto?> GetLecturerByIdAsync(Guid lecturerId, CancellationToken cancellationToken);
    Task<UserLookupDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}