using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface ITagRepository : IRepository<Tag, TagId>
{
    Task<List<Tag>> GetPopularTagsAsync(DateTime sinceDate, int limit);
}
