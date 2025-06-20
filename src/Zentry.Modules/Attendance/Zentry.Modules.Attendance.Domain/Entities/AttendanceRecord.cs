using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.Attendance.Domain.Entities;

public class AttendanceRecord : AggregateRoot
{
    private AttendanceRecord() : base(Guid.Empty)
    {
    } // For EF Core

    private AttendanceRecord(Guid attendanceRecordId, Guid enrollmentId, Guid roundId, bool isPresent)
        : base(attendanceRecordId)
    {
        AttendanceRecordId = attendanceRecordId;
        EnrollmentId = enrollmentId != Guid.Empty
            ? enrollmentId
            : throw new ArgumentException("EnrollmentId cannot be empty.", nameof(enrollmentId));
        RoundId = roundId != Guid.Empty
            ? roundId
            : throw new ArgumentException("RoundId cannot be empty.", nameof(roundId));
        IsPresent = isPresent;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid AttendanceRecordId { get; private set; }
    public Guid EnrollmentId { get; private set; }
    public Guid RoundId { get; private set; }
    public bool IsPresent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Enrollment? Enrollment { get; private set; }
    public Round? Round { get; private set; }

    public static AttendanceRecord Create(Guid enrollmentId, Guid roundId, bool isPresent)
    {
        return new AttendanceRecord(Guid.NewGuid(), enrollmentId, roundId, isPresent);
    }

    public void UpdatePresence(bool isPresent)
    {
        IsPresent = isPresent;
    }
}
