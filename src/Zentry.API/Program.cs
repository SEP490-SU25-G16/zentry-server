using System.Collections;
using Microsoft.EntityFrameworkCore;
using Polly;
using Zentry.Infrastructure;
using Zentry.Modules.Attendance.Infrastructure;
using Zentry.Modules.Attendance.Infrastructure.Persistence;
using Zentry.Modules.Configuration.Infrastructure;
using Zentry.Modules.Configuration.Infrastructure.Persistence;
using Zentry.Modules.DeviceManagement.Infrastructure;
using Zentry.Modules.DeviceManagement.Infrastructure.Persistence;
using Zentry.Modules.Notification;
using Zentry.Modules.Reporting;
using Zentry.Modules.Reporting.Persistence;
using Zentry.Modules.Schedule.Infrastructure;
using Zentry.Modules.Schedule.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAttendanceInfrastructure(builder.Configuration);
builder.Services.AddScheduleInfrastructure(builder.Configuration);
builder.Services.AddDeviceManagementInfrastructure(builder.Configuration);
builder.Services.AddConfigurationInfrastructure(builder.Configuration);
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddReportingInfrastructure(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var serviceProvider = scope.ServiceProvider;

    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(5),
            (exception, timeSpan, retryCount, context) =>
            {
                logger.LogWarning(exception, "Migration attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...",
                    retryCount, timeSpan.TotalSeconds);
            });

    // Migrate Attendance
    retryPolicy.Execute(() =>
    {
        var attendanceContext = serviceProvider.GetRequiredService<AttendanceDbContext>();
        logger.LogInformation("Applying migrations for AttendanceDbContext...");
        attendanceContext.Database.Migrate();
        logger.LogInformation("Attendance migrations applied successfully.");
    });

    // Migrate Schedule
    retryPolicy.Execute(() =>
    {
        var scheduleContext = serviceProvider.GetRequiredService<ScheduleDbContext>();
        logger.LogInformation("Applying migrations for ScheduleDbContext...");
        scheduleContext.Database.Migrate();
        logger.LogInformation("Schedule migrations applied successfully.");
    });

    // Migrate DeviceManagement
    retryPolicy.Execute(() =>
    {
        var deviceDbContext = serviceProvider.GetRequiredService<DeviceManagementDbContext>();
        logger.LogInformation("Applying migrations for DeviceManagementDbContext...");
        deviceDbContext.Database.Migrate();
        logger.LogInformation("DeviceManagement migrations applied successfully.");
    });

    // Migrate Configuration
    retryPolicy.Execute(() =>
    {
        var configDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
        logger.LogInformation("Applying migrations for ConfigurationDbContext...");
        configDbContext.Database.Migrate();
        logger.LogInformation("Configuration migrations applied successfully.");
    });

    // Migrate Reporting
    retryPolicy.Execute(() =>
    {
        var reportingDbContext = serviceProvider.GetRequiredService<ReportingDbContext>();
        logger.LogInformation("Applying migrations for ReportingDbContext...");
        reportingDbContext.Database.Migrate();
        logger.LogInformation("Reporting migrations applied successfully.");
    });
}

app.Run();
