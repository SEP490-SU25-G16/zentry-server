using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetCourseById;

public class GetCourseByIdQueryHandler(ICourseRepository courseRepository)
    : IQueryHandler<GetCourseByIdQuery, CourseDetailDto?>
{
    public async Task<CourseDetailDto?> Handle(GetCourseByIdQuery query, CancellationToken cancellationToken)
    {
        // 1. Lấy Course Entity từ Repository
        var course = await courseRepository.GetByIdAsync(query.Id, cancellationToken);

        // 2. Kiểm tra nếu không tìm thấy
        if (course == null)
            // Có thể ném một ngoại lệ NotFoundException để middleware xử lý thành 404
            throw new Exception($"Course with ID '{query.Id}' not found.");
        // Hoặc đơn giản là trả về null và Controller sẽ xử lý thành NotFound()
        // return null;
        // 3. Ánh xạ từ Domain Entity sang DTO để trả về
        var courseDetailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Description = course.Description,
            Semester = course.Semester,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };

        return courseDetailDto;
    }
}