using Microsoft.EntityFrameworkCore;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Data;

public static class AttendanceSeed
{
    public static async Task SeedData(AttendanceDbContext dbContext)
    {
        if (!await dbContext.Sessions.AnyAsync())
        {
            var testScheduleId = Guid.NewGuid();
            var testLecturerUserId = Guid.NewGuid();
            var testDeviceId = Guid.NewGuid();
            var testStudentUserId = Guid.NewGuid();

            // var session = Session.Create(testScheduleId, testLecturerUserId, DateTime.UtcNow.AddDays(-1),
            //     DateTime.UtcNow.AddDays(-1).AddHours(2));
            // dbContext.Sessions.Add(session);
            //
            // var round = Round.Create(session.Id, testDeviceId, session.StartTime, session.EndTime,
            //     "Initial client request");
            // dbContext.Rounds.Add(round);
            //
            // var attendanceRecord =
            //     AttendanceRecord.Create(testStudentUserId, session.Id, AttendanceStatus.Present, false);
            // dbContext.AttendanceRecords.Add(attendanceRecord);

            await dbContext.SaveChangesAsync();
        }
    }
}
