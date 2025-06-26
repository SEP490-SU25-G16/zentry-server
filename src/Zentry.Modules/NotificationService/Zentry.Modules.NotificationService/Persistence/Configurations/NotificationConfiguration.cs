using MongoDB.Driver;
using Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotification;

namespace Zentry.Modules.NotificationService.Persistence.Configurations;

public static class NotificationConfiguration
{
    public static void Configure(IMongoDatabase database)
    {
        var indexKeys =
            Builders<Notification>.IndexKeys.Ascending(n => n.SentAt);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
        var indexModel =
            new CreateIndexModel<Notification>(indexKeys, indexOptions);

        var collection = database.GetCollection<Notification>("Notifications");
        collection.Indexes.CreateOne(indexModel);
    }
}