using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;

public class CreateSessionCommandHandler(ISessionRepository sessionRepository)
    : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    // Có thể inject thêm các service khác nếu cần (ví dụ:INotificationService)

    public async Task<CreateSessionResponse> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        // 1. Tạo đối tượng Session từ dữ liệu request
        var session = Session.Create(
            request.ScheduleId,
            request.UserId,
            request.StartTime,
            request.EndTime
        );

        // 2. Lưu Session vào database
        await sessionRepository.AddAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken); // Đảm bảo thay đổi được lưu

        // 3. (Tùy chọn) Gửi thông báo đến các máy trong lớp
        // Ở đây bạn sẽ gọi một Notification Service, ví dụ:
        // _notificationService.SendSessionStartedNotification(session.Id, session.ScheduleId, session.StartTime);

        // 4. Trả về Response
        return new CreateSessionResponse
        {
            SessionId = session.Id,
            ScheduleId = session.ScheduleId,
            UserId = session.UserId,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            CreatedAt = session.CreatedAt
        };
    }
}
