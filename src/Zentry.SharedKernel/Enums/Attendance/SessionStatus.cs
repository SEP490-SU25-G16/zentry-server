using Zentry.SharedKernel.Domain;

namespace Zentry.SharedKernel.Enums.Attendance;

public class SessionStatus : Enumeration
{
    // Định nghĩa các trạng thái
    public static readonly SessionStatus Pending = new(1, "PENDING");
    public static readonly SessionStatus Active = new(2, "ACTIVE");
    public static readonly SessionStatus Completed = new(3, "COMPLETED");
    public static readonly SessionStatus Cancelled = new(4, "CANCELLED");
    public static readonly SessionStatus Archived = new(5, "ARCHIVED");

    private SessionStatus(int id, string name) : base(id, name)
    {
    }

    public static SessionStatus FromName(string name)
    {
        return FromName<SessionStatus>(name);
    }

    public static SessionStatus FromId(int id)
    {
        return FromId<SessionStatus>(id);
    }
}
