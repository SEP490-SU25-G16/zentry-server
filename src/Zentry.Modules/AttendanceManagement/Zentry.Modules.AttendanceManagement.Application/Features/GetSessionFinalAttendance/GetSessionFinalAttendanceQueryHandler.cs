using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Enums.Attendance;
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

        // Lấy CourseCode và SectionCode từ SessionConfigs của Session
        var courseCode = session.GetConfig<string>("courseCode");
        var sectionCode = session.GetConfig<string>("sectionCode");
        var classInfo = string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(sectionCode)
            ? null
            : $"{courseCode} - {sectionCode}";

        var classSectionIdResponse = await mediator.Send(
            new GetClassSectionIdByScheduleIdIntegrationQuery(session.ScheduleId),
            cancellationToken);

        var classSectionId = classSectionIdResponse.ClassSectionId;

        if (classSectionId == Guid.Empty)
            throw new NotFoundException("ClassSection", $"for ScheduleId {session.ScheduleId}.");


        var enrollmentsResponse = await mediator.Send(
            new GetEnrollmentsByClassSectionIdIntegrationQuery(classSectionId),
            cancellationToken);

        if (enrollmentsResponse.Enrollments.Count == 0)
            return []; // Trả về danh sách rỗng nếu không có sinh viên nào đăng ký

        var enrollments = enrollmentsResponse.Enrollments;

        // Lấy tất cả bản ghi điểm danh trong session
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
                Email = user?.Email, // Gán email
                Status = attendanceStatus.ToString(), // Trạng thái Present/Late/Absent

                EnrolledAt = enrollment.EnrolledAt, // Gán ngày đăng ký
                EnrollmentStatus = enrollment.Status, // Gán trạng thái đăng ký

                ClassInfo = classInfo, // Gán thông tin lớp học
                SessionStartTime = session.StartTime, // Gán thời gian bắt đầu session

                DetailedAttendanceStatus = attendanceStatus.ToString(), // Gán trạng thái điểm danh chi tiết
                LastAttendanceTime =
                    lastAttendanceRecord?.CreatedAt ?? session.StartTime // Gán thời gian điểm danh cuối cùng
            });
        }

        return finalAttendance;
    }
}