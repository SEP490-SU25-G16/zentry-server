namespace Zentry.Modules.AttendanceManagement.Presentation.Requests;

public class CreateSessionRequest
{
    public Guid ScheduleId { get; set; }
    public Guid UserId { get; set; } // ID của giảng viên tạo session
}