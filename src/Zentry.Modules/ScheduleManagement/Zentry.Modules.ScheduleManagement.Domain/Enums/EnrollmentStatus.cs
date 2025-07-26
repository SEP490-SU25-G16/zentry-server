using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Enums;

public class EnrollmentStatus : Enumeration
{
    // Định nghĩa các trạng thái
    public static readonly EnrollmentStatus Active = new(1, "ACTIVE");
    public static readonly EnrollmentStatus Cancelled = new(2, "CANCELLED");
    public static readonly EnrollmentStatus Completed = new(3, "COMPLETED");

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
