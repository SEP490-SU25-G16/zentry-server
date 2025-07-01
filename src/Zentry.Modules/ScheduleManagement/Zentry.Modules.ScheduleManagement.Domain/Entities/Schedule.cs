using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class Schedule : AggregateRoot<Guid>
{
    private Schedule() : base(Guid.Empty)
    {
    }

    private Schedule(Guid id, Guid lecturerId, Guid courseId, Guid roomId, DateTime startTime, DateTime endTime,
        DayOfWeekEnum dayOfWeek)
        : base(id)
    {
        LecturerId = lecturerId;
        CourseId = courseId;
        RoomId = roomId;
        StartTime = startTime;
        EndTime = endTime;
        DayOfWeek = dayOfWeek;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid LecturerId { get; private set; }
    public Guid CourseId { get; private set; }
    public Guid RoomId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DayOfWeekEnum DayOfWeek { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual Course? Course { get; private set; }
    public virtual Room? Room { get; private set; }

    public static Schedule Create(Guid lecturerId, Guid courseId, Guid roomId, DateTime startTime, DateTime endTime,
        DayOfWeekEnum dayOfWeek)
    {
        return new Schedule(Guid.NewGuid(), lecturerId, courseId, roomId, startTime, endTime, dayOfWeek);
    }

    public void Update(Guid? lecturerId = null, Guid? courseId = null, Guid? roomId = null, DateTime? startTime = null,
        DateTime? endTime = null, DayOfWeekEnum? dayOfWeek = null)
    {
        if (lecturerId.HasValue) LecturerId = lecturerId.Value;
        if (courseId.HasValue) CourseId = courseId.Value;
        if (roomId.HasValue) RoomId = roomId.Value;
        if (startTime.HasValue) StartTime = startTime.Value;
        if (endTime.HasValue) EndTime = endTime.Value;
        if (dayOfWeek != null) DayOfWeek = dayOfWeek;
        UpdatedAt = DateTime.UtcNow;
    }
}