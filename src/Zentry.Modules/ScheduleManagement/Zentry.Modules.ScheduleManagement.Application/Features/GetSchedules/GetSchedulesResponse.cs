using Zentry.Modules.ScheduleManagement.Application.Dtos;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;

public class GetSchedulesResponse
{
    public List<ScheduleDto> Schedules { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}