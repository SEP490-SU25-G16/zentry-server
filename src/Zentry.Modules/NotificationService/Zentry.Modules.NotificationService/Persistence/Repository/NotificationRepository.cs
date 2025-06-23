using MongoDB.Driver;

namespace Zentry.Modules.NotificationService.Persistence.Repository;

public class NotificationRepository(IMongoDatabase database) : INotificationRepository
{
    private readonly IMongoCollection<Features.ReceiveAttendanceNotificationService.Notification> _collection =
        database.GetCollection<Features.ReceiveAttendanceNotificationService.Notification>("Notifications");

    public async Task AddAsync(Features.ReceiveAttendanceNotificationService.Notification notification,
        CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(notification, null, cancellationToken);
    }

    public async Task<List<Features.ReceiveAttendanceNotificationService.Notification>> GetByUserIdAsync(Guid userId, int skip,
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