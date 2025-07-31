using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Extensions;

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

        // Lấy schedules bao gồm ClassSection, Course và Room
        var schedules = await scheduleRepository.GetLecturerSchedulesForDateAsync(
            request.LecturerId,
            request.Date,
            dayOfWeek,
            cancellationToken
        );

        var result = new List<LecturerDailyClassDto>();

        foreach (var schedule in schedules)
        {
            // Kiểm tra null cho các navigation properties để tránh NRE
            var classSection = schedule.ClassSection;
            var course = classSection?.Course;
            var room = schedule.Room;

            // Bỏ qua nếu dữ liệu liên quan không đầy đủ
            if (classSection == null || course == null || room == null)
                // Có thể log warning ở đây nếu cần
                continue;

            // Lấy tất cả các sessions cho Schedule này
            var getSessionsQuery = new GetSessionsByScheduleIdIntegrationQuery(schedule.Id);
            var allSessionsForSchedule = await mediator.Send(getSessionsQuery, cancellationToken);

            var totalSessions = allSessionsForSchedule.Count;

            // Sắp xếp sessions theo StartTime để đảm bảo tính đúng đắn của SessionNumber
            var orderedSessions = allSessionsForSchedule.OrderBy(s => s.StartTime).ToList();

            // Tìm session hiện tại của ngày đang xét
            var currentSession = orderedSessions
                .FirstOrDefault(s => s.StartTime.Date == request.Date.Date); // So sánh chỉ phần ngày

            var sessionStatus = currentSession?.Status ?? SessionStatus.Pending.ToString();

            var currentSessionNumber = currentSession is not null
                ? orderedSessions.FindIndex(s => s.SessionId == currentSession.SessionId) + 1
                : 0; // Nếu không tìm thấy session cho ngày này, số buổi là 0

            var enrolledStudents = await enrollmentRepository.GetActiveStudentIdsByClassSectionIdAsync(
                schedule.ClassSectionId, cancellationToken);

            var canStartSession = false;
            // Kiểm tra CanStartSession chỉ khi là ngày hiện tại và session đang Pending
            if (request.Date.Date == DateTime.Today.Date && sessionStatus.Equals(SessionStatus.Pending.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                var now = DateTime.UtcNow;

                // Tạo DateTime UTC từ ngày yêu cầu và thời gian bắt đầu của schedule
                // Điều này đòi hỏi hàm mở rộng ToUtcFromVietnamLocalTime phải chính xác
                // hoặc đảm bảo rằng request.Date.Date là UTC nếu bạn đang làm việc hoàn toàn với UTC.
                // Nếu request.Date là DateTime.Today (local time), thì cần chuyển đổi cẩn thận.
                // Giả định request.Date là ngày trong local time zone của server/ứng dụng.
                var scheduleStartTimeTodayLocal = new DateTime(
                    request.Date.Year, request.Date.Month, request.Date.Day,
                    schedule.StartTime.Hour, schedule.StartTime.Minute, schedule.StartTime.Second,
                    DateTimeKind.Unspecified // Quan trọng: Đây là local time nhưng không được đánh dấu là Local hay Utc
                );

                // Chuyển đổi từ "unspecified" local time sang UTC
                // Hàm mở rộng ToUtcFromVietnamLocalTime phải xử lý việc này đúng cách.
                // Ví dụ: var sessionStartTimeUtc = TimeZoneInfo.ConvertTimeToUtc(scheduleStartTimeTodayLocal, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")); // Hoặc múi giờ Việt Nam
                var sessionStartTimeUtc =
                    scheduleStartTimeTodayLocal.ToUtcFromVietnamLocalTime(); // Giữ lại hàm mở rộng của bạn

                var timeDifference = sessionStartTimeUtc - now;

                // Cho phép bắt đầu trong khoảng 5 phút trước đến 5 phút sau giờ bắt đầu
                if (timeDifference.TotalMinutes >= -5 && timeDifference.TotalMinutes <= 5) canStartSession = true;
            }

            // Map tất cả sessions vào SessionInfoDto
            var sessionInfoDtos = orderedSessions.Select((s, index) => new SessionInfoDto
            {
                SessionId = s.SessionId,
                ScheduleId = schedule.Id, // Gán ScheduleId liên quan
                SessionNumber = index + 1, // Buổi thứ n
                Status = s.Status,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList();

            result.Add(new LecturerDailyClassDto
            {
                ScheduleId = schedule.Id,
                ClassSectionId = classSection.Id, // <-- Gán ClassSectionId
                CourseId = course.Id, // <-- Gán CourseId
                CourseCode = course.Code,
                CourseName = course.Name,
                SectionCode = classSection.SectionCode,
                RoomId = room.Id, // <-- Gán RoomId
                RoomName = room.RoomName,
                Building = room.Building,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                EnrolledStudentsCount = enrolledStudents.Count,
                TotalSessions = totalSessions,
                SessionProgress = $"Buổi {currentSessionNumber}/{totalSessions}",
                SessionStatus = sessionStatus,
                CanStartSession = canStartSession,
                Weekday = dayOfWeek.ToString(),
                DateInfo = DateOnly.FromDateTime(request.Date),
                LecturerId = request.LecturerId, // <-- Gán LecturerId
                LecturerName = lecturerName,
                Sessions = sessionInfoDtos // <-- Gán danh sách sessions
            });
        }

        return result.OrderBy(s => s.StartTime).ToList();
    }
}