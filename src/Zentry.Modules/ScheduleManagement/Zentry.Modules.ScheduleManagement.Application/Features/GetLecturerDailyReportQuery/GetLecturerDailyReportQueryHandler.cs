using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Contracts.User;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerDailyReportQuery;

public class GetLecturerDailyReportQueryHandler(
    IScheduleRepository scheduleRepository,
    IEnrollmentRepository enrollmentRepository,
    IMediator mediator)
    : IQueryHandler<GetLecturerDailyReportQuery, List<LecturerDailyReportDto>>
{
    public async Task<List<LecturerDailyReportDto>> Handle(
        GetLecturerDailyReportQuery request,
        CancellationToken cancellationToken)
    {
        // Lấy thông tin giảng viên
        var getUserQuery = new GetUserByIdAndRoleIntegrationQuery(Role.Lecturer, request.LecturerId);
        var lecturerInfo = await mediator.Send(getUserQuery, cancellationToken);
        var lecturerName = lecturerInfo?.FullName ?? "N/A";

        var dayOfWeek = request.Date.DayOfWeek.ToWeekDayEnum();

        // Lấy các lịch trình của giảng viên cho ngày đã cho.
        // Cần đảm bảo repository đã include ClassSection, Course và Room.
        var schedules = await scheduleRepository.GetLecturerSchedulesForDateAsync(
            request.LecturerId,
            request.Date,
            dayOfWeek,
            cancellationToken);

        var result = new List<LecturerDailyReportDto>();

        foreach (var schedule in schedules)
        {
            // Kiểm tra null cho các navigation properties
            var classSection = schedule.ClassSection;
            var course = classSection?.Course;
            var room = schedule.Room;

            // Bỏ qua nếu dữ liệu không đủ để tạo báo cáo
            if (classSection == null || course == null || room == null)
                // Log warning nếu cần
                continue;

            // Lấy tổng số sinh viên đăng ký từ module Schedule
            var totalStudents = await enrollmentRepository.CountActiveStudentsByClassSectionIdAsync(
                schedule.ClassSectionId, cancellationToken);

            // Lấy thông tin điểm danh từ module Attendance
            // GetAttendanceSummaryIntegrationQuery cần một SessionId cụ thể để hoạt động hiệu quả.
            // Nếu bạn muốn báo cáo cho một ngày, bạn cần lấy SessionId của buổi học diễn ra trong ngày đó.
            // Điều này đòi hỏi một truy vấn Session khác hoặc GetAttendanceSummaryIntegrationQuery có thể tổng hợp theo ScheduleId và Date.
            // Giả định: GetAttendanceSummaryIntegrationQuery có thể xử lý ScheduleId và Date để tìm session tương ứng.
            // HOẶC: Bạn cần tìm SessionId tương ứng với schedule.Id và request.Date ở đây.

            // Ví dụ, tìm session diễn ra trong ngày này cho schedule này (nếu có 1 session/ngày/schedule)
            // (Bạn cần một IAttendanceService hoặc IAttendanceRepository để lấy Session Entity)
            // Hoặc GetSessionsByScheduleIdIntegrationQuery đã dùng trước đó có thể giúp.
            var getSessionsQuery = new GetSessionsByScheduleIdIntegrationQuery(schedule.Id);
            var sessionsForSchedule = await mediator.Send(getSessionsQuery, cancellationToken);

            var targetSession = sessionsForSchedule.FirstOrDefault(s => s.StartTime.Date == request.Date.Date);

            if (targetSession == null)
                // Nếu không tìm thấy session cụ thể cho ngày này, có thể bỏ qua báo cáo cho schedule này hoặc báo cáo với 0 học sinh
                // Tùy thuộc vào yêu cầu nghiệp vụ
                continue; // Bỏ qua lịch trình này nếu không có session tương ứng trong ngày

            // Truyền SessionId cụ thể vào GetAttendanceSummaryIntegrationQuery
            var attendanceSummary = await mediator.Send(
                new GetAttendanceSummaryIntegrationQuery(targetSession.SessionId, schedule.ClassSectionId,
                    request.Date), // Cần SessionId
                cancellationToken);

            var attendedCount = attendanceSummary.PresentCount; // Tổng số có mặt (đúng giờ + muộn)
            var total = totalStudents > 0 ? totalStudents : 1; // Tránh chia cho 0

            result.Add(new LecturerDailyReportDto
            {
                // Gán các ID
                LecturerId = request.LecturerId, // <-- Gán LecturerId
                ClassSectionId = classSection.Id, // <-- Gán ClassSectionId
                CourseId = course.Id, // <-- Gán CourseId
                ScheduleId = schedule.Id, // <-- Gán ScheduleId
                RoomId = room.Id, // <-- Gán RoomId
                // SessionId = targetSession.SessionId, // <-- Tùy chọn, nếu cần SessionId cụ thể
                ReportDate = DateOnly.FromDateTime(request.Date), // <-- Gán ReportDate

                LecturerName = lecturerName,
                CourseCode = course.Code,
                CourseName = course.Name,
                SectionCode = classSection.SectionCode,
                RoomInfo = $"{room.RoomName} - {room.Building}",
                TimeSlot = $"{schedule.StartTime.ToShortTimeString()} - {schedule.EndTime.ToShortTimeString()}",
                TotalStudents = totalStudents,
                AttendedStudents = attendedCount,
                PresentStudents = attendanceSummary.PresentCount,
                AbsentStudents = attendanceSummary.AbsentCount,
                AttendanceRate = totalStudents > 0 ? $"{Math.Round(attendedCount * 100.0 / total, 1)}%" : "0%",
                OnTimeRate = totalStudents > 0
                    ? $"{Math.Round(attendanceSummary.PresentCount * 100.0 / total, 1)}%"
                    : "0%" // Tỷ lệ đúng giờ
            });
        }

        return result.OrderBy(r => r.TimeSlot).ToList(); // Sắp xếp theo giờ học
    }
}