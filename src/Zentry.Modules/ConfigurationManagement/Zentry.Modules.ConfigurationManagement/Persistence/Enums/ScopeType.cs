using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Persistence.Enums;

public class ScopeType : Enumeration
{
    public static readonly ScopeType GLOBAL = new(1, nameof(GLOBAL));
    public static readonly ScopeType COURSE = new(2, nameof(COURSE));
    public static readonly ScopeType USER = new(3, nameof(USER));
    public static readonly ScopeType SESSION = new(4, nameof(SESSION));
    public static readonly ScopeType DEVICE = new(5, nameof(DEVICE));

    private ScopeType(int id, string name) : base(id, name)
    {
    }

    public static ScopeType FromName(string name)
    {
        return FromName<ScopeType>(name);
    }

    public static ScopeType FromId(int id)
    {
        return FromId<ScopeType>(id);
    }
}