using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;

namespace Zentry.Modules.AttendanceManagement.Application.Features.EndSession;

public class EndSessionCommand : ICommand<EndSessionResponse>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
}

public class EndSessionResponse
{
    public Guid SessionId { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime EndTime { get; set; } // Thời điểm kết thúc thực tế
    public DateTime? UpdatedAt { get; set; }
}

