using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IKifuRepository : IRepository<Kifu, KifuId>
{
    Task ReplaceAllByArticleIdAsync(ArticleId articleId, List<Kifu> kifus);
}
