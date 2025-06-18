using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.Notification.Features.ReceiveAttendanceNotification;

namespace Zentry.Modules.Notification.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<Features.ReceiveAttendanceNotification.Notification>>> GetNotifications(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new ReceiveAttendanceNotificationQuery(userId, page, pageSize);
        var notifications = await _mediator.Send(query, cancellationToken);
        return Ok(notifications);
    }
}
