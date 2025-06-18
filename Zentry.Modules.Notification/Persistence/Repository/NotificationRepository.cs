using MongoDB.Driver;

namespace Zentry.Modules.Notification.Persistence.Repository;

public class NotificationRepository(IMongoDatabase database) : INotificationRepository
{
    private readonly IMongoCollection<Features.ReceiveAttendanceNotification.Notification> _collection =
        database.GetCollection<Features.ReceiveAttendanceNotification.Notification>("Notifications");

    public async Task AddAsync(Features.ReceiveAttendanceNotification.Notification notification,
        CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(notification, null, cancellationToken);
    }

    public async Task<List<Features.ReceiveAttendanceNotification.Notification>> GetByUserIdAsync(Guid userId, int skip,
        int take, CancellationToken cancellationToken)
    {
        return await _collection
            .Find(n => n.UserId == userId)
            .SortByDescending(n => n.SentAt)
            .Skip(skip)
            .Limit(take)
            .ToListAsync(cancellationToken);
    }
}
