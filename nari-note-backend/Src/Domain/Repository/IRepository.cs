using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Repository;

public interface IRepository<T> where T : EntityBase
{
    Task<T> CreateAsync(T entity);
    Task<T?> FindByIdAsync(int id);
    Task<T> FindForceByIdAsync(int id);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
