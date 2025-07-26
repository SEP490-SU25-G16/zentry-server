using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.Modules.AttendanceManagement.Application.Features.GetSessionFinalAttendance;
using Zentry.Modules.AttendanceManagement.Application.Features.GetSessionRounds;
using Zentry.Modules.AttendanceManagement.Application.Features.StartSession;
using Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;
using Zentry.Modules.AttendanceManagement.Presentation.Requests;

// Thêm using này

// Cần tạo StartSessionRequest

namespace Zentry.Modules.AttendanceManagement.Presentation.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController(IMediator mediator) : ControllerBase
{
    [HttpGet("sessions/{sessionId}/rounds")]
    [ProducesResponseType(typeof(List<RoundAttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionRounds(Guid sessionId, CancellationToken cancellationToken)
    {
        var query = new GetSessionRoundsQuery(sessionId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("sessions/{sessionId}/final")]
    [ProducesResponseType(typeof(List<FinalAttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionFinalAttendance(Guid sessionId, CancellationToken cancellationToken)
    {
        var query = new GetSessionFinalAttendanceQuery(sessionId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("sessions/{sessionId}/start")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> StartSession(Guid sessionId, [FromBody] StartSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            return BadRequest(new { Message = "User ID is required to start a session." });

        var command = new StartSessionCommand
        {
            SessionId = sessionId,
            UserId = request.UserId
        };

        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
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