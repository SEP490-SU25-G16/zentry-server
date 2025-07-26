using Zentry.SharedKernel.Domain;

namespace Zentry.SharedKernel.Enums.Attendance;

public class UserRequestStatus : Enumeration
{
    public static readonly UserRequestStatus Pending = new(1, nameof(Pending));
    public static readonly UserRequestStatus Approved = new(2, nameof(Approved));
    public static readonly UserRequestStatus Rejected = new(3, nameof(Rejected));

    private UserRequestStatus(int id, string name) : base(id, name)
    {
    }

    public static UserRequestStatus FromName(string name)
    {
        return FromName<UserRequestStatus>(name);
    }

    public static UserRequestStatus FromId(int id)
    {
        return FromId<UserRequestStatus>(id);
    }
}
