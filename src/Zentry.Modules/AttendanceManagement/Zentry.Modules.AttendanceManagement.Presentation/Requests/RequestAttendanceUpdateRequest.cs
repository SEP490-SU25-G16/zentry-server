using System.ComponentModel.DataAnnotations;

namespace Zentry.Modules.AttendanceManagement.Presentation.Requests;

public class RequestAttendanceUpdateRequest
{
    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string Reason { get; set; } = string.Empty;
}
