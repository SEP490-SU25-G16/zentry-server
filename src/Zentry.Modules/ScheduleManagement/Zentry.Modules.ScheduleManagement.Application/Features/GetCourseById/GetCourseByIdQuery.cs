using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetCourseById;

public record GetCourseByIdQuery(Guid Id)
    : IQuery<CourseDto>;
