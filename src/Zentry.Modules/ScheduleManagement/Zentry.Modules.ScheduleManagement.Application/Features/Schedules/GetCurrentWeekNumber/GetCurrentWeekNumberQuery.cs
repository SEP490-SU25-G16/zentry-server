using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetTermWeek;

public record GetCurrentWeekNumberQuery(Guid ClassSectionId, DateOnly Date)
    : IQuery<GetCurrentWeekNumberResponse>;

public class GetCurrentWeekNumberResponse
{
    public Guid ClassSectionId { get; set; }
    public DateOnly QueryDate { get; set; }
    public DateOnly? EarliestStartDate { get; set; }
    public int? WeekNumber { get; set; }
    public bool IsInSession { get; set; }
    public string Message { get; set; } = string.Empty;
}
