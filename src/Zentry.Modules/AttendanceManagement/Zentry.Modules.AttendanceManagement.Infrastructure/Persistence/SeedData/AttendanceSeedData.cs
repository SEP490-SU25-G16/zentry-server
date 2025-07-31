using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Schedule;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.SeedData;

public static class AttendanceSeedData
{
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
                        .ToListAsync(cancellationToken);
                return;
            }

            Randomizer.Seed = new Random(205);
            var faker = new Faker();

            var sessionsToAdd = new List<Session>();
            var roundsToAdd = new List<Round>();

            // Sắp xếp schedules theo thời gian để đảm bảo tính liên tục
            var sortedScheduleDtos = seededScheduleDtos
                .OrderBy(s => s.StartDate)
                .ThenBy(s => s.StartTime)
                .ToList();

            logger?.LogInformation($"Processing {sortedScheduleDtos.Count} schedules in chronological order...");

            foreach (var scheduleDto in sortedScheduleDtos)
            {
                // Tìm ClassSection DTO tương ứng
                var classSectionDto = seededClassSectionDtos.FirstOrDefault(cs => cs.Id == scheduleDto.ClassSectionId);
                if (classSectionDto == null)
                {
                    logger?.LogWarning(
                        $"ClassSection DTO for ScheduleId {scheduleDto.Id} not found. Skipping Session creation for this schedule.");
                    continue;
                }

                // Chuyển đổi từ DateOnly và TimeOnly sang DateTime
                var sessionStartTime = scheduleDto.StartDate.ToDateTime(scheduleDto.StartTime);
                var sessionEndTime = scheduleDto.StartDate.ToDateTime(scheduleDto.EndTime);

                // Xử lý trường hợp thời gian kết thúc nhỏ hơn thời gian bắt đầu (qua ngày)
                if (scheduleDto.EndTime < scheduleDto.StartTime) sessionEndTime = sessionEndTime.AddDays(1);

                // Đảm bảo UTC Kind
                sessionStartTime = DateTime.SpecifyKind(sessionStartTime, DateTimeKind.Utc);
                sessionEndTime = DateTime.SpecifyKind(sessionEndTime, DateTimeKind.Utc);

                try
                {
                    // Tạo Session với config mặc định, sử dụng Id từ DTO
                    var session = Session.Create(
                        scheduleDto.Id, // ScheduleId từ DTO
                        classSectionDto.LecturerId, // LecturerId từ ClassSection DTO
                        sessionStartTime,
                        sessionEndTime,
                        DefaultSessionConfigs // Sử dụng config mặc định
                    );
                    sessionsToAdd.Add(session);

                    // Tạo Rounds cho Session
                    var rounds = CreateRoundsForSession(session);
                    roundsToAdd.AddRange(rounds);

                    logger?.LogDebug($"Created session and {rounds.Count} rounds for schedule {scheduleDto.Id}: " +
                                     $"{sessionStartTime:yyyy-MM-dd HH:mm} - {sessionEndTime:yyyy-MM-dd HH:mm}");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, $"Failed to create session for schedule {scheduleDto.Id}");
                }
            }

            // Lưu Sessions
            if (sessionsToAdd.Count > 0)
            {
                await context.Sessions.AddRangeAsync(sessionsToAdd, cancellationToken);
                logger?.LogInformation($"Added {sessionsToAdd.Count} Sessions in chronological order.");
                SeededSessions = sessionsToAdd;

                // Log thời gian của session đầu tiên và cuối cùng
                if (sessionsToAdd.Any())
                {
                    var firstSession = sessionsToAdd.OrderBy(s => s.StartTime).First();
                    var lastSession = sessionsToAdd.OrderBy(s => s.StartTime).Last();
                    logger?.LogInformation($"Sessions span from {firstSession.StartTime:yyyy-MM-dd HH:mm} " +
                                           $"to {lastSession.EndTime:yyyy-MM-dd HH:mm}");
                }
            }
            else
            {
                logger?.LogWarning("No Sessions were generated or added during seeding.");
            }

            // Lưu Rounds
            if (roundsToAdd.Count > 0)
            {
                await context.Rounds.AddRangeAsync(roundsToAdd, cancellationToken);
                logger?.LogInformation($"Added {roundsToAdd.Count} Rounds for all sessions.");
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

    private static List<Round> CreateRoundsForSession(Session session)
    {
        var rounds = new List<Round>();

        var totalDuration = session.EndTime.Subtract(session.StartTime);
        var totalAttendanceRounds = session.TotalAttendanceRounds; // Lấy từ SessionConfigs

        if (totalAttendanceRounds <= 0 ||
            !(totalDuration.TotalSeconds > 0)) return rounds; // Trả về list rỗng nếu không hợp lệ

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
            rounds.Add(newRound);
        }

        return rounds;
    }

    // Method để lấy thông tin sessions đã seed (nếu cần thiết)
    public static List<Session> GetSeededSessions()
    {
        return SeededSessions.ToList();
    }

    // Method để lấy thông tin rounds đã seed (nếu cần thiết)
    public static List<Round> GetSeededRounds()
    {
        return SeededRounds.ToList();
    }
}