using Zentry.SharedKernel.Abstractions.Domain;

namespace Zentry.SharedKernel.Abstractions.Data;

public interface IRepository<TEntity, in TId>
    where TEntity : IAggregateRoot<TId>
    where TId : notnull
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    // Các phương thức cơ bản mà mọi repository nên có
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}