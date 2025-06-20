using Zentry.Modules.Attendance.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.Attendance.Application.Abstractions;

public interface IBluetoothScanRepository : IRepository<BluetoothScan>
{
    Task AddAsync(BluetoothScan scan);
    Task<IEnumerable<BluetoothScan>> GetByDeviceIdAsync(Guid deviceId);
}
