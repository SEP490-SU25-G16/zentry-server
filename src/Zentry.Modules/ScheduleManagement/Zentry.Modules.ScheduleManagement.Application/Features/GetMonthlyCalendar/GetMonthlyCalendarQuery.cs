using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetMonthlyCalendar;

public record GetMonthlyCalendarQuery(Guid LecturerId, int Month, int Year)
    : IQuery<MonthlyCalendarResponseDto>;
