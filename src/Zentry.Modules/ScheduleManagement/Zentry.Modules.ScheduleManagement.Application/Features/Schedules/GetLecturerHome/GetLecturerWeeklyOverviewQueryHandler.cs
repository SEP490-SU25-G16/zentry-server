using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetLecturerHome;

public class GetLecturerWeeklyOverviewQueryHandler(
    IClassSectionRepository classSectionRepository,
    IMediator mediator
) : IQueryHandler<GetLecturerWeeklyOverviewQuery, List<WeeklyOverviewDto>>
{
    public async Task<List<WeeklyOverviewDto>> Handle(GetLecturerWeeklyOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var weeklyOverview = new List<WeeklyOverviewDto>();
        var classSections =
            await classSectionRepository.GetLecturerClassSectionsAsync(request.LecturerId, cancellationToken);

        var allScheduleIds = classSections.SelectMany(cs => cs.Schedules.Select(s => s.Id)).ToList();
        var allAttendanceData = await mediator.Send(
            new GetLecturerClassOverviewIntegrationQuery(allScheduleIds),
            cancellationToken);

        var now = DateTime.UtcNow;
        var startOfWeek = now.StartOfWeek(DayOfWeek.Monday);
        var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);

        foreach (var cs in classSections)
        {
            var classScheduleIds = cs.Schedules.Select(s => s.Id).ToList();
            var classSessions = allAttendanceData.Sessions
                .Where(s => classScheduleIds.Contains(s.ScheduleId))
                .ToList();

            var totalSessions = classSessions.Count;
            var currentSession = classSessions.Count(s => s.EndTime < now);

            var sessionsThisWeek = classSessions
                .Count(s => s.StartTime >= startOfWeek && s.StartTime <= endOfWeek);

            var completedSessionsThisWeek = classSessions
                .Count(s => (s.Status == "completed" || s.Status == "active") && s.StartTime >= startOfWeek &&
                            s.StartTime <= endOfWeek);

            var attendanceRate = await mediator.Send(new GetAttendanceRateIntegrationQuery(cs.Id), cancellationToken);

            weeklyOverview.Add(new WeeklyOverviewDto
            {
                ClassId = cs.Id,
                ClassName = $"{cs.Course?.Name} - {cs.SectionCode}",
                CourseCode = cs.Course?.Code ?? string.Empty,
                SectionCode = cs.SectionCode,
                EnrolledStudents = cs.Enrollments.Count,
                TotalSessions = totalSessions,
                CurrentSession = currentSession,
                SessionsThisWeek = sessionsThisWeek,
                CompletedSessionsThisWeek = completedSessionsThisWeek,
                AttendanceRate = attendanceRate,
                WeekProgress = new WeekProgressDto
                {
                    Completed = completedSessionsThisWeek,
                    Total = sessionsThisWeek,
                    Percentage = sessionsThisWeek > 0 ? (double)completedSessionsThisWeek / sessionsThisWeek * 100 : 0
                }
            });
        }

        return weeklyOverview;
    }
}
