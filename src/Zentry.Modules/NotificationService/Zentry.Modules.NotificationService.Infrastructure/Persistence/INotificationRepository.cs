
using Zentry.Modules.NotificationService.Domain.Entities;

namespace Zentry.Modules.NotificationService.Infrastructure.Persistence;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken);
    Task UpdateRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken);
    Task DeleteAsync(Notification notification, CancellationToken cancellationToken);
    Task DeleteRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
} 