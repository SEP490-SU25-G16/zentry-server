using Zentry.SharedKernel.Contracts.Messages;

namespace Zentry.Modules.AttendanceManagement.Application.Services.Interface;

public interface IAttendanceProcessorService
{
    Task ProcessBluetoothScanData(ProcessScanDataMessage message, CancellationToken cancellationToken);
}
