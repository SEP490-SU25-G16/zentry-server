using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Enums;

public class EnrollmentStatus : Enumeration
{
    // Định nghĩa các trạng thái
    public static readonly EnrollmentStatus Active = new(1, "active");
    public static readonly EnrollmentStatus Cancelled = new(2, "cancelled");
    public static readonly EnrollmentStatus Completed = new(3, "completed");

    private EnrollmentStatus(int id, string name) : base(id, name)
    {
    }

    public static EnrollmentStatus FromName(string name)
    {
        return FromName<EnrollmentStatus>(name);
    }

    public static EnrollmentStatus FromId(int id)
    {
        return FromId<EnrollmentStatus>(id);
    }
}
