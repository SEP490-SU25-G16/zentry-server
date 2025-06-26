using MongoDB.Driver;
using Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotification;

namespace Zentry.Modules.NotificationService.Persistence.Repository;

public class NotificationRepository(IMongoDatabase database) : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection =
        database.GetCollection<Notification>("Notifications");

    public async Task AddAsync(Notification notification,
        CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(notification, null, cancellationToken);
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int skip,
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