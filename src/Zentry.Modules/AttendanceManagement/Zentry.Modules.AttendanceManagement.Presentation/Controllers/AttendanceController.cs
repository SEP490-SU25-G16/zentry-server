using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;
using Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;
using Zentry.Modules.AttendanceManagement.Presentation.Requests;

namespace Zentry.Modules.AttendanceManagement.Presentation.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController(IMediator mediator) : ControllerBase
{
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        // Loại bỏ StartTime và EndTime từ Command
        var command = new CreateSessionCommand(
            request.ScheduleId,
            request.UserId
        );
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateSession), new { id = result.SessionId }, result);
    }

    [HttpPost("sessions/scan")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SubmitScanData([FromBody] SubmitScanRequest request)
    {
        var command = new SubmitScanDataCommand(
            request.DeviceId,
            request.SubmitterUserId,
            request.SessionId,
            request.ScannedDevices,
            request.Timestamp
        );

        var response = await mediator.Send(command);

        if (response.Success) return Ok(response);

        return BadRequest(response);
    }
}
