using System.ComponentModel.DataAnnotations;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public record CreateScheduleRequest(
    [Required] Guid LecturerId,
    [Required] Guid ClassSectionId,
    [Required] Guid RoomId,
    [Required] DateTime StartTime,
    [Required] DateTime EndTime,
    [Required] string DayOfWeek
);
