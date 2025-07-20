using Zentry.SharedKernel.Domain;
using Zentry.Modules.AttendanceManagement.Domain.Enums;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class Round : AggregateRoot<Guid>
{
    private Round() : base(Guid.Empty)
    {
    }

    private Round(Guid id, Guid sessionId, int roundNumber, DateTime startTime, RoundStatus status)
        : base(id)
    {
        SessionId = sessionId;
        RoundNumber = roundNumber;
        StartTime = startTime;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SessionId { get; private set; }
    public int RoundNumber { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public RoundStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static Round Create(Guid sessionId, int roundNumber, DateTime startTime)
    {
        return new Round(Guid.NewGuid(), sessionId, roundNumber, startTime, RoundStatus.Pending);
    }

    public void CompleteRound(DateTime endTime)
    {
        if (endTime <= StartTime)
        {
            throw new ArgumentException("EndTime must be after StartTime.");
        }
        EndTime = endTime;
        Status = RoundStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(RoundStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}
