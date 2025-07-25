
using Zentry.Modules.NotificationService.Domain.Entities;

namespace Zentry.Modules.NotificationService.Infrastructure.Persistence;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
} 