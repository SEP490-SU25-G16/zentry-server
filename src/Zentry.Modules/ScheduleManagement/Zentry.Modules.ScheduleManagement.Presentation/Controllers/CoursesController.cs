using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateCourse;
using Zentry.Modules.ScheduleManagement.Application.Features.DeleteCourse;
using Zentry.Modules.ScheduleManagement.Application.Features.GetCourseById;
using Zentry.Modules.ScheduleManagement.Application.Features.GetCourses;
using Zentry.Modules.ScheduleManagement.Application.Features.UpdateCourse;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CourseCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await mediator.Send(request, cancellationToken);

        return CreatedAtAction(nameof(CreateCourse), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetCoursesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCourses([FromQuery] GetCoursesQuery query, CancellationToken cancellationToken)
    {
        // Model binding sẽ tự động điền các thuộc tính của GetCoursesQuery từ query string
        // Bạn có thể thêm validation ở đây nếu cần (ví dụ: dùng FluentValidation)
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")] // Định tuyến để nhận ID là một GUID
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery(id);

        try
        {
            var response = await mediator.Send(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return NotFound(new { errors = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Invalid input or business logic error
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Course not found
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var command = new UpdateCourseCommand(id, request);

        try
        {
            var response = await mediator.Send(command, cancellationToken);
            return Ok(response);
        }
        catch (DirectoryNotFoundException ex)
        {
            return NotFound(new { errors = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCourseCommand(id);

        try
        {
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (DirectoryNotFoundException ex)
        {
            return NotFound(new { errors = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = ex.Message });
        }
    }
}