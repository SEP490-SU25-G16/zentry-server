using MongoDB.Driver;

namespace Zentry.Modules.NotificationService.Persistence.Configurations;

public static class NotificationConfiguration
{
    public static void Configure(IMongoDatabase database)
    {
        var indexKeys =
            Builders<Features.ReceiveAttendanceNotificationService.Notification>.IndexKeys.Ascending(n => n.SentAt);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
        var indexModel =
            new CreateIndexModel<Features.ReceiveAttendanceNotificationService.Notification>(indexKeys, indexOptions);

        var collection = database.GetCollection<Features.ReceiveAttendanceNotificationService.Notification>("Notifications");
        collection.Indexes.CreateOne(indexModel);
    }
}