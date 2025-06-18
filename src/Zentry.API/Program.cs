using System.Collections;
using Microsoft.EntityFrameworkCore;
using Polly;
using Zentry.Infrastructure;
using Zentry.Modules.DeviceManagement.Infrastructure;
using Zentry.Modules.DeviceManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// DEBUG: In ra tất cả environment variables
Console.WriteLine("=== DEBUG: Environment Variables ===");
foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
{
    Console.WriteLine($"{env.Key}={env.Value}");
}

// DEBUG: Kiểm tra connection strings
Console.WriteLine("=== DEBUG: Connection Strings ===");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"DefaultConnection: {connectionString}");

var postgresConnection = builder.Configuration["Postgres_ConnectionString"];
Console.WriteLine($"Postgres_ConnectionString: {postgresConnection}");

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddDeviceManagementInfrastructure(builder.Configuration);

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

    // DEBUG: Kiểm tra connection string trong DbContext
    var dbContext = scope.ServiceProvider.GetRequiredService<DeviceManagementDbContext>();
    var connString = dbContext.Database.GetConnectionString();
    logger.LogInformation("DbContext Connection String: {ConnectionString}", connString);

    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(5), (exception, timeSpan, retryCount, context) =>
        {
            logger.LogWarning(exception, "Migration attempt {RetryCount} failed. Retrying in {TimeSpan} seconds...", retryCount, timeSpan.TotalSeconds);
        });

    retryPolicy.Execute(() =>
    {
        logger.LogInformation("Applying migrations for DeviceManagementDbContext...");
        dbContext.Database.Migrate();
        logger.LogInformation("Migrations applied successfully.");
    });
}

app.Run();
