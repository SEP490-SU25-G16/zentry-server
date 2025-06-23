namespace Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotificationService;

public record Notification
{
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
}