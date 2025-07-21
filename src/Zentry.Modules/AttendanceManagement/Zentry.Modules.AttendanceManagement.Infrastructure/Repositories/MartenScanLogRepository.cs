using Marten;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class MartenScanLogRepository(IDocumentSession session) : IScanLogRepository
{
    public async Task AddScanDataAsync(ScanLog record)
    {
        session.Store(record);
        await session.SaveChangesAsync();
    }

    public async Task<ScanLog> GetScanDataByIdAsync(Guid id)
    {
        return await session.LoadAsync<ScanLog>(id) ?? throw new NotFoundException(nameof(ScanLog), id);
    }
}