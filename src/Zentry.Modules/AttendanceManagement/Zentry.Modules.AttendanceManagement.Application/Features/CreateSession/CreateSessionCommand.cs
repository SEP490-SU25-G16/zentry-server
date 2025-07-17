using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;

public record CreateSessionCommand(
    Guid ScheduleId,
    Guid UserId, // ID của giảng viên
    DateTime StartTime,
    DateTime EndTime
) : ICommand<CreateSessionResponse>;

public class CreateSessionResponse
{
    public Guid SessionId { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
