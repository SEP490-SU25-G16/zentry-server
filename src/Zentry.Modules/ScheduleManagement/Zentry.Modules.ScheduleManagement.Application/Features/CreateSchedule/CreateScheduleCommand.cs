using System.ComponentModel.DataAnnotations;
using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public record CreateScheduleCommand(
    [Required] Guid LecturerId,
    [Required] Guid CourseId,
    [Required] Guid RoomId,
    [Required] DateTime StartTime,
    [Required] DateTime EndTime,
    [Required] DayOfWeekEnum DayOfWeek // Enum sẽ được binding từ string (ví dụ: "Monday")
) : ICommand<ScheduleCreatedResponseDto>
{
    public bool IsValidTimeRange()
    {
        return StartTime < EndTime;
    }
}
