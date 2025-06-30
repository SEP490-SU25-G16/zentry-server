using System.ComponentModel.DataAnnotations;
using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;

// Đảm bảo import DayOfWeekEnum

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;

public record GetSchedulesQuery(
    // Pagination
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
    int PageNumber = 1,
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
    int PageSize = 10,

    // Filtering
    Guid? LecturerId = null,
    Guid? CourseId = null,
    Guid? RoomId = null,
    DayOfWeekEnum? DayOfWeek = null,
    // Optional: DateTime StartTimeFilter? // Nếu muốn lọc theo khoảng thời gian cụ thể
    // Optional: DateTime EndTimeFilter?

    // Searching
    string? SearchTerm = null, // Có thể tìm kiếm theo tên giảng viên, khóa học, phòng

    // Sorting
    string? SortBy = null, // e.g., "StartTime", "DayOfWeek", "LecturerName"
    string? SortOrder = "asc" // "asc" or "desc"
) : IQuery<GetSchedulesResponse>;
