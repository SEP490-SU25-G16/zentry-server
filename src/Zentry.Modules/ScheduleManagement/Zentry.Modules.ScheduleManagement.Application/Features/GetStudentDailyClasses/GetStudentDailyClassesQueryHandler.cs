// File: Zentry.Modules.ScheduleManagement.Application.Features.GetStudentDailyClasses/GetStudentDailyClassesQueryHandler.cs

using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers; // Để dùng ToWeekDayEnum
using Zentry.Modules.ScheduleManagement.Application.Services; // Để dùng IUserScheduleService
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Attendance; // GetSessionsByScheduleIdIntegrationQuery

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetStudentDailyClasses;

public class GetStudentDailyClassesQueryHandler(
    IEnrollmentRepository enrollmentRepository,
    IScheduleRepository scheduleRepository,
    IMediator mediator,
    IUserScheduleService userScheduleService
) : IQueryHandler<GetStudentDailyClassesQuery, List<StudentDailyClassDto>>
{
    public async Task<List<StudentDailyClassDto>> Handle(GetStudentDailyClassesQuery request,
        CancellationToken cancellationToken)
    {
        var dayOfWeek = request.Date.DayOfWeek.ToWeekDayEnum();

        var student =
            await userScheduleService.GetUserByIdAndRoleAsync(Role.Student, request.StudentId, cancellationToken);
        var studentName = student?.FullName ?? "N/A";

        var enrollments = await enrollmentRepository.GetEnrollmentsByStudentIdAsync(
            request.StudentId, cancellationToken);

        var result = new List<StudentDailyClassDto>();

        foreach (var enrollment in enrollments)
        {
            // Lấy ClassSection từ enrollment
            var classSection = enrollment.ClassSection;
            if (classSection == null || classSection.IsDeleted) continue;

            // Lấy các Schedules cho ClassSection này khớp với ngày yêu cầu
            var schedulesForClassSection = await scheduleRepository.GetSchedulesByClassSectionIdAndDateAsync(
                classSection.Id,
                request.Date,
                dayOfWeek,
                cancellationToken
            );

            foreach (var schedule in schedulesForClassSection)
            {
                var course = classSection.Course;
                var room = schedule.Room;

                // Bỏ qua nếu dữ liệu liên quan không đầy đủ
                if (course == null || room == null)
                    continue;

                // Lấy thông tin giảng viên của ClassSection
                var lecturer =
                    await userScheduleService.GetUserByIdAndRoleAsync(Role.Lecturer, classSection.LecturerId,
                        cancellationToken);
                var lecturerName = lecturer?.FullName ?? "N/A";

                // Lấy tất cả các sessions cho Schedule này
                var getSessionsQuery = new GetSessionsByScheduleIdIntegrationQuery(schedule.Id);
                var allSessionsForSchedule = await mediator.Send(getSessionsQuery, cancellationToken);

                var totalSessions = allSessionsForSchedule.Count;
                var orderedSessions = allSessionsForSchedule.OrderBy(s => s.StartTime).ToList();

                var currentSession = orderedSessions
                    .FirstOrDefault(s => s.StartTime.Date == request.Date.Date);

                var sessionStatus = currentSession?.Status ?? SessionStatus.Pending.ToString();
                var currentSessionNumber = currentSession is not null
                    ? orderedSessions.FindIndex(s => s.SessionId == currentSession.SessionId) + 1
                    : 0;

                // Map tất cả sessions vào SessionInfoDto
                var sessionInfoDtos = orderedSessions.Select((s, index) => new SessionInfoDto
                {
                    SessionId = s.SessionId,
                    ScheduleId = schedule.Id,
                    SessionNumber = index + 1,
                    Status = s.Status,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList();

                result.Add(new StudentDailyClassDto
                {
                    ScheduleId = schedule.Id,
                    ClassSectionId = classSection.Id,
                    CourseId = course.Id,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    SectionCode = classSection.SectionCode,
                    LecturerId = classSection.LecturerId,
                    LecturerName = lecturerName,
                    RoomId = room.Id,
                    RoomName = room.RoomName,
                    Building = room.Building,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    Weekday = dayOfWeek.ToString(),
                    DateInfo = DateOnly.FromDateTime(request.Date),
                    StudentId = request.StudentId,
                    Sessions = sessionInfoDtos
                    // Thêm logic để lấy trạng thái điểm danh của sinh viên nếu cần
                    // StudentAttendanceStatus = GetStudentAttendanceStatus(schedule.Id, request.StudentId, currentSession?.SessionId)
                });
            }
        }

        // Sắp xếp kết quả theo thời gian bắt đầu của lịch trình
        return result.OrderBy(s => s.StartTime).ToList();
    }
}
