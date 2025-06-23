using MediatR;
using Zentry.Modules.NotificationService.Persistence.Repository;
using Zentry.SharedKernel.Common;

namespace Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotificationService;

public class ReceiveAttendanceNotificationServiceQueryHandler(INotificationRepository repository)
    : IRequestHandler<ReceiveAttendanceNotificationServiceQuery, List<Notification>>
{
    public async Task<List<Notification>> Handle(ReceiveAttendanceNotificationServiceQuery request,
        CancellationToken cancellationToken)
    {
        Guard.AgainstNegativeOrZero(request.Page, nameof(request.Page));
        Guard.AgainstNegativeOrZero(request.PageSize, nameof(request.PageSize));

        var notifications = await repository.GetByUserIdAsync(
            request.UserId,
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            cancellationToken);

        return notifications;
    }
}