namespace Zentry.SharedKernel.Abstractions.Data;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
}