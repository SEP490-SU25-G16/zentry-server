namespace Zentry.Modules.AttendanceManagement.Application.Services;

public interface IAppConfigurationService
{
    // Cấu hình liên quan đến thời gian và ngưỡng điểm danh
    Task<TimeSpan> GetAttendanceWindowAsync(Guid? scopeId = null); // Có thể có window khác nhau cho từng Course/Session
    Task<TimeSpan> GetFaceIdVerificationTimeoutAsync();
    Task<int> GetBluetoothRssiThresholdAsync();
    Task<TimeSpan> GetContinuousScanIntervalAsync();

    Task<int>
        GetTotalAttendanceRoundsAsync(
            Guid? scopeId = null); // Tổng số round dựa trên thời lượng buổi học, có thể theo Course/Session

    // Cấu hình liên quan đến các grace period cho báo cáo/điều chỉnh
    Task<TimeSpan> GetAbsentReportGracePeriodAsync();
    Task<TimeSpan> GetManualAdjustmentGracePeriodAsync();

    // Phương thức chung để lấy cấu hình theo key và scope
    Task<string?> GetSettingValueAsync(string key, string scopeType, Guid scopeId);
    Task<string?> GetGlobalSettingValueAsync(string key);
}