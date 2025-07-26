using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyClasses;

public class GetLecturerDailyClassesQueryHandler(
    IScheduleRepository scheduleRepository,
    IEnrollmentRepository enrollmentRepository,
    IMediator mediator,
    IUserScheduleService userScheduleService
) : IQueryHandler<GetLecturerDailyClassesQuery, List<LecturerDailyClassDto>>
{
    public async Task<List<LecturerDailyClassDto>> Handle(GetLecturerDailyClassesQuery request,
        CancellationToken cancellationToken)
    {
        var dayOfWeek = request.Date.DayOfWeek.ToWeekDayEnum();

        var lecturer =
            await userScheduleService.GetUserByIdAndRoleAsync(Role.Lecturer, request.LecturerId, cancellationToken);
        var lecturerName = lecturer?.FullName ?? "N/A";

        var schedules = await scheduleRepository.GetLecturerSchedulesForDateAsync(
            request.LecturerId,
            request.Date,
            dayOfWeek,
            cancellationToken
        );

        var result = new List<LecturerDailyClassDto>();

        foreach (var schedule in schedules)
        {
            var getSessionsQuery = new GetSessionsByScheduleIdIntegrationQuery(schedule.Id);
            var allSessions = await mediator.Send(getSessionsQuery, cancellationToken);

            var totalSessions = allSessions.Count;

            // FIX LỖI: So sánh trực tiếp phần ngày của hai DateTime
            var currentSession = allSessions
                .FirstOrDefault(s => s.StartTime.Date == request.Date.Date);

            var sessionStatus = currentSession?.Status ?? "PENDING";

            // Có thể cần điều chỉnh logic tìm currentSessionNumber nếu allSessions không được sắp xếp
            // Tốt nhất là sắp xếp allSessions theo StartTime trước khi dùng FindIndex
            var currentSessionNumber = currentSession is not null
                ? allSessions
                    .OrderBy(s => s.StartTime) // Đảm bảo thứ tự để FindIndex đúng
                    .ToList() // Chuyển sang List để dùng FindIndex
                    .FindIndex(s => s.SessionId == currentSession.SessionId) + 1
                : 0;

            var enrolledStudents = await enrollmentRepository.GetActiveStudentIdsByClassSectionIdAsync(
                schedule.ClassSectionId, cancellationToken);

            var canStartSession = false;
            // Dùng request.Date.Date.ToUniversalTime() để đảm bảo so sánh đúng nếu DateTime.Today có Kind khác
            if (request.Date.Date == DateTime.Today.Date && sessionStatus == "PENDING")
            {
                var now = DateTime.UtcNow; // Luôn dùng UTC để so sánh

                // Kết hợp request.Date (DateTime) với schedule.StartTime (TimeOnly)
                // request.Date.Date sẽ tạo ra một DateTime với giờ là 00:00:00 và Kind=Unspecified
                // schedule.StartTime là TimeOnly
                // Dùng phương thức Date.ToDateTime(TimeOnly) trên request.Date (nếu nó là DateOnly)
                // HOẶC tạo DateTime từ các thành phần nếu request.Date là DateTime.
                // Vì request.Date là DateTime, ta có thể dùng trực tiếp các thành phần của nó
                // và kết hợp với TimeOnly
                var localSessionStartUnspecified = new DateTime(
                    request.Date.Year, request.Date.Month, request.Date.Day,
                    schedule.StartTime.Hour, schedule.StartTime.Minute, schedule.StartTime.Second,
                    DateTimeKind.Unspecified // Khai báo rõ là Unspecified
                );

                // Chuyển đổi giờ cục bộ đó sang UTC để so sánh với DateTime.UtcNow
                var sessionStartTimeUtc = localSessionStartUnspecified.ToUtcFromVietnamLocalTime();

                var timeDifference = sessionStartTimeUtc - now;

                if (timeDifference.TotalMinutes >= -5 && timeDifference.TotalMinutes <= 5)
                {
                    canStartSession = true;
                }
            }

            result.Add(new LecturerDailyClassDto
            {
                ScheduleId = schedule.Id,
                ClassSectionId = schedule.ClassSectionId,
                CourseCode = schedule.ClassSection?.Course?.Code!,
                CourseName = schedule.ClassSection?.Course?.Name!,
                SectionCode = schedule.ClassSection?.SectionCode!,
                RoomName = schedule.Room?.RoomName!,
                Building = schedule.Room?.Building!,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                EnrolledStudentsCount = enrolledStudents.Count,
                TotalSessions = totalSessions,
                SessionProgress = $"Buổi {currentSessionNumber}/{totalSessions}",
                SessionStatus = sessionStatus,
                CanStartSession = canStartSession,
                Weekday = dayOfWeek.ToString(),
                DateInfo = DateOnly.FromDateTime(request.Date), // Chuyển DateTime sang DateOnly
                LecturerName = lecturerName
            });
        }

        return result.OrderBy(s => s.StartTime).ToList();
    }
}
