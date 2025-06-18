using MongoDB.Driver;

// Thêm using System; để sử dụng Guid

namespace Zentry.Modules.Notification.Persistence.Data;

public static class NotificationSeed
{
    public static void SeedData(IMongoDatabase database)
    {
        var collection = database.GetCollection<Features.ReceiveAttendanceNotification.Notification>("Notifications");

        // Kiểm tra nếu collection đã có dữ liệu
        if (collection.CountDocuments(FilterDefinition<Features.ReceiveAttendanceNotification.Notification>.Empty) >
            0) return;

        var notifications = new List<Features.ReceiveAttendanceNotification.Notification>
        {
            new()
            {
                NotificationId = Guid.NewGuid(),
                // Sửa lỗi Guid ở đây: thêm đủ ký tự để thành 32 chữ số
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Message = "Attendance rate below 70% for course CS101.",
                Type = "in_app",
                SentAt = DateTime.Parse("2025-06-18T10:00:00Z")
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Message = "Error in session #123: Device not found.",
                Type = "email",
                SentAt = DateTime.Parse("2025-06-18T11:00:00Z")
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                // Sửa lỗi Guid ở đây: thêm đủ ký tự để thành 32 chữ số và các dấu gạch nối
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Message = "Your attendance for MATH202 is confirmed.",
                Type = "in_app",
                SentAt = DateTime.Parse("2025-06-18T12:00:00Z")
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Message = "Error in session #124: BLE signal lost.",
                SentAt = DateTime.Parse("2025-06-18T13:00:00Z")
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Message = "Reminder: Attend session #125 tomorrow.",
                Type = "email",
                SentAt = DateTime.Parse("2025-06-18T14:00:00Z")
            }
        };

        collection.InsertMany(notifications);
    }
}