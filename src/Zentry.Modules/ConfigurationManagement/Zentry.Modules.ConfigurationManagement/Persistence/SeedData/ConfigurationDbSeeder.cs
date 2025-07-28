using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ConfigurationManagement.Entities;
using Zentry.SharedKernel.Constants.Configuration;
using System.Collections.Generic; // Thêm namespace này
using System.Linq; // Thêm namespace này

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
        // Lưu ý: Cập nhật AllowedScopeTypes thành List<ScopeType>
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.AttendanceWindowMinutesAttrId, "attendanceWindowMinutes", "Thời gian mở cửa điểm danh",
            "Số phút trước và sau thời gian biểu cho phép tạo phiên điểm danh.", DataType.Int,
            new List<ScopeType> { ScopeType.Global, ScopeType.Course, ScopeType.Session }, // <-- AllowedScopeTypes
            "phút", "10", false // DefaultValue và IsDeletable
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.TotalAttendanceRoundsAttrId, "totalAttendanceRounds", "Tổng số vòng điểm danh",
            "Tổng số vòng điểm danh trong một phiên học.", DataType.Int,
            new List<ScopeType> { ScopeType.Global, ScopeType.Course, ScopeType.Session }, // <-- AllowedScopeTypes
            "vòng", "3", false
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.AbsentReportGracePeriodHoursAttrId, "absentReportGracePeriodHours", "Thời gian ân hạn báo vắng",
            "Số giờ sau khi phiên kết thúc để cho phép gửi báo cáo vắng mặt.", DataType.Int,
            new List<ScopeType> { ScopeType.Global, ScopeType.Course }, // Có thể chỉ Global, Course
            "giờ", "24", false
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.ManualAdjustmentGracePeriodHoursAttrId, "manualAdjustmentGracePeriodHours",
            "Thời gian ân hạn điều chỉnh thủ công",
            "Số giờ sau khi phiên kết thúc để cho phép điều chỉnh điểm danh thủ công.", DataType.Int,
            new List<ScopeType> { ScopeType.Global, ScopeType.Course, ScopeType.Session }, // Có thể Global, Course, Session
            "giờ", "48", false
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.MinRssiThresholdAttrId, "minRssiThreshold", "Ngưỡng RSSI tối thiểu",
            "Ngưỡng cường độ tín hiệu Bluetooth (RSSI) tối thiểu để coi thiết bị là có mặt.", DataType.Int,
            new List<ScopeType> { ScopeType.Global, ScopeType.Session }, // Có thể Global, Session
            "dBm", "-75", true // IsDeletable có thể là true nếu không quá quan trọng
        ).Generate());
        attributeDefinitions.Add(new AttributeDefinitionFaker(
            SeedGuids.MaxHopDistanceAttrId, "maxHopDistance", "Khoảng cách Hop tối đa",
            "Số bước nhảy tối đa cho thuật toán điểm danh đa bước (multi-hop).", DataType.Int,
            new List<ScopeType> { ScopeType.Global, ScopeType.Course }, // Có thể Global, Course
            "hops", "2", true
        ).Generate());

        // Thêm một AttributeDefinition kiểu Selection để seed Options
        var courseVisibilityAttr = new AttributeDefinitionFaker(
            Guid.NewGuid(), "courseVisibility", "Chế độ hiển thị khóa học",
            "Xác định liệu khóa học có công khai, riêng tư hay ẩn.", DataType.Selection,
            new List<ScopeType> { ScopeType.Course }, // Chỉ có thể cấu hình ở cấp Course
            null, "Public", false // DefaultValue và IsDeletable
        ).Generate();
        attributeDefinitions.Add(courseVisibilityAttr);


        await context.AttributeDefinitions.AddRangeAsync(attributeDefinitions);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {attributeDefinitions.Count} Attribute Definitions.");


        // --- Seed Settings ---
        var settings = new List<Setting>();

        // Global Settings (sử dụng ID cố định của AttributeDefinition)
        // DefaultValue của AttributeDefinition sẽ được sử dụng nếu không có Setting cụ thể
        // Ở đây chúng ta vẫn tạo Setting Global để nó xuất hiện trong DB nếu muốn xem
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
        settings.Add(new SettingFaker(courseVisibilityAttr.Id, ScopeType.Global, Guid.Empty, "Public").Generate()); // Default Global for Course Visibility

        // Course-specific Settings (Overrides Global)
        // Kiểm tra xem AttributeDefinition có AllowedScopeTypes chứa ScopeType.Course không
        if (attributeDefinitions.First(ad => ad.Id == SeedGuids.AttendanceWindowMinutesAttrId).AllowedScopeTypes.Contains(ScopeType.Course))
        {
            settings.Add(new SettingFaker(SeedGuids.AttendanceWindowMinutesAttrId, ScopeType.Course,
                SeedGuids.SampleCourseId, "15").Generate());
        }
        if (attributeDefinitions.First(ad => ad.Id == SeedGuids.TotalAttendanceRoundsAttrId).AllowedScopeTypes.Contains(ScopeType.Course))
        {
            settings.Add(new SettingFaker(SeedGuids.TotalAttendanceRoundsAttrId, ScopeType.Course, SeedGuids.SampleCourseId,
                "2").Generate());
        }
        if (attributeDefinitions.First(ad => ad.Id == SeedGuids.MaxHopDistanceAttrId).AllowedScopeTypes.Contains(ScopeType.Course))
        {
            settings.Add(new SettingFaker(SeedGuids.MaxHopDistanceAttrId, ScopeType.Course, SeedGuids.SampleCourseId, "1")
                .Generate());
        }
        // Course Visibility for SampleCourseId
        if (courseVisibilityAttr.AllowedScopeTypes.Contains(ScopeType.Course))
        {
            settings.Add(new SettingFaker(courseVisibilityAttr.Id, ScopeType.Course, SeedGuids.SampleCourseId, "Private").Generate());
        }


        // Session-specific Settings (Overrides Global and Course)
        // Kiểm tra xem AttributeDefinition có AllowedScopeTypes chứa ScopeType.Session không
        if (attributeDefinitions.First(ad => ad.Id == SeedGuids.AttendanceWindowMinutesAttrId).AllowedScopeTypes.Contains(ScopeType.Session))
        {
            settings.Add(new SettingFaker(SeedGuids.AttendanceWindowMinutesAttrId, ScopeType.Session,
                SeedGuids.SampleScheduleId, "5").Generate());
        }
        if (attributeDefinitions.First(ad => ad.Id == SeedGuids.MinRssiThresholdAttrId).AllowedScopeTypes.Contains(ScopeType.Session))
        {
            settings.Add(new SettingFaker(SeedGuids.MinRssiThresholdAttrId, ScopeType.Session, SeedGuids.SampleScheduleId,
                "-80").Generate());
        }


        await context.Settings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {settings.Count} Settings.");

        // --- Seed Options (cho các AttributeDefinition kiểu Selection) ---
        var options = new List<Option>();
        // Chỉ thêm options nếu courseVisibilityAttr được tạo thành công và là DataType.Selection
        if (courseVisibilityAttr != null && courseVisibilityAttr.DataType == DataType.Selection)
        {
            options.Add(new OptionFaker(courseVisibilityAttr.Id, "Public", "Công khai", 1).Generate());
            options.Add(new OptionFaker(courseVisibilityAttr.Id, "Private", "Riêng tư", 2).Generate());
            options.Add(new OptionFaker(courseVisibilityAttr.Id, "Hidden", "Ẩn", 3).Generate());
        }
        await context.Options.AddRangeAsync(options);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {options.Count} Options.");


        logger.LogInformation("Configuration Management seed data completed successfully.");
    }
}
