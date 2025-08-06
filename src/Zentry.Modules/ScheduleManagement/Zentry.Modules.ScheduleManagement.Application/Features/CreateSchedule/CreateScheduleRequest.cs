using System.ComponentModel.DataAnnotations;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreateScheduleRequest
{
    // Sử dụng [Required] để đảm bảo các trường Guid không phải null
    [Required] public Guid ClassSectionId { get; set; }

    [Required] public Guid RoomId { get; set; }

    [Required] public DateOnly StartDate { get; set; }

    [Required] public DateOnly EndDate { get; set; }

    [Required] public TimeOnly StartTime { get; set; }

    [Required] public TimeOnly EndTime { get; set; }

    // Sử dụng [Required] và [StringLength] để validation chuỗi
    [Required]
    [StringLength(10)] // Ví dụ: "Monday", "Tuesday"
    public string WeekDay { get; set; } = string.Empty;
}
