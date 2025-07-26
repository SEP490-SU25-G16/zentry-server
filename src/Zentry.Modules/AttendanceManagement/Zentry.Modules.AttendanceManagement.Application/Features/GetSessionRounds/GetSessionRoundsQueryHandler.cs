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
            throw new NotFoundException("Session", request.SessionId);

        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            throw new NotFoundException("Session", request.SessionId);

        var classSectionIdResponse =
            await mediator.Send(new GetClassSectionIdByScheduleIdIntegrationQuery(session.ScheduleId),
                cancellationToken);
        var classSectionId = classSectionIdResponse.ClassSectionId;

        if (classSectionId == Guid.Empty)
            throw new NotFoundException("ClassSection", $"corresponding to ScheduleId {session.ScheduleId}");

        var resultCount = await mediator.Send(
            new CountActiveStudentsByClassSectionIdIntegrationQuery(classSectionId),
            cancellationToken);

        var allAttendanceRecords = await attendanceRecordRepository.GetAttendanceRecordsBySessionIdAsync(
            request.SessionId, cancellationToken);

        var result = new List<RoundAttendanceDto>();

        // Lấy CourseCode và SectionCode từ SessionConfigs
        // Giả sử SessionConfigSnapshot có thể truy cập các giá trị này qua GetConfig hoặc tương tự
        var courseCode = session.GetConfig<string>("courseCode");
        var sectionCode = session.GetConfig<string>("sectionCode");

        foreach (var round in rounds)
        {
            var attendedCount = allAttendanceRecords
                .Count(ar =>
                    ar.CreatedAt >= round.StartTime && (round.EndTime == null || ar.CreatedAt <= round.EndTime));

            result.Add(new RoundAttendanceDto
            {
                RoundNumber = round.RoundNumber,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                AttendedCount = attendedCount,
                TotalStudents = resultCount.TotalStudents,
                Status = round.Status.ToString(),
                CreatedAt = round.CreatedAt,
                UpdatedAt = round.UpdatedAt,
                CourseCode = courseCode,
                SectionCode = sectionCode
            });
        }

        return result;
    }
}
