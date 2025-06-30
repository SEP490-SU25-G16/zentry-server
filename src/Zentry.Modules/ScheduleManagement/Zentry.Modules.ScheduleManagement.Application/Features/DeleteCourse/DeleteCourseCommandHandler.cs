using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteCourse;

public class DeleteCourseCommandHandler(ICourseRepository courseRepository) : ICommandHandler<DeleteCourseCommand, bool>
{
    public async Task<bool> Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        // GetByIdAsync sẽ chỉ trả về Course nếu IsDeleted = false
        var course = await courseRepository.GetByIdAsync(command.Id, cancellationToken);

        if (course == null) throw new Exception($"Course with ID '{command.Id}' not found or already deleted.");

        // Gọi phương thức soft delete trên repository
        await courseRepository.SoftDeleteAsync(command.Id, cancellationToken);
        await courseRepository.SaveChangesAsync(cancellationToken);

        return true; // Trả về true để xác nhận xóa mềm thành công
    }
}
