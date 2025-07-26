using Zentry.SharedKernel.Domain;
using Zentry.SharedKernel.Enums.Attendance;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class UserRequest : AggregateRoot<Guid>
{
    private UserRequest() : base(Guid.Empty)
    {
    }

    private UserRequest(Guid id, Guid requestedByUserId, Guid targetUserId, string requestType, Guid relatedEntityId,
        string reason)
        : base(id)
    {
        RequestedByUserId = requestedByUserId;
        TargetUserId = targetUserId;
        RequestType = requestType;
        RelatedEntityId = relatedEntityId;
        Status = UserRequestStatus.Pending;
        Reason = reason;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid RequestedByUserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public string RequestType { get; private set; }
    public Guid RelatedEntityId { get; private set; }
    public UserRequestStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public static UserRequest Create(Guid requestedByUserId, Guid targetUserId, string requestType,
        Guid relatedEntityId, string reason)
    {
        return new UserRequest(Guid.NewGuid(), requestedByUserId, targetUserId, requestType, relatedEntityId, reason);
    }

    public void Approve()
    {
        Status = UserRequestStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = UserRequestStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }
}