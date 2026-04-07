using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Application.Service;

public class DeleteArticleService
{
    readonly IArticleRepository articleRepository;
    
    public DeleteArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }

    public async Task ExecuteAsync(UserId userId, DeleteArticleRequest request)
    {
        var article = await articleRepository.FindForceByIdAsync(request.Id);
        if (article.AuthorId != userId) throw new UnauthorizedAccessException("この記事を削除する権限がありません");
            
        await articleRepository.DeleteAsync(request.Id);
    }
}
