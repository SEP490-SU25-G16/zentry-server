using MediatR;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;

namespace Zentry.Modules.AttendanceManagement.Application.Features.ViewAttendanceRate;

public class ViewAttendanceRateQueryHandler(IAttendanceRepository attendanceRepository)
    : IRequestHandler<ViewAttendanceRateQuery, AttendanceRateDto>
{
    public async Task<AttendanceRateDto> Handle(ViewAttendanceRateQuery request, CancellationToken cancellationToken)
    {
        var (totalSessions, attendedSessions) =
            await attendanceRepository.GetAttendanceStatsAsync(request.StudentId, request.CourseId);

        if (totalSessions == 0)
            return new AttendanceRateDto
            {
                CourseId = request.CourseId,
                StudentId = request.StudentId,
                AttendanceRate = 0,
                AbsenceStatus = "NoData",
                TotalSessions = 0,
                AttendedSessions = 0
            };

        var attendanceRate = (double)attendedSessions / totalSessions * 100;
        var threshold = 80.0;

        var absenceStatus = attendanceRate >= threshold ? "Normal"
            : attendanceRate > threshold * 0.5 ? "Warning"
            : "Critical";

        return new AttendanceRateDto
        {
            CourseId = request.CourseId,
            StudentId = request.StudentId,
            AttendanceRate = Math.Round(attendanceRate, 1),
            AbsenceStatus = absenceStatus,
            TotalSessions = totalSessions,
            AttendedSessions = attendedSessions
        };
    }
}