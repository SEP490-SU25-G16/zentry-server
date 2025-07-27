using Zentry.SharedKernel.Constants.Reporting;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ReportingService.Persistence.Entities;

public class AttendanceReport : AggregateRoot<Guid>
{
    private AttendanceReport() : base(Guid.Empty)
    {
    }

    private AttendanceReport(Guid id, Guid scopeId, ReportScopeType scopeType, ReportType reportType,
        string reportContent, Guid? createdBy = null)
        : base(id)
    {
        ScopeId = scopeId;
        ScopeType = scopeType;
        ReportType = reportType;
        ReportContent = reportContent;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public Guid ScopeId { get; private set; }
    public ReportScopeType ScopeType { get; private set; }
    public ReportType ReportType { get; private set; }
    public string ReportContent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ExpiredAt { get; private set; }

    public static AttendanceReport Create(Guid scopeId, ReportScopeType scopeType, ReportType reportType,
        string reportContent, Guid? createdBy = null)
    {
        return new AttendanceReport(Guid.NewGuid(), scopeId, scopeType, reportType, reportContent, createdBy);
    }

    public void UpdateContent(string newContent)
    {
        ReportContent = newContent;
    }
}