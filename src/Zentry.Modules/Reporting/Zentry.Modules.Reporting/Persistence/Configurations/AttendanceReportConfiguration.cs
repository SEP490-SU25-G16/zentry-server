using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.Reporting.Features.ViewClassAttendanceReport;

namespace Zentry.Modules.Reporting.Persistence.Configurations;

public class AttendanceReportConfiguration : IEntityTypeConfiguration<AttendanceReport>
{
    public void Configure(EntityTypeBuilder<AttendanceReport> builder)
    {
        builder.ToTable("AttendanceReports");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.CourseId).IsRequired();
        builder.Property(ar => ar.GeneratedAt).IsRequired();
        builder.Property(ar => ar.TotalStudents).IsRequired();
        builder.Property(ar => ar.TotalSessions).IsRequired();
        builder.Property(ar => ar.AverageAttendanceRate).HasPrecision(5, 2).IsRequired();
    }
}
