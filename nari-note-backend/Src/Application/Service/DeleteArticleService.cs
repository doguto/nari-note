using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class DeleteArticleService
{
    private readonly IArticleRepository _articleRepository;
    
    public DeleteArticleService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }
    
    public async Task ExecuteAsync(DeleteArticleRequest request)
    {
        var article = await _articleRepository.FindByIdAsync(request.Id)
            ?? throw new NotFoundException($"記事ID {request.Id} が見つかりません");
            
        if (article.AuthorId != request.UserId)
            throw new ForbiddenException("この記事を削除する権限がありません");
            
        await _articleRepository.DeleteAsync(request.Id);
    }
}
