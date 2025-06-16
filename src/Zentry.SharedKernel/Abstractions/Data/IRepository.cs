using Zentry.SharedKernel.Abstractions.Domain;

namespace Zentry.SharedKernel.Abstractions.Data;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> FindAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}