using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateCourse;

// Record để tạo Command immutable. Id là từ URL, các trường còn lại từ Body.
public record UpdateCourseCommand(
    Guid Id, // ID của khóa học cần cập nhật
    string Name,
    string Description,
    string Semester
) : ICommand<CourseDetailDto>; // Trả về DTO của khóa học đã cập nhật