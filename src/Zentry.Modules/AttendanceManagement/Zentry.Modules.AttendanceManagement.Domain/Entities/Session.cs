using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class Session : AggregateRoot<Guid>
{
    // Private constructor cho EF Core
    private Session() : base(Guid.Empty)
    {
        SessionConfigs = new SessionConfigSnapshot(0, 0, 0, 0, 0);
    }

    // Constructor chính, nhận ID và các thuộc tính cơ bản
    private Session(Guid id, Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime, SessionConfigSnapshot sessionConfigs)
        : base(id)
    {
        ScheduleId = scheduleId;
        UserId = userId; // Đổi tên từ LecturerId thành UserId cho phù hợp với request.UserId
        StartTime = startTime;
        EndTime = endTime;
        CreatedAt = DateTime.UtcNow;
        SessionConfigs = sessionConfigs; // Gán Value Object ở đây
    }

    public Guid ScheduleId { get; private set; }
    public Guid UserId { get; private set; } // Đã đổi tên
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Thêm thuộc tính SessionConfigs kiểu Value Object
    public SessionConfigSnapshot SessionConfigs { get; private set; } // private set để EF Core có thể hydrate

    // Factory method để tạo Session, nhận thêm SessionConfigSnapshot
    public static Session Create(Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime, SessionConfigSnapshot sessionConfigs)
    {
        return new Session(Guid.NewGuid(), scheduleId, userId, startTime, endTime, sessionConfigs);
    }

    public void Update(DateTime? startTime = null, DateTime? endTime = null)
    {
        if (startTime.HasValue) StartTime = startTime.Value;
        if (endTime.HasValue) EndTime = endTime.Value;
        // UpdatedAt cần được thêm vào nếu bạn muốn theo dõi
        // UpdatedAt = DateTime.UtcNow; // Thêm vào nếu Session có CreatedAt và UpdatedAt như AggregateRoot
    }

    // --- Các hành vi nghiệp vụ sử dụng SessionConfigs ---
    public bool IsWithinAttendanceWindow(DateTime currentTime)
    {
        var sessionStartTimeLimit = StartTime.Subtract(TimeSpan.FromMinutes(SessionConfigs.AttendanceWindowMinutes));
        var sessionEndTimeLimit = EndTime.Add(TimeSpan.FromMinutes(SessionConfigs.AttendanceWindowMinutes));
        return currentTime >= sessionStartTimeLimit && currentTime <= sessionEndTimeLimit;
    }

    // Các phương thức khác của Session có thể sử dụng SessionConfigs
    // public bool ShouldVerifyFaceId(TimeSpan elapsed) => elapsed.TotalSeconds > SessionConfigs.FaceIdVerificationTimeoutSeconds;
    // public int GetRemainingRounds(int currentRound) => SessionConfigs.TotalAttendanceRounds - currentRound;
}
