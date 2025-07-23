using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;
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
    [HttpPost("sessions")]
    [ProducesResponseType(201)] // Thêm các kiểu response
    [ProducesResponseType(400)]
    [ProducesResponseType(404)] // Giảng viên/Lịch trình không tìm thấy
    [ProducesResponseType(409)] // Lỗi BusinessRuleException (e.g. LECTURER_NOT_ASSIGNED, OUT_OF_COURSE_PERIOD)
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSessionCommand(request.ScheduleId, request.UserId);
        var result = await mediator.Send(command, cancellationToken);
        // CreatedAtAction cần tên action và route values để tạo URL cho tài nguyên mới.
        // Nếu bạn muốn URL trỏ đến session đã tạo, bạn cần một action GetSessionById.
        // Tạm thời, dùng Created (201) với kết quả.
        return Created("", result);
    }

    // --- API MỚI: StartSession ---
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