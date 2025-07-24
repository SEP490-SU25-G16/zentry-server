using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Helpers;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Contracts.User; // Cần thêm namespace này để dùng GetUserByIdAndRoleIntegrationQuery

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
        var getUserQuery = new GetUserByIdAndRoleIntegrationQuery("Lecturer", request.LecturerId);
        var lecturerInfo = await mediator.Send(getUserQuery, cancellationToken);

        var dayOfWeek = request.Date.DayOfWeek.ToWeekDayEnum();
        var schedules = await scheduleRepository.GetLecturerSchedulesForDateAsync(
            request.LecturerId,
            request.Date,
            dayOfWeek,
            cancellationToken);

        var result = new List<LecturerDailyReportDto>();

        foreach (var schedule in schedules)
        {
            // Lấy tổng số sinh viên đăng ký từ module Schedule
            var totalStudents = await enrollmentRepository.CountActiveStudentsByClassSectionIdAsync(
                schedule.ClassSectionId, cancellationToken);

            // Lấy thông tin điểm danh từ module Attendance
            var attendanceSummary = await mediator.Send(
                new GetAttendanceSummaryIntegrationQuery(schedule.Id, schedule.ClassSectionId, request.Date),
                cancellationToken);

            var attendedCount = attendanceSummary.PresentCount;
            var total = totalStudents > 0 ? totalStudents : 1;

            result.Add(new LecturerDailyReportDto
            {
                // Lấy tên giảng viên từ kết quả của mediator.Send
                LecturerName = lecturerInfo?.FullName,
                CourseCode = schedule.ClassSection?.Course?.Code!,
                CourseName = schedule.ClassSection?.Course?.Name!,
                SectionCode = schedule.ClassSection?.SectionCode!,
                RoomInfo = $"{schedule.Room?.RoomName} - {schedule.Room?.Building}",
                TimeSlot = $"{schedule.StartTime.ToShortTimeString()} - {schedule.EndTime.ToShortTimeString()}",
                TotalStudents = totalStudents,
                AttendedStudents = attendedCount,
                PresentStudents = attendanceSummary.PresentCount,
                AbsentStudents = attendanceSummary.AbsentCount,
                AttendanceRate = $"{Math.Round((attendedCount * 100.0 / total), 1)}%",
                OnTimeRate = $"{Math.Round((attendanceSummary.PresentCount * 100.0 / total), 1)}%"
            });
        }

        return result;
    }
}
