using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteCourse;

public class DeleteCourseCommandHandler(
    ICourseRepository courseRepository,
    IClassSectionRepository classSectionRepository)
    : ICommandHandler<DeleteCourseCommand, bool>
{
    public async Task<bool> Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        // GetByIdAsync sẽ chỉ trả về Course nếu IsDeleted = false
        var course = await courseRepository.GetByIdAsync(command.Id, cancellationToken);

        if (course is null) throw new Exception($"Course with ID '{command.Id}' not found or already deleted.");
        if (await classSectionRepository.IsExistClassSectionByCourseIdAsync(course.Id, cancellationToken))
            throw new ResourceCannotBeDeletedException($"Course with ID '{command.Id}' can not be deleted.");

        await courseRepository.SoftDeleteAsync(command.Id, cancellationToken);

        return true;
    }
}
