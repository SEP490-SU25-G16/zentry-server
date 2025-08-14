using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetStudentMonthlyCalendar;

public class GetStudentMonthlyCalendarQueryHandler(
    IEnrollmentRepository enrollmentRepository,
    IScheduleRepository scheduleRepository,
    IMediator mediator
) : IQueryHandler<GetStudentMonthlyCalendarQuery, MonthlyCalendarResponseDto>
{
    public async Task<MonthlyCalendarResponseDto> Handle(GetStudentMonthlyCalendarQuery request,
        CancellationToken cancellationToken)
    {
        var response = new MonthlyCalendarResponseDto
        {
            CalendarDays = new List<DailyScheduleDto>()
        };

        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Bước 1: Lấy tất cả enrollments của student
        var studentEnrollments = await enrollmentRepository.GetActiveEnrollmentsByStudentIdAsync(
            request.StudentId,
            cancellationToken
        );

        if (studentEnrollments.Count == 0) return response; // Trả về empty calendar nếu student không enroll lớp nào

        var classSectionIds = studentEnrollments.Select(e => e.ClassSectionId).ToList();

        var dailySchedulesMap = new Dictionary<DateOnly, List<ScheduleProjectionDto>>();
        var sessionLookups = new List<ScheduleDateLookup>();

        // Bước 2: Thu thập tất cả Schedules cho tất cả các ngày trong tháng của student
        for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
        {
            var currentDateOnly = DateOnly.FromDateTime(currentDate);
            var dayOfWeek = currentDate.DayOfWeek.ToWeekDayEnum();

            var schedulesForDay = await scheduleRepository.GetSchedulesByClassSectionIdsAndDateAsync(
                classSectionIds,
                currentDateOnly,
                dayOfWeek,
                cancellationToken
            );

            if (schedulesForDay.Count == 0) continue;
            dailySchedulesMap[currentDateOnly] = schedulesForDay.OrderBy(s => s.StartTime).ToList();

            sessionLookups.AddRange(schedulesForDay.Select(scheduleProjection =>
                new ScheduleDateLookup(scheduleProjection.ScheduleId, currentDateOnly)));
        }

        var allSessionsForMonth = new List<GetSessionsByScheduleIdAndDateIntegrationResponse>();
        if (sessionLookups.Count != 0)
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

        // Tạo dictionary để lookup session
        var sessionLookupDict = allSessionsForMonth
            .ToDictionary(
                s => (s.ScheduleId, DateOnly.FromDateTime(s.StartTime.ToVietnamLocalTime().Date)),
                s => s.SessionId
            );

        // Bước 3: Xây dựng response với định dạng chính xác
        for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
        {
            var currentDateOnly = DateOnly.FromDateTime(currentDate);

            if (dailySchedulesMap.TryGetValue(currentDateOnly, out var schedulesOnThisDay))
            {
                var vietnamDateTime = currentDate.ToVietnamLocalTime();

                var dailyScheduleDto = new DailyScheduleDto
                {
                    Date = vietnamDateTime, // Sử dụng Vietnam time
                    Classes = new List<CalendarClassDto>()
                };

                foreach (var scheduleProjection in schedulesOnThisDay)
                {
                    Guid? sessionId = null;
                    if (sessionLookupDict.TryGetValue((scheduleProjection.ScheduleId, currentDateOnly),
                            out var foundSessionId))
                        sessionId = foundSessionId;

                    // Format StartTime as TimeOnly string (HH:mm:ss)
                    var startTimeString = scheduleProjection.StartTime.ToString("HH:mm:ss");

                    dailyScheduleDto.Classes.Add(new CalendarClassDto
                    {
                        StartTime = startTimeString, // Chuyển thành string format
                        CourseName = scheduleProjection.CourseName,
                        SectionCode = scheduleProjection.SectionCode,
                        RoomName = scheduleProjection.RoomName,
                        Building = scheduleProjection.Building,
                        SessionId = sessionId,
                        ClassSectionId = scheduleProjection.ClassSectionId
                    });
                }

                if (dailyScheduleDto.Classes.Count > 0) response.CalendarDays.Add(dailyScheduleDto);
            }
        }

        return response;
    }
}