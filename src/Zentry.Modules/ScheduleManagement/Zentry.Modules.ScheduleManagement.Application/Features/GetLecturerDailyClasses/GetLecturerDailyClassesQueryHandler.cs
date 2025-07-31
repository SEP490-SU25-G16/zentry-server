using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyClasses;

public class GetLecturerDailyClassesQueryHandler(
    IScheduleRepository scheduleRepository,
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
            var classSection = schedule.ClassSection;
            var course = classSection?.Course;
            var room = schedule.Room;

            if (classSection is null || course is null || room is null)
                continue;

            var requestDateOnly = DateOnly.FromDateTime(request.Date);
            var currentSessionInfo = await mediator.Send(
                new GetSessionByScheduleIdAndDateIntegrationQuery(schedule.Id, requestDateOnly),
                cancellationToken);

            var sessionStatus = currentSessionInfo?.Status ?? SessionStatus.Pending.ToString();
            var sessionId = currentSessionInfo?.SessionId;

            result.Add(new LecturerDailyClassDto
            {
                ClassSectionId = classSection.Id,
                CourseCode = course.Code,
                CourseName = course.Name,
                SectionCode = classSection.SectionCode,
                Weekday = dayOfWeek.ToString(),
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                RoomName = room.RoomName,
                Building = room.Building,
                SessionStatus = sessionStatus,
                SessionId = sessionId
            });
        }

        return result.OrderBy(s => s.StartTime).ToList();
    }
}
