using Zentry.SharedKernel.Domain;

namespace Zentry.SharedKernel.Enums.Attendance;

public class SessionStatus : Enumeration
{
    // Định nghĩa các trạng thái
    public static readonly SessionStatus Pending = new(1, nameof(Pending));
    public static readonly SessionStatus Active = new(2, nameof(Active));
    public static readonly SessionStatus Completed = new(3, nameof(Completed));
    public static readonly SessionStatus Cancelled = new(4, nameof(Cancelled));
    public static readonly SessionStatus Archived = new(5, nameof(Archived));

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
