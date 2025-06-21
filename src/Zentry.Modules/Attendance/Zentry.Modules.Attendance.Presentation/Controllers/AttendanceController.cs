using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.Attendance.Application.Features.ViewAttendanceRate;

namespace Zentry.Modules.Attendance.Presentation.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("rate")]
    public async Task<IActionResult> GetAttendanceRate([FromQuery] Guid studentId, [FromQuery] Guid courseId)
    {
        var query = new ViewAttendanceRateQuery(studentId, courseId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}