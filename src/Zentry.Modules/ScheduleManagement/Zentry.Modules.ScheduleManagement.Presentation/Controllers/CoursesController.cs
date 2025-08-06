using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateCourse;
using Zentry.Modules.ScheduleManagement.Application.Features.DeleteCourse;
using Zentry.Modules.ScheduleManagement.Application.Features.GetCourseById;
using Zentry.Modules.ScheduleManagement.Application.Features.GetCourses;
using Zentry.Modules.ScheduleManagement.Application.Features.UpdateCourse;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(IMediator mediator) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetCoursesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCourses([FromQuery] GetCoursesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response, "Courses retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetCourseByIdQuery(id);
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response, "Course retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseCreatedResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await mediator.Send(request, cancellationToken);
            return HandleCreated(response, nameof(CreateCourse), new { id = response.Id });
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateCourseCommand(id, request);
            var response = await mediator.Send(command, cancellationToken);
            return HandleResult(response, "Course updated successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteCourseCommand(id);
            await mediator.Send(command, cancellationToken);
            return HandleNoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}
