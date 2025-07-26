using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyClasses;

public class GetLecturerDailyClassesQueryHandler(
    IScheduleRepository scheduleRepository,
    IEnrollmentRepository enrollmentRepository,
    IMediator mediator
) : IQueryHandler<GetLecturerDailyClassesQuery, List<LecturerDailyClassDto>>
{
    public async Task<List<LecturerDailyClassDto>> Handle(GetLecturerDailyClassesQuery request,
        CancellationToken cancellationToken)
    {
        var dayOfWeek = request.Date.DayOfWeek.ToWeekDayEnum();

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

            var currentSession = allSessions
                .FirstOrDefault(s => s.StartTime.Date == request.Date.Date);

            // Cải tiến: sử dụng enum trực tiếp thay vì so sánh chuỗi
            var sessionStatus = currentSession?.Status ?? "PENDING";

            var currentSessionNumber = currentSession is not null
                ? allSessions.FindIndex(s => s.SessionId == currentSession.SessionId) + 1
                : 0;

            var enrolledStudents = await enrollmentRepository.GetActiveStudentIdsByClassSectionIdAsync(
                schedule.ClassSectionId, cancellationToken);

            var canStartSession = false;
            // Chỉ kiểm tra logic "Start Session" nếu ngày request là ngày hiện tại
            if (request.Date.Date == DateTime.Today.Date && sessionStatus == "PENDING")
            {
                var now = DateTime.UtcNow;
                var sessionStartTime = request.Date.Date.Add(schedule.StartTime.ToTimeSpan());
                var timeDifference = sessionStartTime - now;

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
                CanStartSession = canStartSession
            });
        }

        return result.OrderBy(s => s.StartTime).ToList();
    }
}
