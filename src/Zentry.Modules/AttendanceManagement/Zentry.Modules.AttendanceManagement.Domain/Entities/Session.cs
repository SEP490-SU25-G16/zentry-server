using Zentry.Modules.AttendanceManagement.Domain.Enums;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Domain;
using Zentry.SharedKernel.Exceptions; // Thêm using này cho BusinessRuleException

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class Session : AggregateRoot<Guid>
{
    // Private constructor cho EF Core
    private Session() : base(Guid.Empty)
    {
        SessionConfigs = new SessionConfigSnapshot();
    }

    private Session(Guid id, Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime, SessionStatus status,
        SessionConfigSnapshot sessionConfigs)
        : base(id)
    {
        ScheduleId = scheduleId;
        UserId = userId;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        SessionConfigs = sessionConfigs;
    }

    public Guid ScheduleId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Thêm thuộc tính SessionConfigs kiểu Value Object
    public SessionConfigSnapshot SessionConfigs { get; private set; }

    // Các property shortcuts cho config thông dụng (để dễ sử dụng)
    public int AttendanceWindowMinutes => SessionConfigs.AttendanceWindowMinutes;
    public int TotalAttendanceRounds => SessionConfigs.TotalAttendanceRounds;
    public int AbsentReportGracePeriodHours => SessionConfigs.AbsentReportGracePeriodHours;
    public int ManualAdjustmentGracePeriodHours => SessionConfigs.ManualAdjustmentGracePeriodHours;

    // Factory method để tạo Session từ dictionary configs
    public static Session Create(Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime,
        Dictionary<string, string> configs)
    {
        var sessionConfigs = SessionConfigSnapshot.FromDictionary(configs);
        // Mặc định tạo là Pending
        return new Session(Guid.NewGuid(), scheduleId, userId, startTime, endTime, SessionStatus.Pending,
            sessionConfigs);
    }

    // Factory method để tạo Session từ SessionConfigSnapshot (backward compatibility)
    public static Session Create(Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime,
        SessionConfigSnapshot sessionConfigs)
    {
        // Mặc định tạo là Pending
        return new Session(Guid.NewGuid(), scheduleId, userId, startTime, endTime, SessionStatus.Pending,
            sessionConfigs);
    }

    // Đã có hàm Update cho StartTime/EndTime
    public void Update(DateTime? startTime = null, DateTime? endTime = null)
    {
        if (startTime.HasValue && startTime.Value != StartTime)
        {
            StartTime = startTime.Value;
            UpdatedAt = DateTime.UtcNow;
        }

        if (endTime.HasValue && endTime.Value != EndTime)
        {
            EndTime = endTime.Value;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Update config method - cho phép cập nhật config sau khi tạo session
    public void UpdateConfig(string key, string value)
    {
        var newConfigs = SessionConfigs.ToDictionary();
        newConfigs[key] = value;
        SessionConfigs = SessionConfigSnapshot.FromDictionary(newConfigs);
        UpdatedAt = DateTime.UtcNow; // Cập nhật UpdatedAt khi config thay đổi
    }

    // Update multiple configs
    public void UpdateConfigs(Dictionary<string, string> configs)
    {
        SessionConfigs = SessionConfigs.Merge(configs);
        UpdatedAt = DateTime.UtcNow; // Cập nhật UpdatedAt khi config thay đổi
    }

    // --- Các hành vi nghiệp vụ thay đổi trạng thái ---
    public void ActivateSession()
    {
        if (Status != SessionStatus.Pending)
        {
            throw new BusinessRuleException("SESSION_NOT_PENDING",
                "Không thể kích hoạt phiên khi trạng thái không phải Pending.");
        }

        Status = SessionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteSession()
    {
        if (Status != SessionStatus.Active)
        {
            throw new BusinessRuleException("SESSION_NOT_ACTIVE",
                "Không thể hoàn thành phiên khi trạng thái không phải Active.");
        }

        Status = SessionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelSession()
    {
        // Có thể thêm logic kiểm tra nếu session đã Active thì cần xác nhận đặc biệt
        // Hoặc có thể hủy bỏ một phiên Active nếu cần
        Status = SessionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ArchiveSession()
    {
        Status = SessionStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    // --- Các hành vi nghiệp vụ sử dụng SessionConfigs ---

    public bool IsWithinAttendanceWindow(DateTime currentTime)
    {
        var windowMinutes = SessionConfigs.AttendanceWindowMinutes;
        var sessionStartTimeLimit = StartTime.Subtract(TimeSpan.FromMinutes(windowMinutes));
        var sessionEndTimeLimit = EndTime.Add(TimeSpan.FromMinutes(windowMinutes));
        return currentTime >= sessionStartTimeLimit && currentTime <= sessionEndTimeLimit;
    }

    public int GetRemainingRounds(int currentRound)
    {
        var totalRounds = SessionConfigs.TotalAttendanceRounds;
        return Math.Max(0, totalRounds - currentRound);
    }

    public bool IsWithinAbsentReportGracePeriod(DateTime reportTime)
    {
        var gracePeriodHours = SessionConfigs.AbsentReportGracePeriodHours;
        var gracePeriodEnd = EndTime.AddHours(gracePeriodHours);
        return reportTime <= gracePeriodEnd;
    }

    public bool IsWithinManualAdjustmentGracePeriod(DateTime adjustmentTime)
    {
        var gracePeriodHours = SessionConfigs.ManualAdjustmentGracePeriodHours;
        var gracePeriodEnd = EndTime.AddHours(gracePeriodHours);
        return adjustmentTime <= gracePeriodEnd;
    }

    // Helper methods để truy cập config dễ dàng
    public T GetConfig<T>(string key, T defaultValue = default)
    {
        var value = SessionConfigs[key];
        if (value == null) return defaultValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    public void SetConfig<T>(string key, T value)
    {
        UpdateConfig(key, value?.ToString() ?? "");
    }
}
