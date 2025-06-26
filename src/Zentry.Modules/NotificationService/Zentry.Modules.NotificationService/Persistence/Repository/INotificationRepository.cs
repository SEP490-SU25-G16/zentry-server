using Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotification;

namespace Zentry.Modules.NotificationService.Persistence.Repository;

public interface INotificationRepository
{
    Task AddAsync(Notification notification,
        CancellationToken cancellationToken);

    Task<List<Notification>> GetByUserIdAsync(Guid userId, int skip, int take,
        CancellationToken cancellationToken);
}