using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetMonthlyCalendar;

public class GetMonthlyCalendarQueryHandler(
    IScheduleRepository scheduleRepository,
    IMediator mediator
) : IQueryHandler<GetMonthlyCalendarQuery, MonthlyCalendarResponseDto>
{
    public async Task<MonthlyCalendarResponseDto> Handle(GetMonthlyCalendarQuery request,
        CancellationToken cancellationToken)
    {
        var response = new MonthlyCalendarResponseDto();

        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var dailySchedulesMap = new Dictionary<DateOnly, List<ScheduleProjectionDto>>();
        var sessionLookups = new List<ScheduleDateLookup>();

        // Bước 1: Thu thập tất cả Schedules cho tất cả các ngày trong tháng
        for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
        {
            var currentDateOnly = DateOnly.FromDateTime(currentDate);
            var dayOfWeek = currentDate.DayOfWeek.ToWeekDayEnum();

            var schedulesForDay = await scheduleRepository.GetLecturerSchedulesForDateAsync(
                request.LecturerId,
                currentDate,
                dayOfWeek,
                cancellationToken
            );

            if (schedulesForDay.Count == 0) continue;
            dailySchedulesMap[currentDateOnly] = schedulesForDay.OrderBy(s => s.StartTime).ToList();

            sessionLookups.AddRange(schedulesForDay.Select(scheduleProjection =>
                new ScheduleDateLookup(scheduleProjection.ScheduleId, currentDateOnly)));
        }

        var allSessionsForMonth = new List<GetSessionsByScheduleIdAndDateIntegrationResponse>();
        if (sessionLookups.Count != 0) // Chỉ gọi nếu có lookups để tránh gọi Mediator với list rỗng
        {
            var distinctSessionLookups = sessionLookups
                .GroupBy(x => new { x.ScheduleId, x.Date })
                .Select(g => g.First())
                .ToList();

            allSessionsForMonth = await mediator.Send(
                new GetSessionsByScheduleIdsAndDatesIntegrationQuery(distinctSessionLookups),
                cancellationToken
            );
        }

        var sessionLookupDict = allSessionsForMonth
            .ToDictionary(s => (s.ScheduleId, DateOnly.FromDateTime(s.StartTime.Date)), s => s.SessionId);

        for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
        {
            var currentDateOnly = DateOnly.FromDateTime(currentDate);
            var dailyScheduleDto = new DailyScheduleDto
            {
                Date = currentDate
            };

            if (dailySchedulesMap.TryGetValue(currentDateOnly, out var schedulesOnThisDay))
                foreach (var scheduleProjection in schedulesOnThisDay)
                {
                    Guid? sessionId = null;
                    if (sessionLookupDict.TryGetValue((scheduleProjection.ScheduleId, currentDateOnly),
                            out var foundSessionId))
                        sessionId = foundSessionId;

                    dailyScheduleDto.Classes.Add(new CalendarClassDto
                    {
                        StartTime = scheduleProjection.StartTime,
                        CourseName = scheduleProjection.CourseName,
                        SectionCode = scheduleProjection.SectionCode,
                        RoomName = scheduleProjection.RoomName,
                        Building = scheduleProjection.Building,
                        SessionId = sessionId,
                        ClassSectionId = scheduleProjection.ClassSectionId
                    });
                }

            if (dailyScheduleDto.Classes.Count != 0) response.CalendarDays.Add(dailyScheduleDto);
        }

        return response;
    }
}
