using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Integration;

public class GetScheduleByIdHandler(IScheduleRepository scheduleRepository)
    : IQueryHandler<GetScheduleByIdIntegrationQuery, GetScheduleByIdIntegrationResponse>
{
    public async Task<GetScheduleByIdIntegrationResponse> Handle(GetScheduleByIdIntegrationQuery query,
        CancellationToken cancellationToken)
    {
        // 1. Lấy Room Entity từ Repository
        var schedule = await scheduleRepository.GetByIdAsync(query.Id, cancellationToken);

        // 2. Kiểm tra nếu không tìm thấy
        if (schedule is null)
            // Ném một NotFoundException để middleware xử lý thành 404
            throw new NotFoundException(nameof(GetScheduleByIdHandler), $"Schedule with ID '{query.Id}' not found.");
        // Hoặc đơn giản là trả về null và Controller sẽ xử lý thành NotFound()

        // 3. Ánh xạ từ Domain Entity sang DTO để trả về
        var response = new GetScheduleByIdIntegrationResponse
        {
            Id = schedule.Id,
            CourseId = schedule.CourseId,
            RoomId = schedule.RoomId,
            LecturerId = schedule.LecturerId,
            ScheduledStartTime = schedule.StartTime,
            ScheduledEndTime = schedule.EndTime,
            IsActive = schedule.StartTime < DateTime.Now && schedule.EndTime > DateTime.Now
        };

        return response;
    }
}