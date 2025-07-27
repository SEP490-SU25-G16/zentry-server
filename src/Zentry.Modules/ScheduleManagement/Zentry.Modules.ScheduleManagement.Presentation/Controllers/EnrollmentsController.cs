using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Features.EnrollMultipleStudents;
using Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;
using Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IMediator mediator) : BaseController
{
    [HttpPost("bulk-enroll-students")]
    [ProducesResponseType(typeof(ApiResponse<BulkEnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkEnrollStudents([FromBody] BulkEnrollStudentsRequest request)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var command = new BulkEnrollStudentsCommand
            {
                ClassSectionId = request.ClassSectionId,
                StudentIds = request.StudentIds
            };
            var response = await mediator.Send(command);
            return HandleResult(response, "Students bulk enrolled successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("enroll-student")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrollStudent([FromBody] EnrollStudentRequest request)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var command = new EnrollStudentCommand
            {
                ClassSectionId = request.ClassSectionId,
                StudentId = request.StudentId
            };
            var response = await mediator.Send(command);
            return HandleResult(response, "Student enrolled successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetEnrollmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEnrollments([FromQuery] GetEnrollmentsRequest request)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var query = new GetEnrollmentsQuery(request);
            var response = await mediator.Send(query);
            return HandleResult(response, "Enrollments retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}