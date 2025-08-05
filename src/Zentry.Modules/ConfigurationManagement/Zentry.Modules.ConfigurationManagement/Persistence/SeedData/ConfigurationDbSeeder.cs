using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ConfigurationManagement.Entities;
using Zentry.SharedKernel.Constants.Configuration;

namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public class ConfigurationDbSeeder(IServiceProvider serviceProvider, ILogger<ConfigurationDbSeeder> logger)
{
    public async Task SeedAllAsync(bool recreateDatabase = false)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

        try
        {
            logger.LogInformation("Starting database seeding process for Configuration Management module...");

            if (recreateDatabase)
            {
                logger.LogWarning("Recreating database for Configuration Management module...");
                await context.Database.EnsureDeletedAsync();
            }

            logger.LogInformation("Applying pending migrations for Configuration Management module...");
            await context.Database.MigrateAsync();

            await SeedConfigurationData(context, logger);

            logger.LogInformation("Configuration Management module data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Configuration Management module database");
            throw;
        }
    }

    private static async Task SeedConfigurationData(ConfigurationDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting Configuration Management seed data...");

        if (await context.AttributeDefinitions.AnyAsync())
        {
            logger.LogInformation("Configuration data already exists. Skipping seed.");
            return;
        }

        Randomizer.Seed = new Random(400);

        var attributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinitionFaker(
                SeedGuids.AttendanceWindowMinutesAttrId, "attendanceWindowMinutes", "Thời gian mở cửa điểm danh",
                "Số phút trước và sau thời gian biểu cho phép tạo phiên điểm danh.", DataType.Int,
                new List<ScopeType> { ScopeType.Global },
                "phút", "10", false
            ).Generate(),
            new AttributeDefinitionFaker(
                SeedGuids.TotalAttendanceRoundsAttrId, "totalAttendanceRounds", "Tổng số vòng điểm danh",
                "Tổng số vòng điểm danh trong một phiên học.", DataType.Int,
                new List<ScopeType> { ScopeType.Global, ScopeType.Course, ScopeType.Session },
                "vòng", "3", false
            ).Generate(),
            new AttributeDefinitionFaker(
                SeedGuids.AbsentReportGracePeriodHoursAttrId, "absentReportGracePeriodHours",
                "Thời gian ân hạn báo vắng",
                "Số giờ sau khi phiên kết thúc để cho phép gửi báo cáo vắng mặt.", DataType.Int,
                [ScopeType.Global],
                "giờ", "24", false
            ).Generate(),
            new AttributeDefinitionFaker(
                SeedGuids.ManualAdjustmentGracePeriodHoursAttrId, "manualAdjustmentGracePeriodHours",
                "Thời gian ân hạn điều chỉnh thủ công",
                "Số giờ sau khi phiên kết thúc để cho phép điều chỉnh điểm danh thủ công.", DataType.Int,
                [ScopeType.Global],
                "giờ", "48", false
            ).Generate()
        };


        await context.AttributeDefinitions.AddRangeAsync(attributeDefinitions);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {attributeDefinitions.Count} Attribute Definitions.");

        var settings = new List<Setting>
        {
            new SettingFaker(SeedGuids.AttendanceWindowMinutesAttrId, ScopeType.Global, Guid.Empty, "10")
                .Generate(),
            new SettingFaker(SeedGuids.TotalAttendanceRoundsAttrId, ScopeType.Global, Guid.Empty, "3").Generate(),
            new SettingFaker(SeedGuids.AbsentReportGracePeriodHoursAttrId, ScopeType.Global, Guid.Empty, "24")
                .Generate(),
            new SettingFaker(SeedGuids.ManualAdjustmentGracePeriodHoursAttrId, ScopeType.Global, Guid.Empty,
                "48").Generate()
        };

        await context.Settings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
    }
}