using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zentry.Modules.Schedule.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Domain.Entities.Schedule>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Schedule> builder)
    {
        builder.ToTable("Schedules");

        builder.HasKey(s => s.ScheduleId);

        builder.Property(s => s.CourseId).IsRequired()
            .HasColumnType("uuid")
            .IsRequired();
        builder.Property(s => s.RoomId).IsRequired()
            .HasColumnType("uuid")
            .IsRequired();
        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();

        builder.HasOne(s => s.Course).WithMany().HasForeignKey(s => s.CourseId);
        builder.HasOne(s => s.Room).WithMany().HasForeignKey(s => s.RoomId);
    }
}