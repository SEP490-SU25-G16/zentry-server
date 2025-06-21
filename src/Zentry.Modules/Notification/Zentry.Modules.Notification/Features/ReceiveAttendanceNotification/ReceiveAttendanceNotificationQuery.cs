using MediatR;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.Notification.Features.ReceiveAttendanceNotification;

public record ReceiveAttendanceNotificationQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 10) : IQuery<List<Notification>>, IRequest<List<Notification>>;