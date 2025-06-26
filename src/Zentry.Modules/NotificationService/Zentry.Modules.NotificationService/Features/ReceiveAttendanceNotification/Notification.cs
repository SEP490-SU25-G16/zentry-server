using Zentry.Modules.NotificationService.Enums;

namespace Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotification;

public class Notification
{
    private Notification() { } // Private constructor for factory method
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType? Type { get; set; }
    public NotificationPriority? Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public static Notification Create(Guid userId, string title, string content, NotificationType type, NotificationPriority priority)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow,
            ReadAt = null
        };
    }

    public void MarkAsRead()
    {
        ReadAt = DateTime.UtcNow;
    }
}
