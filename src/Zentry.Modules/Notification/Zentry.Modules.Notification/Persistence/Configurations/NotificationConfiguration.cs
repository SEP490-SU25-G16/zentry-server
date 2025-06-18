using MongoDB.Driver;

namespace Zentry.Modules.Notification.Persistence.Configurations;

public static class NotificationConfiguration
{
    public static void Configure(IMongoDatabase database)
    {
        var indexKeys =
            Builders<Features.ReceiveAttendanceNotification.Notification>.IndexKeys.Ascending(n => n.SentAt);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
        var indexModel =
            new CreateIndexModel<Features.ReceiveAttendanceNotification.Notification>(indexKeys, indexOptions);

        var collection = database.GetCollection<Features.ReceiveAttendanceNotification.Notification>("Notifications");
        collection.Indexes.CreateOne(indexModel);
    }
}