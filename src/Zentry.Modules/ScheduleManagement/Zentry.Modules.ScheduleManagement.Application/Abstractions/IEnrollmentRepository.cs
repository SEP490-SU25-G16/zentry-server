using Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IEnrollmentRepository : IRepository<Enrollment, Guid>
{
    Task<bool> ExistsAsync(Guid studentId, Guid scheduleId, CancellationToken cancellationToken);

    Task<(List<Enrollment> Enrollments, int TotalCount)> GetPagedEnrollmentsAsync(
        EnrollmentListCriteria criteria,
        CancellationToken cancellationToken);

    Task<List<Guid>> GetActiveStudentIdsByClassSectionIdAsync(Guid classSectionId, CancellationToken cancellationToken);
    Task<int> CountActiveStudentsByClassSectionIdAsync(Guid classSectionId, CancellationToken cancellationToken);

    Task<List<Enrollment>>
        GetEnrollmentsByClassSectionIdAsync(Guid classSectionId, CancellationToken cancellationToken);
    Task BulkAddAsync(List<Enrollment> enrollments, CancellationToken cancellationToken);
    Task<List<Enrollment>> GetEnrollmentsByClassSectionAsync(Guid classSectionId, CancellationToken cancellationToken);
}
