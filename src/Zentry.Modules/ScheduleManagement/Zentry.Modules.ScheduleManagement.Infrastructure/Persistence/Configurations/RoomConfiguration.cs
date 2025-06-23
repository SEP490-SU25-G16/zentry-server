using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ScheduleManagement.Domain.Entities;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(r => r.RoomId);

        builder.Property(r => r.RoomId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Building).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Capacity).IsRequired();

        // Ignore property Id từ Entity base class
        builder.Ignore(r => r.Id);

        // Add indexes for performance
        builder.HasIndex(r => r.Name);
        builder.HasIndex(r => r.Building);
    }
}