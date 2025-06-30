using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;
using Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/schedules")]
public class SchedulesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleCreatedResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Nếu Course/Room/Lecturer không tồn tại
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleCommand request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await mediator.Send(request, cancellationToken);
            return CreatedAtAction(nameof(CreateSchedule), new { id = response.Id }, response);
        }
        catch (BadHttpRequestException ex)
        {
            return NotFound(new { errors = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetSchedulesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSchedules([FromQuery] GetSchedulesQuery query,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await mediator.Send(query, cancellationToken);
        return Ok(response);
    }
}
