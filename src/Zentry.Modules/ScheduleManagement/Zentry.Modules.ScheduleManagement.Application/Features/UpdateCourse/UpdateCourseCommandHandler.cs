using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateCourse;

public class UpdateCourseCommandHandler : ICommandHandler<UpdateCourseCommand, CourseDetailDto>
{
    private readonly ICourseRepository _courseRepository;

    public UpdateCourseCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<CourseDetailDto> Handle(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        // 1. Tìm khóa học trong database
        var course = await _courseRepository.GetByIdAsync(command.Id, cancellationToken);

        // 2. Kiểm tra nếu không tìm thấy
        if (course == null) throw new Exception($"Course with ID '{command.Id}' not found.");

        // 3. Áp dụng các thay đổi cho Domain Entity
        // Lưu ý: Code thường không được thay đổi. Nếu cần thay đổi Code, cần thêm logic kiểm tra trùng lặp.
        // Hiện tại, giả định Code là immutable sau khi tạo.
        course.Update(
            command.Name,
            command.Description,
            command.Semester
        );

        // 4. Lưu các thay đổi vào database
        _courseRepository.Update(course); // Entity Framework sẽ theo dõi và cập nhật
        await _courseRepository.SaveChangesAsync(cancellationToken);

        // 5. Ánh xạ từ Domain Entity đã cập nhật sang DTO để trả về
        var responseDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code, // Code không thay đổi
            Name = course.Name,
            Description = course.Description,
            Semester = course.Semester,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt // UpdatedAt sẽ được cập nhật bởi Entity.Update()
        };

        return responseDto;
    }
}