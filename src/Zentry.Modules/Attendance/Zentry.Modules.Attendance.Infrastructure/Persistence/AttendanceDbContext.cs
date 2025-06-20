using Microsoft.EntityFrameworkCore;
using Zentry.Modules.Attendance.Domain.Entities;

namespace Zentry.Modules.Attendance.Infrastructure.Persistence;

public class AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : DbContext(options)
{
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<ErrorReport> ErrorReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AttendanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
