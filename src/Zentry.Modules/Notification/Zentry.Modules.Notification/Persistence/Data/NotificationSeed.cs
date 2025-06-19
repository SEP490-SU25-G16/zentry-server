using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Zentry.Modules.Notification.Persistence.Data;

public static class NotificationSeed
{
    private static readonly Guid User1Id = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid User2Id = new("10000000-0000-0000-0000-000000000002");
    private static readonly Guid User3Id = new("10000000-0000-0000-0000-000000000003");

    public static void SeedData(IMongoDatabase database)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        var collection = database.GetCollection<Features.ReceiveAttendanceNotification.Notification>("Notifications");

        // Kiểm tra nếu collection đã có dữ liệu
        if (collection.CountDocuments(FilterDefinition<Features.ReceiveAttendanceNotification.Notification>.Empty) > 0)
            return;

        var notifications = new List<Features.ReceiveAttendanceNotification.Notification>
        {
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = User1Id,
                Message = "Attendance rate below 70% for course CS101.",
                Type = "in_app",
                SentAt = new DateTime(2025, 6, 18, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = User1Id,
                Message = "Error in session #123: Device not found.",
                Type = "email",
                SentAt = new DateTime(2025, 6, 18, 11, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = User2Id,
                Message = "Your attendance for MATH202 is confirmed.",
                Type = "in_app",
                SentAt = new DateTime(2025, 6, 18, 12, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = User2Id,
                Message = "Error in session #124: BLE signal lost.",
                Type = "push",
                SentAt = new DateTime(2025, 6, 18, 13, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                NotificationId = Guid.NewGuid(),
                UserId = User3Id,
                Message = "Reminder: Attend session #125 tomorrow.",
                Type = "email",
                SentAt = new DateTime(2025, 6, 18, 14, 0, 0, DateTimeKind.Utc)
            }
        };

        try
        {
            collection.InsertMany(notifications);
            Console.WriteLine("Notification seed data inserted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding notification data: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
