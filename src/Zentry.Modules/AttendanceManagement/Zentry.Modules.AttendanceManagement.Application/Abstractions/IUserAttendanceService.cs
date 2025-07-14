using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IUserAttendanceService
{
    Task<LecturerLookupDto?> GetLecturerByIdAsync(Guid lecturerId, CancellationToken cancellationToken);
    Task<UserLookupDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}
