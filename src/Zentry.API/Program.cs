using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using Zentry.Infrastructure;
using Zentry.Modules.AttendanceManagement.Application;
using Zentry.Modules.AttendanceManagement.Infrastructure;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.Modules.ConfigurationManagement;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.DeviceManagement;
using Zentry.Modules.DeviceManagement.Persistence;
using Zentry.Modules.NotificationService;
using Zentry.Modules.ReportingService;
using Zentry.Modules.ReportingService.Persistence;
using Zentry.Modules.ScheduleManagement.Application;
using Zentry.Modules.ScheduleManagement.Infrastructure;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;
using Zentry.Modules.UserManagement;
using Zentry.Modules.UserManagement.Persistence.DbContext;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Helpers;
using Zentry.SharedKernel.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ===== CẤU HÌNH CONTROLLERS VÀ JSON =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tùy chọn này nếu bạn muốn PascalCase
        // THÊM CÁC CONVERTER DATETIME TẠI ĐÂY
        options.JsonSerializerOptions.Converters.Add(new DateTimeToLocalConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateTimeToLocalConverter());
    });

// ===== CẤU HÌNH MODEL VALIDATION =====
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var firstError = context.ModelState
            .SelectMany(x => x.Value.Errors)
            .FirstOrDefault();

        var message = firstError?.ErrorMessage ?? ErrorMessages.InvalidDataFormat;

        if (IsGuidFormatError(firstError?.ErrorMessage)) message = ErrorMessages.GuidFormatInvalid;

        var apiResponse = ApiResponse.ErrorResult(ErrorCodes.ValidationError, message);
        return new BadRequestObjectResult(apiResponse);
    };
});

// ===== CẤU HÌNH CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsPolicyBuilder =>
        corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddAuthorization();

// ===== CẤU HÌNH MASSTRANSIT =====
builder.Services.AddMassTransit(x =>
{
    x.AddAttendanceMassTransitConsumers();
    x.AddUserMassTransitConsumers();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnectionString = builder.Configuration["RabbitMQ_ConnectionString"];

        if (string.IsNullOrEmpty(rabbitMqConnectionString))
            throw new InvalidOperationException(
                "RabbitMQ_ConnectionString is not configured in appsettings.json or environment variables.");

        cfg.Host(new Uri(rabbitMqConnectionString));
        cfg.ConfigureAttendanceReceiveEndpoints(context);
        cfg.ConfigureUserReceiveEndpoints(context);
    });
});

// ===== ĐĂNG KÝ CÁC MODULES =====
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddScheduleInfrastructure(builder.Configuration);
builder.Services.AddScheduleApplication();
builder.Services.AddConfigurationInfrastructure(builder.Configuration);
builder.Services.AddDeviceInfrastructure(builder.Configuration);
builder.Services.AddAttendanceInfrastructure(builder.Configuration);
builder.Services.AddAttendanceApplication();
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddReportingInfrastructure(builder.Configuration);

var app = builder.Build();

// ===== CẤU HÌNH MIDDLEWARE PIPELINE =====
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseValidationExceptionMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===== DATABASE MIGRATION CODE =====
await RunDatabaseMigrationsAsync(app);

app.Run();

// ===== HELPER METHODS =====
static bool IsGuidFormatError(string? errorMessage)
{
    if (string.IsNullOrEmpty(errorMessage)) return false;

    return errorMessage.Contains("GUID", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("is not valid", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("format", StringComparison.OrdinalIgnoreCase);
}

static async Task RunDatabaseMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var serviceProvider = scope.ServiceProvider;

    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(5),
            (exception, timeSpan, retryCount, context) =>
            {
                logger.LogWarning(exception,
                    "Migration attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...",
                    retryCount, timeSpan.TotalSeconds);
            });

    var migrations = new[]
    {
        (typeof(AttendanceDbContext), "AttendanceDbContext"),
        (typeof(ScheduleDbContext), "ScheduleDbContext"),
        (typeof(DeviceDbContext), "DeviceManagementDbContext"),
        (typeof(ConfigurationDbContext), "ConfigurationDbContext"),
        (typeof(ReportingDbContext), "ReportingDbContext"),
        (typeof(UserDbContext), "UserDbContext")
    };

    foreach (var (contextType, contextName) in migrations)
        await retryPolicy.ExecuteAsync(async () =>
        {
            var dbContext = (DbContext)serviceProvider.GetRequiredService(contextType);
            logger.LogInformation("Applying migrations for {ContextName}...", contextName);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("{ContextName} migrations applied successfully.", contextName);
        });
}