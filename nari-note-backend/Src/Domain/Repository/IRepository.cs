using NariNoteBackend.Domain.Entity;

namespace NariNoteBackend.Domain.Repository;

public interface IRepository<T, TId> where T : EntityBase where TId : struct
{
    Task<T> CreateAsync(T entity);
    Task<T?> FindByIdAsync(TId id);
    Task<T> FindForceByIdAsync(TId id);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(TId id);
}
