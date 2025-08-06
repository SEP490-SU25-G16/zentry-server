using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class AttendanceRecord : AggregateRoot<Guid>
{
    private AttendanceRecord() : base(Guid.Empty)
    {
    }

    private AttendanceRecord(Guid id, Guid userId, Guid sessionId, AttendanceStatus status, bool isManual,
        double percentageAttended)
        : base(id)
    {
        UserId = userId;
        SessionId = sessionId;
        Status = status;
        IsManual = isManual;
        PercentageAttended = percentageAttended;
        CreatedAt = DateTime.UtcNow;
        ExpiredAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid SessionId { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public bool IsManual { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiredAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public double PercentageAttended { get; private set; }

    public static AttendanceRecord Create(Guid userId, Guid sessionId, AttendanceStatus status, bool isManual,
        double percentageAttended)
    {
        return new AttendanceRecord(Guid.NewGuid(), userId, sessionId, status, isManual, percentageAttended);
    }

    public void Update(AttendanceStatus? status = null, bool? isManual = null, DateTime? expiredAt = null,
        double? percentageAttended = null)
    {
        if (status != null) Status = status;

        if (isManual.HasValue) IsManual = isManual.Value;
        if (expiredAt.HasValue) ExpiredAt = expiredAt.Value;
        if (percentageAttended.HasValue) PercentageAttended = percentageAttended.Value; // Cập nhật percentage
        UpdatedAt = DateTime.UtcNow;
    }
}