using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;

public class GetEnrollmentsQuery : ICommand<GetEnrollmentsResponse>
{
    public Guid AdminId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? ScheduleId { get; set; }
    public Guid? CourseId { get; set; }
    public EnrollmentStatus? Status { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }

    public GetEnrollmentsQuery(GetEnrollmentsRequest request)
    {
        PageNumber = request.PageNumber;
        PageSize = request.PageSize;
        SearchTerm = request.SearchTerm;
        StudentId = request.StudentId;
        ScheduleId = request.ScheduleId;
        CourseId = request.CourseId;
        SortBy = request.SortBy;
        SortOrder = request.SortOrder;
        Status = ParseEnrollmentStatus(request.Status);
    }

    private EnrollmentStatus? ParseEnrollmentStatus(string? statusString)
    {
        if (string.IsNullOrWhiteSpace(statusString))
        {
            return null;
        }

        if (Enum.TryParse<Domain.Enums.EnrollmentStatus>(statusString, true, out var status))
        {
            return status;
        }

        return null;
    }
}
