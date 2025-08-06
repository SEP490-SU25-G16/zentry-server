using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetSessionsByClassSectionId;

public class GetSessionsByClassSectionQueryHandler(IScheduleRepository scheduleRepository, IMediator mediator)
    : IQueryHandler<GetSessionsByClassSectionIdQuery, GetSessionsByClassSectionIdResponse>
{
    public async Task<GetSessionsByClassSectionIdResponse> Handle(GetSessionsByClassSectionIdQuery request,
        CancellationToken cancellationToken)
    {
        var schedules =
            await scheduleRepository.GetSchedulesByClassSectionIdAsync(request.ClassSectionId, cancellationToken);

        var sessionDtos = new List<SessionDto>();

        foreach (var scheduleInfoDto in schedules)
        {
            var sessions =
                await mediator.Send(new GetSessionsByScheduleIdIntegrationQuery(scheduleInfoDto.Id), cancellationToken);
            if (sessions is null || sessions.Count == 0)
                throw new NotFoundException("Sessions for Schedule", request.ClassSectionId);

            sessionDtos.AddRange(sessions.Select(s => new SessionDto
            {
                Id = s.SessionId,
                ScheduleId = s.ScheduleId,
                Status = s.Status.ToString(),
                WeekDay = scheduleInfoDto.WeekDay.ToString(),
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList());
        }

        return new GetSessionsByClassSectionIdResponse(sessionDtos.OrderBy(s => s.StartTime).ToList());
    }
}
