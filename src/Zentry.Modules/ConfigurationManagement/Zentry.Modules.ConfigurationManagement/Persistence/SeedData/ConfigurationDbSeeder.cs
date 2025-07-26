using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

// Cần Bogus để Randomizer.Seed

namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

// DbSeeder được inject ILogger, ServiceProvider, tương tự như User module
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

        // Đảm bảo Bogus sử dụng seed cố định để dữ liệu giả mạo được tạo ra nhất quán
        Randomizer.Seed = new Random(400);

        // --- Seed AttributeDefinitions ---
        var attributeDefinitions = new List<AttributeDefinition>();

        // Các AttributeDefinition quan trọng với ID cố định (dùng FromSeedingData)
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.AttendanceWindowMinutesAttrId, "attendanceWindowMinutes", "Thời gian mở cửa điểm danh",
            "Số phút trước và sau thời gian biểu cho phép tạo phiên điểm danh.", DataType.Int, ScopeType.Global, "phút"
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.TotalAttendanceRoundsAttrId, "totalAttendanceRounds", "Tổng số vòng điểm danh",
            "Tổng số vòng điểm danh trong một phiên học.", DataType.Int, ScopeType.Global, "vòng"
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.AbsentReportGracePeriodHoursAttrId, "absentReportGracePeriodHours", "Thời gian ân hạn báo vắng",
            "Số giờ sau khi phiên kết thúc để cho phép gửi báo cáo vắng mặt.", DataType.Int, ScopeType.Global, "giờ"
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.ManualAdjustmentGracePeriodHoursAttrId, "manualAdjustmentGracePeriodHours",
            "Thời gian ân hạn điều chỉnh thủ công",
            "Số giờ sau khi phiên kết thúc để cho phép điều chỉnh điểm danh thủ công.", DataType.Int, ScopeType.Global,
            "giờ"
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.MinRssiThresholdAttrId, "minRssiThreshold", "Ngưỡng RSSI tối thiểu",
            "Ngưỡng cường độ tín hiệu Bluetooth (RSSI) tối thiểu để coi thiết bị là có mặt.", DataType.Int,
            ScopeType.Global, "dBm"
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.MaxHopDistanceAttrId, "maxHopDistance", "Khoảng cách Hop tối đa",
            "Số bước nhảy tối đa cho thuật toán điểm danh đa bước (multi-hop).", DataType.Int, ScopeType.Global, "hops"
        ).Generate());

        await context.AttributeDefinitions.AddRangeAsync(attributeDefinitions);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {attributeDefinitions.Count} Attribute Definitions.");


        // --- Seed Settings ---
        var settings = new List<Setting>();

        // Global Settings (sử dụng ID cố định của AttributeDefinition)
        settings.Add(new SettingFaker(SeedGuids.AttendanceWindowMinutesAttrId, ScopeType.Global, Guid.Empty, "10")
            .Generate());
        settings.Add(
            new SettingFaker(SeedGuids.TotalAttendanceRoundsAttrId, ScopeType.Global, Guid.Empty, "3").Generate());
        settings.Add(new SettingFaker(SeedGuids.AbsentReportGracePeriodHoursAttrId, ScopeType.Global, Guid.Empty, "24")
            .Generate());
        settings.Add(new SettingFaker(SeedGuids.ManualAdjustmentGracePeriodHoursAttrId, ScopeType.Global, Guid.Empty,
            "48").Generate());
        settings.Add(new SettingFaker(SeedGuids.MinRssiThresholdAttrId, ScopeType.Global, Guid.Empty, "-75")
            .Generate());
        settings.Add(new SettingFaker(SeedGuids.MaxHopDistanceAttrId, ScopeType.Global, Guid.Empty, "2").Generate());

        // Course-specific Settings (Overrides Global)
        settings.Add(new SettingFaker(SeedGuids.AttendanceWindowMinutesAttrId, ScopeType.Course,
            SeedGuids.SampleCourseId, "15").Generate());
        settings.Add(new SettingFaker(SeedGuids.TotalAttendanceRoundsAttrId, ScopeType.Course, SeedGuids.SampleCourseId,
            "2").Generate());
        settings.Add(new SettingFaker(SeedGuids.MaxHopDistanceAttrId, ScopeType.Course, SeedGuids.SampleCourseId, "1")
            .Generate());

        // Session-specific Settings (Overrides Global and Course)
        settings.Add(new SettingFaker(SeedGuids.AttendanceWindowMinutesAttrId, ScopeType.Session,
            SeedGuids.SampleScheduleId, "5").Generate());
        settings.Add(new SettingFaker(SeedGuids.MinRssiThresholdAttrId, ScopeType.Session, SeedGuids.SampleScheduleId,
            "-80").Generate());

        await context.Settings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {settings.Count} Settings.");

        // --- Seed Options (nếu có các AttributeDefinition kiểu Selection) ---
        // Thêm logic seed Option nếu bạn có các AttributeDefinition có DataType.Selection
        /*
        var selectionAttrDef = AttributeDefinition.FromSeedingData(Guid.NewGuid(), "attendanceMethod", "Phương thức điểm danh", "Cách thức sinh viên điểm danh.", DataType.Selection, ScopeType.GLOBAL, null);
        await context.AttributeDefinitions.AddAsync(selectionAttrDef);
        await context.SaveChangesAsync();

        var options = new List<Option>();
        options.Add(new OptionFaker(selectionAttrDef.Id, "Bluetooth", "Điểm danh Bluetooth", 1).Generate());
        options.Add(new OptionFaker(selectionAttrDef.Id, "Manual", "Điểm danh thủ công", 2).Generate());
        options.Add(new OptionFaker(selectionAttrDef.Id, "Hybrid", "Điểm danh hỗn hợp", 3).Generate());
        await context.Options.AddRangeAsync(options);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {options.Count} Options.");
        */

        logger.LogInformation("Configuration Management seed data completed successfully.");
    }
}