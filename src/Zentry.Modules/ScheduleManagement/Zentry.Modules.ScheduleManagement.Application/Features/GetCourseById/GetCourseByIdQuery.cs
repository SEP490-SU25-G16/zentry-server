using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo using này có mặt

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetCourseById;

// Sử dụng record để tạo Query immutable
public record GetCourseByIdQuery(Guid Id)
    : IQuery<CourseDetailDto>; // Trả về CourseDetailDto hoặc null nếu không tìm thấy