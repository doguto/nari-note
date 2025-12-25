using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Repository;

namespace NariNoteBackend.Application.Service;

public class DeleteArticleService
{
    private readonly IArticleRepository _articleRepository;
    
    public DeleteArticleService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }
    
    public async Task<DeleteArticleResponse> ExecuteAsync(DeleteArticleRequest request)
    {
        var article = await _articleRepository.FindByIdAsync(request.Id);
        if (article == null)
        {
            return DeleteArticleResponse.NotFound(request.Id);
        }
            
        if (article.AuthorId != request.UserId)
        {
            return DeleteArticleResponse.Forbidden(request.Id);
        }
            
        await _articleRepository.DeleteAsync(request.Id);
        return DeleteArticleResponse.Success();
    }
}
