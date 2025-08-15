using Zentry.Modules.FaceId.Entities;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.Modules.FaceId.Dtos;

namespace Zentry.Modules.FaceId.Interfaces;

public interface IFaceIdRepository : IRepository<FaceEmbedding, Guid>
{
    Task<FaceEmbedding?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(Guid UserId, DateTime CreatedAt, DateTime UpdatedAt)?> GetMetaByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FaceEmbedding> CreateAsync(Guid userId, float[] embedding, CancellationToken cancellationToken = default);
    Task<FaceEmbedding> UpdateAsync(Guid userId, float[] embedding, CancellationToken cancellationToken = default);

    Task<(bool IsMatch, float Similarity)> VerifyAsync(Guid userId, float[] embedding, float threshold = 0.7f,
        CancellationToken cancellationToken = default);

    Task<FaceIdVerifyRequest> CreateVerifyRequestAsync(
        Guid requestGroupId,
        Guid targetUserId,
        Guid? initiatorUserId,
        Guid? sessionId,
        Guid? classSectionId,
        float threshold,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);

    Task<FaceIdVerifyRequest?> GetVerifyRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task CompleteVerifyRequestAsync(FaceIdVerifyRequest request, bool matched, float similarity, CancellationToken cancellationToken = default);

    Task CancelVerifyRequestsByGroupAsync(Guid requestGroupId, CancellationToken cancellationToken = default);

    Task<IEnumerable<UserFaceIdStatusDto>> GetAllUsersWithFaceIdStatusAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<UserFaceIdStatusDto>> GetUsersFaceIdStatusAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}