using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateCourse;

public class UpdateCourseCommandHandler(ICourseRepository courseRepository)
    : ICommandHandler<UpdateCourseCommand, CourseDto>
{
    public async Task<CourseDto> Handle(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetByIdAsync(command.Id, cancellationToken);

        if (course == null) throw new Exception($"Course with ID '{command.Id}' not found.");

        course.Update(
            command.Name,
            command.Description
        );

        await courseRepository.UpdateAsync(course, cancellationToken);

        var responseDto = new CourseDto
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Description = course.Description,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };

        return responseDto;
    }
}