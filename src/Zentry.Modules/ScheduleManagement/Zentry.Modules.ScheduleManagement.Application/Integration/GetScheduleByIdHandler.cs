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

        var response = new GetScheduleByIdIntegrationResponse
        {
            Id = schedule.Id,
            CourseId = schedule.ClassSection.CourseId,
            RoomId = schedule.RoomId,
            LecturerId = schedule.ClassSection.LecturerId,
            ScheduledStartTime = schedule.StartTime,
            ScheduledEndTime = schedule.EndTime,
            IsActive = schedule.StartTime < DateTime.Now && schedule.EndTime > DateTime.Now
        };

        return response;
    }
}
