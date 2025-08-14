using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Threading.RateLimiting;
using Zentry.Infrastructure;
using Zentry.Modules.AttendanceManagement.Application;
using Zentry.Modules.AttendanceManagement.Infrastructure;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.Modules.ConfigurationManagement;
using Zentry.Modules.ConfigurationManagement.Persistence;
using Zentry.Modules.DeviceManagement;
using Zentry.Modules.DeviceManagement.Persistence;
using Zentry.Modules.FaceId;
using Zentry.Modules.NotificationService;
using Zentry.Modules.NotificationService.Hubs;
using Zentry.Modules.NotificationService.Persistence.Repository;
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

// ===== CẤU HÌNH RATE LIMITING =====
builder.Services.AddRateLimiter(options =>
{
    // Fixed Window Policy - Giới hạn số request trong một khoảng thời gian cố định
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 100; // Cho phép tối đa 100 requests
        opt.Window = TimeSpan.FromMinutes(1); // Trong 1 phút
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10; // Queue tối đa 10 requests chờ
    });

    // Sliding Window Policy - Cho phép giới hạn linh hoạt hơn
    options.AddSlidingWindowLimiter("SlidingPolicy", opt =>
    {
        opt.PermitLimit = 50; // Cho phép tối đa 50 requests
        opt.Window = TimeSpan.FromMinutes(1); // Trong 1 phút
        opt.SegmentsPerWindow = 6; // Chia thành 6 segment (10 giây mỗi segment)
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Token Bucket Policy - Cho phép burst traffic
    options.AddTokenBucketLimiter("TokenPolicy", opt =>
    {
        opt.TokenLimit = 100; // Bucket chứa tối đa 100 tokens
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // Thêm token mỗi 10 giây
        opt.TokensPerPeriod = 20; // Thêm 20 tokens mỗi lần
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    // Concurrency Limiter - Giới hạn số request đồng thời
    options.AddConcurrencyLimiter("ConcurrencyPolicy", opt =>
    {
        opt.PermitLimit = 50; // Tối đa 50 request đồng thời
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 20;
    });

    // Policy riêng cho các endpoint nhạy cảm (login, register, etc.)
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 5; // Chỉ cho phép 5 requests
        opt.Window = TimeSpan.FromMinutes(1); // Trong 1 phút
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // Không cho phép queue
    });

    // Global rate limiter cho tất cả requests
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 200, // 200 requests per IP
                Window = TimeSpan.FromMinutes(1) // trong 1 phút
            }));

    // Cấu hình response khi bị rate limit
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var apiResponse = ApiResponse.ErrorResult(
            ErrorCodes.RateLimitExceeded,
            "Too many requests. Please try again later."
        );

        await context.HttpContext.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(apiResponse),
            token
        );
    };
});

// ===== CẤU HÌNH CONTROLLERS VÀ JSON =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new DateTimeToLocalConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateTimeToLocalConverter());
    });

builder.Services.AddFluentValidationAutoValidation(config => { config.DisableDataAnnotationsValidation = true; });
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ===== CẤU HÌNH MODEL VALIDATION =====
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var firstError = context.ModelState
            .SelectMany(x => x.Value?.Errors!)
            .FirstOrDefault();

        var message = firstError?.ErrorMessage ?? ErrorMessages.InvalidDataFormat;

        if (IsGuidFormatError(firstError?.ErrorMessage)) message = ErrorMessages.GuidFormatInvalid;

        var apiResponse = ApiResponse.ErrorResult(ErrorCodes.ValidationError, message);
        return new BadRequestObjectResult(apiResponse);
    };
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

// ===== CẤU HÌNH CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsPolicyBuilder =>
        corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddAuthorization();

// --- Thêm health check ---
builder.Services.AddHealthChecks();

// ===== CẤU HÌNH MASSTRANSIT =====
builder.Services.AddMassTransit(x =>
{
    x.AddAttendanceMassTransitConsumers();
    x.AddUserMassTransitConsumers();
    x.AddNotificationMassTransitConsumers();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnectionString = builder.Configuration["RabbitMQ_ConnectionString"];
        if (string.IsNullOrEmpty(rabbitMqConnectionString))
            throw new InvalidOperationException("RabbitMQ_ConnectionString is not configured.");

        cfg.Host(new Uri(rabbitMqConnectionString));

        cfg.ConfigureAttendanceReceiveEndpoints(context);
        cfg.ConfigureUserReceiveEndpoints(context);
        cfg.ConfigureNotificationReceiveEndpoints(context);
    });
});

// --- Đăng ký các module ---
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddScheduleInfrastructure(builder.Configuration);
builder.Services.AddScheduleApplication();
builder.Services.AddConfigurationInfrastructure(builder.Configuration);
builder.Services.AddDeviceInfrastructure(builder.Configuration);
builder.Services.AddAttendanceInfrastructure(builder.Configuration);
builder.Services.AddAttendanceApplication();
builder.Services.AddNotificationModule(builder.Configuration);
builder.Services.AddFaceIdInfrastructure(builder.Configuration);

var app = builder.Build();

// ===== CẤU HÌNH MIDDLEWARE PIPELINE =====
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// *** QUAN TRỌNG: Thêm Rate Limiting middleware ***
app.UseRateLimiter();

app.UseValidationExceptionMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHealthChecks("/health");

// ===== DATABASE MIGRATION CODE =====
await RunDatabaseMigrationsAndSeedDataAsync(app);

app.Run();

// ===== HELPER METHODS =====
static bool IsGuidFormatError(string? errorMessage)
{
    if (string.IsNullOrEmpty(errorMessage)) return false;

    return errorMessage.Contains("GUID", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("is not valid", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("format", StringComparison.OrdinalIgnoreCase);
}

static async Task RunDatabaseMigrationsAndSeedDataAsync(WebApplication app)
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
        (typeof(UserDbContext), "UserDbContext"),
        (typeof(NotificationDbContext), "NotificationDbContext")
    };

    foreach (var (contextType, contextName) in migrations)
    {
        await retryPolicy.ExecuteAsync(async () =>
        {
            var dbContext = (DbContext)serviceProvider.GetRequiredService(contextType);
            logger.LogInformation("Applying migrations for {ContextName}...", contextName);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("{ContextName} migrations applied successfully.", contextName);
        });

        if (contextType == typeof(ConfigurationDbContext))
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                var configContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
                logger.LogInformation("Seeding data for ConfigurationDbContext...");
                await ConfigurationDbContext.SeedDataAsync(configContext, logger);
                logger.LogInformation("ConfigurationDbContext data seeded successfully.");
            });
        }
    }
}
