using MediatR;
using Zentry.Modules.Notification.Persistence.Repository;
using Zentry.SharedKernel.Common;

namespace Zentry.Modules.Notification.Features.ReceiveAttendanceNotification;

public class ReceiveAttendanceNotificationQueryHandler(INotificationRepository repository)
    : IRequestHandler<ReceiveAttendanceNotificationQuery, List<Notification>>
{
    public async Task<List<Notification>> Handle(ReceiveAttendanceNotificationQuery request,
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
