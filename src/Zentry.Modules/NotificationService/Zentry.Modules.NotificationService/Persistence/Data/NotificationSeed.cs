using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Zentry.Modules.NotificationService.Enums;
using Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotification;

namespace Zentry.Modules.NotificationService.Persistence.Data;

public static class NotificationSeed
{
    private static readonly Guid User1Id = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid User2Id = new("10000000-0000-0000-0000-000000000002");
    private static readonly Guid User3Id = new("10000000-0000-0000-0000-000000000003");

    public static void SeedData(IMongoDatabase database)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        var collection = database.GetCollection<Notification>("Notifications");

        if (collection.CountDocuments(FilterDefinition<Notification>.Empty) > 0)
            return;

        var notifications = new List<Notification>
        {
            Notification.Create( // Sử dụng Create method
                User1Id,
                "Attendance Alert for CS101", // Title
                "Attendance rate below 70% for course CS101. Please check student progress.", // Content
                NotificationType.InApp, // Type (Enumeration)
                NotificationPriority.High // Priority (Enumeration)
            ),
            Notification.Create(
                User1Id,
                "Session Error Notification",
                "Error in session #123: Device not found. Action required.",
                NotificationType.Email,
                NotificationPriority.High
            ),
            Notification.Create(
                User2Id,
                "Attendance Confirmation",
                "Your attendance for MATH202 is confirmed. Thank you.",
                NotificationType.InApp,
                NotificationPriority.Normal
            ),
            Notification.Create(
                User2Id,
                "Bluetooth Issue Detected",
                "Error in session #124: BLE signal lost. Please re-check device connection.",
                NotificationType.Push,
                NotificationPriority.Normal
            ),
            Notification.Create(
                User3Id,
                "Session Reminder",
                "Reminder: Attend session #125 tomorrow at 09:00 AM.",
                NotificationType.Email,
                NotificationPriority.Low
            )
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