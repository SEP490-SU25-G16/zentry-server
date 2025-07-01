using Zentry.Modules.ScheduleManagement.Application.Dtos;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;

public class GetEnrollmentsResponse
{
    public List<EnrollmentListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}