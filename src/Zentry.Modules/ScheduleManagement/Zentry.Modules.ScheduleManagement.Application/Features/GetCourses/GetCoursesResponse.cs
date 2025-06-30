// File: Zentry.Modules.ScheduleManagement.Application/Features/GetCourses/GetCoursesResponse.cs

using Zentry.Modules.ScheduleManagement.Application.Dtos;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetCourses;

public class GetCoursesResponse
{
    public List<CourseListItemDto> Items { get; set; } = new List<CourseListItemDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}
