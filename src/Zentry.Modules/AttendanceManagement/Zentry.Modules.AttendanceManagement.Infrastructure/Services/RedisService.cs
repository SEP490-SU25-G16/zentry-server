// File: Zentry.Infrastructure.Services/RedisService.cs (hoặc Zentry.Modules.AttendanceManagement.Infrastructure.Services/RedisService.cs)

using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Services; // Hoặc namespace phù hợp với module của bạn

public class RedisService : IRedisService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisService> _logger;

    public RedisService(string connectionString, ILogger<RedisService> logger)
    {
        _logger = logger;
        try
        {
            var redis = ConnectionMultiplexer.Connect(connectionString);
            _database = redis.GetDatabase();
            _logger.LogInformation("Successfully connected to Redis at {ConnectionString}", connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis at {ConnectionString}", connectionString);
            throw; // Re-throw the exception to prevent the application from starting without Redis
        }
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(value); // Serialize đối tượng thành chuỗi JSON
            return await _database.StringSetAsync(key, jsonValue, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set Redis key '{Key}' with value '{Value}'.", key, value);
            return false;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var redisValue = await _database.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(redisValue!); // Deserialize chuỗi JSON thành đối tượng T
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Redis key '{Key}'.", key);
            return default;
        }
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of Redis key '{Key}'.", key);
            return false;
        }
    }

    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            return await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove Redis key '{Key}'.", key);
            return false;
        }
    }
}
