using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.UserManagement.Persistence.Enums;

public class UserRequestStatus : Enumeration
{
    public static readonly UserRequestStatus PENDING = new(1, nameof(PENDING));
    public static readonly UserRequestStatus APPROVED = new(2, nameof(APPROVED));
    public static readonly UserRequestStatus REJECTED = new(3, nameof(REJECTED));
    private UserRequestStatus(int id, string name) : base(id, name) { }
    public static UserRequestStatus FromName(string name) => FromName<UserRequestStatus>(name);
    public static UserRequestStatus FromId(int id) => FromId<UserRequestStatus>(id);
}
