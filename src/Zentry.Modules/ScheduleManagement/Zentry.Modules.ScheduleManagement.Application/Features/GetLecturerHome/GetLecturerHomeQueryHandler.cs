using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerHome;

public class GetLecturerHomeQueryHandler(
    IClassSectionRepository classSectionRepository,
    IMediator mediator,
    IUserScheduleService userScheduleService
) : IQueryHandler<GetLecturerHomeQuery, List<LecturerHomeDto>>
{
    public async Task<List<LecturerHomeDto>> Handle(GetLecturerHomeQuery request, CancellationToken cancellationToken)
    {
        var lecturer =
            await userScheduleService.GetUserByIdAndRoleAsync(Role.Lecturer, request.LecturerId, cancellationToken);
        var lecturerName = lecturer?.FullName ?? "N/A";

        var classSections =
            await classSectionRepository.GetLecturerClassSectionsAsync(request.LecturerId, cancellationToken);

        var result = new List<LecturerHomeDto>();

        foreach (var cs in classSections)
        {
            var totalSessions = 0;
            var completedSessions = 0;

            foreach (var schedule in cs.Schedules)
            {
                var getSessionsQuery = new GetSessionsByScheduleIdIntegrationQuery(schedule.Id);
                var allSessionsForSchedule = await mediator.Send(getSessionsQuery, cancellationToken);

                totalSessions += allSessionsForSchedule.Count;

                completedSessions += allSessionsForSchedule
                    .Count(s => s.Status == SessionStatus.Completed.ToString() ||
                                s.Status == SessionStatus.Active.ToString());
            }

            result.Add(new LecturerHomeDto
            {
                ClassSectionId = cs.Id,
                CourseId = cs.Course?.Id ?? Guid.Empty,
                LecturerId = request.LecturerId,

                CourseCode = cs.Course?.Code ?? string.Empty,
                CourseName = cs.Course?.Name ?? string.Empty,
                SectionCode = cs.SectionCode,
                EnrolledStudents = cs.Enrollments.Count,
                TotalSessions = totalSessions,
                SessionProgress = $"Buổi {completedSessions}/{totalSessions}",
                Schedules = cs.Schedules.Select(s => new ScheduleInfoDto
                {
                    ScheduleId = s.Id,
                    RoomId = s.Room?.Id ?? Guid.Empty,

                    RoomInfo = $"{s.Room?.RoomName} ({s.Room?.Building})",
                    ScheduleInfo = $"{s.WeekDay} {s.StartTime.ToShortTimeString()}-{s.EndTime.ToShortTimeString()}"
                }).ToList(),
                LecturerName = lecturerName
            });
        }

        return result;
    }
}