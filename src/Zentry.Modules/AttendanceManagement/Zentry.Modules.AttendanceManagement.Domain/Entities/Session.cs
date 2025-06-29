using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class Session : AggregateRoot<Guid>
{
    private Session() : base(Guid.Empty)
    {
    }

    private Session(Guid id, Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime)
        : base(id)
    {
        ScheduleId = scheduleId;
        UserId = userId;
        StartTime = startTime;
        EndTime = endTime;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ScheduleId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Session Create(Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime)
    {
        return new Session(Guid.NewGuid(), scheduleId, userId, startTime, endTime);
    }

    public void Update(DateTime? startTime = null, DateTime? endTime = null)
    {
        if (startTime.HasValue) StartTime = startTime.Value;
        if (endTime.HasValue) EndTime = endTime.Value;
    }
}