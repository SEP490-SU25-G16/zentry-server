using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.AssignLecturer;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.DeleteClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.GetAllClassSectionsWithEnrollmentCount;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSectionById;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyReportQuery;
using Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerHome;
using Zentry.Modules.ScheduleManagement.Application.Features.UpdateClassSection;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/class-sections")]
public class ClassSectionsController(IMediator mediator) : BaseController
{
    [HttpPost("{classSectionId:guid}/lecturers/{lecturerId:guid}")]
    [ProducesResponseType(typeof(AssignLecturerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignLecturer(
        [FromRoute] Guid classSectionId,
        [FromRoute] Guid lecturerId,
        CancellationToken cancellationToken)
    {
        var command = new AssignLecturerCommand(classSectionId, lecturerId);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("all-with-enrollment-count")]
    [ProducesResponseType(typeof(ApiResponse<List<ClassSectionWithEnrollmentCountDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllClassSectionsWithEnrollmentCount()
    {
        try
        {
            var query = new GetAllClassSectionsWithEnrollmentCountQuery();
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{lecturerId:guid}/report/daily")]
    [ProducesResponseType(typeof(ApiResponse<List<LecturerDailyReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLecturerDailyReport(
        Guid lecturerId,
        [FromQuery] DateTime? date = null)
    {
        try
        {
            var queryDate = date?.ToUniversalTime() ?? DateTime.UtcNow;
            var query = new GetLecturerDailyReportQuery(lecturerId, queryDate);
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{lecturerId:guid}/home")]
    [ProducesResponseType(typeof(ApiResponse<List<LecturerHomeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLecturerHome(Guid lecturerId)
    {
        try
        {
            var query = new GetLecturerHomeQuery(lecturerId);
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateClassSectionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClassSection([FromBody] CreateClassSectionRequest request)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var command = new CreateClassSectionCommand(
                new Guid(request.CourseId),
                request.SectionCode,
                request.Semester
            );

            var response = await mediator.Send(command);
            return HandleCreated(response, nameof(CreateClassSection), new { id = response.Id });
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClassSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var dto = await mediator.Send(new GetClassSectionByIdQuery(id));
            return HandleResult(dto);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetClassSectionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClassSections([FromQuery] GetClassSectionsQuery query)
    {
        try
        {
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await mediator.Send(new DeleteClassSectionCommand(id));
            return HandleNoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClassSection(Guid id, [FromBody] UpdateClassSectionRequest req)
    {
        try
        {
            var cmd = new UpdateClassSectionCommand(id, req.SectionCode, req.Semester);
            await mediator.Send(cmd);
            return HandleNoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}
