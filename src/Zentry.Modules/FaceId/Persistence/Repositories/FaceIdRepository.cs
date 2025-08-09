using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Zentry.Modules.FaceId.Entities;
using Zentry.Modules.FaceId.Interfaces;

namespace Zentry.Modules.FaceId.Persistence.Repositories;

public class FaceIdRepository : IFaceIdRepository
{
    private readonly FaceIdDbContext _dbContext;

    public FaceIdRepository(FaceIdDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FaceEmbedding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FaceEmbeddings.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<FaceEmbedding?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FaceEmbeddings
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FaceEmbeddings
            .AnyAsync(e => e.UserId == userId, cancellationToken);
    }

    public async Task<FaceEmbedding> CreateAsync(Guid userId, Vector embedding,
        CancellationToken cancellationToken = default)
    {
        var faceEmbedding = FaceEmbedding.Create(userId, embedding);
        _dbContext.FaceEmbeddings.Add(faceEmbedding);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return faceEmbedding;
    }

    public async Task<FaceEmbedding> UpdateAsync(Guid userId, Vector embedding,
        CancellationToken cancellationToken = default)
    {
        // Check if user has face ID using simple query
        var exists = await _dbContext.FaceEmbeddings
            .AnyAsync(e => e.UserId == userId, cancellationToken);

        if (!exists) throw new InvalidOperationException($"Face embedding for user {userId} not found");

        // Use raw SQL to update the vector to avoid Entity Framework issues
        var embeddingArray = embedding.ToArray();
        var vectorString = "[" +
                           string.Join(",",
                               embeddingArray.Select(f => f.ToString("F6", CultureInfo.InvariantCulture))) + "]";

        var sql =
            $"UPDATE \"FaceEmbeddings\" SET \"Embedding\" = '{vectorString}'::vector, \"UpdatedAt\" = NOW() WHERE \"UserId\" = '{userId}'";
        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);

        if (rowsAffected == 0)
            throw new InvalidOperationException($"Failed to update face embedding for user {userId}");

        // Return a new entity with updated data
        return FaceEmbedding.Create(userId, embedding);
    }

    public async Task<(bool IsMatch, float Similarity)> VerifyAsync(Guid userId, Vector embedding,
        float threshold = 0.7f, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user exists first
            var exists = await _dbContext.FaceEmbeddings
                .AnyAsync(e => e.UserId == userId, cancellationToken);

            if (!exists) return (false, 0);

            // For verification, we'll use a simpler approach - compare with cosine distance in SQL
            var embeddingArray = embedding.ToArray();
            var vectorString = "[" +
                               string.Join(",",
                                   embeddingArray.Select(f => f.ToString("F6", CultureInfo.InvariantCulture))) + "]";

            var sql =
                $@"SELECT (1 - (""Embedding"" <=> '{vectorString}'::vector))::real FROM ""FaceEmbeddings"" WHERE ""UserId"" = '{userId}'";

            // Use raw ADO.NET to get the result
            using var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync(cancellationToken);
            var similarity = result != null ? Convert.ToSingle(result) : 0f;

            return (similarity >= threshold, similarity);
        }
        catch (Exception)
        {
            // Fallback: If SQL fails, just return false for security
            return (false, 0);
        }
    }

    public async Task<IEnumerable<FaceEmbedding>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FaceEmbeddings.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FaceEmbedding entity, CancellationToken cancellationToken = default)
    {
        _dbContext.FaceEmbeddings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FaceEmbedding entity, CancellationToken cancellationToken = default)
    {
        _dbContext.FaceEmbeddings.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(FaceEmbedding entity, CancellationToken cancellationToken = default)
    {
        _dbContext.FaceEmbeddings.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<FaceEmbedding> entities, CancellationToken cancellationToken = default)
    {
        await _dbContext.FaceEmbeddings.AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static float CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            throw new ArgumentException("Vectors must have the same length");

        float dotProduct = 0;
        float magnitude1 = 0;
        float magnitude2 = 0;

        for (var i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = MathF.Sqrt(magnitude1);
        magnitude2 = MathF.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null) await DeleteAsync(entity, cancellationToken);
    }
}