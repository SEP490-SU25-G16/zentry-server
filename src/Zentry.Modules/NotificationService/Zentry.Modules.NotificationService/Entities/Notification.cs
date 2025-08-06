using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.NotificationService.Entities;

/// <summary>
/// Đại diện cho một thông báo trong hệ thống.
/// </summary>
public class Notification : Entity<Guid>
{
    /// <summary>
    /// ID của người nhận.
    /// </summary>
    public Guid RecipientUserId { get; private set; }

    /// <summary>
    /// Tiêu đề thông báo.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Nội dung thông báo.
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// Thời gian tạo.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Trạng thái (đã đọc/chưa đọc).
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// Dữ liệu đi kèm (nếu có).
    /// </summary>
    public string? Data { get; private set; } // Stored as JSON string

    // Private constructor for EF Core
    private Notification() : base(Guid.NewGuid()) { }

    private Notification(
        Guid id,
        Guid recipientUserId,
        string title,
        string body,
        string? data) : base(id)
    {
        RecipientUserId = recipientUserId;
        Title = title;
        Body = body;
        Data = data;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Notification Create(
        Guid recipientUserId,
        string title,
        string body,
        Dictionary<string, string>? data)
    {
        var jsonData = data is not null
            ? System.Text.Json.JsonSerializer.Serialize(data)
            : null;

        return new Notification(Guid.NewGuid(), recipientUserId, title, body, jsonData);
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
