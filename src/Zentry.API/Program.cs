using Microsoft.EntityFrameworkCore;
using Polly;
using Zentry.Infrastructure;
using Zentry.Modules.AttendanceManagement.Application;
using Zentry.Modules.AttendanceManagement.Infrastructure;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.Modules.ConfigurationManagement;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.DeviceManagement.Application;
using Zentry.Modules.DeviceManagement.Infrastructure;
using Zentry.Modules.DeviceManagement.Infrastructure.Persistence;
using Zentry.Modules.NotificationService;
using Zentry.Modules.ReportingService;
using Zentry.Modules.ReportingService.Persistence;
using Zentry.Modules.ScheduleManagement.Application;
using Zentry.Modules.ScheduleManagement.Infrastructure;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;
using Zentry.Modules.UserManagement;
using Zentry.Modules.UserManagement.Persistence.DbContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsPolicyBuilder =>
        corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAttendanceInfrastructure(builder.Configuration);
builder.Services.AddAttendanceApplication();
builder.Services.AddScheduleInfrastructure(builder.Configuration);
builder.Services.AddScheduleApplication();
builder.Services.AddDeviceInfrastructure(builder.Configuration);
builder.Services.AddDeviceApplication();
builder.Services.AddConfigurationInfrastructure(builder.Configuration);
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddReportingInfrastructure(builder.Configuration);
builder.Services.AddUserInfrastructure(builder.Configuration);

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
        var deviceDbContext = serviceProvider.GetRequiredService<DeviceDbContext>();
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

    // Migrate User
    retryPolicy.Execute(() =>
    {
        var userDbContext = serviceProvider.GetRequiredService<UserDbContext>();
        logger.LogInformation("Applying migrations for UserDbContext...");
        userDbContext.Database.Migrate();
        logger.LogInformation("User migrations applied successfully.");
    });
}

app.Run();