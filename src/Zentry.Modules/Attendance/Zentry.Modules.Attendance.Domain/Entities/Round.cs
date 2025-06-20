using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.Attendance.Domain.Entities;

public class Round : AggregateRoot
{
    private Round() : base(Guid.Empty) { } // For EF Core

    private Round(Guid roundId, Guid scheduleId, DateTime startTime, DateTime endTime)
        : base(roundId)
    {
        RoundId = roundId;
        ScheduleId = scheduleId != Guid.Empty
            ? scheduleId
            : throw new ArgumentException("ScheduleId cannot be empty.", nameof(scheduleId));
        StartTime = startTime > DateTime.MinValue
            ? startTime
            : throw new ArgumentException("StartTime must be valid.", nameof(startTime));
        EndTime = endTime > startTime
            ? endTime
            : throw new ArgumentException("EndTime must be after StartTime.", nameof(endTime));
    }

    public Guid RoundId { get; private set; }
    public Guid ScheduleId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    // Navigation properties
    public ICollection<AttendanceRecord> AttendanceRecords { get; private set; } = new List<AttendanceRecord>();

    public static Round Create(Guid scheduleId, DateTime startTime, DateTime endTime)
    {
        return new Round(Guid.NewGuid(), scheduleId, startTime, endTime);
    }

    public void UpdateSchedule(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be after StartTime.");

        StartTime = startTime;
        EndTime = endTime;
    }
}
