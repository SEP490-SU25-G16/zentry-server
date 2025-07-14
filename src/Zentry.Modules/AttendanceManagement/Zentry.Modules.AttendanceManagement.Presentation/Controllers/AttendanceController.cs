using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;
using Zentry.Modules.AttendanceManagement.Application.Features.ViewAttendanceRate;
using Zentry.Modules.AttendanceManagement.Presentation.Requests;

namespace Zentry.Modules.AttendanceManagement.Presentation.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController(IMediator mediator) : ControllerBase
{
    [HttpGet("rate")]
    public async Task<IActionResult> GetAttendanceRate([FromQuery] Guid studentId, [FromQuery] Guid courseId)
    {
        var query = new ViewAttendanceRateQuery(studentId, courseId);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("sessions")] // Endpoint mới để tạo session
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSessionCommand(
            request.ScheduleId,
            request.UserId,
            request.StartTime,
            request.EndTime
        );
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateSession), new { id = result.SessionId }, result);
    }
}
