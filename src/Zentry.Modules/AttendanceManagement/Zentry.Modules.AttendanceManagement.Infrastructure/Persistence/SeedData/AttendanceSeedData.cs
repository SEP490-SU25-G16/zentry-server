using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Schedule;

// Cần nếu dùng RoundStatus hay các Enums khác của Attendance

// Cần nếu dùng SessionConfigSnapshot

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.SeedData;

public static class AttendanceSeedData
{
    // private static List<AttendanceRecord> SeededAttendanceRecords { get; set; } = []; // Sẽ thêm sau

    // Cấu hình mặc định cho SessionConfigSnapshot khi seeding
    private static readonly Dictionary<string, string> DefaultSessionConfigs = new(StringComparer.OrdinalIgnoreCase)
    {
        { "AttendanceWindowMinutes", "15" },
        { "TotalAttendanceRounds", "10" },
        { "AbsentReportGracePeriodHours", "24" },
        { "ManualAdjustmentGracePeriodHours", "24" }
    };

    private static List<Session> SeededSessions { get; set; } = [];
    private static List<Round> SeededRounds { get; set; } = [];

    public static async Task SeedSessionsAndRoundsAsync(
        AttendanceDbContext context,
        List<SeededScheduleDto> seededScheduleDtos,
        List<SeededClassSectionDto> seededClassSectionDtos,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Starting Session and Round seed data...");

            if (!seededScheduleDtos.Any())
            {
                logger?.LogWarning("No Schedule DTOs provided to create Sessions. Skipping Session and Round seeding.");
                return;
            }

            if (await context.Sessions.AnyAsync(cancellationToken))
            {
                logger?.LogInformation("Session data already exists. Skipping Session and Round seed.");
                SeededSessions = await context.Sessions.AsNoTracking().ToListAsync(cancellationToken);
                SeededRounds =
                    await context.Rounds.AsNoTracking()
                        .ToListAsync(cancellationToken); // Load rounds nếu sessions đã có
                return;
            }

            Randomizer.Seed = new Random(205); // Seed mới cho Bogus
            var faker = new Faker();

            var sessionsToAdd = new List<Session>();
            var roundsToAdd = new List<Round>();

            // Lặp qua các DTO của Schedule thay vì entity
            foreach (var scheduleDto in seededScheduleDtos)
            {
                // Tìm ClassSection DTO tương ứng
                var classSectionDto = seededClassSectionDtos.FirstOrDefault(cs => cs.Id == scheduleDto.ClassSectionId);
                if (classSectionDto == null)
                {
                    logger?.LogWarning(
                        $"ClassSection DTO for ScheduleId {scheduleDto.Id} not found. Skipping Session creation for this schedule.");
                    continue;
                }

                DayOfWeek systemDayOfWeek;
                try
                {
                    systemDayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), scheduleDto.WeekDay, true);
                }
                catch (ArgumentException ex)
                {
                    logger?.LogError(ex,
                        $"Invalid WeekDay string '{scheduleDto.WeekDay}' for ScheduleId {scheduleDto.Id}. Skipping session creation.");
                    continue; // Bỏ qua lịch trình này nếu WeekDay không hợp lệ
                }

                // Lặp qua từng ngày trong khoảng thời gian của Schedule DTO
                for (var date = scheduleDto.StartDate; date <= scheduleDto.EndDate; date = date.AddDays(1))
                    if (date.DayOfWeek == systemDayOfWeek)
                    {
                        var sessionStartTime = date.ToDateTime(scheduleDto.StartTime);
                        var sessionEndTime = date.ToDateTime(scheduleDto.EndTime);

                        // Xử lý trường hợp thời gian kết thúc nhỏ hơn thời gian bắt đầu (qua ngày)
                        if (scheduleDto.EndTime < scheduleDto.StartTime) sessionEndTime = sessionEndTime.AddDays(1);

                        // Đảm bảo UTC Kind
                        sessionStartTime = DateTime.SpecifyKind(sessionStartTime, DateTimeKind.Utc);
                        sessionEndTime = DateTime.SpecifyKind(sessionEndTime, DateTimeKind.Utc);

                        // Tạo Session với config mặc định, sử dụng Id từ DTO
                        var session = Session.Create(
                            scheduleDto.Id, // ScheduleId từ DTO
                            classSectionDto.LecturerId, // LecturerId từ ClassSection DTO
                            sessionStartTime,
                            sessionEndTime,
                            DefaultSessionConfigs // Sử dụng config mặc định
                        );
                        sessionsToAdd.Add(session);

                        // Sau khi tạo Session, tạo Rounds cho Session đó
                        var totalDuration = session.EndTime.Subtract(session.StartTime);
                        var totalAttendanceRounds = session.TotalAttendanceRounds; // Lấy từ SessionConfigs

                        if (totalAttendanceRounds <= 0 || !(totalDuration.TotalSeconds > 0)) continue;
                        var durationPerRoundSeconds = totalDuration.TotalSeconds / totalAttendanceRounds;

                        for (var i = 1; i <= totalAttendanceRounds; i++)
                        {
                            var roundStartTime = session.StartTime.AddSeconds(durationPerRoundSeconds * (i - 1));
                            var roundEndTime = roundStartTime.AddSeconds(durationPerRoundSeconds);

                            // Đảm bảo EndTime của round cuối cùng không vượt quá EndTime của session
                            if (i == totalAttendanceRounds) roundEndTime = session.EndTime;

                            var newRound = Round.Create(
                                session.Id,
                                i,
                                roundStartTime,
                                roundEndTime
                            );
                            roundsToAdd.Add(newRound);
                        }
                    }
            }

            if (sessionsToAdd.Count > 0)
            {
                await context.Sessions.AddRangeAsync(sessionsToAdd, cancellationToken);
                logger?.LogInformation($"Added {sessionsToAdd.Count} Sessions.");
                SeededSessions = sessionsToAdd; // Lưu lại để dùng cho AttendanceRecord nếu cần seed sau
            }
            else
            {
                logger?.LogWarning("No Sessions were generated or added during seeding.");
            }

            if (roundsToAdd.Count > 0)
            {
                await context.Rounds.AddRangeAsync(roundsToAdd, cancellationToken);
                logger?.LogInformation($"Added {roundsToAdd.Count} Rounds.");
                SeededRounds = roundsToAdd;
            }
            else
            {
                logger?.LogWarning("No Rounds were generated or added during seeding.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding Session and Round data");
            throw;
        }
    }

    // Phương thức seed AttendanceRecord sẽ được thêm vào đây sau khi bạn cung cấp Entity.
    // public static async Task SeedAttendanceRecordsAsync(...) { ... }
}