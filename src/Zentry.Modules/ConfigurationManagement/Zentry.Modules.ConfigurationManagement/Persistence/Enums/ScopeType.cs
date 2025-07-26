using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Persistence.Enums;

public class ScopeType : Enumeration
{
    public static readonly ScopeType Global = new(1, nameof(Global));
    public static readonly ScopeType Course = new(2, nameof(Course));
    public static readonly ScopeType Session = new(4, nameof(Session));

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
