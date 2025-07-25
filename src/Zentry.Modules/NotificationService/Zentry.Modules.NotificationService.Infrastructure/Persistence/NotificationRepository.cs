using Microsoft.EntityFrameworkCore;
using Zentry.Modules.NotificationService.Domain.Entities;

namespace Zentry.Modules.NotificationService.Infrastructure.Persistence;

public class NotificationRepository(NotificationDbContext dbContext) : INotificationRepository
{
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        await dbContext.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Notifications.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Notifications
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
} 