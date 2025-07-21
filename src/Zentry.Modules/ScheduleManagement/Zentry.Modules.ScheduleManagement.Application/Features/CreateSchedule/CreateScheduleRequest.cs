using System.ComponentModel.DataAnnotations;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public record CreateScheduleRequest(
    [Required] Guid LecturerId,
    [Required] Guid ClassSectionId,
    [Required] Guid RoomId,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required] TimeOnly StartTime,
    [Required] TimeOnly EndTime,
    [Required] string WeekDay
);
