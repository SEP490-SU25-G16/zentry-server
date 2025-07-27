using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.Attendance;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    [Obsolete("Obsolete")]
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.ScheduleId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion(
                s => s.ToString(),
                s => SessionStatus.FromName(s)
            )
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired(false); // Có thể null

        builder.HasIndex(s => s.ScheduleId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.StartTime);

        builder.HasCheckConstraint("CK_Sessions_EndTime_After_StartTime",
            "\"EndTime\" > \"StartTime\"");

        builder.OwnsOne(s => s.SessionConfigs, ownedBuilder => { ownedBuilder.ToJson(); });
    }
}