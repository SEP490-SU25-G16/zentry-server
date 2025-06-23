using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ReportingService.Features.ViewClassAttendanceReport;

namespace Zentry.Modules.ReportingService.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("class")]
    public async Task<IActionResult> GetClassAttendanceReport([FromQuery] Guid courseId,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = new ViewClassAttendanceReportQuery(courseId, startDate, endDate);
        var result = await mediator.Send(query);
        return Ok(result);
    }
}