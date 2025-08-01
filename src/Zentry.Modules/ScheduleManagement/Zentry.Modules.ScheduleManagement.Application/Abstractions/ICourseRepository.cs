﻿using Zentry.Modules.ScheduleManagement.Application.Features.GetCourses;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface ICourseRepository : IRepository<Course, Guid>
{
    Task<bool> IsCodeUniqueAsync(string code, CancellationToken cancellationToken);

    Task<Tuple<List<Course>, int>> GetPagedCoursesAsync(CourseListCriteria criteria,
        CancellationToken cancellationToken);

    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken);
}