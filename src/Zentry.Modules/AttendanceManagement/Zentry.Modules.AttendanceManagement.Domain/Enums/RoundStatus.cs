using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Enums;

public class RoundStatus : Enumeration
{
    public static readonly RoundStatus Pending = new(1, "pending");
    public static readonly RoundStatus Active = new(2, "active");
    public static readonly RoundStatus Completed = new(3, "completed");
    public static readonly RoundStatus Finalized = new(4, "finalized");
    public static readonly RoundStatus Cancelled = new(5, "cancelled");

    private RoundStatus(int id, string name) : base(id, name)
    {
    }

    public static RoundStatus FromName(string name)
    {
        return FromName<RoundStatus>(name);
    }

    public static RoundStatus FromId(int id)
    {
        return FromId<RoundStatus>(id);
    }
}