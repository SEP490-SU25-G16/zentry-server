// File: Zentry.Modules.ScheduleManagement.Domain.Entities/Enrollment.cs
using Zentry.Modules.ScheduleManagement.Domain.Enums; // Đảm bảo import namespace này
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class Enrollment : AggregateRoot<Guid>
{
    private Enrollment() : base(Guid.Empty)
    {
    }

    private Enrollment(Guid id, Guid studentId, Guid scheduleId)
        : base(id)
    {
        StudentId = studentId;
        ScheduleId = scheduleId;
        EnrolledAt = DateTime.UtcNow;
        Status = EnrollmentStatus.Active;
    }

    public Guid StudentId { get; private set; }
    public Guid ScheduleId { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public EnrollmentStatus Status { get; private set; }

    public virtual Schedule? Schedule { get; private set; }

    public static Enrollment Create(Guid studentId, Guid scheduleId)
    {
        return new Enrollment(Guid.NewGuid(), studentId, scheduleId);
    }

    public void CancelEnrollment()
    {
        Status = EnrollmentStatus.Cancelled;
    }
}
