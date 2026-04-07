using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IArticleTagRepository : IRepository<ArticleTag, ArticleTagId>
{
}
