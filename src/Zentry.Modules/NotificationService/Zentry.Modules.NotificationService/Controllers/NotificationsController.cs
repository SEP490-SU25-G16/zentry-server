using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotification;
using Zentry.Modules.NotificationService.Features.ReceiveAttendanceNotificationService;

namespace Zentry.Modules.NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetNotifications(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new ReceiveAttendanceNotificationServiceQuery(userId, page, pageSize);
        var notifications = await mediator.Send(query, cancellationToken);
        return Ok(notifications);
    }
}