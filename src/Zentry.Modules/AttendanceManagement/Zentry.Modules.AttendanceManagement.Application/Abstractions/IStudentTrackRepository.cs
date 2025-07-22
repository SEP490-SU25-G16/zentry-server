using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IStudentTrackRepository
{
    Task AddOrUpdateAsync(StudentTrack studentTrack, CancellationToken cancellationToken);
    Task<StudentTrack?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task<StudentTrack?> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken); // Thêm để dễ truy vấn
}
