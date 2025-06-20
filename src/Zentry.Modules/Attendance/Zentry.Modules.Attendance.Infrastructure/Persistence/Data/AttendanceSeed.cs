using Microsoft.EntityFrameworkCore;
using Zentry.Modules.Attendance.Domain.Entities;

namespace Zentry.Modules.Attendance.Infrastructure.Persistence.Data;

public static class AttendanceSeed
{
    public static async Task SeedData(AttendanceDbContext context)
    {
        if (!await context.Enrollments.AnyAsync())
        {
            var enrollment = Enrollment.Create(Guid.NewGuid(), Guid.NewGuid());
            context.Enrollments.Add(enrollment);

            var round = Round.Create(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(-1).AddHours(1));
            context.Rounds.Add(round);

            context.AttendanceRecords.Add(AttendanceRecord.Create(enrollment.EnrollmentId, round.RoundId, true));
            context.ErrorReports.Add(ErrorReport.Create(Guid.NewGuid(), "ERR001", "BLE connection failed"));

            await context.SaveChangesAsync();
        }
    }
}