using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.Schedule.Domain.Entities;

public class Schedule : AggregateRoot
{
    public Guid ScheduleId { get; private set; } = Guid.NewGuid();
    public Guid CourseId { get; private set; }
    public Guid RoomId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public Course Course { get; private set; }
    public Room Room { get; private set; }

    private Schedule() : base(Guid.Empty)
    {
    } // For EF Core

    public Schedule(Guid scheduleId, Guid courseId, Guid roomId, DateTime startTime, DateTime endTime) :
        base(scheduleId)
    {
        ScheduleId  = scheduleId;
        CourseId = courseId != Guid.Empty
            ? courseId
            : throw new ArgumentException("CourseId cannot be empty.", nameof(courseId));
        RoomId = roomId != Guid.Empty ? roomId : throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));
        StartTime = startTime != default
            ? startTime
            : throw new ArgumentException("StartTime cannot be empty.", nameof(startTime));
        EndTime = endTime > startTime
            ? endTime
            : throw new ArgumentException("EndTime must be after StartTime.", nameof(endTime));
    }
    public void Update(Guid courseId, Guid roomId, DateTime startTime, DateTime endTime)
    {
        CourseId = courseId != Guid.Empty ? courseId : throw new ArgumentException("CourseId cannot be empty.", nameof(courseId));
        RoomId = roomId != Guid.Empty ? roomId : throw new ArgumentException("RoomId cannot be empty.", nameof(roomId));
        StartTime = startTime != default ? startTime : throw new ArgumentException("StartTime cannot be empty.", nameof(startTime));
        EndTime = endTime > startTime ? endTime : throw new ArgumentException("EndTime must be after StartTime.", nameof(endTime));
    }
}
