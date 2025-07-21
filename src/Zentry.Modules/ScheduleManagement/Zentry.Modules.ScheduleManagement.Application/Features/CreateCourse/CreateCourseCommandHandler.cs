using System.Data;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateCourse;

public class CreateCourseCommandHandler(ICourseRepository courseRepository)
    : ICommandHandler<CreateCourseCommand, CourseCreatedResponse>
{
    public async Task<CourseCreatedResponse> Handle(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        // 1. Business Rule: Code khóa học phải là duy nhất
        var isCodeUnique = await courseRepository.IsCodeUniqueAsync(command.Code, cancellationToken);
        if (!isCodeUnique) throw new DuplicateNameException($"Course with code '{command.Code}' already exists.");

        // 2. Tạo đối tượng Course Domain Entity
        var course = Course.Create(
            command.Code,
            command.Name,
            command.Description
        );

        // 3. Lưu vào cơ sở dữ liệu thông qua Repository
        await courseRepository.AddAsync(course, cancellationToken);
        await courseRepository.SaveChangesAsync(cancellationToken);

        // 4. Ánh xạ từ Domain Entity sang DTO để trả về
        var responseDto = new CourseCreatedResponse
        {
            Id = course.Id,
            Name = course.Name,
            Code = course.Code,
            Description = course.Description,
        };

        return responseDto;
    }
}
