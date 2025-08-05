using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.EndSession;

public class EndSessionCommand : ICommand<EndSessionResponse>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
}

public class EndSessionResponse
{
    public Guid SessionId { get; set; }
    public string Status { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ActualRoundsCompleted { get; set; } // Số round thực tế đã hoàn thành
    public int RoundsFinalized { get; set; } // Số round bị đánh dấu Finalized
}