namespace Zentry.Modules.Notification.Persistence.Repository;

public interface INotificationRepository
{
    Task AddAsync(Features.ReceiveAttendanceNotification.Notification notification,
        CancellationToken cancellationToken);

    Task<List<Features.ReceiveAttendanceNotification.Notification>> GetByUserIdAsync(Guid userId, int skip, int take,
        CancellationToken cancellationToken);
}