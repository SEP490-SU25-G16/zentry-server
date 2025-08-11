using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.AssignLecturer;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.CreateClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.DeleteClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetAllClassSectionsWithEnrollmentCount;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetClassOverview;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetClassSectionById;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetClassSections;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetClassSessions;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetSessionsByClassSectionId;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetStudentClasses;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetStudentCountBySemester;
using Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.UpdateClassSection;
using Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetLecturerDailyReportQuery;
using Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetLecturerNextSessions;
using Zentry.Modules.ScheduleManagement.Application.Features.Schedules.GetLecturerWeeklyOverview;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/class-sections")]
public class ClassSectionsController(IMediator mediator) : BaseController
{
    // === API CRUD Cơ bản (Primary CRUD APIs) ===

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetClassSectionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClassSections([FromQuery] GetClassSectionsQuery query,
        CancellationToken cancellationToken)
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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClassSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await mediator.Send(new GetClassSectionByIdQuery(id), cancellationToken);
            return HandleResult(dto);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateClassSectionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClassSection([FromBody] CreateClassSectionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateClassSectionCommand(
                request.CourseId,
                request.SectionCode,
                request.Semester
            );
            var response = await mediator.Send(command, cancellationToken);
            return HandleCreated(response, nameof(CreateClassSection), new { id = response.Id });
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClassSection(Guid id, [FromBody] UpdateClassSectionRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var cmd = new UpdateClassSectionCommand(id, req.SectionCode, req.Semester);
            await mediator.Send(cmd, cancellationToken);
            return HandleNoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(new DeleteClassSectionCommand(id), cancellationToken);
            return HandleNoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // === API liên quan đến các vai trò và dữ liệu đặc biệt ===

    [HttpGet("all-with-enrollment-count")]
    [ProducesResponseType(typeof(ApiResponse<List<ClassSectionWithEnrollmentCountDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllClassSectionsWithEnrollmentCount(CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAllClassSectionsWithEnrollmentCountQuery();
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{classSectionId}/sessions")]
    [ProducesResponseType(typeof(ApiResponse<List<SessionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionsByScheduleId(Guid classSectionId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetSessionsByClassSectionIdQuery(classSectionId);
            var result = await mediator.Send(query, cancellationToken);
            return HandleResult(result, "Sessions retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // === API liên quan đến vai trò giảng viên ===

    [HttpGet("{lecturerId}/home")]
    [ProducesResponseType(typeof(ApiResponse<LecturerHomeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLecturerHome(Guid lecturerId, CancellationToken cancellationToken)
    {
        try
        {
            var nextSessions = await mediator.Send(new GetLecturerNextSessionsQuery(lecturerId), cancellationToken);

            var weeklyOverview = await mediator.Send(new GetLecturerWeeklyOverviewQuery(lecturerId), cancellationToken);

            var response = new LecturerHomeResponse
            {
                NextSessions = nextSessions,
                WeeklyOverview = weeklyOverview
            };

            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{classId}/overview-sessions")]
    [ProducesResponseType(typeof(ApiResponse<ClassDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassDetails(
        [FromRoute] Guid classId,
        CancellationToken cancellationToken)
    {
        try
        {
            var overviewQuery = new GetClassOverviewQuery { ClassId = classId };
            var sessionsQuery = new GetClassSessionsQuery { ClassId = classId };

            var overview = await mediator.Send(overviewQuery, cancellationToken);
            var sessions = await mediator.Send(sessionsQuery, cancellationToken);


            var response = new ClassDetailDto
            {
                Overview = overview.Data,
                Sessions = sessions.Data
            };

            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{studentId}/classes")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentClassDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentClasses(Guid studentId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetStudentClassesQuery { StudentId = studentId };
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response.Data);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{lecturerId}/report/daily")]
    [ProducesResponseType(typeof(ApiResponse<List<LecturerDailyReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLecturerDailyReport(
        Guid lecturerId,
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        try
        {
            var queryDate = date?.ToUniversalTime() ?? DateTime.UtcNow;
            var query = new GetLecturerDailyReportQuery(lecturerId, queryDate);
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("{classSectionId}/lecturers/{lecturerId}")]
    [ProducesResponseType(typeof(AssignLecturerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignLecturer(
        [FromRoute] Guid classSectionId,
        [FromRoute] Guid lecturerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AssignLecturerCommand(classSectionId, lecturerId);
            var result = await mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
    [HttpGet("student-count/year/{year}")]
    [ProducesResponseType(typeof(ApiResponse<GetStudentCountBySemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStudentCountByYear(int year, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetStudentCountBySemesterQuery(year);
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}
