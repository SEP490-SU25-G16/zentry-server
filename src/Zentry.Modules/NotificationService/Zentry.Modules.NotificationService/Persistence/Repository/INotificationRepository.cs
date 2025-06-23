namespace Zentry.Modules.NotificationService.Persistence.Repository;

public interface INotificationRepository
{
    Task AddAsync(Features.ReceiveAttendanceNotificationService.Notification notification,
        CancellationToken cancellationToken);

    Task<List<Features.ReceiveAttendanceNotificationService.Notification>> GetByUserIdAsync(Guid userId, int skip, int take,
        CancellationToken cancellationToken);
}