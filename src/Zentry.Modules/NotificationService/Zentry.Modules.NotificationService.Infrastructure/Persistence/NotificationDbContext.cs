using Microsoft.EntityFrameworkCore;
using Zentry.Modules.NotificationService.Domain.Entities;

namespace Zentry.Modules.NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(builder =>
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(255);
            builder.Property(n => n.Body).IsRequired();
            builder.Property(n => n.RecipientUserId).IsRequired();
            builder.Property(n => n.IsRead).IsRequired();
            builder.Property(n => n.CreatedAt).IsRequired();
            // Thiết lập index cho RecipientUserId để tăng tốc độ truy vấn
            builder.HasIndex(n => n.RecipientUserId);
        });
    }
} 