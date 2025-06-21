using Microsoft.EntityFrameworkCore;
using Zentry.Modules.Reporting.Features.ViewClassAttendanceReport;

namespace Zentry.Modules.Reporting.Persistence;

public class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<AttendanceReport> AttendanceReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}