using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ConfigurationManagement.Entities;
using Zentry.SharedKernel.Constants.Configuration;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ConfigurationManagement.Persistence;

public static class ConfigurationDbContextSeed
{
    public static async Task SeedAsync(ConfigurationDbContext dbContext, ILogger logger)
    {
        try
        {
            await SeedAttributeDefinitionsAsync(dbContext);
            await SeedSettingsAsync(dbContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the Configuration database.");
            throw;
        }
    }

    private static async Task SeedAttributeDefinitionsAsync(ConfigurationDbContext dbContext)
    {
        if (!await dbContext.AttributeDefinitions.AnyAsync())
        {
            var userScope = new List<ScopeType> { ScopeType.User };
            var sessionScope = new List<ScopeType> { ScopeType.Session };

            var attributeDefinitions = new List<AttributeDefinition>
            {
                AttributeDefinition.Create(
                    key: "StudentCode",
                    displayName: "Mã số sinh viên",
                    description: "Mã định danh duy nhất cho sinh viên",
                    dataType: DataType.String,
                    allowedScopeTypes: userScope,
                    unit: null, 
                    defaultValue: null, 
                    isDeletable: false),

                AttributeDefinition.Create(
                    key: "EmployeeCode",
                    displayName: "Mã số giảng viên",
                    description: "Mã định danh duy nhất cho giảng viên",
                    dataType: DataType.String,
                    allowedScopeTypes: userScope,
                    unit: null, 
                    defaultValue: null, 
                    isDeletable: false),

                AttributeDefinition.Create(
                    key: "AttendanceWindowMinutes",
                    displayName: "Thời gian cho phép điểm danh",
                    description: "Thời gian (phút) cho phép điểm danh sau khi bắt đầu phiên học",
                    dataType: DataType.Int,
                    allowedScopeTypes: sessionScope,
                    unit: "minutes", 
                    defaultValue: "15",
                    isDeletable: false),

                AttributeDefinition.Create(
                    key: "TotalAttendanceRounds",
                    displayName: "Số lần điểm danh",
                    description: "Tổng số lần điểm danh trong một phiên học",
                    dataType: DataType.Int,
                    allowedScopeTypes: sessionScope,
                    unit: null, 
                    defaultValue: "2",
                    isDeletable: false),

                AttributeDefinition.Create(
                    key: "AbsentReportGracePeriodHours",
                    displayName: "Thời gian ân hạn báo vắng",
                    description: "Thời gian (giờ) ân hạn để báo vắng có lý do sau khi phiên học kết thúc",
                    dataType: DataType.Int,
                    allowedScopeTypes: sessionScope,
                    unit: "hours", 
                    defaultValue: "24",
                    isDeletable: false),

                AttributeDefinition.Create(
                    key: "ManualAdjustmentGracePeriodHours",
                    displayName: "Thời gian ân hạn điều chỉnh",
                    description: "Thời gian (giờ) ân hạn để điều chỉnh điểm danh thủ công",
                    dataType: DataType.Int,
                    allowedScopeTypes: sessionScope,
                    unit: "hours", 
                    defaultValue: "24",
                    isDeletable: false)
            };

            await dbContext.AttributeDefinitions.AddRangeAsync(attributeDefinitions);
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedSettingsAsync(ConfigurationDbContext dbContext)
    {
        if (!await dbContext.Settings.AnyAsync())
        {
            var attributeDefinitions = await dbContext.AttributeDefinitions.ToListAsync();
            var settings = new List<Setting>();

            var attendanceWindowId = attributeDefinitions.Single(ad => ad.Key == "AttendanceWindowMinutes").Id;
            var totalRoundsId = attributeDefinitions.Single(ad => ad.Key == "TotalAttendanceRounds").Id;
            var absentReportId = attributeDefinitions.Single(ad => ad.Key == "AbsentReportGracePeriodHours").Id;
            var manualAdjId = attributeDefinitions.Single(ad => ad.Key == "ManualAdjustmentGracePeriodHours").Id;

            settings.Add(Setting.Create(
                attributeId: attendanceWindowId,
                scopeType: ScopeType.Global,
                scopeId: Guid.Empty,
                value: "15"));

            settings.Add(Setting.Create(
                attributeId: totalRoundsId,
                scopeType: ScopeType.Global,
                scopeId: Guid.Empty,
                value: "2"));

            settings.Add(Setting.Create(
                attributeId: absentReportId,
                scopeType: ScopeType.Global,
                scopeId: Guid.Empty,
                value: "24"));

            settings.Add(Setting.Create(
                attributeId: manualAdjId,
                scopeType: ScopeType.Global,
                scopeId: Guid.Empty,
                value: "24"));

            await dbContext.Settings.AddRangeAsync(settings);
            await dbContext.SaveChangesAsync();
        }
    }
}
