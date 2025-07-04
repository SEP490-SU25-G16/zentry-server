﻿using Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IEnrollmentRepository : IRepository<Enrollment, Guid>
{
    Task<bool> ExistsAsync(Guid studentId, Guid scheduleId, CancellationToken cancellationToken);

    Task<(List<Enrollment> Enrollments, int TotalCount)> GetPagedEnrollmentsAsync(
        EnrollmentListCriteria criteria,
        CancellationToken cancellationToken);
}