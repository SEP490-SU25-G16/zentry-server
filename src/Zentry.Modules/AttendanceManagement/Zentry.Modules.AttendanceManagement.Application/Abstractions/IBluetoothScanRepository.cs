using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IBluetoothScanRepository : IRepository<BluetoothScan>
{
    Task AddAsync(BluetoothScan scan);
    Task<IEnumerable<BluetoothScan>> GetByDeviceIdAsync(Guid deviceId);
}