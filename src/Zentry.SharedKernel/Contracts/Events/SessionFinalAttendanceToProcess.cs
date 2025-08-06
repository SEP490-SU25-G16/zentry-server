namespace Zentry.SharedKernel.Contracts.Events;

public class SessionFinalAttendanceToProcess
{
    public Guid SessionId { get; set; }
    public int ActualRoundsCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}