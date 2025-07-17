using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class Session : AggregateRoot<Guid>
{
    // Private constructor cho EF Core
    private Session() : base(Guid.Empty)
    {
        SessionConfigs = new SessionConfigSnapshot();
    }

    // Constructor chính, nhận ID và các thuộc tính cơ bản
    private Session(Guid id, Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime,
        SessionConfigSnapshot sessionConfigs)
        : base(id)
    {
        ScheduleId = scheduleId;
        UserId = userId;
        StartTime = startTime;
        EndTime = endTime;
        CreatedAt = DateTime.UtcNow;
        SessionConfigs = sessionConfigs ?? new SessionConfigSnapshot();
    }

    public Guid ScheduleId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DateTime CreatedAt { get; private set; }

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
        return new Session(Guid.NewGuid(), scheduleId, userId, startTime, endTime, sessionConfigs);
    }

    // Factory method để tạo Session từ SessionConfigSnapshot (backward compatibility)
    public static Session Create(Guid scheduleId, Guid userId, DateTime startTime, DateTime endTime,
        SessionConfigSnapshot sessionConfigs)
    {
        return new Session(Guid.NewGuid(), scheduleId, userId, startTime, endTime, sessionConfigs);
    }

    public void Update(DateTime? startTime = null, DateTime? endTime = null)
    {
        if (startTime.HasValue) StartTime = startTime.Value;
        if (endTime.HasValue) EndTime = endTime.Value;
    }

    // Update config method - cho phép cập nhật config sau khi tạo session
    public void UpdateConfig(string key, string value)
    {
        var newConfigs = SessionConfigs.ToDictionary();
        newConfigs[key] = value;
        SessionConfigs = SessionConfigSnapshot.FromDictionary(newConfigs);
    }

    // Update multiple configs
    public void UpdateConfigs(Dictionary<string, string> configs)
    {
        SessionConfigs = SessionConfigs.Merge(configs);
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