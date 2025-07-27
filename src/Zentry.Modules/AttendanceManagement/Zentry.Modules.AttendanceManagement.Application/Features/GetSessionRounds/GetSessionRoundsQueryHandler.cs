using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.GetSessionRounds;

public class GetSessionRoundsQueryHandler(
    IRoundRepository roundRepository,
    IAttendanceRecordRepository attendanceRecordRepository,
    ISessionRepository sessionRepository,
    IMediator mediator)
    : IQueryHandler<GetSessionRoundsQuery, List<RoundAttendanceDto>>
{
    public async Task<List<RoundAttendanceDto>> Handle(GetSessionRoundsQuery request,
        CancellationToken cancellationToken)
    {
        var rounds = await roundRepository.GetRoundsBySessionIdAsync(request.SessionId, cancellationToken);
        if (rounds is null || rounds.Count == 0)
            throw new NotFoundException("Rounds for Session", request.SessionId);

        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            throw new NotFoundException("Session", request.SessionId);

        // Lấy thông tin ClassSection từ Schedule module
        var classSectionResponse =
            await mediator.Send(
                new GetClassSectionByScheduleIdIntegrationQuery(session
                    .ScheduleId), // <-- Sửa Query để lấy cả ClassSection info
                cancellationToken);

        if (classSectionResponse == null || classSectionResponse.ClassSectionId == Guid.Empty)
            throw new NotFoundException("ClassSection", $"corresponding to ScheduleId {session.ScheduleId}");

        var classSectionId = classSectionResponse.ClassSectionId;
        var courseId = classSectionResponse.CourseId; // <-- Lấy CourseId
        var courseCode = classSectionResponse.CourseCode; // <-- Lấy CourseCode
        var courseName = classSectionResponse.CourseName; // <-- Lấy CourseName
        var sectionCode = classSectionResponse.SectionCode; // <-- Lấy SectionCode

        // Lấy tổng số sinh viên đăng ký
        var totalStudentsCountResponse = await mediator.Send(
            new CountActiveStudentsByClassSectionIdIntegrationQuery(classSectionId),
            cancellationToken);

        var allAttendanceRecords = await attendanceRecordRepository.GetAttendanceRecordsBySessionIdAsync(
            request.SessionId, cancellationToken);

        var result = new List<RoundAttendanceDto>();

        foreach (var round in rounds)
        {
            var attendedCount = allAttendanceRecords
                .Count(ar =>
                    ar.CreatedAt >= round.StartTime && (round.EndTime == null || ar.CreatedAt <= round.EndTime));

            result.Add(new RoundAttendanceDto
            {
                RoundId = round.Id, // <-- Gán RoundId
                SessionId = request.SessionId, // <-- Gán SessionId

                RoundNumber = round.RoundNumber,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                AttendedCount = attendedCount,
                TotalStudents = totalStudentsCountResponse.TotalStudents,
                Status = round.Status.ToString(),
                CreatedAt = round.CreatedAt,
                UpdatedAt = round.UpdatedAt,

                CourseId = courseId, // <-- Gán CourseId
                CourseCode = courseCode, // <-- Gán CourseCode
                CourseName = courseName, // <-- Gán CourseName

                ClassSectionId = classSectionId, // <-- Gán ClassSectionId
                SectionCode = sectionCode // <-- Gán SectionCode
            });
        }

        return result.OrderBy(r => r.RoundNumber).ToList();
    }
}
