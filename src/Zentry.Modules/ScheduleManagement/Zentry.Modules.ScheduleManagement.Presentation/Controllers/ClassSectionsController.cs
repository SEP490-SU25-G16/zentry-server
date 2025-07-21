using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.DeleteClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSectionById;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Application.Features.UpdateClassSection;
// Thêm using này

// Thêm using này

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/class-sections")]
public class ClassSectionsController(IMediator mediator) : ControllerBase
{
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

    // --- Thêm endpoint để lấy danh sách ClassSections ---
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