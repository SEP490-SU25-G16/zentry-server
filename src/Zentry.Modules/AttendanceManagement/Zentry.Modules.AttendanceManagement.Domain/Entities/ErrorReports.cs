using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class ErrorReport : AggregateRoot
{
    private ErrorReport() : base(Guid.Empty)
    {
    } // For EF Core

    private ErrorReport(Guid errorReportId, Guid deviceId, string errorCode, string? description)
        : base(errorReportId)
    {
        ErrorReportId = errorReportId;
        DeviceId = deviceId != Guid.Empty
            ? deviceId
            : throw new ArgumentException("DeviceId cannot be empty.", nameof(deviceId));
        ErrorCode = !string.IsNullOrWhiteSpace(errorCode)
            ? errorCode
            : throw new ArgumentException("ErrorCode cannot be empty.", nameof(errorCode));
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ErrorReportId { get; private set; }
    public Guid DeviceId { get; private set; }
    public string ErrorCode { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static ErrorReport Create(Guid deviceId, string errorCode, string? description = null)
    {
        return new ErrorReport(Guid.NewGuid(), deviceId, errorCode, description);
    }
}