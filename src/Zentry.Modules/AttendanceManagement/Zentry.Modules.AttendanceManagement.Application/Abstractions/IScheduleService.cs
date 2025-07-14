using Zentry.SharedKernel.Contracts;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IScheduleService
{
    // Cần một phương thức để lấy thông tin chi tiết của lịch trình (Schedule)
    // bao gồm StartTime, EndTime của buổi học và ID của giảng viên được phân công.
    Task<ScheduleLookupDto?> GetScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken);

    // Có thể cần phương thức để kiểm tra xem giảng viên có được phân công cho buổi học không
    // Hoặc thông tin này được trả về trong ScheduleLookupDto
    Task<bool> IsLecturerAssignedToScheduleAsync(Guid lecturerId, Guid scheduleId, CancellationToken cancellationToken);
}
