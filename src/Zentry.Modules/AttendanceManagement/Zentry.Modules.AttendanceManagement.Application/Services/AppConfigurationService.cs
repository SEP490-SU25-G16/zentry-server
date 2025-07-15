using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.SharedKernel.Contracts.Configuration;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Services;

public static class AttendanceScopeTypes
{
    public const string Global = "GLOBAL";
    public const string Course = "COURSE";
    public const string Session = "SESSION";
}

public class AppConfigurationService(
    IMediator mediator,
    IRedisService redisService,
    ILogger<AppConfigurationService> logger)
    : IAppConfigurationService
{
    private readonly TimeSpan _localCacheExpiry = TimeSpan.FromMinutes(30);

    public async Task<string?> GetConfigurationValueAsync(string key, string scopeType, Guid scopeId)
    {
        // Tạo cache key cục bộ cho Attendance Module
        var localCacheKey = $"appconfig:{key}:{scopeType}:{scopeId}";

        // 1. Thử lấy từ cache cục bộ của Attendance Module
        var cachedValue = await redisService.GetAsync<string>(localCacheKey);
        if (!string.IsNullOrEmpty(cachedValue))
        {
            logger.LogDebug("Cache hit for local config key: {Key}", localCacheKey);
            return cachedValue;
        }

        logger.LogDebug("Local cache miss for config key: {Key}. Fetching from Configuration Module.", localCacheKey);

        // 2. Nếu không có trong cache, GỌI IConfigurationService TỪ CONFIGURATION MODULE
        var request = new GetConfigurationsIntegrationQuery(key, scopeType, scopeId);

        try
        {
            var response = await mediator.Send(request);
            var configDto = response.Items.FirstOrDefault(c =>
                c.AttributeKey.Equals(key,
                    StringComparison.OrdinalIgnoreCase));

            if (configDto != null)
            {
                // 3. Cache vào Redis cục bộ nếu là scope type được phép
                // So sánh bằng chuỗi, không phụ thuộc vào Smart Enum của Config Module
                if (!configDto.ScopeType.Equals(AttendanceScopeTypes.Global, StringComparison.OrdinalIgnoreCase) &&
                    !configDto.ScopeType.Equals(AttendanceScopeTypes.Course, StringComparison.OrdinalIgnoreCase) &&
                    !configDto.ScopeType.Equals(AttendanceScopeTypes.Session, StringComparison.OrdinalIgnoreCase))
                    return configDto.Value;
                await redisService.SetAsync(localCacheKey, configDto.Value, _localCacheExpiry);
                logger.LogInformation("Cached config '{Key}' (Scope: {ScopeType}, ID: {ScopeId}) locally.", key,
                    scopeType, scopeId);

                return configDto.Value;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve configuration '{Key}' (Scope: {ScopeType}, ID: {ScopeId}) from Configuration Module.",
                key, scopeType, scopeId);
            // Có thể throw lại, hoặc trả về giá trị mặc định, tùy theo chính sách lỗi của bạn
        }

        // 4. Trả về null nếu không tìm thấy hoặc có lỗi
        return null;
    }

    public async Task<string?> GetGlobalConfigurationValueAsync(string key)
    {
        // Đối với cấu hình GLOBAL, ScopeId là Guid.Empty
        return await GetConfigurationValueAsync(key, AttendanceScopeTypes.Global, Guid.Empty);
    }

    // --- Các phương thức lấy cấu hình cụ thể (giữ nguyên logic gọi GetConfigurationValueAsync) ---

    public async Task<TimeSpan> GetAttendanceWindowAsync(Guid? scopeId = null)
    {
        string? value = null;
        if (scopeId.HasValue && scopeId.Value != Guid.Empty)
        {
            value = await GetConfigurationValueAsync("AttendanceWindowMinutes", AttendanceScopeTypes.Session,
                        scopeId.Value)
                    ?? await GetConfigurationValueAsync("AttendanceWindowMinutes", AttendanceScopeTypes.Course,
                        scopeId.Value);
        }

        value ??= await GetGlobalConfigurationValueAsync("AttendanceWindowMinutes");

        if (int.TryParse(value, out var minutes)) return TimeSpan.FromMinutes(minutes);
        logger.LogWarning("Invalid or missing 'AttendanceWindowMinutes' configuration. Using default: 15 minutes.");
        return TimeSpan.FromMinutes(15);
    }

    public async Task<TimeSpan> GetFaceIdVerificationTimeoutAsync()
    {
        var value = await GetGlobalConfigurationValueAsync("FaceIdVerificationTimeoutSeconds");
        if (int.TryParse(value, out var seconds)) return TimeSpan.FromSeconds(seconds);
        logger.LogWarning(
            "Invalid or missing 'FaceIdVerificationTimeoutSeconds' configuration. Using default: 30 seconds.");
        return TimeSpan.FromSeconds(30);
    }

    public async Task<int> GetBluetoothRssiThresholdAsync()
    {
        var value = await GetGlobalConfigurationValueAsync("BluetoothRssiThreshold");
        if (int.TryParse(value, out var threshold)) return threshold;
        logger.LogWarning("Invalid or missing 'BluetoothRssiThreshold' configuration. Using default: -70 dBm.");
        return -70;
    }

    public async Task<TimeSpan> GetContinuousScanIntervalAsync()
    {
        var value = await GetGlobalConfigurationValueAsync("ContinuousScanIntervalSeconds");
        if (int.TryParse(value, out var seconds)) return TimeSpan.FromSeconds(seconds);
        logger.LogWarning(
            "Invalid or missing 'ContinuousScanIntervalSeconds' configuration. Using default: 30 seconds.");
        return TimeSpan.FromSeconds(30);
    }

    public async Task<int> GetTotalAttendanceRoundsAsync(Guid? scopeId = null)
    {
        string? value = null;
        if (scopeId.HasValue && scopeId.Value != Guid.Empty)
        {
            value = await GetConfigurationValueAsync("TotalAttendanceRounds", AttendanceScopeTypes.Session,
                        scopeId.Value)
                    ?? await GetConfigurationValueAsync("TotalAttendanceRounds", AttendanceScopeTypes.Course,
                        scopeId.Value);
        }

        value ??= await GetGlobalConfigurationValueAsync("TotalAttendanceRounds");

        if (int.TryParse(value, out var rounds)) return rounds;
        logger.LogWarning("Invalid or missing 'TotalAttendanceRounds' configuration. Using default: 10 rounds.");
        return 10;
    }

    public async Task<TimeSpan> GetAbsentReportGracePeriodAsync()
    {
        var value = await GetGlobalConfigurationValueAsync("AbsentReportGracePeriodHours");
        if (int.TryParse(value, out var hours)) return TimeSpan.FromHours(hours);
        logger.LogWarning("Invalid or missing 'AbsentReportGracePeriodHours' configuration. Using default: 24 hours.");
        return TimeSpan.FromHours(24);
    }

    public async Task<TimeSpan> GetManualAdjustmentGracePeriodAsync()
    {
        var value = await GetGlobalConfigurationValueAsync("ManualAdjustmentGracePeriodHours");
        if (int.TryParse(value, out var hours)) return TimeSpan.FromHours(hours);
        logger.LogWarning(
            "Invalid or missing 'ManualAdjustmentGracePeriodHours' configuration. Using default: 24 hours.");
        return TimeSpan.FromHours(24);
    }
}
