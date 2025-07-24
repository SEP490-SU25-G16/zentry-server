using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.Modules.AttendanceManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule; // Cần thiết cho GetClassSectionIdByScheduleIdIntegrationQuery
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.GetSessionFinalAttendance;

public class GetSessionFinalAttendanceQueryHandler(
    IAttendanceRecordRepository attendanceRecordRepository,
    ISessionRepository sessionRepository, // Thêm ISessionRepository
    IMediator mediator)
    : IQueryHandler<GetSessionFinalAttendanceQuery, List<FinalAttendanceDto>>
{
    public async Task<List<FinalAttendanceDto>> Handle(GetSessionFinalAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        if (session is null)
            throw new NotFoundException("Session", request.SessionId);

        var classSectionIdResponse = await mediator.Send(
            new GetClassSectionIdByScheduleIdIntegrationQuery(session.ScheduleId),
            cancellationToken);

        var classSectionId = classSectionIdResponse.ClassSectionId;

        if (classSectionId == Guid.Empty)
            throw new NotFoundException("ClassSection", $"for ScheduleId {session.ScheduleId}.");


        var enrollmentsResponse = await mediator.Send(
            new GetEnrollmentsByClassSectionIdIntegrationQuery(classSectionId),
            cancellationToken);

        if (!enrollmentsResponse.Enrollments.Any())
            return [];

        var enrollments = enrollmentsResponse.Enrollments;

        // Lấy tất cả bản ghi điểm danh trong session
        var attendanceRecords = await attendanceRecordRepository.GetAttendanceRecordsBySessionIdAsync(
            request.SessionId, cancellationToken);

        var studentIds = enrollments.Select(e => e.StudentId).ToList();
        var usersResponse = await mediator.Send(new GetUsersByIdsIntegrationQuery(studentIds), cancellationToken);
        var userDict = usersResponse.Users.ToDictionary(u => u.Id, u => u); // Truy cập thuộc tính Users từ response DTO

        var finalAttendance = new List<FinalAttendanceDto>();

        foreach (var enrollment in enrollments)
        {
            var studentId = enrollment.StudentId;
            var user = userDict.GetValueOrDefault(studentId);

            var lastAttendanceRecord = attendanceRecords
                .Where(ar => ar.UserId == studentId)
                .OrderByDescending(ar => ar.CreatedAt)
                .FirstOrDefault();

            var status = lastAttendanceRecord?.Status ?? AttendanceStatus.Absent;

            finalAttendance.Add(new FinalAttendanceDto
            {
                StudentId = studentId,
                StudentFullName = user?.FullName,
                Status = status.ToString()
            });
        }

        return finalAttendance;
    }
}
