using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.Notification.Features.ReceiveAttendanceNotification;

namespace Zentry.Modules.Notification.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Features.ReceiveAttendanceNotification.Notification>>> GetNotifications(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new ReceiveAttendanceNotificationQuery(userId, page, pageSize);
        var notifications = await mediator.Send(query, cancellationToken);
        return Ok(notifications);
    }
}