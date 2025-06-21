using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.Attendance.Domain.Entities;

namespace Zentry.Modules.Attendance.Infrastructure.Persistence.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    [Obsolete("Obsolete")]
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");

        // Use RoundId as primary key
        builder.HasKey(r => r.RoundId);

        builder.Property(r => r.RoundId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(r => r.ScheduleId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        // Add indexes for performance
        builder.HasIndex(r => r.ScheduleId);
        builder.HasIndex(r => r.StartTime);
        builder.HasIndex(r => r.EndTime);

        // Add constraint to ensure EndTime > StartTime
        builder.HasCheckConstraint("CK_Rounds_EndTime_After_StartTime",
            "\"EndTime\" > \"StartTime\"");
    }
}