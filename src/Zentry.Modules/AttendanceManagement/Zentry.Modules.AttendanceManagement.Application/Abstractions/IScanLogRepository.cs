using Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface IScanLogRepository
{
    Task AddScanDataAsync(ScanLog record);
    Task<ScanLog> GetScanDataByIdAsync(Guid id);
}