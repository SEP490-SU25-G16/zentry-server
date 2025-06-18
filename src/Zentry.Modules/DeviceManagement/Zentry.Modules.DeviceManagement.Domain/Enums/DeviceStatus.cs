using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.DeviceManagement.Domain.Enums;

public class DeviceStatus : Enumeration
{
    public static readonly DeviceStatus Active = new(1, nameof(Active));
    public static readonly DeviceStatus Inactive = new(2, nameof(Inactive));

    private DeviceStatus(int id, string name) : base(id, name)
    {
    }

    public static DeviceStatus FromName(string name)
    {
        var status = GetAll<DeviceStatus>()
            .FirstOrDefault(s => s.ToString().Equals(name, StringComparison.OrdinalIgnoreCase));
        if (status == null)
            throw new ArgumentException($"Invalid status name: {name}");
        return status;
    }
}