using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateCourse;

public record CreateCourseCommand(
    string Name,
    string Code,
    string Description,
    string Semester // Thêm Semester
) : ICommand<CourseCreatedResponseDto>;
