using Zentry.SharedKernel.Domain;
using Zentry.SharedKernel.Enums.Attendance;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class AttendanceRecord : AggregateRoot<Guid>
{
    private AttendanceRecord() : base(Guid.Empty)
    {
    }

    private AttendanceRecord(Guid id, Guid userId, Guid sessionId, AttendanceStatus status, bool isManual)
        : base(id)
    {
        UserId = userId;
        SessionId = sessionId;
        Status = status;
        IsManual = isManual;
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

    public static AttendanceRecord Create(Guid userId, Guid sessionId, AttendanceStatus status, bool isManual)
    {
        return new AttendanceRecord(Guid.NewGuid(), userId, sessionId, status, isManual);
    }

    public void Update(AttendanceStatus? status = null, bool? isManual = null, DateTime? expiredAt = null)
    {
        if (status != null)
        {
            Status = status;
        }

        if (isManual.HasValue) IsManual = isManual.Value;
        if (expiredAt.HasValue) ExpiredAt = expiredAt.Value;
        UpdatedAt = DateTime.UtcNow;
    }
}
