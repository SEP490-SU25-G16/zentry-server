using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Enums.Attendance;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;

public record CreateSessionCommand(
    Guid ScheduleId,
    Guid UserId
) : ICommand<CreateSessionResponse>;

public class CreateSessionResponse
{
    public Guid SessionId { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public SessionStatus Status { get; set; }
}