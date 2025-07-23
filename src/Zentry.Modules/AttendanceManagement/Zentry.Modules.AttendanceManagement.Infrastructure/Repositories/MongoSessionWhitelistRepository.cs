using MongoDB.Driver;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Repositories;

public class MongoSessionWhitelistRepository(IMongoDatabase database) : IScanLogWhitelistRepository
{
    private readonly IMongoCollection<SessionWhitelist> _collection =
        database.GetCollection<SessionWhitelist>("SessionWhitelists"); // Tên collection

    public async Task AddAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(whitelist, cancellationToken: cancellationToken);
    }

    public async Task<SessionWhitelist?> GetBySessionIdAsync(Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _collection.Find(w => w.SessionId == sessionId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(SessionWhitelist whitelist, CancellationToken cancellationToken = default)
    {
        // MongoDB cần bạn chỉ định điều kiện để tìm document và bản cập nhật
        // Chúng ta sẽ tìm theo Id của whitelist và thay thế toàn bộ document
        var filter = Builders<SessionWhitelist>.Filter.Eq(w => w.Id, whitelist.Id);
        await _collection.ReplaceOneAsync(filter, whitelist, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        // IsUpsert = true sẽ tạo mới nếu không tìm thấy, hữu ích cho trường hợp cập nhật lần đầu
    }
}