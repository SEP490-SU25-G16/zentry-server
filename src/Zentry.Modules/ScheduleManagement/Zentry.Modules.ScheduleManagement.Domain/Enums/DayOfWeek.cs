using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Enums;

public class DayOfWeekEnum : Enumeration
{
    public static readonly DayOfWeekEnum Monday = new(1, nameof(Monday));
    public static readonly DayOfWeekEnum Tuesday = new(2, nameof(Tuesday));
    public static readonly DayOfWeekEnum Wednesday = new(3, nameof(Wednesday));
    public static readonly DayOfWeekEnum Thursday = new(4, nameof(Thursday));
    public static readonly DayOfWeekEnum Friday = new(5, nameof(Friday));
    public static readonly DayOfWeekEnum Saturday = new(6, nameof(Saturday));
    public static readonly DayOfWeekEnum Sunday = new(7, nameof(Sunday));

    private DayOfWeekEnum(int id, string name) : base(id, name)
    {
    }

    public static DayOfWeekEnum FromName(string name)
    {
        return FromName<DayOfWeekEnum>(name);
    }

    public static DayOfWeekEnum FromId(int id)
    {
        return FromId<DayOfWeekEnum>(id);
    }
}