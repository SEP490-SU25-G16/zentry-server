using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;
using Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailySchedules;
using Zentry.Modules.ScheduleManagement.Application.Features.GetMonthlyCalendar;
using Zentry.Modules.ScheduleManagement.Application.Features.GetScheduleDetail;
using Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;
using Zentry.Modules.ScheduleManagement.Application.Features.GetStudentDailySchedules;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/schedules")]
public class SchedulesController(IMediator mediator) : BaseController
{
    [HttpGet("student/daily-schedule")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentDailyClassDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStudentDailySchedules([FromQuery] Guid studentId,
        [FromQuery] DateTime? date = null)
    {
        try
        {
            var queryDate = date ?? DateTime.Today;
            var query = new GetStudentDailySchedulesQuery(studentId, queryDate);
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("lecturer/daily-schedule")]
    [ProducesResponseType(typeof(ApiResponse<List<LecturerDailyClassDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLecturerDailySchedules([FromQuery] Guid lecturerId,
        [FromQuery] DateTime? date = null)
    {
        try
        {
            var queryDate = date ?? DateTime.Today;
            var query = new GetLecturerDailySchedulesQuery(lecturerId, queryDate);
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{scheduleId:guid}/detail")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassDetail(Guid scheduleId)
    {
        try
        {
            var query = new GetScheduleDetailQuery(scheduleId);
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("lecturer/{lecturerId:guid}/monthly-calendar")]
    [ProducesResponseType(typeof(ApiResponse<MonthlyCalendarResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlyCalendar(
        Guid lecturerId,
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12 || year < 1900 || year > 2100)
            return BadRequest(ApiResponse.ErrorResult("VALIDATION_ERROR", "Month or year is out of valid range."));

        try
        {
            var query = new GetMonthlyCalendarQuery(lecturerId, month, year);
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

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