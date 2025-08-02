using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.Modules.AttendanceManagement.Application.Features.CalculateRoundAttendance;
using Zentry.Modules.AttendanceManagement.Application.Features.GetRoundResult;
using Zentry.Modules.AttendanceManagement.Application.Features.GetSessionFinalAttendance;
using Zentry.Modules.AttendanceManagement.Application.Features.GetSessionRounds;
using Zentry.Modules.AttendanceManagement.Application.Features.GetStudentFinalAttendance;
using Zentry.Modules.AttendanceManagement.Application.Features.StartSession;
using Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;
using Zentry.Modules.AttendanceManagement.Application.Features.UpdateStudentAttendanceStatus;
using Zentry.Modules.AttendanceManagement.Presentation.Requests;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Exceptions;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.AttendanceManagement.Presentation.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController(IMediator mediator) : BaseController
{
    // Lấy kết quả điểm danh cuối cùng của một sinh viên trong một session
    [HttpGet("sessions/{sessionId:guid}/students/{studentId:guid}/final-result")]
    [ProducesResponseType(typeof(ApiResponse<StudentFinalAttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentFinalAttendance(Guid sessionId, Guid studentId)
    {
        try
        {
            var query = new GetStudentFinalAttendanceQuery(sessionId, studentId);
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // Lấy kết quả điểm danh của một round
    [HttpGet("rounds/{roundId:guid}/result")]
    [ProducesResponseType(typeof(ApiResponse<RoundResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoundResult(Guid roundId)
    {
        try
        {
            var query = new GetRoundResultQuery(roundId);
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("sessions/{sessionId}/rounds/{roundId}/calculate-attendance")]
    [ProducesResponseType(typeof(ApiResponse<CalculateRoundAttendanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CalculateRoundAttendance(Guid sessionId, Guid roundId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var command = new CalculateRoundAttendanceCommand(sessionId, roundId);
            var result = await mediator.Send(command, cancellationToken);
            return HandleResult(result, "Attendance calculation completed successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // Lấy danh sách các round trong một session (Đã sửa URL)
    [HttpGet("sessions/{sessionId:guid}/rounds")]
    [ProducesResponseType(typeof(ApiResponse<List<RoundAttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionRounds(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var query = new GetSessionRoundsQuery(sessionId);
            var result = await mediator.Send(query, cancellationToken);
            return HandleResult(result, "Session rounds retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // Lấy kết quả điểm danh cuối cùng của toàn bộ session (Đã sửa URL)
    [HttpGet("sessions/{sessionId:guid}/final")]
    [ProducesResponseType(typeof(ApiResponse<List<FinalAttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionFinalAttendance(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var query = new GetSessionFinalAttendanceQuery(sessionId);
            var result = await mediator.Send(query, cancellationToken);
            return HandleResult(result, "Final session attendance retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // Bắt đầu một session (Đã sửa URL)
    [HttpPost("sessions/{sessionId:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartSession(Guid sessionId, [FromBody] StartSessionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();

        if (request.UserId == Guid.Empty)
            return BadRequest(ApiResponse.ErrorResult("VALIDATION_ERROR", "User ID is required to start a session."));

        try
        {
            var command = new StartSessionCommand
            {
                SessionId = sessionId,
                UserId = request.UserId
            };
            await mediator.Send(command, cancellationToken);
            return HandleResult("Session started successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("sessions/scan")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitScanData([FromBody] SubmitScanRequest request)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var utcTimestamp = request.Timestamp.ToUtcFromVietnamLocalTime();
            var command = new SubmitScanDataCommand(
                request.SubmitterDeviceMacAddress,
                request.SessionId,
                request.ScannedDevices,
                utcTimestamp
            );
            await mediator.Send(command);
            return HandleResult("Scan data submitted successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // Cập nhật trạng thái điểm danh của sinh viên (Đã sửa URL)
    [HttpPut("sessions/{sessionId:guid}/students/{studentId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudentAttendanceStatus(
        Guid sessionId,
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();

        try
        {
            var command = new UpdateStudentAttendanceStatusCommand(
                sessionId,
                studentId
            );
            await mediator.Send(command, cancellationToken);
            return HandleNoContent();
        }
        catch (NotFoundException ex)
        {
            return HandleNotFound(ex.Message, studentId);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}
