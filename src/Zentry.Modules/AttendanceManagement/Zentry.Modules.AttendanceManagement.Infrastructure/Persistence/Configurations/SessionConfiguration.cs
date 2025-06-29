using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.ScheduleId)
            .IsRequired();

        builder.Property(s => s.UserId) // LecturerId
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(s => s.ScheduleId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.StartTime);

        builder.HasCheckConstraint("CK_Sessions_EndTime_After_StartTime",
            "\"EndTime\" > \"StartTime\"");
    }
}