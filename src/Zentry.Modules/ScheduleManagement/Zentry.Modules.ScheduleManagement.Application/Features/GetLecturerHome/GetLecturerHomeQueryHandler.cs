using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Attendance;
using Zentry.SharedKernel.Enums.Attendance;
using Zentry.SharedKernel.Enums.User;
// Cần dùng để truy vấn sessions
// Đảm bảo có using này

// Cần MediatR để gửi GetSessionsByScheduleIdIntegrationQuery

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerHome;

public class GetLecturerHomeQueryHandler(
    IClassSectionRepository classSectionRepository,
    IMediator mediator, // Thêm MediatR để gửi query lấy sessions
    IUserScheduleService userScheduleService // Thêm UserScheduleService
) : IQueryHandler<GetLecturerHomeQuery, List<LecturerHomeDto>>
{
    public async Task<List<LecturerHomeDto>> Handle(GetLecturerHomeQuery request, CancellationToken cancellationToken)
    {
        var lecturer =
            await userScheduleService.GetUserByIdAndRoleAsync(Role.Lecturer, request.LecturerId, cancellationToken);
        var lecturerName = lecturer?.FullName ?? "N/A";

        // Lấy tất cả ClassSection của giảng viên, bao gồm Course, Schedules và Enrollments
        var classSections =
            await classSectionRepository.GetLecturerClassSectionsAsync(request.LecturerId, cancellationToken);

        var result = new List<LecturerHomeDto>();

        foreach (var cs in classSections)
        {
            // Lấy tất cả các sessions cho ClassSection này thông qua Schedules của nó
            // Đây là phần tính toán phức tạp hơn
            var totalSessions = 0;
            var completedSessions = 0;

            foreach (var schedule in cs.Schedules)
            {
                var getSessionsQuery = new GetSessionsByScheduleIdIntegrationQuery(schedule.Id);
                var allSessionsForSchedule = await mediator.Send(getSessionsQuery, cancellationToken);

                totalSessions += allSessionsForSchedule.Count;

                completedSessions += allSessionsForSchedule
                    .Count(s => s.Status == SessionStatus.Completed.ToString() ||
                                s.Status == SessionStatus.Active.ToString());
            }

            result.Add(new LecturerHomeDto
            {
                CourseCode = cs.Course?.Code ?? string.Empty,
                CourseName = cs.Course?.Name ?? string.Empty,
                SectionCode = cs.SectionCode,
                EnrolledStudents = cs.Enrollments.Count,
                TotalSessions = totalSessions,
                SessionProgress = $"Buổi {completedSessions}/{totalSessions}", // Tính toán và gán
                Schedules = cs.Schedules.Select(s => new ScheduleInfoDto
                {
                    RoomInfo = $"{s.Room?.RoomName} ({s.Room?.Building})",
                    ScheduleInfo = $"{s.WeekDay} {s.StartTime.ToShortTimeString()}-{s.EndTime.ToShortTimeString()}"
                }).ToList(),
                LecturerName = lecturerName
            });
        }

        return result;
    }
}