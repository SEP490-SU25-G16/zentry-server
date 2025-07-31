using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;

namespace Zentry.Modules.AttendanceManagement.Application.Integrations;

public class GetSessionsByScheduleIdsAndDatesIntegrationQueryHandler(ISessionRepository sessionRepository)
    : IQueryHandler<GetSessionsByScheduleIdsAndDatesIntegrationQuery,
        List<GetSessionsByScheduleIdAndDateIntegrationResponse>>
{
    public async Task<List<GetSessionsByScheduleIdAndDateIntegrationResponse>> Handle(
        GetSessionsByScheduleIdsAndDatesIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Lookups.Count == 0) return [];

        // Tạo danh sách các cặp (ScheduleId, DateTime.Date (UTC)) để truy vấn
        var lookupTuples = query.Lookups.Select(l =>
        {
            var utcDateStart = l.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            return new { l.ScheduleId, UtcDate = utcDateStart.Date };
        }).ToList();

        var sessions = await sessionRepository.GetSessionsByScheduleIdsAndDatesAsync(
            lookupTuples.Select(x => x.ScheduleId).ToList(),
            lookupTuples.Select(x => x.UtcDate).ToList(),
            cancellationToken
        );

        // Ánh xạ kết quả sang DTO phản hồi. Cần lấy ScheduleId từ session entity.
        return sessions.Select(s => new GetSessionsByScheduleIdAndDateIntegrationResponse
        {
            ScheduleId = s.ScheduleId, // <--- Gán ScheduleId từ entity
            SessionId = s.Id,
            Status = s.Status.ToString(),
            StartTime = s.StartTime,
            EndTime = s.EndTime
        }).ToList();
    }
}