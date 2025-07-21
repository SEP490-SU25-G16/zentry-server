using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;
using Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{
    [HttpPost("enroll-student")]
    [ProducesResponseType(typeof(EnrollmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrollStudent([FromBody] EnrollStudentRequest request)
    {
        var command = new EnrollStudentCommand
        {
            ClassSectionId = request.ClassSectionId,
            StudentId = request.StudentId
        };

        var response = await mediator.Send(command);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetEnrollmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEnrollments([FromQuery] GetEnrollmentsRequest request)
    {
        var query = new GetEnrollmentsQuery(request);

        var response = await mediator.Send(query);

        return Ok(response);
    }
}