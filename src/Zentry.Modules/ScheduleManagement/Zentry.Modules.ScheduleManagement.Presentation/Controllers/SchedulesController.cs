using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;
using Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/schedules")]
public class SchedulesController(IMediator mediator) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreatedScheduleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();

        try
        {
            var command = new CreateScheduleCommand(request);
            var response = await mediator.Send(command, cancellationToken);
            return HandleCreated(response, nameof(CreateSchedule), new { id = response.Id });
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSchedulesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSchedules([FromQuery] GetSchedulesQuery query,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();

        try
        {
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}