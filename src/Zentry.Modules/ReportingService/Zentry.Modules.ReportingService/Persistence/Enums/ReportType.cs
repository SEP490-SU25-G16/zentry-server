using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ReportingService.Persistence.Enums;

public class ReportType : Enumeration
{
    public static readonly ReportType Summary = new(1, "Summary");
    public static readonly ReportType Detailed = new(2, "Detailed");
    public static readonly ReportType Warning = new(3, "Warning");
    private ReportType(int id, string name) : base(id, name) { }
    public static ReportType FromName(string name) => FromName<ReportType>(name);
    public static ReportType FromId(int id) => FromId<ReportType>(id);
}
