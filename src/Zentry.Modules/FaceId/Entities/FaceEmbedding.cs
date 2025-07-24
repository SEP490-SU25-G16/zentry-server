using Pgvector;
using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.FaceId.Entities;

public class FaceEmbedding : AggregateRoot<Guid>
{
    private FaceEmbedding() : base(Guid.Empty)
    {
        Embedding = null!; // Will be set by Entity Framework or factory method
    }

    private FaceEmbedding(Guid id, Guid userId, Vector embedding) : base(id)
    {
        UserId = userId;
        Embedding = embedding;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Vector Embedding { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static FaceEmbedding Create(Guid userId, Vector embedding)
    {
        return new FaceEmbedding(Guid.NewGuid(), userId, embedding);
    }

    public void UpdateEmbedding(Vector embedding)
    {
        Embedding = embedding;
        UpdatedAt = DateTime.UtcNow;
    }
} 