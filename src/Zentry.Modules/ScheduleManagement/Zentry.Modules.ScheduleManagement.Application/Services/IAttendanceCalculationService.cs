using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Contracts.Attendance;

namespace Zentry.Modules.ScheduleManagement.Application.Services;

public interface IAttendanceCalculationService
{
    double CalculateAttendanceRate(
        List<OverviewSessionDto> classSessions,
        List<OverviewAttendanceDto> classAttendanceRecords,
        int enrolledStudentsCount);
}
