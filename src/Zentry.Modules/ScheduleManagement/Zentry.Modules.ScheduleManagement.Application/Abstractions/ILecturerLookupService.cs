using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface ILecturerLookupService
{
    Task<LecturerLookupDto?> GetLecturerByIdAsync(Guid lecturerId, CancellationToken cancellationToken);
}
