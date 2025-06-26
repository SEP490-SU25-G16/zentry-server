using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class Enrollment : AggregateRoot<Guid>
{
    private Enrollment() : base(Guid.Empty)
    {
    } // For EF Core

    private Enrollment(Guid enrollmentId, Guid studentId, Guid courseId) : base(enrollmentId)
    {
        StudentId = studentId != Guid.Empty
            ? studentId
            : throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));
        CourseId = courseId != Guid.Empty
            ? courseId
            : throw new ArgumentException("CourseId cannot be empty.", nameof(courseId));
    }

    public Guid StudentId { get; private set; }
    public Guid CourseId { get; private set; }

    // Navigation properties
    public ICollection<AttendanceRecord> AttendanceRecords { get; private set; } = new List<AttendanceRecord>();

    public static Enrollment Create(Guid studentId, Guid courseId)
    {
        return new Enrollment(Guid.NewGuid(), studentId, courseId);
    }
}
