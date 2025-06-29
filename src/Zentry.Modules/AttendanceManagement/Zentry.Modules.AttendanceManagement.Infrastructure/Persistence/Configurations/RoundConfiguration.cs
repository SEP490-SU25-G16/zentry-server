using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.SessionId)
            .IsRequired();

        builder.Property(r => r.DeviceId)
            .IsRequired();

        builder.Property(r => r.ClientRequest)
            .HasMaxLength(255);

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(r => r.SessionId);
        builder.HasIndex(r => r.DeviceId);
        builder.HasIndex(r => r.StartTime);
        builder.HasIndex(r => r.EndTime);

        builder.HasCheckConstraint("CK_Rounds_EndTime_After_StartTime",
            "\"EndTime\" > \"StartTime\"");
    }
}