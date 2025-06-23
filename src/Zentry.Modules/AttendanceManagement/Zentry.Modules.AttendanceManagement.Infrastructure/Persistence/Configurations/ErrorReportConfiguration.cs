using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class ErrorReportConfiguration : IEntityTypeConfiguration<ErrorReport>
{
    public void Configure(EntityTypeBuilder<ErrorReport> builder)
    {
        builder.ToTable("ErrorReports");

        // Use ErrorReportId as primary key
        builder.HasKey(er => er.ErrorReportId);

        builder.Property(er => er.ErrorReportId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(er => er.DeviceId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(er => er.ErrorCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(er => er.Description)
            .HasMaxLength(500);

        builder.Property(er => er.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Add indexes for performance
        builder.HasIndex(er => er.DeviceId);
        builder.HasIndex(er => er.ErrorCode);
        builder.HasIndex(er => er.CreatedAt);
    }
}