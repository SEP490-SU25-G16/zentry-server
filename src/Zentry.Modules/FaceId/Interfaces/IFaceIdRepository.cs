using Pgvector;
using Zentry.Modules.FaceId.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.FaceId.Interfaces;

public interface IFaceIdRepository : IRepository<FaceEmbedding, Guid>
{
    Task<FaceEmbedding?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FaceEmbedding> CreateAsync(Guid userId, Vector embedding, CancellationToken cancellationToken = default);
    Task<FaceEmbedding> UpdateAsync(Guid userId, Vector embedding, CancellationToken cancellationToken = default);

    Task<(bool IsMatch, float Similarity)> VerifyAsync(Guid userId, Vector embedding, float threshold = 0.7f,
        CancellationToken cancellationToken = default);
}