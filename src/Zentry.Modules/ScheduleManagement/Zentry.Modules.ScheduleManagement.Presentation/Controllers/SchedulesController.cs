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
using Zentry.Modules.ScheduleManagement.Application.Features.ImportSchedules;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Exceptions;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/schedules")]
public class SchedulesController(
    IMediator mediator,
    IFileProcessor<ScheduleImportDto> fileProcessor) : BaseController
{
    // === API CRUD Cơ bản ===

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSchedulesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSchedules([FromQuery] GetSchedulesQuery query, CancellationToken cancellationToken)
    {
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

    [HttpGet("{scheduleId}/detail")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassDetail(Guid scheduleId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetScheduleDetailQuery(scheduleId);
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
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request, CancellationToken cancellationToken)
    {
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

    // === API dành cho vai trò và dữ liệu đặc biệt ===

    [HttpGet("student/daily-schedule")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentDailyClassDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStudentDailySchedules(
        [FromQuery] Guid studentId,
        [FromQuery] string? date,
        CancellationToken cancellationToken)
    {
        try
        {
            DateOnly queryDate;
            if (!string.IsNullOrEmpty(date))
            {
                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out queryDate))
                    return BadRequest(new ApiResponse
                        { Success = false, Message = "Invalid date format. Use yyyy-MM-dd" });
            }
            else
            {
                queryDate = DateOnly.FromDateTime(DateTime.Today);
            }

            var query = new GetStudentDailySchedulesQuery(studentId, queryDate);
            var response = await mediator.Send(query, cancellationToken);
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
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        try
        {
            var queryDate = date ?? DateTime.Today;
            var query = new GetLecturerDailySchedulesQuery(lecturerId, queryDate);
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("lecturer/{lecturerId}/monthly-calendar")]
    [ProducesResponseType(typeof(ApiResponse<MonthlyCalendarResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlyCalendar(
        Guid lecturerId,
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken cancellationToken)
    {
        // Validation thủ công này vẫn cần thiết vì không có request DTO
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

    // === API Import dữ liệu ===

    [HttpPost("import")]
    [ProducesResponseType(typeof(ApiResponse<ImportSchedulesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ImportSchedules([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(ApiResponse.ErrorResult("INVALID_INPUT", "File không được rỗng."));
        }

        List<ScheduleImportDto> schedulesToImport;
        try
        {
            schedulesToImport = await fileProcessor.ProcessFileAsync(file, cancellationToken);
        }
        catch (InvalidFileFormatException ex)
        {
            return BadRequest(ApiResponse.ErrorResult("INVALID_FILE_FORMAT", ex.Message));
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }

        // Validation thủ công này vẫn cần thiết vì nó liên quan đến file processor,
        // không phải request DTO.
        if (!schedulesToImport.Any())
        {
            return BadRequest(ApiResponse.ErrorResult("INVALID_INPUT", "File không chứa dữ liệu hợp lệ."));
        }

        var command = new ImportSchedulesCommand(schedulesToImport);
        try
        {
            var response = await mediator.Send(command, cancellationToken);
            if (response.Success)
            {
                return HandleResult(response, $"Import thành công {response.ImportedCount} lịch học.");
            }
            else
            {
                return HandlePartialSuccess(response,
                    $"Đã import thành công {response.ImportedCount} lịch học.",
                    $"Có {response.FailedCount} lỗi trong file.");
            }
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}
