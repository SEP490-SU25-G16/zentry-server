using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class Schedule : AggregateRoot<Guid>
{
    private Schedule() : base(Guid.Empty) {}

    private Schedule(Guid id, Guid classSectionId, Guid roomId, DateTime startTime, DateTime endTime, DayOfWeekEnum dayOfWeek)
        : base(id)
    {
        ClassSectionId = classSectionId;
        RoomId = roomId;
        StartTime = startTime;
        EndTime = endTime;
        DayOfWeek = dayOfWeek;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ClassSectionId { get; private set; }
    public Guid RoomId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DayOfWeekEnum DayOfWeek { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Room? Room { get; private set; }
    public virtual ClassSection? ClassSection { get; private set; }

    public static Schedule Create(Guid classSectionId, Guid roomId, DateTime startTime, DateTime endTime, DayOfWeekEnum dayOfWeek)
    {
        return new Schedule(Guid.NewGuid(), classSectionId, roomId, startTime, endTime, dayOfWeek);
    }

    public void Update(Guid? roomId = null, DateTime? startTime = null, DateTime? endTime = null, DayOfWeekEnum? dayOfWeek = null)
    {
        if (roomId.HasValue) RoomId = roomId.Value;
        if (startTime.HasValue) StartTime = startTime.Value;
        if (endTime.HasValue) EndTime = endTime.Value;
        if (dayOfWeek != null) DayOfWeek = dayOfWeek;
        UpdatedAt = DateTime.UtcNow;
    }
}
