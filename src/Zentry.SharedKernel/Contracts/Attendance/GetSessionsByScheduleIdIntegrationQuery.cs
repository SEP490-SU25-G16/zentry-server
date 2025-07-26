using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Attendance;

public record GetSessionsByScheduleIdIntegrationQuery(Guid ScheduleId)
    : IQuery<List<GetSessionsByScheduleIdIntegrationResponse>>;

public class GetSessionsByScheduleIdIntegrationResponse
{
    public Guid SessionId { get; set; }
    public string Status { get; set; }
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}