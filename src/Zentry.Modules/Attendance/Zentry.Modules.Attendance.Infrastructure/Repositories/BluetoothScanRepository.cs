using MongoDB.Driver;
using Zentry.Modules.Attendance.Application.Abstractions;
using Zentry.Modules.Attendance.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.Attendance.Infrastructure.Repositories;

public class BluetoothScanRepository : IBluetoothScanRepository
{
    private readonly IMongoCollection<BluetoothScan> _collection;

    public BluetoothScanRepository(IMongoClient client)
    {
        var database = client.GetDatabase("zentry");
        _collection = database.GetCollection<BluetoothScan>("BluetoothScans");
    }

    public async Task AddAsync(BluetoothScan scan)
    {
        await _collection.InsertOneAsync(scan);
    }

    public async Task<IEnumerable<BluetoothScan>> GetByDeviceIdAsync(Guid deviceId)
    {
        return await _collection.Find(s => s.DeviceId == deviceId).ToListAsync();
    }

    public Task<BluetoothScan> GetByIdAsync(object id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BluetoothScan>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BluetoothScan>> FindAsync(ISpecification<BluetoothScan> specification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(BluetoothScan entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(BluetoothScan entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(BluetoothScan entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
