using MongoDB.Driver;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class MongoScanLogRepository(IMongoDatabase database) : IScanLogRepository
{
    private readonly IMongoCollection<ScanLog> _collection = database.GetCollection<ScanLog>("scanData");

    public async Task AddScanDataAsync(ScanLog record)
    {
        await _collection.InsertOneAsync(record);
    }

    public async Task<ScanLog> GetScanDataByIdAsync(Guid id)
    {
        return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }
}