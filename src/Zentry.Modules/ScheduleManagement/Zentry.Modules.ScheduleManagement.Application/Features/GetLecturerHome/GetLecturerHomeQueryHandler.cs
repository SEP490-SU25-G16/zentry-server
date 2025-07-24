using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetLecturerHome;

public class GetLecturerHomeQueryHandler(IClassSectionRepository classSectionRepository)
    : IQueryHandler<GetLecturerHomeQuery, List<LecturerHomeDto>>
{
    public async Task<List<LecturerHomeDto>> Handle(GetLecturerHomeQuery request, CancellationToken cancellationToken)
    {
        // Lấy tất cả ClassSection của giảng viên, bao gồm Course, Schedules và Enrollments
        var classSections = await classSectionRepository.GetLecturerClassSectionsAsync(request.LecturerId, cancellationToken);

        var result = classSections.Select(cs => new LecturerHomeDto
        {
            CourseCode = cs.Course?.Code ?? string.Empty,
            CourseName = cs.Course?.Name ?? string.Empty,
            SectionCode = cs.SectionCode,
            EnrolledStudents = cs.Enrollments.Count,
            TotalSessions = cs.Schedules.Count, // Tính total sessions từ số lượng schedule
            Schedules = cs.Schedules.Select(s => new ScheduleInfoDto
            {
                RoomInfo = $"{s.Room?.RoomName} ({s.Room?.Building})",
                ScheduleInfo = $"{s.WeekDay} {s.StartTime.ToShortTimeString()}-{s.EndTime.ToShortTimeString()}"
            }).ToList()
        }).ToList();

        return result;
    }
}
