// File: Zentry.Modules.AttendanceManagement.Application.Features.Integration/GetSessionsByScheduleIdsAndDateIntegrationHandler.cs
using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.Modules.AttendanceManagement.Application.Features.Integration;

// Handler này nằm trong module AttendanceManagement
public class GetSessionsByScheduleIdsAndDateIntegrationHandler(
    ISessionRepository sessionRepository
) : IQueryHandler<GetSessionsByScheduleIdsAndDateIntegrationQuery, GetSessionsByScheduleIdsAndDateIntegrationResponse>
{
    public async Task<GetSessionsByScheduleIdsAndDateIntegrationResponse> Handle(
        GetSessionsByScheduleIdsAndDateIntegrationQuery request,
        CancellationToken cancellationToken)
    {
        // Chuyển đổi DateOnly thành DateTime (UTC)
        var utcDateStart = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        
        // Gọi phương thức repository để lấy các sessions theo danh sách ScheduleId và một ngày duy nhất
        var sessions = await sessionRepository.GetSessionsByScheduleIdsAndDateAsync(
            request.ScheduleIds, 
            request.Date, // Truyền DateOnly trực tiếp
            cancellationToken);

        // Ánh xạ danh sách các Sessions từ DB sang Dictionary của response DTO
        var sessionsByScheduleId = sessions
            .ToDictionary(
                s => s.ScheduleId, // Key là ScheduleId
                s => new SessionInfoForDateIntegrationResponse(
                    s.Id,
                    s.Status.ToString(), // Chuyển enum sang string
                    DateOnly.FromDateTime(s.StartTime),
                    TimeOnly.FromDateTime(s.StartTime),
                    TimeOnly.FromDateTime(s.EndTime)
                )
            );

        return new GetSessionsByScheduleIdsAndDateIntegrationResponse(sessionsByScheduleId);
    }
}
