using Microsoft.EntityFrameworkCore;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Infrastructure.Persistence;
using Zentry.SharedKernel.Constants.Attendance;

// Có thể cần nếu bạn dùng NotFoundException ở đây

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class AttendanceRecordRepository(AttendanceDbContext dbContext) : IAttendanceRecordRepository
{
    // Phương thức bạn yêu cầu: Lấy bản ghi điểm danh theo UserId và SessionId
    public async Task<AttendanceRecord?> GetByUserIdAndSessionIdAsync(Guid userId, Guid sessionId,
        CancellationToken cancellationToken)
    {
        return await dbContext.AttendanceRecords
            .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.SessionId == sessionId, cancellationToken);
    }

    // Phương thức tiện ích để thêm hoặc cập nhật
    public async Task AddOrUpdateAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        var existingEntity = await dbContext.AttendanceRecords
                                 .FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken)
                             ?? await dbContext.AttendanceRecords.AsNoTracking()
                                 .FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken);

        if (existingEntity is null)
        {
            await dbContext.AttendanceRecords.AddAsync(entity, cancellationToken);
        }
        else
        {
            dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            dbContext.Entry(existingEntity).State = EntityState.Modified;
        }
    }


    public async Task<List<AttendanceRecord>> GetAttendanceRecordsBySessionIdAsync(Guid sessionId,
        CancellationToken cancellationToken)
    {
        return await dbContext.AttendanceRecords
            .Where(ar => ar.SessionId == sessionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.AttendanceRecords.ToListAsync(cancellationToken);
    }

    public async Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.AttendanceRecords.FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }

    public async Task AddAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        await dbContext.AttendanceRecords.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<AttendanceRecord> entities, CancellationToken cancellationToken)
    {
        await dbContext.AttendanceRecords.AddRangeAsync(entities, cancellationToken);
    }

    public Task UpdateAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        // EF Core sẽ theo dõi thay đổi nếu entity được load từ cùng context và thay đổi properties
        // Nếu entity đến từ ngoài context, bạn cần Attach hoặc dùng Update.
        dbContext.AttendanceRecords.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AttendanceRecord entity, CancellationToken cancellationToken)
    {
        dbContext.AttendanceRecords.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(int TotalSessions, int AttendedSessions)> GetAttendanceStatsAsync(Guid studentId, Guid courseId)
    {
        // Giả định `courseId` có thể được dùng để filter sessions.
        // Bạn cần có một cách nào đó để liên kết Session với Course.
        // Hiện tại AttendanceRecord chỉ có SessionId, không có CourseId trực tiếp.
        // Nếu CourseId không có trong AttendanceRecord, bạn cần join với bảng Session hoặc Course.
        // Ví dụ tạm thời dựa trên tất cả các sessions mà sinh viên đó có record.

        // Đây là ví dụ chung, bạn cần điều chỉnh để phù hợp với schema của mình
        var totalSessionsQuery = dbContext.AttendanceRecords
            .Where(ar => ar.UserId == studentId)
            .Select(ar => ar.SessionId)
            .Distinct();

        var totalSessions = await totalSessionsQuery.CountAsync();

        var attendedSessions = await totalSessionsQuery
            .Where(sessionId => dbContext.AttendanceRecords
                .Any(ar => ar.UserId == studentId &&
                           ar.SessionId == sessionId &&
                           ar.Status ==
                           AttendanceStatus.Present)) // Giả sử Present là Attended
            .CountAsync();

        return (totalSessions, attendedSessions);
    }
}