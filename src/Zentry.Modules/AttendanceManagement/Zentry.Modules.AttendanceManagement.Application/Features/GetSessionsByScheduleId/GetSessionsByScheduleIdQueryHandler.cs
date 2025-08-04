using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.GetSessionsByScheduleId;

public class GetSessionsByScheduleIdQueryHandler(ISessionRepository sessionRepository)
    : IQueryHandler<GetSessionsByScheduleIdQuery, List<SessionDto>>
{
    public async Task<List<SessionDto>> Handle(GetSessionsByScheduleIdQuery request, CancellationToken cancellationToken)
    {
        var sessions = await sessionRepository.GetSessionsByScheduleIdAsync(request.ScheduleId, cancellationToken);

        if (sessions is null || !sessions.Any())
        {
            throw new NotFoundException("Sessions for Schedule", request.ScheduleId);
        }

        var sessionDtos = sessions.Select(s => new SessionDto
        {
            Id = s.Id,
            ScheduleId = s.ScheduleId,
            Status = s.Status.ToString(),
            StartTime = s.StartTime,
            ActualEndTime = s.ActualEndTime,
        }).ToList();

        return sessionDtos;
    }
}
