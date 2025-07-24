using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.DeleteClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSectionById;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Application.Features.UpdateClassSection;
// Thêm using cho query mới
using Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyClasses;
using Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyReportQuery;
using Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerHome;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/class-sections")]
public class ClassSectionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{lecturerId:guid}/report/daily")]
    [ProducesResponseType(typeof(List<LecturerDailyReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLecturerDailyReport(
        Guid lecturerId,
        [FromQuery] DateTime? date = null)
    {
        var queryDate = date?.ToUniversalTime() ?? DateTime.UtcNow;

        var query = new GetLecturerDailyReportQuery(lecturerId, queryDate);
        var response = await mediator.Send(query);
        return Ok(response);
    }

    [HttpGet("{lecturerId:guid}/home")]
    [ProducesResponseType(typeof(List<LecturerHomeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLecturerHome(Guid lecturerId)
    {
        var query = new GetLecturerHomeQuery(lecturerId);
        var response = await mediator.Send(query);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateClassSectionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClassSection([FromBody] CreateClassSectionRequest request)
    {
        var command = new CreateClassSectionCommand(
            request.CourseId,
            request.LecturerId,
            request.SectionCode,
            request.Semester
        );

        var response = await mediator.Send(command);

        return CreatedAtAction(nameof(CreateClassSection), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClassSectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await mediator.Send(new GetClassSectionByIdQuery(id));
        return Ok(dto);
    }

    // --- Endpoint để lấy danh sách các lớp học trong ngày của giảng viên ---
    [HttpGet("daily-schedule")]
    [ProducesResponseType(typeof(List<LecturerDailyClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLecturerDailyClasses([FromQuery] Guid lecturerId,
        [FromQuery] DateTime? date = null)
    {
        var queryDate = date ?? DateTime.Today;

        var query = new GetLecturerDailyClassesQuery(lecturerId, queryDate);

        var response = await mediator.Send(query);

        return Ok(response);
    }
    // ----------------------------------------------------------------------

    [HttpGet]
    [ProducesResponseType(typeof(GetClassSectionsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClassSections([FromQuery] GetClassSectionsQuery query)
    {
        var response = await mediator.Send(query);
        return Ok(response);
    }
    // ---------------------------------------------------

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteClassSectionCommand(id));
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateClassSection(Guid id, [FromBody] UpdateClassSectionRequest req)
    {
        var cmd = new UpdateClassSectionCommand(id, req.SectionCode, req.Semester);
        await mediator.Send(cmd);
        return NoContent();
    }
}
