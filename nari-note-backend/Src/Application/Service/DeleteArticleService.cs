using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class DeleteArticleService
{
    readonly IArticleRepository articleRepository;
    
    public DeleteArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository;
    }
    
    public async Task ExecuteAsync(DeleteArticleRequest request)
    {
        var article = await articleRepository.GetByIdAsync(request.Id);
            
        if (article.AuthorId != request.UserId)
        {
            throw new ForbiddenException("この記事を削除する権限がありません");
        }
            
        await articleRepository.DeleteAsync(request.Id);
    }
}
