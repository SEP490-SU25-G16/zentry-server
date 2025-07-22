using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.DeviceManagement.Enums;

public class DeviceStatus : Enumeration
{
    public static readonly DeviceStatus Active = new(1, nameof(Active));
    public static readonly DeviceStatus Inactive = new(2, nameof(Inactive));

    private DeviceStatus(int id, string name) : base(id, name)
    {
    }

    public static DeviceStatus FromName(string name)
    {
        return FromName<DeviceStatus>(name);
    }

    public static DeviceStatus FromId(int id)
    {
        return FromId<DeviceStatus>(id);
    }
}
