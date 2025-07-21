using Zentry.Modules.ScheduleManagement.Domain.Enums;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;

public class ScheduleListCriteria
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? LecturerId { get; set; }
    public Guid? ClassSectionId { get; set; }
    public Guid? RoomId { get; set; }
    public WeekDayEnum? WeekDay { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
}