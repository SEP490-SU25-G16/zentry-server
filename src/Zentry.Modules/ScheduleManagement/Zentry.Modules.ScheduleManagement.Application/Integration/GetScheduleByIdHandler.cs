using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Integration;

public class GetScheduleByIdHandler(IScheduleRepository scheduleRepository) :
    IQueryHandler<GetScheduleByIdIntegrationQuery, GetScheduleByIdIntegrationResponse>
{
    public async Task<GetScheduleByIdIntegrationResponse> Handle(GetScheduleByIdIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        var schedule = await scheduleRepository.GetByIdWithClassSectionAsync(query.Id, cancellationToken);

        if (schedule is null)
            throw new NotFoundException(nameof(GetScheduleByIdHandler), $"Schedule with ID '{query.Id}' not found.");

        if (schedule.ClassSection is null)
            throw new NotFoundException(nameof(GetScheduleByIdHandler),
                $"ClassSection for Schedule '{query.Id}' is missing.");

        // Tính thời điểm hiện tại để so với lịch
        var now = DateTime.UtcNow;
        var nowDate = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var isActive = schedule.StartDate <= nowDate && schedule.EndDate >= nowDate &&
                       schedule.StartTime <= nowTime && schedule.EndTime >= nowTime;

        var response = new GetScheduleByIdIntegrationResponse
        {
            Id = schedule.Id,
            CourseId = schedule.ClassSection.CourseId,
            RoomId = schedule.RoomId,
            LecturerId = schedule.ClassSection.LecturerId,

            ScheduledStartDate = schedule.StartDate,
            ScheduledEndDate = schedule.EndDate,
            ScheduledStartTime = schedule.StartTime,
            ScheduledEndTime = schedule.EndTime,

            IsActive = isActive
        };

        return response;
    }
}
