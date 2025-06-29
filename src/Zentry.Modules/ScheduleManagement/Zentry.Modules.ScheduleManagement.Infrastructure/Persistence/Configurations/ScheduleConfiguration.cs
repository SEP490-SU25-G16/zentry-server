using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Domain.Enums;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    [Obsolete("Obsolete")]
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.LecturerId) // Thêm LecturerId
            .IsRequired();

        builder.Property(s => s.CourseId)
            .IsRequired();

        builder.Property(s => s.RoomId)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.DayOfWeek) // Cấu hình DayOfWeek (Enumeration)
            .HasConversion(
                dw => dw.ToString(),
                dw => DayOfWeekEnum.FromName(dw)
            )
            .IsRequired()
            .HasMaxLength(20); // Giả định độ dài

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        // Indices
        builder.HasIndex(s => s.LecturerId);
        builder.HasIndex(s => s.CourseId);
        builder.HasIndex(s => s.RoomId);
        builder.HasIndex(s => s.StartTime);
        builder.HasIndex(s => s.EndTime);
        builder.HasIndex(s => s.DayOfWeek);

        builder.HasCheckConstraint("CK_Schedules_EndTime_After_StartTime",
            "\"EndTime\" > \"StartTime\"");

        // Navigation properties (Nếu có trong entity Schedule)
        // builder.HasOne(s => s.Course).WithMany().HasForeignKey(s => s.CourseId);
        // builder.HasOne(s => s.Room).WithMany().HasForeignKey(s => s.RoomId);
        // builder.HasOne(s => s.Lecturer).WithMany().HasForeignKey(s => s.LecturerId);
    }
}