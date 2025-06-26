using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;
public class Enrollment : AggregateRoot<Guid>
{
    private Enrollment() : base(Guid.Empty) { }
    private Enrollment(Guid id, Guid studentId, Guid scheduleId)
        : base(id)
    {
        StudentId = studentId;
        ScheduleId = scheduleId;
        EnrolledAt = DateTime.UtcNow;
    }
    public Guid StudentId { get; private set; }
    public Guid ScheduleId { get; private set; }
    public DateTime EnrolledAt { get; private set; }

    public static Enrollment Create(Guid studentId, Guid scheduleId)
    {
        return new Enrollment(Guid.NewGuid(), studentId, scheduleId);
    }
}
