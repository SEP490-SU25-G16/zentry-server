using Zentry.SharedKernel.Domain;

namespace Zentry.SharedKernel.Constants.Attendance;

public class AttendanceStatus : Enumeration
{
    public static readonly AttendanceStatus Present = new(1, nameof(Present));
    public static readonly AttendanceStatus Absent = new(2, nameof(Absent));

    private AttendanceStatus(int id, string name) : base(id, name)
    {
    }

    public static AttendanceStatus FromName(string name)
    {
        return FromName<AttendanceStatus>(name);
    }

    public static AttendanceStatus FromId(int id)
    {
        return FromId<AttendanceStatus>(id);
    }
}
