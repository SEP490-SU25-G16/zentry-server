using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Enums;

public class AttendanceStatus : Enumeration
{
    public static readonly AttendanceStatus Present = new(1, "present");
    public static readonly AttendanceStatus Absent = new(2, "absent");
    private AttendanceStatus(int id, string name) : base(id, name) { }
    public static AttendanceStatus FromName(string name) => FromName<AttendanceStatus>(name);
    public static AttendanceStatus FromId(int id) => FromId<AttendanceStatus>(id);
}
