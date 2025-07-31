using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.GetSessionFinalAttendance;

public class GetSessionFinalAttendanceQueryHandler(
    IAttendanceRecordRepository attendanceRecordRepository,
    ISessionRepository sessionRepository,
    IMediator mediator)
    : IQueryHandler<GetSessionFinalAttendanceQuery, List<FinalAttendanceDto>>
{
    public async Task<List<FinalAttendanceDto>> Handle(GetSessionFinalAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            throw new NotFoundException("Session", request.SessionId);

        var classSectionResponse = await mediator.Send(
            new GetClassSectionByScheduleIdIntegrationQuery(session.ScheduleId),
            cancellationToken);

        if (classSectionResponse == null || classSectionResponse.ClassSectionId == Guid.Empty)
            throw new NotFoundException("ClassSection", $"for ScheduleId {session.ScheduleId}.");

        var classSectionId = classSectionResponse.ClassSectionId;
        var courseId = classSectionResponse.CourseId;
        var courseCode = classSectionResponse.CourseCode;
        var courseName = classSectionResponse.CourseName;
        var sectionCode = classSectionResponse.SectionCode;
        var classInfo = string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(sectionCode)
            ? null
            : $"{courseCode} - {sectionCode}";

        var enrollmentsResponse = await mediator.Send(
            new GetEnrollmentsByClassSectionIdIntegrationQuery(classSectionId),
            cancellationToken);

        if (enrollmentsResponse.Enrollments.Count == 0)
            return [];

        var enrollments = enrollmentsResponse.Enrollments;

        var attendanceRecords = await attendanceRecordRepository.GetAttendanceRecordsBySessionIdAsync(
            request.SessionId, cancellationToken);

        var studentIds = enrollments.Select(e => e.StudentId).ToList();
        var usersResponse = await mediator.Send(new GetUsersByIdsIntegrationQuery(studentIds), cancellationToken);
        var userDict = usersResponse.Users.ToDictionary(u => u.Id, u => u);

        var finalAttendance = new List<FinalAttendanceDto>();

        foreach (var enrollment in enrollments)
        {
            var studentId = enrollment.StudentId;
            var user = userDict.GetValueOrDefault(studentId);

            var lastAttendanceRecord = attendanceRecords
                .Where(ar => ar.UserId == studentId)
                .OrderByDescending(ar => ar.CreatedAt)
                .FirstOrDefault();

            var attendanceStatus = lastAttendanceRecord?.Status ?? AttendanceStatus.Absent;

            finalAttendance.Add(new FinalAttendanceDto
            {
                StudentId = studentId,
                StudentFullName = user?.FullName,
                Email = user?.Email,
                Status = attendanceStatus.ToString(),

                EnrollmentId = enrollment.Id,
                EnrolledAt = enrollment.EnrolledAt,
                EnrollmentStatus = enrollment.Status,

                SessionId = request.SessionId,
                ClassSectionId = classSectionId,
                ScheduleId = session.ScheduleId,
                CourseId = courseId,
                ClassInfo = classInfo,
                SessionStartTime = session.StartTime,

                LastAttendanceRecordId = lastAttendanceRecord?.Id,
                DetailedAttendanceStatus = attendanceStatus.ToString(),
            });
        }

        return finalAttendance.OrderBy(fa => fa.StudentFullName).ToList();
    }
}
