using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ReportingService.Persistence.Entities;
using Zentry.Modules.ReportingService.Persistence.Enums;

namespace Zentry.Modules.ReportingService.Persistence.Configurations;

public class AttendanceReportConfiguration : IEntityTypeConfiguration<AttendanceReport>
{
    public void Configure(EntityTypeBuilder<AttendanceReport> builder)
    {
        builder.ToTable("AttendanceReports");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ar => ar.ScopeId)
            .IsRequired();

        builder.Property(ar => ar.ScopeType)
            .HasConversion(
                st => st.ToString(),
                st => ReportScopeType.FromName(st)
            )
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ar => ar.ReportType)
            .HasConversion(
                rt => rt.ToString(),
                rt => ReportType.FromName(rt)
            )
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ar => ar.ReportContent)
            .IsRequired()
            .HasColumnType("text"); // Use 'text' for potentially large content

        builder.Property(ar => ar.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(ar => ar.CreatedBy); // Nullable Guid

        builder.Property(ar => ar.ExpiredAt); // Nullable DateTime

        // Add indexes for performance
        builder.HasIndex(ar => ar.ScopeId);
        builder.HasIndex(ar => ar.ScopeType);
        builder.HasIndex(ar => ar.ReportType);
        builder.HasIndex(ar => ar.CreatedAt);
    }
}