using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Features.ViewStudentSchedule;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/schedules")]
public class SchedulesController(IMediator mediator) : ControllerBase
{
    [HttpGet("student")]
    public async Task<IActionResult> GetStudentSchedule([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var request = new ViewStudentScheduleRequest(startDate, endDate);
        var result = await mediator.Send(request);
        return Ok(result);
    }
}