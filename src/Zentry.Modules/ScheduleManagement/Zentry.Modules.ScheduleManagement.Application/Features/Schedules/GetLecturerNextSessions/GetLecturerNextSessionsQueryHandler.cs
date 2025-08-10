using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetLecturerNextSessions;

public class GetLecturerNextSessionsQueryHandler(
    IClassSectionRepository classSectionRepository,
    IMediator mediator
) : IQueryHandler<GetLecturerNextSessionsQuery, List<NextSessionDto>>
{
    public async Task<List<NextSessionDto>> Handle(GetLecturerNextSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var nextSessions = new List<NextSessionDto>();

        var classSections =
            await classSectionRepository.GetLecturerClassSectionsAsync(request.LecturerId, cancellationToken);

        var allScheduleIds = classSections.SelectMany(cs => cs.Schedules.Select(s => s.Id)).ToList();

        var upcomingSessionsResponse = await mediator.Send(
            new GetUpcomingSessionsByScheduleIdsIntegrationQuery(allScheduleIds),
            cancellationToken);

        foreach (var session in upcomingSessionsResponse.Sessions)
        {
            var correspondingClassSection = classSections
                .FirstOrDefault(cs => cs.Schedules.Any(s => s.Id == session.ScheduleId));

            if (correspondingClassSection is null) continue;

            var room = correspondingClassSection.Schedules
                .FirstOrDefault(s => s.Id == session.ScheduleId)?.Room;

            nextSessions.Add(new NextSessionDto
            {
                SessionId = session.Id,
                ClassSectionId = correspondingClassSection.Id,
                ClassTitle = $"{correspondingClassSection.Course?.Name} - {correspondingClassSection.SectionCode}",
                CourseCode = correspondingClassSection.Course?.Code ?? string.Empty,
                SectionCode = correspondingClassSection.SectionCode,
                StartDate = DateOnly.FromDateTime(session.StartTime.ToLocalTime()),
                StartTime = TimeOnly.FromDateTime(session.StartTime.ToLocalTime()),
                EndDate = DateOnly.FromDateTime(session.EndTime.ToLocalTime()),
                EndTime = TimeOnly.FromDateTime(session.EndTime.ToLocalTime()),
                RoomInfo = $"{room?.RoomName} ({room?.Building})",
                EnrolledStudents = correspondingClassSection.Enrollments.Count,
                Status = session.Status
            });
        }

        return nextSessions.OrderBy(s => s.StartDate).ThenBy(s => s.StartTime).ToList();
    }
}
